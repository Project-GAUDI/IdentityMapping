#! /bin/bash

cat application.info

exec dotnet IdentityMapping.dll "$@"
