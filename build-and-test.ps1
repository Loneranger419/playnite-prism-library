$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $root "PrismLibrary.sln"
$defaultOut = Join-Path $root "src\PrismLibrary\bin\Release\net462"
$verifyOut = Join-Path $root "artifacts\verify"

Write-Host "Restoring packages..."
dotnet restore $solution
if ($LASTEXITCODE -ne 0) { throw "Restore failed." }

Write-Host "Building Release..."
dotnet build $solution -c Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Default output appears locked (Playnite may be running). Building to artifacts\verify instead..."
    if (Test-Path $verifyOut) {
        Remove-Item -Recurse -Force $verifyOut
    }

    $msbuildArgs = @(
        $solution,
        "-c", "Release",
        "--no-restore",
        "-p:OutputPath=$verifyOut\",
        "-p:AppendTargetFrameworkToOutputPath=false"
    )
    & dotnet build @msbuildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed."
    }

    Write-Host "Running tests from artifacts\verify..."
    dotnet test (Join-Path $verifyOut "PrismLibrary.Tests.dll") --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed."
    }

    Write-Host "OK: build and tests succeeded (verify output)."
    Write-Host "Close Playnite and rebuild to refresh the external extension folder:"
    Write-Host "  $defaultOut"
    exit 0
}

Write-Host "Running tests..."
dotnet test $solution -c Release --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) {
    throw "Tests failed."
}

Write-Host "OK: build and tests succeeded."
Write-Host "Playnite external extension folder:"
Write-Host "  $defaultOut"
