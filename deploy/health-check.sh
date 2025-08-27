#!/bin/bash

# IronHive 헬스체크 스크립트
set -e

# 색상 정의
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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

info() {
    echo -e "${BLUE}[INFO] $1${NC}"
}

# 기본 설정
HOST=${1:-localhost}
PORT=${2:-8080}
BASE_URL="http://$HOST:$PORT"

log "Starting health check for $BASE_URL"

# 헬스체크 함수
check_endpoint() {
    local endpoint=$1
    local description=$2
    
    info "Checking $description..."
    
    response=$(curl -s -w "%{http_code}" -o /tmp/response.txt "$BASE_URL$endpoint" || echo "000")
    
    if [ "$response" = "200" ]; then
        log "✓ $description: OK"
        if [ "$endpoint" != "/system/healthz" ]; then
            echo "Response: $(cat /tmp/response.txt)"
        fi
        return 0
    else
        error "✗ $description: Failed (HTTP $response)"
        if [ -f /tmp/response.txt ]; then
            echo "Response: $(cat /tmp/response.txt)"
        fi
        return 1
    fi
}

# 전체 상태 확인
overall_status=0

# 1. 기본 헬스체크
if ! check_endpoint "/system/healthz" "Basic Health Check"; then
    overall_status=1
fi

# 2. 시간 확인
if ! check_endpoint "/system/time" "System Time"; then
    overall_status=1
fi

# 3. 버전 확인
if ! check_endpoint "/system/version" "Application Version"; then
    overall_status=1
fi

# 4. Docker 컨테이너 상태 확인 (선택적)
if command -v docker &> /dev/null; then
    log "Checking Docker container status..."
    
    # 실행 중인 IronHive 컨테이너 확인
    running_containers=$(docker ps --filter "name=ironhive" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}")
    
    if [ -n "$running_containers" ]; then
        log "Running IronHive containers:"
        echo "$running_containers"
    else
        warn "No running IronHive containers found"
    fi
    
    # 컨테이너 리소스 사용량 확인
    if docker ps --filter "name=ironhive" --quiet | head -1 | xargs docker stats --no-stream &> /dev/null; then
        log "Container resource usage:"
        docker ps --filter "name=ironhive" --quiet | head -1 | xargs docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}"
    fi
fi

# 5. 시스템 리소스 확인
if command -v free &> /dev/null; then
    log "System memory usage:"
    free -h
fi

if command -v df &> /dev/null; then
    log "Disk usage:"
    df -h /
fi

# 결과 출력
echo ""
echo "========================="
if [ $overall_status -eq 0 ]; then
    log "Overall Health Check: PASSED ✓"
else
    error "Overall Health Check: FAILED ✗"
fi
echo "========================="

# 임시 파일 정리
rm -f /tmp/response.txt

exit $overall_status