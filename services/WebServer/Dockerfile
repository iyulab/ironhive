# 1단계: 빌드 단계
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# 빌드 파일 복사
COPY publish .

# ASP.NET Core 포트 노출
EXPOSE 80

# 애플리케이션 실행
ENTRYPOINT ["dotnet", "IronHive.Server.WebApi.dll"]
