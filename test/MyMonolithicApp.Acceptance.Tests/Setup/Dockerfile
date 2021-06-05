FROM mcr.microsoft.com/mssql/server:2019-latest
USER root
RUN mkdir -p /src
WORKDIR /src
COPY . /src
ENTRYPOINT /bin/bash ./Setup/entrypoint.sh
