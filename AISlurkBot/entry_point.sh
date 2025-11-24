#!/usr/bin/env bash
set -x

cmd=$1

if test -z $cmd; then
  echo "please specify command"
  exit 1
fi

function start_gunicorn {
  /.venv/bin/gunicorn\
    --log-level DEBUG\
    --error-logfile -\
    --capture-output\
    --access-logfile -\
    $*
}

case $cmd in
  chatbot)
      start_gunicorn \
        -b 0.0.0.0:84\
        --worker-class aiohttp.GunicornWebWorker\
        slurk_setup_descil.api.chatbot_api:app
      ;;
  concierge_plus)
      start_gunicorn \
        -b 0.0.0.0:82\
        --worker-class aiohttp.GunicornWebWorker\
        slurk_setup_descil.api.concierge_plus_api:app
      ;;
  managerbot)
      start_gunicorn \
        -b 0.0.0.0:85\
        --worker-class aiohttp.GunicornWebWorker\
        slurk_setup_descil.api.managerbot_api:app
      ;;
  setup_service)
      start_gunicorn \
        -b 0.0.0.0:81\
        --worker-class uvicorn.workers.UvicornWorker\
        slurk_setup_descil.api.setup_service_api:app
      ;;
  *)
      echo "invalid command: $cmd"
      exit 1;
      ;;
esac
