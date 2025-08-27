#!/bin/bash

# IronHive 롤백 스크립트
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
PREVIOUS_TAG=${2:-}

if [ -z "$PREVIOUS_TAG" ]; then
    error "Please specify the previous image tag to rollback to"
    echo "Usage: $0 <environment> <previous_tag>"
    echo "Example: $0 production v1.2.3"
    exit 1
fi

log "Starting rollback for environment: $ENVIRONMENT to tag: $PREVIOUS_TAG"

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

# 현재 실행 중인 컨테이너 정보 백업
log "Backing up current container state..."
docker-compose -f $COMPOSE_FILE ps > "container_backup_$(date +%Y%m%d_%H%M%S).txt"

# 이미지 태그 임시 변경
log "Temporarily updating image tag to $PREVIOUS_TAG..."
if [ "$ENVIRONMENT" == "production" ]; then
    sed -i.bak "s|ghcr.io/iyulab/ironhive:.*|ghcr.io/iyulab/ironhive:$PREVIOUS_TAG|g" $COMPOSE_FILE
else
    # 개발 환경의 경우 build 대신 이미지 사용으로 변경
    sed -i.bak 's/build:/# build:/g' $COMPOSE_FILE
    sed -i 's/context:/# context:/g' $COMPOSE_FILE
    sed -i 's/dockerfile:/# dockerfile:/g' $COMPOSE_FILE
    sed -i "/# build:/i\\    image: ghcr.io/iyulab/ironhive:$PREVIOUS_TAG" $COMPOSE_FILE
fi

# 이전 버전 이미지 풀
log "Pulling previous version image..."
docker pull "ghcr.io/iyulab/ironhive:$PREVIOUS_TAG"

# 현재 컨테이너 중지
log "Stopping current containers..."
docker-compose -f $COMPOSE_FILE down

# 이전 버전으로 시작
log "Starting containers with previous version..."
docker-compose -f $COMPOSE_FILE up -d

# 헬스 체크
log "Performing health check..."
sleep 10

max_attempts=30
attempt=0

while [ $attempt -lt $max_attempts ]; do
    if curl -f http://localhost:$PORT/system/healthz &> /dev/null; then
        log "Rollback successful! Application is healthy."
        break
    fi
    
    attempt=$((attempt + 1))
    warn "Health check attempt $attempt/$max_attempts failed. Retrying in 5 seconds..."
    sleep 5
done

if [ $attempt -eq $max_attempts ]; then
    error "Health check failed after $max_attempts attempts"
    log "Restoring original compose file..."
    mv $COMPOSE_FILE.bak $COMPOSE_FILE
    log "Showing container logs:"
    docker-compose -f $COMPOSE_FILE logs --tail=50
    exit 1
fi

# 백업 파일 정리
rm -f $COMPOSE_FILE.bak

log "Rollback completed successfully!"
log "Application is available at http://localhost:$PORT"
log "Current version: $PREVIOUS_TAG"