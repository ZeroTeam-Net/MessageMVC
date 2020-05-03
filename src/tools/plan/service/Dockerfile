#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/tools/plan/service/ZeroTeam.MessageMVC.Plan.Service.csproj", "src/tools/plan/service/"]
COPY ["src/transfer/redis/RedisMQ/ZeroTeam.MessageMVC.RedisMQ.csproj", "src/transfer/redis/RedisMQ/"]
COPY ["src/Core/Core/ZeroTeam.MessageMVC.Core.csproj", "src/Core/Core/"]
COPY ["src/Core/Abstractions/ZeroTeam.MessageMVC.Abstractions.csproj", "src/Core/Abstractions/"]
COPY ["src/agebull/LogRecorder/Agebull.Common.LogRecorder.csproj", "src/agebull/LogRecorder/"]
COPY ["src/agebull/Extend/Agebull.Common.Extend.Core.csproj", "src/agebull/Extend/"]
COPY ["src/transfer/Http/ZeroTeam.MessageMVC.Http.csproj", "src/transfer/Http/"]
COPY ["src/tools/plan/client/ZeroTeam.MessageMVC.Plan.Client.csproj", "src/tools/plan/client/"]
COPY ["src/tools/Tools/ZeroTeam.MessageMVC.Tools.csproj", "src/tools/Tools/"]
COPY ["src/tools/Contract/ZeroTeam.MessageMVC.ApiContract.csproj", "src/tools/Contract/"]
RUN dotnet restore "src/tools/plan/service/ZeroTeam.MessageMVC.Plan.Service.csproj"
COPY . .
WORKDIR "/src/src/tools/plan/service"
RUN dotnet build "ZeroTeam.MessageMVC.Plan.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZeroTeam.MessageMVC.Plan.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZeroTeam.MessageMVC.Plan.Service.dll"]