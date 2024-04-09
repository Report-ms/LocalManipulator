FROM mcr.microsoft.com/dotnet/sdk:7.0

RUN apt-get update \
    && apt-get install -y python3 \
    && apt-get install -y python3-pip \
    && apt-get install -y p7zip-full \
    && apt-get install -y ffmpeg

RUN pip3 install --upgrade pip \
    && pip3 install requests \
    && pip3 install paramiko \
    && pip3 install selenium \
    && pip3 install chromedriver \
    && pip3 install python-docx \
    && pip3 install edge-tts \
    && pip3 install pydub \
    && pip3 install opencv-python \
    && pip3 install Pillow

WORKDIR /LocalManipulator
COPY ./LocalManipulator .
ENV TZ="Europe/Moscow"
ENTRYPOINT ["dotnet", "run"]
