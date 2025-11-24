import logging
import os
import re
from datetime import datetime
from functools import cache

from openai import AsyncAzureOpenAI, AsyncOpenAI

from slurk_setup_descil.components.utils import fetch_prompt_from_url

from .prompti import fallback_prompt


async def gpt_bot(bot_id, bot_name, past_messages, room_number):
    bot_messages = []

    prompt = await fetch_prompt(bot_id, room_number)

    bot_messages.append({"role": "system", "content": prompt})

    for msg in past_messages:
        if msg["sender"] == bot_name:
            bot_role = "assistant"
        else:
            bot_role = "user"
        bot_messages.append(
            {"role": bot_role, "content": msg["sender"] + ": " + msg["text"]}
        )

    question_message = {
        "role": "user",
        "content": "You are a human participant in a chat."
        "You are eager to engage in the discussion with the other people."
        "Based on the previous messages, do you think you want to answer now? "
        "Please answer only yes or no."
        "You most of the times want to answer when you are asked a question."
        "You can also answer when you want to add something to the discussion."
        "You can also answer when you want to ask a question."
        "You can also answer when you want to make a comment."
        "You can also answer when you want to make a suggestion."
        "You can also answer when you want to make a remark."
        "You can also answer when you want to make a statement."
        "You can also answer when you want to make a contribution."
        "You can also answer when you want to make a point."
        "You can also answer when you want to make a proposal."
        "You can also answer when you want to make a request."
        "You must not answer to irrelevant messages."
        "You must not answer to messages that are off topic."
        "You must not answer to messages that are not related to the discussion."
        "You must not answer like a bot or an assistant."
        "You must answer like a human."
        "You must answer in 1-2 sentences"
        "Avoid being too friendly."
        "Avoid being too formal."
        "Avoid answering in sophisticated language."
        "Avoid answering in academic language."
        "Make spelling mistakes."
        "Speak only English."
        "Please answer only yes or no.",
    }

    model, temperature, max_tokens = get_ai_parameters()

    client = connect()

    now = datetime.now

    print(now(), "ASK", flush=True)
    response = await client.chat.completions.create(
        model=model,
        messages=[question_message],
        # temperature designates how creative you want the chatbot to be on a scale of
        # 0...1
        temperature=temperature,
        # max_tokens states how long the answer can be
        max_tokens=max_tokens,
        stop=["\n"],
    )
    answer = response.choices[0].message.content
    print(now(), "ANSWER", answer, flush=True)

    if answer.lower().rstrip(".") != "yes":
        logging.debug("Irrelevant answer, not answering")
        return None

    logging.debug("Answering yes")
    response = await client.chat.completions.create(
        model=model,
        messages=bot_messages,
        temperature=temperature,
        max_tokens=max_tokens,
        stop=["\n"],
    )

    answer = response.choices[0].message.content
    answer = re.sub(r"^[a-zA-Z]*:\s*", "", answer)

    return answer


@cache
def connect():
    if use_azure_openai():
        return AsyncAzureOpenAI(
            api_key=os.getenv("AZURE_OPENAI_API_KEY"),
            api_version=os.getenv("AZURE_OPENAI_API_VERSION"),
            azure_endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        )
    return AsyncOpenAI(api_key=os.getenv("OPENAI_API_KEY"))


def use_azure_openai():
    return os.getenv("AI_PROVIDER").upper().strip() == "AZURE"


def get_ai_parameters():
    if use_azure_openai():
        model = os.getenv("AZURE_OPENAI_MODEL", "css-openai-gpt35")
    else:
        model = os.getenv("OPENAI_MODEL", "gtp-3.5-turbo-1106")
    temperature = float(os.getenv("AI_MODEL_TEMPERATURE", "0.9"))
    max_tokens = int(os.getenv("AI_MODEL_MAX_TOKENS", "80"))
    return model, temperature, max_tokens


async def fetch_prompt(bot_id, room_number, cache={}):
    if url := os.environ.get("PROMPT_URL"):
        return await fetch_prompt_from_url(url, bot_id, room_number, cache)
    return fallback_prompt
