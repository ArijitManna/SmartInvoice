FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY Directory.Build.props ./
COPY SmartInvoice.slnx ./
COPY src/SmartInvoice.Domain/SmartInvoice.Domain.csproj src/SmartInvoice.Domain/
COPY src/SmartInvoice.Application/SmartInvoice.Application.csproj src/SmartInvoice.Application/
COPY src/SmartInvoice.Infrastructure/SmartInvoice.Infrastructure.csproj src/SmartInvoice.Infrastructure/
COPY src/SmartInvoice.API/SmartInvoice.API.csproj src/SmartInvoice.API/

# Restore
RUN dotnet restore src/SmartInvoice.API/SmartInvoice.API.csproj

# Copy everything and build
COPY . .
RUN dotnet publish src/SmartInvoice.API/SmartInvoice.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SmartInvoice.API.dll"]
