FROM microsoft/dotnet:2.1-aspnetcore-runtime

ARG EXE_DIR=.

WORKDIR /app

COPY $EXE_DIR ./

CMD ["dotnet", "ApiNotificationBot.dll"]

EXPOSE 80/tcp
