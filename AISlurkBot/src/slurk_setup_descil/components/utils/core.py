import os
import traceback
from asyncio.exceptions import CancelledError
from functools import wraps

import aiohttp


def catch_error(coro):
    @wraps(coro)
    async def wrapped(*a, **kw):
        try:
            return await coro(*a, **kw)
        except CancelledError as e:
            print("CANCELLED TASK", e, flush=True)
            raise
        except:  # noqa F702
            traceback.print_exc()
            raise

    return wrapped


async def fetch_prompt_from_url(url, bot_id, room_id, cache):
    prompt = cache.get((bot_id, room_id))
    if prompt is not None:
        print("GOT PROMPT FROM CACHE", prompt)
        return prompt
    async with aiohttp.ClientSession() as session:
        async with session.get(
            url,
            params=dict(bot_id=bot_id, room_id=room_id),
            proxy=os.environ.get("https_proxy"),
        ) as resp:
            if resp.status != 200:
                print("REQUEST TO FETCH PROMPT FAILED", resp.reason)
                return ""
            prompt = await resp.text()
            cache[bot_id, room_id] = prompt
            print("GOT PROMPT DATA", repr(prompt))
            return prompt
