# IronHive Deployment Guide

## 개요

IronHive는 AI 서비스를 제공하는 웹 애플리케이션으로, Docker와 GitHub Actions를 통한 자동 배포를 지원합니다.

## 배포 환경

### 필요 조건

- Docker 20.10+
- Docker Compose v2.0+
- Git
- curl (헬스체크용)

### 지원 환경

- **Development/Staging**: 로컬 빌드 및 테스트
- **Production**: GitHub Container Registry 이미지 사용

## 자동 배포 설정

### 1. GitHub Actions CI/CD

프로젝트는 다음과 같은 자동 배포 파이프라인을 제공합니다:

- **main 브랜치**: 프로덕션 환경 자동 배포
- **develop 브랜치**: 스테이징 환경 자동 배포
- **Pull Request**: 빌드 및 테스트 검증

### 2. 환경 변수 설정

배포 전에 다음 환경 변수를 설정해야 합니다:

```bash
cp .env.example .env
```

`.env` 파일을 편집하여 필요한 API 키를 설정하세요:

```bash
# OpenAI API Key
OPENAI_KEY=your_openai_api_key_here

# Anthropic API Key
ANTHROPIC_KEY=your_anthropic_api_key_here

# Google API Key  
GOOGLE_KEY=your_google_api_key_here

# xAI API Key
XAI_KEY=your_xai_api_key_here

# GPUStack API Key
GPUSTACK_KEY=your_gpustack_api_key_here
```

## 배포 방법

### 자동 배포 (권장)

#### 프로덕션 배포
```bash
./deploy/deploy.sh production
```

#### 스테이징 배포
```bash
./deploy/deploy.sh staging
```

### 수동 배포

#### 개발/스테이징 환경
```bash
docker-compose up -d
```

#### 프로덕션 환경
```bash
docker-compose -f docker-compose.prod.yml up -d
```

## 헬스체크

### 자동 헬스체크
```bash
./deploy/health-check.sh [host] [port]
```

예시:
```bash
# 로컬 스테이징 확인
./deploy/health-check.sh localhost 8080

# 프로덕션 확인
./deploy/health-check.sh localhost 80
```

### 수동 헬스체크

다음 엔드포인트들을 통해 애플리케이션 상태를 확인할 수 있습니다:

- `GET /system/healthz` - 기본 헬스체크
- `GET /system/time` - 서버 시간 확인
- `GET /system/version` - 애플리케이션 버전 확인

## 롤백

배포 중 문제가 발생한 경우 이전 버전으로 롤백할 수 있습니다:

```bash
./deploy/rollback.sh production [previous_tag]
```

예시:
```bash
./deploy/rollback.sh production v1.2.3
```

## 모니터링

### 컨테이너 상태 확인
```bash
docker-compose ps
```

### 로그 확인
```bash
# 실시간 로그
docker-compose logs -f

# 최근 로그
docker-compose logs --tail=100
```

### 리소스 사용량 확인
```bash
docker stats
```

## 트러블슈팅

### 일반적인 문제들

#### 1. 포트 충돌
```bash
# 사용 중인 포트 확인
sudo lsof -i :80
sudo lsof -i :8080

# 기존 컨테이너 중지
docker-compose down
```

#### 2. 이미지 업데이트 문제
```bash
# 최신 이미지 강제 풀
docker-compose pull
docker-compose up -d --force-recreate
```

#### 3. 환경 변수 문제
```bash
# 환경 변수 확인
docker-compose config
```

#### 4. 헬스체크 실패
- `.env` 파일의 API 키가 올바른지 확인
- 네트워크 연결 상태 확인
- 컨테이너 로그 확인

### 로그 위치

- **개발환경**: `./data/logs/`
- **프로덕션**: `/var/log/ironhive/`

## 보안 고려사항

1. **환경 변수**: API 키 등 민감한 정보는 반드시 환경 변수로 관리
2. **컨테이너 보안**: 비특권 사용자로 애플리케이션 실행
3. **네트워크**: 필요한 포트만 외부에 노출
4. **로그 관리**: 민감한 정보가 로그에 기록되지 않도록 주의

## 업데이트

### 자동 업데이트 (GitHub Actions)
- main 브랜치에 push하면 자동으로 프로덕션 배포
- develop 브랜치에 push하면 자동으로 스테이징 배포

### 수동 업데이트
```bash
git pull origin main
./deploy/deploy.sh production
```

## 지원

문제가 발생한 경우 다음을 확인하세요:

1. 헬스체크 스크립트 실행
2. 컨테이너 로그 확인  
3. 시스템 리소스 상태 확인
4. 네트워크 연결 상태 확인