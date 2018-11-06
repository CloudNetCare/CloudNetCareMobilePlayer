FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY CloudNetCareMobilePlayer.sln ./
COPY CloudNetCareMobilePlayer/CloudNetCare.MobilePlayer.csproj CloudNetCareMobilePlayer/
COPY CloudNetCare.AppiumPilot/CloudNetCare.AppiumPilot.csproj CloudNetCare.AppiumPilot/
COPY appium-dotnet-driver/appium-dotnet-driver-netstandard2.csproj appium-dotnet-driver/
COPY CloudNetCare.SeleniumWrapper/CloudNetCare.SeleniumWrapper.csproj CloudNetCare.SeleniumWrapper/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/CloudNetCareMobilePlayer
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CloudNetCare.MobilePlayer.dll"]
