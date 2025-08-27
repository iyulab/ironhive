# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# 비특권 사용자 생성
RUN groupadd -r appuser && useradd -r -g appuser appuser

# wwwroot 디렉터리 생성 및 권한 설정
RUN mkdir -p /var/app/wwwroot && chown -R appuser:appuser /var/app/wwwroot
RUN chown -R appuser:appuser /app

# 빌드된 파일 복사 (GitHub Actions에서 빌드됨)
COPY bin/Publish .

# 환경 변수 설정
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# 사용자 변경
USER appuser

# ASP.NET Core 포트 설정
EXPOSE 80

# 헬스체크 추가
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost/system/healthz || exit 1

# 애플리케이션 실행
ENTRYPOINT ["dotnet", "WebServer.dll"]