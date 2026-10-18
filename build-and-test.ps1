$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $root "PrismLibrary.sln"

Write-Host "Restoring packages..."
dotnet restore $solution

Write-Host "Building Release..."
dotnet build $solution -c Release --no-restore

Write-Host "Running tests..."
dotnet test $solution -c Release --no-build --verbosity normal

Write-Host "OK: build and tests succeeded."
Write-Host "Playnite external extension folder:"
Write-Host "  $(Join-Path $root 'src\PrismLibrary\bin\Release\net462')"
