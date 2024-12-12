FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:8.0-bookworm-slim
WORKDIR /app
COPY --from=build-env /app/out ./

USER root
COPY templates/entrypoint.sh ./
RUN chmod +x ./entrypoint.sh

RUN { \
    echo "module name : IdentityMapping"; \
    echo "module version : 6.0.1"; \
} > ./application.info

RUN useradd -ms /bin/bash moduleuser
USER moduleuser

LABEL org.opencontainers.image.description="IdentityMapping is an Azure IoT edge module that performs property assignment to messages."

ENTRYPOINT ["/app/entrypoint.sh"]
