import asyncio
import logging
import os
import time

import aiohttp
import socketio

from slurk_setup_descil.components.slurk_api import (create_forward_room,
                                                     create_room_token,
                                                     create_user, get,
                                                     redirect_user,
                                                     set_permissions)
from slurk_setup_descil.components.utils import catch_error

LOG = logging.getLogger(__name__)
LOG.setLevel(logging.DEBUG)

_async_tasks = dict()

CHATBOT_URL = os.environ.get("CHATBOT_URL", "http://localhost:84")
MANAGERBOT_URL = os.environ.get("MANAGERBOT_URL", "http://localhost:84")


class ConciergeBot:
    def __init__(self, setup, host, port):
        """This bot lists users joining a designated
        waiting room and sends a group of users to a task room
        as soon as the minimal number of users needed for the
        task is reached.

        :param setup: configuration dict from REST endpoint
        :param host: Full URL including protocol and hostname.
        :type host: str
        :param port: Port used by the slurk chat server.
        :type port: int
        """

        self.setup = setup
        print("SETUP", setup, flush=True)

        self.api_token = setup["api_token"]
        self.concierge_token = setup["concierge_token"]
        self.concierge_user = setup["concierge_user"]
        self.waiting_room_id = setup["waiting_room_id"]
        self.waiting_room_task_id = setup["waiting_room_task_id"]
        self.chat_room_id = setup["chat_room_id"]
        self.timeout_redirect_url = setup["waiting_room_timeout_url"]
        self.timeout = setup["waiting_room_timeout_seconds"]
        self.num_users = setup["num_users"]
        self.min_num_users = setup["min_num_users"]
        self.chat_room_timeout_seconds = setup["chat_room_timeout_seconds"]
        self.chat_room_timeout_url = setup["chat_room_timeout_url"]
        self.min_num_users_chat_room = setup["min_num_users_chat_room"]

        self.num_users_in_room_missing = self.num_users
        self.timeout_manager_active = False
        self.waiting_room_closed = False
        self.managerbot_id = None

        self.active_users = set()
        self.joined_users = set()

        self.uri = host
        if port is not None:
            self.uri += f":{port}"
        sio = self.sio = socketio.AsyncClient()

        LOG.debug(
            f"Running concierge bot on {self.uri} with token {self.concierge_token}"
        )

        self.redirect_room_id = None

        @sio.event
        async def status(data):
            print("STATUS GOT", data)
            type_ = data["type"]
            if type_ not in ("join", "leave"):
                return
            user = data["user"]
            if data["room"] != self.waiting_room_id:
                print("IGNORE USER", user, "WRONG ROOM")
                return

            task = await self.get_user_task(user)
            if not task:
                # should not happen, but was in the original bot example
                return
            if type_ == "join":
                    await self.user_task_join(user, task)
            elif type_ == "leave":
                await self.user_task_leave(user, task)

    @catch_error
    async def timeout_manager(self):
        started = time.time()
        while time.time() < started + self.timeout:
            await asyncio.sleep(1.0)
            if not self.sio.connected:
                return

            left = max(0, int(started + self.timeout - time.time()))
            await self.sio.emit(
                "client_broadcast",
                {
                    "type": "time_left_chatroom",
                    "room": self.waiting_room_id,
                    "time_left": left,
                },
            )

        print("TIMEOUT!", flush=True)

        if len(self.active_users) >= self.min_num_users:
            print("ROOM FULL ENOUGH, REDIRECT", len(self.active_users), "USERS TO BOT EXPERIMENT", flush=True)
            await self.setup_chatroom_and_forward()
        else:
            print("ROOM NOT FILLED ENOUGH, REDIRECT", len(self.active_users), "USERS TO REDIRECT_URL", flush=True)
            await self.redirect_users_timeout()

        # 15 minutes after timeout we shutdown everything to release 
        # connections and other resources:
        await asyncio.sleep(900) # wait 15 minutes
        if self.sio.connected:
            print("FORCE DISCONNECT", flush=True)
            await self.sio.disconnect()
            print("DISCONNECTED", flush=True)


    @catch_error
    async def redirect_user(self, user_id, to_room):
        await redirect_user(
            self.uri,
            self.concierge_token,
            user_id,
            self.waiting_room_task_id,
            self.waiting_room_id,
            to_room,
            self.sio,
        )

        print("REDIRECTED USER TO ROOM", to_room, flush=True)

    @catch_error
    async def redirect_users_timeout(self):
        self.waiting_room_closed = True
        print("managerbot: closes waiting room due to timeout")

        await self.sio.emit("keypress", dict(typing=True))
        await asyncio.sleep(2)
        await self.sio.emit("keypress", dict(typing=False))

        await self.sio.emit(
            "text",
            {
                "message": (
                    "We could not fill the room with a sufficient number of"
                    " participants. You will be forwarded soon."
                ),
                "room": self.waiting_room_id,
                "html": True,
            },
            callback=self.message_callback,
        )

        await asyncio.sleep(2)

        for user_id in self.active_users.copy():
            # active_users can change during iteration since this
            # triggers user_leave events:
            await self.redirect_user(user_id, self.redirect_room_id)

        print("REDIRECTED USERS AFTER TIMEOUT", flush=True)

    @catch_error
    async def fetch_user_token(self, user_id):
        async with get(
            self.api_token, f"{self.uri}/slurk_api/users/{user_id}"
        ) as response:
            if not response.ok:
                LOG.error(f"Could not get user: {response.status_code}")
                response.raise_for_status()
            return (await response.json())["token_id"]

    @catch_error
    async def run(self):
        # establish a connection to the server
        await self.sio.connect(
            self.uri,
            headers={
                "Authorization": f"Bearer {self.concierge_token}",
                "user": str(self.concierge_user),
            },
            namespaces="/",
        )

        self.redirect_room_id = await create_forward_room(
            self.uri, self.concierge_token, self.timeout_redirect_url
        )

        # wait until the connection with the server ends
        await self.sio.wait()
        print("DONE, CONCIERGE PROCESS EXITS", flush=True)

    @staticmethod
    async def message_callback(success, error_msg=None):
        """Is passed as an optional argument to a server emit.

        Will be invoked after the server has processed the event,
        any values returned by the event handler will be passed
        as arguments.

        :param success: `True` if the message was successfully sent,
            else `False`.
        :type success: bool
        :param error_msg: Reason for an insuccessful message
            transmission. Defaults to None.
        :type status: str, optional
        """
        if not success:
            print(f"Could not send message: {error_msg}", flush=True)
            LOG.error(f"Could not send message: {error_msg}")
        else:
            LOG.debug("Sent message successfully.")
            print("Sent message successfully.", flush=True)

    @catch_error
    async def get_user_task(self, user):
        """Retrieve task assigned to user.

        :param user: Holds keys `id` and `name`.
        :type user: dict
        """
        async with get(
            self.concierge_token, f"{self.uri}/slurk/api/users/{user['id']}/task"
        ) as response:
            if not response.ok:
                LOG.error(f"Could not get task: {response.status_code}")
                response.raise_for_status()
            LOG.debug("Got user task successfully.")
            return await response.json()

    @catch_error
    async def user_task_join(self, user, task):
        """A connected user and their task are registered.

        Once the final user necessary to start a task
        has entered, all users for the task are moved to
        a dynamically created task room.

        :param user: Holds keys `id` and `name`.
        :type user: dict
        :param task: Holds keys `date_created`, `date_modified`, `id`,
            `layout_id`, `name` and `num_users`.
        :type task: dict
        """

        if not self.timeout_manager_active:
            t = asyncio.create_task(self.timeout_manager())
            _async_tasks[id(t)] = t
            self.timeout_manager_active = True

        user_id = user["id"]

        self.active_users.add(user_id)
        if user_id in self.joined_users:
            # reload triggers leave and we avoid accepting the same
            # user multiple times, which gets the number of users
            # count wrong and thus fills the waiting room  prematurely
            return

        self.joined_users.add(user_id)
        self.num_users_in_room_missing -= 1

        if self.waiting_room_closed:
            print("USER", user, "ARRIVED AFTER WAITING ROOM CLOSED")
            await self.redirect_user(user["id"], self.redirect_room_id)
            if self.num_users_in_room_missing <= 0:
                print("ROOM CLOSED, ALL MISSING USERS ARRIVED, CLOSE ROOM", flush=True)
                await self.disconnect_in_one_minute()
            return

        if self.num_users_in_room_missing  <= 0:
            await self.setup_chatroom_and_forward()
            return

        await self.sio.emit("keypress", dict(typing=True))
        await asyncio.sleep(2)
        await self.sio.emit("keypress", dict(typing=False))
        await self.sio.emit(
            "text",
            {
                "message": (
                    f"We are waiting for {self.num_users_in_room_missing} user(s)"
                    " to join before we continue."
                ),
                "room": self.waiting_room_id,
                "html": True,
            },
            callback=self.message_callback,
        )


    @catch_error
    async def setup_chatroom_and_forward(self):
        self.waiting_room_closed = True
        if self.managerbot_id is None:
            self.managerbot_id = await self.setup_and_register_managerbot()
            await self.setup_and_register_chatbots(self.managerbot_id)

        await self.sio.emit("keypress", dict(typing=True))
        await asyncio.sleep(2)
        await self.sio.emit(
            "text",
            {
                "message": "Room complete! You will be forwarded soon.",
                "room": self.waiting_room_id,
                "html": True,
            },
            callback=self.message_callback,
        )
        await self.sio.emit("keypress", dict(typing=False))
        await asyncio.sleep(2)

        for user_id in self.active_users.copy():
            # active_users can change during iteration since this
            # triggers user_leave events:
            await self.redirect_user(user_id, self.chat_room_id)

        if self.num_users_in_room_missing <= 0:
            print("CHATROOM SETUP DONE, NO USER MISSING, FORWARD USERS, CLOSE WAITING ROOM", flush=True)
            await self.disconnect_in_one_minute()

    @catch_error
    async def setup_and_register_chatbots(self, manager_bot_id):
        permissions = {
            "api": True,
            "send_html_message": True,
            "send_message": True,
            "send_privately": True,
            "broadcast": False,
        }
        for bot_id, bot_name in zip(
            self.setup["chatbot_ids"], self.setup["chatbot_names"]
        ):
            permissions_id = await set_permissions(
                self.uri, self.api_token, permissions
            )
            bot_token = await create_room_token(
                self.uri, self.api_token, permissions_id, self.chat_room_id, None, None
            )
            bot_user = await create_user(self.uri, self.api_token, bot_name, bot_token)
            print("SETUP BOT", bot_name, bot_id)

            async with aiohttp.ClientSession() as session:
                async with session.post(
                    f"{CHATBOT_URL}/register",
                    json=(
                        self.setup
                        | dict(
                            chatbot_user=bot_user,
                            chatbot_token=bot_token,
                            chat_room_id=self.chat_room_id,
                            manager_bot_id=manager_bot_id,
                            bot_id=bot_id,
                            bot_name=bot_name,
                        )
                    ),
                ) as r:
                    r.raise_for_status()

    @catch_error
    async def setup_and_register_managerbot(self):
        permissions = {
            "api": True,
            "send_html_message": True,
            "send_privately": True,
            "broadcast": False,
        }
        permissions_id = await set_permissions(self.uri, self.api_token, permissions)
        bot_token = await create_room_token(
            self.uri, self.api_token, permissions_id, self.chat_room_id, None, None
        )

        bot_name = self.setup["chat_room_managerbot_name"]

        bot_user = await create_user(self.uri, self.api_token, bot_name, bot_token)

        setup = self.setup.copy()
        setup.update(
            managerbot_user=bot_user,
            managerbot_token=bot_token,
            chat_room_id=self.chat_room_id,
        )

        async with aiohttp.ClientSession() as session:
            async with session.post(
                f"{MANAGERBOT_URL}/register",
                json=setup,
            ) as r:
                r.raise_for_status()
                print(r)

        return bot_user

    @catch_error
    async def disconnect_in_one_minute(self):
        if not self.sio.connected:
            return
        await asyncio.sleep(60)  # keep room open for one minute to keep ongoing redirections alive
        if not self.sio.connected:
            return
        _async_tasks.pop(self, None)
        await self.sio.disconnect()

    @catch_error
    async def user_task_leave(self, user, task):
        """The task entry of a disconnected user is removed.

        :param user: Holds keys `id` and `name`.
        :type user: dict
        :param task: Holds keys `date_created`, `date_modified`, `id`,
            `layout_id`, `name` and `num_users`.
        :type task: dict
        """
        task_id = task["id"]
        if task_id != self.waiting_room_task_id:
            return
        user_id = user["id"]
        self.active_users.discard(user_id)
        print("LEAVE", self.active_users, self.joined_users)
