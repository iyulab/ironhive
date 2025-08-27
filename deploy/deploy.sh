#!/bin/bash

# IronHive 배포 스크립트
set -e

# 색상 정의
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 로깅 함수
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

error() {
    echo -e "${RED}[ERROR] $1${NC}"
}

warn() {
    echo -e "${YELLOW}[WARN] $1${NC}"
}

# 배포 환경 확인
ENVIRONMENT=${1:-production}
IMAGE_TAG=${2:-latest}

log "Starting deployment for environment: $ENVIRONMENT"

# 환경별 설정
case $ENVIRONMENT in
    "production")
        COMPOSE_FILE="docker-compose.prod.yml"
        PORT=80
        ;;
    "staging")
        COMPOSE_FILE="docker-compose.yml"
        PORT=8080
        ;;
    *)
        error "Invalid environment. Use 'production' or 'staging'"
        exit 1
        ;;
esac

# Docker 및 Docker Compose 확인
if ! command -v docker &> /dev/null; then
    error "Docker is not installed"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    error "Docker Compose is not installed"
    exit 1
fi

# 환경 파일 확인
if [ ! -f .env ]; then
    if [ -f .env.example ]; then
        warn "No .env file found. Please copy .env.example to .env and configure it."
        cp .env.example .env
    else
        error "No environment configuration found"
        exit 1
    fi
fi

# 최신 이미지 풀
log "Pulling latest image..."
docker-compose -f $COMPOSE_FILE pull

# 이전 컨테이너 중지 및 제거
log "Stopping existing containers..."
docker-compose -f $COMPOSE_FILE down --remove-orphans

# 새 컨테이너 시작
log "Starting new containers..."
docker-compose -f $COMPOSE_FILE up -d

# 헬스 체크
log "Performing health check..."
sleep 10

max_attempts=30
attempt=0

while [ $attempt -lt $max_attempts ]; do
    if curl -f http://localhost:$PORT/system/healthz &> /dev/null; then
        log "Deployment successful! Application is healthy."
        break
    fi
    
    attempt=$((attempt + 1))
    warn "Health check attempt $attempt/$max_attempts failed. Retrying in 5 seconds..."
    sleep 5
done

if [ $attempt -eq $max_attempts ]; then
    error "Health check failed after $max_attempts attempts"
    log "Showing container logs:"
    docker-compose -f $COMPOSE_FILE logs --tail=50
    exit 1
fi

# 정리 작업
log "Cleaning up old Docker images..."
docker image prune -f

log "Deployment completed successfully!"
log "Application is available at http://localhost:$PORT"