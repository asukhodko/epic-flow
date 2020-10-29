FROM microsoft/dotnet:2.2-aspnetcore-runtime

ENV TZ=Europe/Moscow
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

EXPOSE 5000

WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "EpicWorkflow.dll"]
