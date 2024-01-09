FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /workspace

COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS release
WORKDIR /workspace
COPY --from=build-env /workspace/out .
ENTRYPOINT [ "dotnet", "myopentelemetryconsole.dll" ]
