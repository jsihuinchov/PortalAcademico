
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src


COPY *.csproj ./
RUN dotnet restore


COPY . ./
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/out .


ENV DOTNET_RUNNING_APP=PortalAcademico.dll

CMD ASPNETCORE_URLS=http://+:$PORT dotnet $DOTNET_RUNNING_APP
