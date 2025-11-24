# University of Potsdam
"""Chatbot agent that both administers an interaction and acts as the
interacting player.
"""

import asyncio
import logging
import os
import random
import time

import socketio

from slurk_setup_descil.components.utils import catch_error

from .gpt_bot import gpt_bot

LOG = logging.getLogger(__name__)
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


class Chatbot:
    def __init__(self, config, host, port):
        """Serves as a template for task bots.
        :param task: Task ID
        :type task: str
        """

        self.config = config
        print("CONFIG", config, flush=True)
        self.manager_bot_id = config["manager_bot_id"]
        self.bot_user_id = config["chatbot_user"]
        self.bot_token = config["chatbot_token"]
        self.api_token = config["api_token"]
        self.bot_id = config["bot_id"]
        self.bot_name = config["bot_name"]
        self.chat_room_id = int(config["chat_room_id"])
        self.num_users = config["num_users"]
        self.proba_cancel_message_in_progress = config[
            "proba_cancel_message_in_progress"
        ]
        self.proba_ignore_message = config["proba_ignore_message"]

        self.uri = host
        if port is not None:
            self.uri += f":{port}"
        self.uri += "/slurk/api"
        self.sio = socketio.AsyncClient()

        self.players_per_room = []
        self.message_history = dict()

        self.interuptable_interaction_tasks = dict()

    @catch_error
    async def run(self):
        """Establish a connection to the slurk chat server."""
        await self.sio.connect(
            self.uri,
            headers={
                "Authorization": f"Bearer {self.bot_token}",
                "user": str(self.bot_user_id),
            },
            namespaces="/",
        )
        self.register_callbacks()
        await self.sio.wait()

    def register_callbacks(self):
        self.register_status_callback()
        self.register_text_message_callback()

    def register_status_callback(self):
        @self.sio.event
        async def status(data):
            if data["room"] != self.chat_room_id:
                print("IGNORE STATUS EVENT, WRONG ROOM")
                return

            # TODO: move this to managerbot, not chatbot:
            if data["type"] == "leave":
                self.num_users -= 1
                if self.num_users == 0:
                    await self.sio.emit(
                        "room_closed",
                        {
                            "message": "room_closed",
                            "room": self.chat_room_id,
                        },
                    )
                    await self.sio.disconnect()
                    self.sio.emit = dev_null  # avoid errors from still running tasks

                    print("CLOSED CHAT ROOM", flush=True)
                    return

                print("SENT MESSAGE about user leaving", self.chat_room_id, flush=True)
                return

            if data["type"] != "join":
                return

            if data["room"] != self.chat_room_id:
                return

            user = data["user"]
            user_id = user["id"]
            if user_id in (self.bot_user_id, self.manager_bot_id):
                return
            self.players_per_room.append(user)

    def register_text_message_callback(self):
        @self.sio.event
        async def text_message(data):
            """Triggered once a text message is sent (no leading /).

            Count user text messages.
            If encountering something that looks like a command
            then pass it on to be parsed as such.
            """
            user_id = data["user"]["id"]

            if data["room"] != self.chat_room_id:
                return

            if user_id in (self.bot_user_id, self.manager_bot_id):
                return

            if random.random() < self.proba_ignore_message:
                # ignore 50% of all messages
                print(
                    self.bot_name,
                    "IGNORE INCOMING MESSAGE BASED ON proba_ignore_message"
                    f"({self.proba_ignore_message})",
                    "TO",
                    repr(data["message"]),
                    flush=True,
                )
                return
            print(
                self.bot_name,
                "CONSIDER INCOMING MESSAGE BASED ON 1-proba_ignore_message"
                f"({self.proba_ignore_message})",
                "TO",
                repr(data["message"]),
                flush=True,
            )

            room_id = data["room"]
            if room_id not in self.message_history:
                self.message_history[room_id] = []

            user_message = data["message"]
            self.message_history[room_id].append(
                {"sender": data["user"]["name"], "text": user_message}
            )

            for reply_id, (
                task,
                message,
            ) in list(self.interuptable_interaction_tasks.items()):  # iterate over copy
                if random.random() < self.proba_cancel_message_in_progress:
                    print(
                        self.bot_name,
                        "CANCEL REPLY BASED ON proba_cancel_message_in_progress"
                        f"({self.proba_cancel_message_in_progress})",
                        "TO",
                        repr(message),
                        flush=True,
                    )
                    self.interuptable_interaction_tasks.pop(reply_id, None)
                    task.cancel()
                else:
                    print(
                        self.bot_name,
                        "CONTINUE REPLY BASED ON 1-proba_cancel_message_in_progress"
                        f"({self.proba_cancel_message_in_progress})",
                        "TO",
                        repr(message),
                        flush=True,
                    )

            reply_id = len(self.interuptable_interaction_tasks)
            task = asyncio.create_task(
                self.finish_reply(reply_id, room_id, user_message)
            )
            self.interuptable_interaction_tasks[reply_id] = (task, user_message)

    @catch_error
    async def finish_reply(self, reply_id, room_id, user_message):
        # don't react immediately to every message, other users can also
        # answer instead. else the bot looks to be "too motivated"?
        sleep = random.random() * 2 * self.num_users
        await asyncio.sleep(sleep)

        print(self.bot_name, "START TO THINK", flush=True)

        # feed message to language model and get response
        bot_reply_task = asyncio.create_task(
            generate_bot_message(self.config, self.message_history[room_id], room_id)
        )

        # 300 words / minute reading speed
        n_words = len(user_message.split())
        sleep_in_seconds = max(0, 1 + random.random() * 3 + n_words / 300 * 60)
        await asyncio.sleep(sleep_in_seconds)

        try:
            await self.sio.emit("keypress", dict(typing=True))

            started = time.time()

            answer = await bot_reply_task
            print(
                self.bot_name,
                "I REPLY",
                repr(answer),
                "TO",
                repr(user_message),
                flush=True,
            )
            if answer is None:
                return

            needed = time.time() - started

            self.message_history[room_id].append({"sender": "Ash", "text": answer})

            num_words = len(answer.split(" "))
            # reported human typing speed is usually between 40 and 60.
            # this feels a bit slow during development so we assume that the
            # chat bot is an excellent typist with 100 words per minute:
            sleep_in_seconds = max(0, num_words / 100 * 60 - needed)
            await asyncio.sleep(sleep_in_seconds)

        finally:
            #  cleanup in case task gets cancelled:
            await self.sio.emit("keypress", dict(typing=False))
            bot_reply_task.cancel()
            self.interuptable_interaction_tasks.pop(reply_id, None)

        await self.sio.emit(
            "text",
            {
                "message": answer,
                "html": True,
                "room": room_id,
            },
        )


def echo_bot(past_messages, room_number):
    if past_messages:
        return past_messages[-1]["text"]
    return None


async def generate_bot_message(config, past_messages, room_number):
    bot_id = config["bot_id"]
    bot_name = config["bot_name"]

    if bot_id == 0:
        return echo_bot(past_messages, room_number)
    return await gpt_bot(bot_id, bot_name, past_messages, room_number)


async def dev_null(*a, **kw):
    await asyncio.sleep(0)
