param(
    [string]$StableRef = '',
    [string]$BetaRef = '',
    [string]$Out = (Join-Path $env:TEMP 'sts2-skinchanger-gates')
)

$ErrorActionPreference = 'Stop'

# Prevent Windows crash/WER dialogs from blocking non-interactive test runs.
Add-Type -TypeDefinition @"
using System.Runtime.InteropServices;
public static class Win32GateErrorMode
{
    [DllImport("kernel32.dll")]
    public static extern uint SetErrorMode(uint uMode);
}
"@ -ErrorAction SilentlyContinue
[Win32GateErrorMode]::SetErrorMode(3) | Out-Null

function Resolve-GameRefDir([string]$Explicit, [string[]]$Candidates, [string]$ParamName) {
    if (-not [string]::IsNullOrWhiteSpace($Explicit)) {
        $full = [System.IO.Path]::GetFullPath($Explicit)
        if (-not (Test-Path -LiteralPath (Join-Path $full 'sts2.dll'))) {
            Write-Host "[GATE] $ParamName does not exist or lacks sts2.dll: $full"
            exit 1
        }
        return $full
    }

    foreach ($candidate in $Candidates) {
        $full = [System.IO.Path]::GetFullPath($candidate)
        if (Test-Path -LiteralPath (Join-Path $full 'sts2.dll')) {
            return $full
        }
    }

    Write-Host "[GATE] Unable to find valid $ParamName directory (needs sts2.dll). Checked:"
    foreach ($candidate in $Candidates) {
        Write-Host "  - $candidate"
    }
    Write-Host "Pass -$ParamName <dir> explicitly."
    exit 1
}

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$projectRoot = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$stableRefFull = Resolve-GameRefDir $StableRef @(
    'C:\sts2refs\stable',
    'D:\mod\游戏版本备份\正式版_v0.111.1_20261117_2249_build26655341\data_sts2_windows_x86_64'
) 'StableRef'
$betaRefFull = Resolve-GameRefDir $BetaRef @(
    'D:\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64',
    'C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64'
) 'BetaRef'
$outRoot = [System.IO.Path]::GetFullPath($Out)
New-Item -ItemType Directory -Path $outRoot -Force | Out-Null

Write-Host "[GATE] StableRef = $stableRefFull"
Write-Host "[GATE] BetaRef   = $betaRefFull"
Write-Host "[GATE] Out       = $outRoot"

$manifestPath = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'tests\required-fixes.json'))
if (-not (Test-Path -LiteralPath $manifestPath)) {
    throw "Required fixes manifest not found: $manifestPath"
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

# LogicTests manifest self-check: verify every Logic suite class exists in LogicTests source files.
$logicSourceDir = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'tests\STS2SkinChanger.LogicTests'))
$logicFiles = Get-ChildItem -LiteralPath $logicSourceDir -Filter '*.cs'
$logicCorpus = ($logicFiles | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 }) -join "`n"
foreach ($entry in $manifest) {
    foreach ($test in $entry.tests) {
        if ($test.suite -eq 'Logic') {
            $pattern = "\bclass\s+$([regex]::Escape($test.name))\b"
            if ($logicCorpus -notmatch $pattern) {
                throw "Manifest self-check failed: item '$($entry.id)' references Logic test class '$($test.name)' not found under $logicSourceDir"
            }
        }
    }
}
Write-Host "[GATE] LogicTests manifest check passed ($logicSourceDir)."

$compatCsproj = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'compat\ThunninoiSkinManagerCompat\ThunninoiSkinManagerCompat.csproj'))
$mainCsproj = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'STS2SkinChanger\STS2SkinChanger.csproj'))
$logicCsproj = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'tests\STS2SkinChanger.LogicTests\STS2SkinChanger.LogicTests.csproj'))
$runtimeCsproj = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'tests\STS2SkinChanger.RuntimeTests\STS2SkinChanger.RuntimeTests.csproj'))

function Invoke-NativeCapture([string[]]$DotnetArgs, [string]$WorkingDir = '') {
    $prevPref = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    if ($WorkingDir) { Push-Location $WorkingDir }
    try {
        $output = & dotnet @DotnetArgs 2>&1
        $code = $LASTEXITCODE
    }
    finally {
        if ($WorkingDir) { Pop-Location }
        $ErrorActionPreference = $prevPref
    }
    return [pscustomobject]@{ Output = @($output); ExitCode = $code }
}

function Test-RequiresExternalPack($testSpec) {
    $prop = $testSpec.PSObject.Properties['requiresExternalPack']
    return ($null -ne $prop -and $prop.Value -eq $true)
}

$separateTests = @()
$externalTests = @()
$seenArgs = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
foreach ($entry in $manifest) {
    foreach ($test in $entry.tests) {
        if ($test.suite -eq 'Runtime' -and -not [string]::IsNullOrWhiteSpace($test.args)) {
            if ($seenArgs.Add($test.args)) {
                if (Test-RequiresExternalPack $test) {
                    $externalTests += [pscustomobject]@{ Id = $entry.id; Name = $test.name; Args = $test.args }
                }
                else {
                    $separateTests += [pscustomobject]@{ Id = $entry.id; Name = $test.name; Args = $test.args }
                }
            }
        }
    }
}

$branches = @(
    @{ Name = 'stable'; Label = 'Stable'; Config = 'Release'; Ref = $stableRefFull },
    @{ Name = 'beta';   Label = 'Beta';   Config = 'ReleaseBeta'; Ref = $betaRefFull }
)

$branchResults = @{}
$anyFailure = $false

foreach ($branch in $branches) {
    $bName = $branch.Name
    $bConfig = $branch.Config
    $bRef = $branch.Ref
    $bOut = [System.IO.Path]::GetFullPath((Join-Path $outRoot $bName))
    $modOut = Join-Path $bOut 'mod'
    $runtimeOut = Join-Path $bOut 'runtime-tests'
    New-Item -ItemType Directory -Path $modOut -Force | Out-Null
    New-Item -ItemType Directory -Path $runtimeOut -Force | Out-Null

    Write-Host ""
    Write-Host "==================== [$($branch.Label)] config=$bConfig ref=$bRef ===================="

    $state = [ordered]@{
        BuildOk = $false
        LogicOk = $false
        RuntimeOk = $false
        RuntimeLastLine = ''
        RuntimeItemStatus = @{}
        SeparateStatus = @{}
    }

    if (-not (Test-Path -LiteralPath $bRef)) {
        Write-Host "[ERROR] GameAssemblyDir not found: $bRef"
        $branchResults[$bName] = $state
        $anyFailure = $true
        continue
    }

    # 1. Build compat + main project
    Write-Host "[$bName] 1/4 Building compat + main project ($bConfig) -> $modOut"
    & dotnet build $compatCsproj -c $bConfig --nologo -v:q "-p:GameAssemblyDir=$bRef"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[FAIL] $bName compat build failed."
        $branchResults[$bName] = $state
        $anyFailure = $true
        continue
    }

    & dotnet build $mainCsproj -c $bConfig --nologo "-p:GameAssemblyDir=$bRef" -o $modOut
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[FAIL] $bName main build failed."
        $branchResults[$bName] = $state
        $anyFailure = $true
        continue
    }
    $state.BuildOk = $true

    # 2. Run LogicTests
    Write-Host "[$bName] 2/4 Running LogicTests ($bConfig)"
    $logicLog = Join-Path $bOut 'logic-tests.log'
    $logicRun = Invoke-NativeCapture @('run', '--project', $logicCsproj, '-c', $bConfig, "-p:GameAssemblyDir=$bRef")
    $logicRun.Output | Set-Content -LiteralPath $logicLog -Encoding UTF8
    if ($logicRun.ExitCode -ne 0) {
        Write-Host "[FAIL] $bName LogicTests failed (exit=$($logicRun.ExitCode)). See $logicLog"
        $logicRun.Output | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
        $branchResults[$bName] = $state
        $anyFailure = $true
        continue
    }
    $state.LogicOk = $true
    Write-Host "[$bName] LogicTests passed."

    # 3. Build + run full no-arg RuntimeTests into $runtimeOut
    Write-Host "[$bName] 3/4 Building & running full no-arg RuntimeTests ($bConfig) -> $runtimeOut"
    & dotnet build $runtimeCsproj -c $bConfig --nologo -v:q "-p:GameAssemblyDir=$bRef" -o $runtimeOut
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[FAIL] $bName RuntimeTests build failed."
        $branchResults[$bName] = $state
        $anyFailure = $true
        continue
    }

    $runtimeDll = Join-Path $runtimeOut 'STS2SkinChanger.RuntimeTests.dll'
    $runtimeLog = Join-Path $bOut 'runtime-tests.log'
    $runtimeRun = Invoke-NativeCapture @($runtimeDll) $projectRoot
    $runtimeRun.Output | Set-Content -LiteralPath $runtimeLog -Encoding UTF8
    $nonEmpty = @($runtimeRun.Output | ForEach-Object { $_.ToString().TrimEnd() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $lastLine = if ($nonEmpty.Count -gt 0) { $nonEmpty[-1] } else { '' }
    $state.RuntimeLastLine = $lastLine

    foreach ($line in $nonEmpty) {
        if ($line -match '^\s*\[(PASS|FAIL|NOT RUN|SEPARATE|GAP)\s*\]\s+([A-Za-z0-9_-]+)\s+\(') {
            $state.RuntimeItemStatus[$Matches[2]] = $Matches[1].Trim()
        }
    }

    if ($runtimeRun.ExitCode -ne 0 -or $lastLine -ne 'Workshop policy tests passed.') {
        Write-Host "[FAIL] $bName RuntimeTests failed (exit=$($runtimeRun.ExitCode), lastLine='$lastLine'). See $runtimeLog"
        $nonEmpty | Select-Object -Last 30 | ForEach-Object { Write-Host $_ }
        $branchResults[$bName] = $state
        $anyFailure = $true
        continue
    }
    $state.RuntimeOk = $true
    Write-Host "[$bName] Full RuntimeTests passed (last line: '$lastLine')."

    # 4. Run SEPARATE parameterless tests
    Write-Host "[$bName] 4/4 Running SEPARATE tests ($($separateTests.Count) items)"
    foreach ($sep in $separateTests) {
        $sepLog = Join-Path $bOut ("separate_" + ($sep.Args -replace '[^A-Za-z0-9_-]', '_') + '.log')
        $sepRun = Invoke-NativeCapture @($runtimeDll, $sep.Args) $projectRoot
        $sepRun.Output | Set-Content -LiteralPath $sepLog -Encoding UTF8
        if ($sepRun.ExitCode -eq 0) {
            $state.SeparateStatus[$sep.Args] = 'PASS'
            Write-Host "  [PASS] $($sep.Args) ($($sep.Name))"
        }
        else {
            $state.SeparateStatus[$sep.Args] = 'FAIL'
            $anyFailure = $true
            Write-Host "  [FAIL] $($sep.Args) ($($sep.Name)) exit=$($sepRun.ExitCode). See $sepLog"
            $sepRun.Output | Select-Object -Last 15 | ForEach-Object { Write-Host $_ }
        }
    }

    $branchResults[$bName] = $state
}

function Resolve-BranchItemStatus($entry, $state) {
    if ($null -eq $state) { return 'NOT RUN' }
    if ($null -eq $entry.tests -or $entry.tests.Count -eq 0) { return 'GAP' }

    $hasFail = $false
    $hasNotRun = $false
    $hasExternal = $false
    $hasPass = $false

    foreach ($t in $entry.tests) {
        if ($t.suite -eq 'Logic') {
            if ($state.LogicOk) { $hasPass = $true } else { $hasFail = $true }
        }
        elseif ([string]::IsNullOrWhiteSpace($t.args)) {
            $rt = $state.RuntimeItemStatus[$entry.id]
            if ($rt -eq 'PASS') { $hasPass = $true }
            elseif ($rt -eq 'FAIL') { $hasFail = $true }
            else { $hasNotRun = $true }
        }
        elseif (Test-RequiresExternalPack $t) {
            $hasExternal = $true
        }
        else {
            $sp = $state.SeparateStatus[$t.args]
            if ($sp -eq 'PASS') { $hasPass = $true }
            elseif ($sp -eq 'FAIL') { $hasFail = $true }
            else { $hasNotRun = $true }
        }
    }

    if ($hasFail) { return 'FAIL' }
    if ($hasNotRun) { return 'NOT RUN' }
    if ($hasPass -and $hasExternal) { return 'PASS+EXT' }
    if ($hasPass) { return 'PASS' }
    if ($hasExternal) { return 'EXTERNAL' }
    return 'NOT RUN'
}

Write-Host ""
Write-Host "==================== Merged Dual-Branch Gate Checklist ===================="
Write-Host ("{0,-48} {1,-10} {2,-10} {3,-10}" -f 'ID', 'Since', 'Stable', 'Beta')
Write-Host ("{0,-48} {1,-10} {2,-10} {3,-10}" -f ('-' * 48), ('-' * 10), ('-' * 10), ('-' * 10))

$mergedPass = 0
$mergedFail = 0
$mergedGap = 0
$mergedNotRun = 0
$mergedPassExt = 0
$mergedExternal = 0

foreach ($entry in $manifest) {
    $stStatus = Resolve-BranchItemStatus $entry $branchResults['stable']
    $btStatus = Resolve-BranchItemStatus $entry $branchResults['beta']
    Write-Host ("{0,-48} {1,-10} {2,-10} {3,-10}" -f $entry.id, $entry.since, $stStatus, $btStatus)

    if ($stStatus -eq 'GAP' -or $btStatus -eq 'GAP') { $mergedGap++ }
    elseif ($stStatus -eq 'FAIL' -or $btStatus -eq 'FAIL') { $mergedFail++; $anyFailure = $true }
    elseif ($stStatus -eq 'NOT RUN' -or $btStatus -eq 'NOT RUN') { $mergedNotRun++; $anyFailure = $true }
    elseif ($stStatus -eq 'EXTERNAL' -or $btStatus -eq 'EXTERNAL') { $mergedExternal++ }
    elseif ($stStatus -eq 'PASS+EXT' -or $btStatus -eq 'PASS+EXT') { $mergedPassExt++ }
    else { $mergedPass++ }
}

Write-Host ""
if ($externalTests.Count -gt 0) {
    Write-Host "需要真实包，另跑 tools/check-protected-characters.ps1 或对应单项："
    foreach ($ext in $externalTests) {
        Write-Host "  - $($ext.Id): $($ext.Name) ($($ext.Args))"
    }
    Write-Host ""
}

$stopwatch.Stop()
$extSummary = if ($mergedExternal -gt 0) { ", EXTERNAL=$mergedExternal" } else { '' }
Write-Host ("Summary: total={0}, PASS={1}, PASS+EXT={2}{3}, FAIL={4}, NOT RUN={5}, GAP={6}, elapsed={7:F1}s" -f `
    $manifest.Count, $mergedPass, $mergedPassExt, $extSummary, $mergedFail, $mergedNotRun, $mergedGap, $stopwatch.Elapsed.TotalSeconds)

if ($anyFailure) {
    Write-Host "GATE VERDICT: FAILED"
    exit 1
}

Write-Host "GATE VERDICT: PASSED"
exit 0
