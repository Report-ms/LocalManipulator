FROM mcr.microsoft.com/dotnet/aspnet:3.1
COPY . .
ENV TZ="Europe/Moscow"
WORKDIR /
ENTRYPOINT ["dotnet", "run"]
