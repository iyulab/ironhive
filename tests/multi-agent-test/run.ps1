# =============================================================================
# IronHive Multi-Agent Orchestration Tests
# =============================================================================
# This script runs orchestration tests using mock agents.
# No API keys required — all tests use in-process mock agents.
#
# Usage:
#   ./run.ps1           # Run all orchestration tests
#   ./run.ps1 -Verbose  # Run with verbose build output
# =============================================================================

param(
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ScriptDir "MultiAgentTest.csproj"

# Build project
Write-Host "`nBuilding multi-agent tests..." -ForegroundColor Cyan
$buildVerbosity = if ($Verbose) { "n" } else { "q" }
dotnet build $ProjectFile -c Release --nologo -v $buildVerbosity
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed" -ForegroundColor Red
    exit 1
}

# Run tests
Write-Host "`n" + "=" * 60 -ForegroundColor DarkGray
Write-Host "Running Multi-Agent Orchestration Tests" -ForegroundColor Cyan
Write-Host "=" * 60 + "`n" -ForegroundColor DarkGray

dotnet run --project $ProjectFile -c Release --no-build

Write-Host ""
