#!/bin/bash

# IronHive 로컬 테스트 및 검증 스크립트
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

# 테스트 모드 확인
TEST_MODE=${1:-full}

log "Starting local test for IronHive deployment infrastructure"

# 1. 환경 확인
log "Checking prerequisites..."

# Docker 확인
if ! command -v docker &> /dev/null; then
    error "Docker is not installed. Please install Docker first."
    exit 1
fi

# Docker Compose 확인
if ! command -v docker-compose &> /dev/null; then
    warn "docker-compose command not found, trying 'docker compose'"
    if ! docker compose version &> /dev/null; then
        error "Docker Compose is not available"
        exit 1
    fi
    DOCKER_COMPOSE_CMD="docker compose"
else
    DOCKER_COMPOSE_CMD="docker-compose"
fi

# .NET SDK 확인
if ! command -v dotnet &> /dev/null; then
    error ".NET SDK is not installed"
    exit 1
fi

log "✓ All prerequisites are available"

# 2. 프로젝트 빌드 테스트
if [ "$TEST_MODE" = "full" ] || [ "$TEST_MODE" = "build" ]; then
    log "Testing project build..."
    
    # Restore dependencies
    dotnet restore
    if [ $? -ne 0 ]; then
        error "Failed to restore dependencies"
        exit 1
    fi
    
    # Build project
    dotnet build --no-restore --configuration Release
    if [ $? -ne 0 ]; then
        error "Failed to build project"
        exit 1
    fi
    
    # Publish WebServer
    dotnet publish services/WebServer/WebServer.csproj --configuration Release --output services/WebServer/bin/Publish
    if [ $? -ne 0 ]; then
        error "Failed to publish WebServer"
        exit 1
    fi
    
    log "✓ Project build successful"
fi

# 3. Docker 이미지 빌드 테스트
if [ "$TEST_MODE" = "full" ] || [ "$TEST_MODE" = "docker" ]; then
    log "Testing Docker image build..."
    
    cd services/WebServer
    docker build -t ironhive-test .
    if [ $? -ne 0 ]; then
        error "Failed to build Docker image"
        exit 1
    fi
    cd ../..
    
    log "✓ Docker image build successful"
fi

# 4. 환경 파일 확인
log "Checking environment configuration..."

if [ ! -f .env ]; then
    if [ -f .env.example ]; then
        log "Creating .env file from template..."
        cp .env.example .env
    else
        error "No .env.example file found"
        exit 1
    fi
fi

log "✓ Environment configuration ready"

# 5. Docker Compose 구성 검증
if [ "$TEST_MODE" = "full" ] || [ "$TEST_MODE" = "compose" ]; then
    log "Validating Docker Compose configurations..."
    
    # Development compose file
    $DOCKER_COMPOSE_CMD -f docker-compose.yml config > /dev/null
    if [ $? -ne 0 ]; then
        error "Invalid docker-compose.yml configuration"
        exit 1
    fi
    
    # Production compose file  
    $DOCKER_COMPOSE_CMD -f docker-compose.prod.yml config > /dev/null
    if [ $? -ne 0 ]; then
        error "Invalid docker-compose.prod.yml configuration"
        exit 1
    fi
    
    log "✓ Docker Compose configurations are valid"
fi

# 6. 배포 스크립트 검증
log "Checking deployment scripts..."

if [ ! -f deploy/deploy.sh ]; then
    error "deploy/deploy.sh not found"
    exit 1
fi

if [ ! -x deploy/deploy.sh ]; then
    error "deploy/deploy.sh is not executable"
    exit 1
fi

if [ ! -f deploy/rollback.sh ]; then
    error "deploy/rollback.sh not found"  
    exit 1
fi

if [ ! -x deploy/rollback.sh ]; then
    error "deploy/rollback.sh is not executable"
    exit 1
fi

if [ ! -f deploy/health-check.sh ]; then
    error "deploy/health-check.sh not found"
    exit 1
fi

if [ ! -x deploy/health-check.sh ]; then
    error "deploy/health-check.sh is not executable" 
    exit 1
fi

log "✓ Deployment scripts are ready"

# 7. GitHub Actions 워크플로우 검증
log "Checking GitHub Actions workflow..."

if [ ! -f .github/workflows/ci-cd.yml ]; then
    error ".github/workflows/ci-cd.yml not found"
    exit 1
fi

log "✓ GitHub Actions workflow is present"

# 8. 통합 테스트 (옵션)
if [ "$TEST_MODE" = "integration" ]; then
    log "Running integration test..."
    
    # 테스트용 컨테이너 시작
    $DOCKER_COMPOSE_CMD up -d
    
    # 헬스체크 대기
    log "Waiting for application to start..."
    sleep 20
    
    # 헬스체크 실행
    ./deploy/health-check.sh localhost 8080
    
    # 테스트 완료 후 정리
    $DOCKER_COMPOSE_CMD down
    
    log "✓ Integration test completed"
fi

# 결과 출력
echo ""
echo "=================================="
log "All tests passed! ✓"
echo "=================================="
echo ""
log "Deployment infrastructure is ready:"
echo "  • GitHub Actions CI/CD workflow"
echo "  • Docker multi-stage build"
echo "  • Docker Compose configurations"
echo "  • Deployment and rollback scripts"
echo "  • Health check monitoring"
echo "  • Environment configuration"
echo ""
log "Next steps:"
echo "  1. Configure environment variables in .env file"
echo "  2. Push to GitHub to trigger CI/CD pipeline"
echo "  3. Use ./deploy/deploy.sh for manual deployment"
echo "  4. Monitor with ./deploy/health-check.sh"