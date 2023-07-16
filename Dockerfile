# Stage 1
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /build
COPY ./LocalManipulator .
RUN dotnet restore
RUN dotnet publish -c Release -o /app
# Stage 2
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Back.dll"]