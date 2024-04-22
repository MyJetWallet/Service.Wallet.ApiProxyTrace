FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim
WORKDIR /app
COPY . .
ARG app_compilation_date
ARG app_version
ENV APP_COMPILATION_DATE=${app_compilation_date}
ENV APP_VERSION=${app_version}
ENTRYPOINT ["dotnet", "Service.Wallet.ApiProxyTrace.dll"]
