# =============================================================================
# IronHive Local Provider Tests
# =============================================================================
# This script runs local integration tests for LLM providers.
# NOT for CI - requires .env file with API keys in repository root.
#
# Usage:
#   ./run.ps1                    # Run all provider tests
#   ./run.ps1 -Provider openai   # Run specific provider test
#   ./run.ps1 -List              # List available providers
# =============================================================================

param(
    [string]$Provider = "",
    [switch]$List,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Resolve-Path "$ScriptDir/../.."
$EnvFile = Join-Path $RootDir ".env"
$ProjectFile = Join-Path $ScriptDir "ChatApiTests.csproj"

# Available providers
$Providers = @(
    "openai",
    "azure-openai",
    "anthropic",
    "google",
    "xai",
    "ollama",
    "lmstudio",
    "gpustack"
)

if ($List) {
    Write-Host "`nAvailable providers:" -ForegroundColor Cyan
    $Providers | ForEach-Object { Write-Host "  - $_" }
    Write-Host ""
    exit 0
}

# Check .env file exists
if (-not (Test-Path $EnvFile)) {
    Write-Host "`n[ERROR] .env file not found at: $EnvFile" -ForegroundColor Red
    Write-Host "Copy .env.example to .env and fill in your API keys.`n" -ForegroundColor Yellow
    exit 1
}

# Load .env file into environment variables
Write-Host "`nLoading environment from .env..." -ForegroundColor Gray
Get-Content $EnvFile | ForEach-Object {
    $line = $_.Trim()
    if ($line -and -not $line.StartsWith("#")) {
        $parts = $line -split "=", 2
        if ($parts.Length -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim()
            if ($value) {
                [Environment]::SetEnvironmentVariable($key, $value, "Process")
                if ($Verbose) {
                    Write-Host "  $key = $($value.Substring(0, [Math]::Min(10, $value.Length)))..." -ForegroundColor DarkGray
                }
            }
        }
    }
}

# Build project
Write-Host "`nBuilding local tests..." -ForegroundColor Cyan
dotnet build $ProjectFile -c Release --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed" -ForegroundColor Red
    exit 1
}

# Run tests
Write-Host "`n" + "=" * 60 -ForegroundColor DarkGray
Write-Host "Running Local Provider Tests" -ForegroundColor Cyan
Write-Host "=" * 60 + "`n" -ForegroundColor DarkGray

$args = @()
if ($Provider) {
    $args += "--provider"
    $args += $Provider
}

dotnet run --project $ProjectFile -c Release --no-build -- @args

Write-Host ""
