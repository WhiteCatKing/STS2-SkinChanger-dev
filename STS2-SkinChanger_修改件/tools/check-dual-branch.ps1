param(
    [Parameter(Mandatory=$true)][string]$StableAssemblyDir,
    [Parameter(Mandatory=$true)][string]$BetaAssemblyDir,
    [string]$LegacyDefectRoot
)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot)
try {
    $project = 'tests/STS2SkinChanger.RuntimeTests/STS2SkinChanger.RuntimeTests.csproj'
    dotnet build STS2SkinChanger/STS2SkinChanger.csproj -c Release --nologo -v:q "-p:GameAssemblyDir=$StableAssemblyDir"
    if ($LASTEXITCODE -ne 0) { throw 'Stable release build failed' }
    $production = (Resolve-Path 'STS2SkinChanger/bin/Release/Gurio.SkinChanger.dll').Path
    $hash = (Get-FileHash -LiteralPath $production).Hash
    foreach ($branch in @(@{ Name='stable'; Config='Release'; Dir=$StableAssemblyDir }, @{ Name='beta'; Config='ReleaseBeta'; Dir=$BetaAssemblyDir })) {
        dotnet build $project -c $branch.Config --nologo -v:q "-p:GameAssemblyDir=$($branch.Dir)"
        if ($LASTEXITCODE -ne 0) { throw "$($branch.Name) test build failed" }
        $runner = Join-Path (Join-Path 'tests/STS2SkinChanger.RuntimeTests/bin' $branch.Config) 'net9.0'
        Copy-Item -LiteralPath $production -Destination (Join-Path $runner 'Gurio.SkinChanger.dll') -Force
        if ((Get-FileHash -LiteralPath (Join-Path $runner 'Gurio.SkinChanger.dll')).Hash -ne $hash) { throw 'Different DLL under test' }
        foreach ($test in @('--test-game-branch','--test-character-select-modes','--test-runtime-safety','--test-protected-boundaries','--test-preview-binding','--test-merchant-lifecycle')) {
            dotnet (Join-Path $runner 'STS2SkinChanger.RuntimeTests.dll') $test
            if ($LASTEXITCODE -ne 0) { throw "$($branch.Name) failed: $test" }
        }
        if ($LegacyDefectRoot) {
            dotnet (Join-Path $runner 'STS2SkinChanger.RuntimeTests.dll') --test-legacy-defect $LegacyDefectRoot
            if ($LASTEXITCODE -ne 0) { throw "$($branch.Name) legacy sprite regression failed" }
        }
        Write-Host "$($branch.Name): same production SHA-256 $hash passed."
    }
}
finally { Pop-Location }
