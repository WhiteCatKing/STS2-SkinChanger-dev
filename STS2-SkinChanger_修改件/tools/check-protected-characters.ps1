param(
    [Parameter(Mandatory=$true)][string]$GameAssemblyDir,
    [Parameter(Mandatory=$true)][string]$GamePack,
    [Parameter(Mandatory=$true)][string]$CrusaderRoot,
    [Parameter(Mandatory=$true)][string]$JoanSfwRoot,
    [Parameter(Mandatory=$true)][string]$JoanWearRoot
)
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot)
try {
    $project = 'tests/STS2SkinChanger.RuntimeTests/STS2SkinChanger.RuntimeTests.csproj'
    dotnet build $project -c Release --nologo -v:q "-p:GameAssemblyDir=$GameAssemblyDir"
    if ($LASTEXITCODE -ne 0) { throw 'Protected regression build failed' }
    foreach ($test in @('--test-runtime-safety', '--test-protected-boundaries')) {
        dotnet run --no-build --project $project -c Release "-p:GameAssemblyDir=$GameAssemblyDir" -- $test
        if ($LASTEXITCODE -ne 0) { throw "Protected regression failed: $test" }
    }
    dotnet run --no-build --project $project -c Release "-p:GameAssemblyDir=$GameAssemblyDir" -- --test-crusader-hooks (Join-Path $CrusaderRoot 'CrusaderSkin.dll')
    if ($LASTEXITCODE -ne 0) { throw 'Crusader actual DLL regression failed' }
    dotnet run --no-build --project $project -c Release "-p:GameAssemblyDir=$GameAssemblyDir" -- --test-protected-characters $GamePack $CrusaderRoot $JoanSfwRoot $JoanWearRoot
    if ($LASTEXITCODE -ne 0) { throw 'Actual package variants regression failed' }
}
finally { Pop-Location }
