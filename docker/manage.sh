#!/bin/bash
export MSYS_NO_PATHCONV=1
set -e


SCRIPT_HOME="$( cd "$( dirname "$0" )" && pwd )"
#export COMPOSE_PROJECT_NAME="${COMPOSE_PROJECT_NAME-fpo}"

# =================================================================================================================
# Usage:
# -----------------------------------------------------------------------------------------------------------------
usage() {
  cat <<-EOF

  Usage: $0 {start|stop|build}

  Options:

  build - Build the docker images for the project.
          You need to do this first, since the builds require
          Docker builds.

  start - Creates the application containers from the built images
          and starts the services based on the docker-compose.yaml file.

          Example:
          $0 start
         
  stop - Stops the services.  This is a non-destructive process.  The containers
         are not deleted so they will be reused the next time you run start.

EOF
exit 1
}
# -----------------------------------------------------------------------------------------------------------------
# Default Settings:
# -----------------------------------------------------------------------------------------------------------------
# -----------------------------------------------------------------------------------------------------------------
# Functions:
# -----------------------------------------------------------------------------------------------------------------

buildImages(){
  cd .. 
  cd scv-api
  docker build -t scv-api .
  cd ..
  cd scv-web
  docker build -t scv-web .
  cd ..
}

toLower() {
  echo $(echo ${@} | tr '[:upper:]' '[:lower:]')
}
# =================================================================================================================

pushd ${SCRIPT_HOME} >/dev/null
COMMAND=$(toLower ${1})
shift

case "$COMMAND" in
  start)   
      docker-compose up
    ;;  
  stop)
      docker-compose stop
    ;;
  build)
      buildImages   
    ;;
  *)
    usage
esac

popd >/dev/null
