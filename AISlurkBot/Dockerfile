FROM ghcr.io/astral-sh/uv:python3.12-alpine

COPY pyproject.toml entry_point.sh README.md src/ /

RUN  uv venv --relocatable --python 3.12 \
  && uv sync --no-cache \
  && rm -f uv.lock \
  && apk add -u bash

ENTRYPOINT ["/entry_point.sh"]
