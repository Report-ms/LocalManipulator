FROM mcr.microsoft.com/dotnet/sdk:7.0

RUN apt-get update \
    && apt-get install -y python3 \
    && apt-get install -y python3-pip \
    && apt-get install -y p7zip-full

RUN pip3 install --upgrade pip \
    && pip3 install requests

WORKDIR /LocalManipulator
COPY ./LocalManipulator .
ENV TZ="Europe/Moscow"
ENTRYPOINT ["dotnet", "run"]
