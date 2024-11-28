#!/usr/bin/env bash

SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

dotnet run --project ${SCRIPT_DIR}/src/IpPool/IpPool.csproj -- $*