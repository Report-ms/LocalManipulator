# Stage 1
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS localmanipulatorstage1

RUN apt-get update \
    && apt-get install -y python3 \
    && apt-get install -y python3-pip

RUN pip3 install --upgrade pip \
    && pip3 install numpy \
    && pip3 install pandas

WORKDIR /build
COPY ./LocalManipulator .
RUN dotnet restore
RUN dotnet publish -c Release -o /app
# Stage 2
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS localmanipulatorstage2
WORKDIR /app
COPY --from=localmanipulatorstage1 /app .
ENTRYPOINT ["dotnet", "LocalManipulator.dll"]
