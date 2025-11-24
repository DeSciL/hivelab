import os
import traceback
import uuid
from typing import Any, Dict, List, Optional

from fastapi import FastAPI, HTTPException, Request
from fastapi.responses import JSONResponse
from pydantic import BaseModel

from slurk_setup_descil.components.setup_service import (
    create_waiting_room_tokens, setup_and_register_concierge, setup_chat_room,
    setup_waiting_room)
from slurk_setup_descil.components.slurk_api import get_api_token

app = FastAPI()


SLURK_HOST = os.environ.get("SLURK_HOST", "http://slurk")
SLURK_PORT = os.environ.get("SLURK_PORT", "80")
CONCIERGE_URL = os.environ.get("CONCIERGE_URL", "http://localhost:83")
CHATBOT_URL = os.environ.get("CHATBOT_URL", "http://localhost:84")


@app.exception_handler(Exception)
async def http_exception_handler(request: Request, exc: Exception):
    # Do some logging here
    return JSONResponse(
        {
            "error": str(exc),
            "traceback": "".join(
                traceback.format_exception(type(exc), exc, exc.__traceback__)
            ),
        }
    )


class SetupData(BaseModel):
    num_users: int = 2
    min_num_users: int = 2
    waiting_room_timeout_url: str = "https://sis.id.ethz.ch"
    waiting_room_timeout_seconds: int = 5
    chat_room_timeout_url: str = "https://sis.id.ethz.ch"
    chat_room_timeout_seconds: int = 20
    chat_room_dropout_url: str = "https://sis.id.ethz.ch"
    min_num_users_chat_room: int = 1
    api_token: str
    chatbot_ids: List[int] = [
        1,
    ]
    chatbot_names: List[str] = ["Ash"]
    waiting_room_conciergebot_name: str = "Concierge"
    chat_room_managerbot_name: str = "Concierge"

    chat_room_layout: Optional[Dict[Any, Any]] = {}
    chat_room_layout_id: Optional[int] = None
    waiting_room_layout: Optional[Dict[Any, Any]] = {}
    waiting_room_layout_id: Optional[int] = None

    proba_cancel_message_in_progress: Optional[float] = 0.8
    proba_ignore_message: Optional[float] = 0.4

    bot_config_json: Optional[Dict[Any, Any]] = {}

    user_left_notification_url: str = ""


@app.post("/setup")
async def setup(setup_data: SetupData):
    """
    setup an experiment with *num_users* using a bot *bot_id*.
    """
    api_token = await get_api_token()
    submittted_api_token = setup_data.api_token
    if api_token != submittted_api_token:
        raise HTTPException(status_code=401, detail="api token invalid.")

    num_users = setup_data.num_users

    slurk_url = f"{SLURK_HOST}:{SLURK_PORT}"

    setup = setup_data.dict()

    waiting_room_id, waiting_room_task_id = await setup_waiting_room(
        slurk_url,
        api_token,
        num_users,
        setup["waiting_room_timeout_seconds"],
        setup.get("waiting_room_layout"),
        setup.get("waiting_room_layout_id"),
    )

    user_tokens = await create_waiting_room_tokens(
        slurk_url, api_token, waiting_room_id, waiting_room_task_id, num_users
    )

    chat_room_id, _ = await setup_chat_room(
        slurk_url,
        api_token,
        num_users,
        setup.get("chat_room_layout"),
        setup.get("chat_room_layout_id"),
    )

    setup.update(
        dict(
            waiting_room_id=waiting_room_id,
            waiting_room_task_id=waiting_room_task_id,
            user_tokens=user_tokens,
            chat_room_id=chat_room_id,
        )
    )

    request_id = uuid.uuid1().hex

    await setup_and_register_concierge(
        slurk_url,
        CONCIERGE_URL,
        setup,
    )

    return dict(
        user_tokens=user_tokens,
        request_id=request_id,
        chat_room_id=chat_room_id,
        waiting_room_id=waiting_room_id,
    )
