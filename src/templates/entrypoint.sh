#! /bin/sh

cat application.info

exec dotnet IdentityMapping.dll "$@"
