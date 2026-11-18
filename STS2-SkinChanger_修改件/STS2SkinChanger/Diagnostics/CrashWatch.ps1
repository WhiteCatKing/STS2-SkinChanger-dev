param([Parameter(Mandatory=$true)][string]$ConfigPath)
$ErrorActionPreference = 'Stop'
$utf8 = [Text.UTF8Encoding]::new($false)
$heldLog = $null
$target = $null
$cfg = Get-Content -LiteralPath $ConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json
$report = [IO.Path]::GetFullPath($cfg.ReportDirectory)
[IO.Directory]::CreateDirectory($report) | Out-Null
function Write-Text([string]$name, [string]$value) { [IO.File]::WriteAllText((Join-Path $report $name), $value, $utf8) }
function Read-Tail {
    if ($null -eq $heldLog) { return '' }
    $heldLog.Seek([Math]::Max(0, $heldLog.Length - 1048576), [IO.SeekOrigin]::Begin) | Out-Null
    $reader = [IO.StreamReader]::new($heldLog, $utf8, $true, 4096, $true)
    try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
}
function Test-HangCondition([bool]$hasWindow, [bool]$responding, [double]$stuckSeconds, [double]$silentSeconds, [int]$threshold) {
    return $hasWindow -and -not $responding -and $stuckSeconds -ge $threshold -and $silentSeconds -ge $threshold
}
function Ensure-CrashNative {
    if ('ScCrashDump' -as [type]) { return }
    Add-Type -TypeDefinition @"
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
public static class ScCrashDump {
 [StructLayout(LayoutKind.Sequential)] struct Info {
  public uint Attributes;
  public System.Runtime.InteropServices.ComTypes.FILETIME Creation, Access, Write;
  public uint Volume, SizeHigh, SizeLow, Links, IndexHigh, IndexLow;
 }
 [DllImport("kernel32.dll", SetLastError=true)] static extern bool GetFileInformationByHandle(IntPtr file, out Info info);
 [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)] static extern uint GetFinalPathNameByHandle(IntPtr file, StringBuilder name, uint size, uint flags);
 [DllImport("dbghelp.dll", SetLastError=true)] static extern bool MiniDumpWriteDump(IntPtr process, uint id, IntPtr file, uint flags, IntPtr a, IntPtr b, IntPtr c);
 public static FileStream Reopen(FileStream original) {
  Info before, after;
  var handle = original.SafeFileHandle.DangerousGetHandle();
  var name = new StringBuilder(32768);
  uint size = GetFinalPathNameByHandle(handle, name, (uint)name.Capacity, 0);
  if (size == 0 || size >= name.Capacity || !GetFileInformationByHandle(handle, out before))
   throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
  var reopened = new FileStream(name.ToString(), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
  if (!GetFileInformationByHandle(reopened.SafeFileHandle.DangerousGetHandle(), out after) ||
      before.Volume != after.Volume || before.IndexHigh != after.IndexHigh || before.IndexLow != after.IndexLow) {
   reopened.Dispose(); throw new IOException("Log file identity changed; refusing to write a new session.");
  }
  return reopened;
 }
 public static string Save(Process process, string path, int timeoutMilliseconds) {
  Exception failure = null;
  var worker = new System.Threading.Thread(delegate() {
   try {
    using(var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read)) {
     if (!MiniDumpWriteDump(process.Handle, (uint)process.Id, file.SafeFileHandle.DangerousGetHandle(), 0x1000, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
      throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }
   } catch (Exception error) { failure = error; }
  });
  worker.IsBackground = true;
  worker.Start();
  if (!worker.Join(timeoutMilliseconds)) {
   try { File.Delete(path); } catch {}
   throw new System.TimeoutException("MiniDumpWriteDump did not finish within " + timeoutMilliseconds + " ms.");
  }
  if (failure != null) throw failure;
  return path;
 }
}
"@
}
function Capture-HangDump {
    try {
        Ensure-CrashNative
        $timeout = if ($cfg.DumpTimeoutSeconds) { [int]$cfg.DumpTimeoutSeconds * 1000 } else { 20000 }
        return [ScCrashDump]::Save($target, (Join-Path $report 'hang.dmp'), $timeout)
    } catch { Write-Text 'dump-error.txt' $_.Exception.Message; return '' }
}

try {
    $target = [Diagnostics.Process]::GetProcessById([int]$cfg.ProcessId)
    # Bind to this exact process, not a later game reusing the PID.
    $actualStart = $target.StartTime.ToUniversalTime().Ticks
    $expectedStart = [DateTime]::Parse($cfg.ProcessStartedUtc).ToUniversalTime().Ticks
    if ([Math]::Abs($actualStart - $expectedStart) -gt 10000000 -or $target.ProcessName -ne $cfg.ProcessName) {
        throw 'Process identity did not match this diagnostic session.'
    }
    $null = $target.Handle
    # Keep the original file handle across Godot log rotation / an immediate game restart.
    try { $heldLog = [IO.File]::Open($cfg.LogPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::ReadWrite -bor [IO.FileShare]::Delete) }
    catch { $heldLog = [IO.File]::Open($cfg.LogPath, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite -bor [IO.FileShare]::Delete) }
    if (-not (Read-Tail).Contains([string]$cfg.SessionId)) { throw 'Log session marker missing; refusing to annotate an unrelated log.' }
    Write-Text 'watcher-ready' 'ready'
    $lastLength = $heldLog.Length
    $lastGrowth = [DateTime]::UtcNow
    $unresponsive = $null
    $hangSeen = $false
    $hangRecovered = $false
    $dump = ''
    $peak = 0L
    while (-not $target.WaitForExit(1000)) {
        try {
        $target.Refresh()
        $peak = [Math]::Max($peak, $target.WorkingSet64)
        if ($heldLog.Length -ne $lastLength) { $lastLength = $heldLog.Length; $lastGrowth = [DateTime]::UtcNow }
        if ($target.MainWindowHandle -ne [IntPtr]::Zero -and -not $target.Responding) {
            if ($null -eq $unresponsive) { $unresponsive = [DateTime]::UtcNow }
            if (-not $hangSeen -and (Test-HangCondition $true $false ([DateTime]::UtcNow - $unresponsive).TotalSeconds ([DateTime]::UtcNow - $lastGrowth).TotalSeconds $cfg.HangSeconds)) {
                $hangSeen = $true
                $dump = Capture-HangDump
                Write-Text 'hang-pending.txt' ('[疑似卡死] 窗口无响应且日志停滞至少 ' + $cfg.HangSeconds + ' 秒；不是已确认崩溃。转储=' + $dump)
            }
        } else {
            if ($hangSeen -and $null -ne $unresponsive -and $target.MainWindowHandle -ne [IntPtr]::Zero -and $target.Responding) { $hangRecovered = $true }
            $unresponsive = $null
        }
        } catch {
            if ($target.HasExited) { break }
            if (-not [IO.File]::Exists((Join-Path $report 'poll-warning.txt'))) { Write-Text 'poll-warning.txt' $_.Exception.Message }
        }
    }
    $exitCode = $target.ExitCode
    $tail = Read-Tail
    $sessionOffset = $tail.LastIndexOf('session=' + [string]$cfg.SessionId)
    if ($sessionOffset -ge 0) { $tail = $tail.Substring($sessionOffset) }
    $eventsPath = Join-Path $report 'managed-events.log'
    $managed = if ([IO.File]::Exists($eventsPath)) { [IO.File]::ReadAllText($eventsPath, $utf8) } else { '' }
    $quitComplete = [IO.File]::Exists((Join-Path $report 'quit-complete'))
    $forced = [IO.File]::Exists((Join-Path $report 'forced-exit'))
    $shutdown = $tail -match 'Steamworks shutdown succeeded|Steamworks shutdown succeeded!'
    $fatal = $tail -match '(?im)^Unhandled exception\.|FATAL ERROR|SIGSEGV|SIGABRT|EXCEPTION_ACCESS_VIOLATION' -or $managed.Contains('[terminating]')
    $kind = if ($fatal -or $exitCode -ne 0) { '异常退出' } elseif ($forced) { '退出收尾超时（原有退出看门狗结束进程）' } elseif (-not ($quitComplete -or $shutdown)) { '退出异常（可能被结束或未完成关闭）' } elseif ($hangSeen) { '疑似卡死后退出' } else { '正常退出' }
    if ($kind -eq '正常退出') { Write-Text 'normal-exit' 'normal'; exit 0 }
    $hex = '0x' + ([BitConverter]::ToUInt32([BitConverter]::GetBytes([int]$exitCode), 0)).ToString('X8')
    $moduleEvent = ''
    try {
        foreach ($evt in @(Get-WinEvent -FilterHashtable @{LogName='Application';Id=1000;StartTime=[DateTime]::Parse($cfg.ProcessStartedUtc).ToLocalTime()} -MaxEvents 20 -ErrorAction SilentlyContinue)) {
            [xml]$xml = $evt.ToXml()
            $values = @{}
            foreach ($data in $xml.Event.EventData.Data) { $values[$data.Name] = $data.'#text' }
            $eventId = [string]$values['ProcessId']
            $eventProcessId = if ($eventId.StartsWith('0x')) { [Convert]::ToInt64($eventId.Substring(2), 16) } elseif ($eventId) { [long]$eventId } else { -1 }
            if ($eventProcessId -eq [int]$cfg.ProcessId) {
                $moduleEvent = $evt.Message; break
            }
        }
    } catch { $moduleEvent = '系统崩溃事件尚不可用：' + $_.Exception.Message }
    if (-not $dump) {
        $wer = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'CrashDumps'
        if ([IO.Directory]::Exists($wer)) {
            $found = Get-ChildItem -LiteralPath $wer -Filter ($cfg.ProcessName + '.exe.' + $cfg.ProcessId + '.dmp') |
                Where-Object { $_.LastWriteTimeUtc -ge [DateTime]::Parse($cfg.ProcessStartedUtc).ToUniversalTime() } | Select-Object -First 1
            if ($found) { $dump = $found.FullName }
        }
    }
    $summary = @"

[SkinChanger.CrashDiagnostics] ===== 自动崩溃诊断 / session=$($cfg.SessionId) =====
类型：$kind；PID=$($cfg.ProcessId)；退出码=$exitCode ($hex)；监视期间峰值内存=$peak 字节。
版本：$($cfg.ModVersion)；疑似卡死=$hangSeen；其后恢复响应=$hangRecovered；已完成退出入口=$quitComplete。
转储：$(if ($dump) {$dump} else {'未获得（进程退出后无法补抓；没有转储不代表未发生崩溃）'})
系统事件（仅按当前 PID 匹配，可能尚未写入）：$moduleEvent
托管异常：$managed
说明：此信息是检测证据，不自动认定皮肤切换器或其他某个 Mod 是根因；普通 ERROR 和退出资源泄漏警告不单独判为崩溃。
[SkinChanger.CrashDiagnostics] ===== 诊断结束 =====
"@
    Write-Text 'diagnostic.txt' $summary
    Write-Text 'godot-tail.log' ($tail + $summary)
    try {
        if (-not $heldLog.CanWrite) {
            # Godot may deny write access while alive. After exit, reopen the original file's
            # final name and verify its Windows file ID before upgrading access, even after rotation.
            Ensure-CrashNative
            $writableLog = [ScCrashDump]::Reopen($heldLog)
            $heldLog.Dispose(); $heldLog = $writableLog
        }
        $heldLog.Seek(0, [IO.SeekOrigin]::End) | Out-Null
        $bytes = $utf8.GetBytes($summary + [Environment]::NewLine)
        $heldLog.Write($bytes, 0, $bytes.Length); $heldLog.Flush()
        Write-Text 'appended' 'appended'
        if ([IO.File]::Exists((Join-Path $report 'pending-recovery'))) { [IO.File]::Delete((Join-Path $report 'pending-recovery')) }
    } catch { Write-Text 'pending-recovery' $_.Exception.Message }
} catch {
    Write-Text 'watcher-error.txt' $_.Exception.ToString()
    # A very early exit can beat PowerShell startup. Preserve honest evidence without inventing
    # an exit code; annotate only if this is still our session and the target no longer exists.
    if (-not [IO.File]::Exists((Join-Path $report 'watcher-ready'))) {
        $stillAlive = $false
        try { $probe = [Diagnostics.Process]::GetProcessById([int]$cfg.ProcessId); $stillAlive = -not $probe.HasExited; $probe.Dispose() } catch { }
        if (-not $stillAlive) {
            try {
                if ($null -eq $heldLog) { $heldLog = [IO.File]::Open($cfg.LogPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::ReadWrite -bor [IO.FileShare]::Delete) }
                $earlyTail = Read-Tail
                $markers = [regex]::Matches($earlyTail, 'session=([0-9a-f]{32})')
                if ($markers.Count -gt 0 -and $markers[$markers.Count-1].Groups[1].Value -eq $cfg.SessionId) {
                    if ([IO.File]::Exists((Join-Path $report 'quit-complete')) -or $earlyTail.Contains('Steamworks shutdown succeeded')) {
                        Write-Text 'normal-exit' 'Exited before observer attached; normal shutdown marker present.'
                    } else {
                        $earlySummary = "`n[SkinChanger.CrashDiagnostics] 启动/退出异常：session=$($cfg.SessionId)；进程在监视器取得句柄前已结束，退出码未知；不能仅凭此确认崩溃。诊断目录=$report`n"
                        Write-Text 'diagnostic.txt' $earlySummary
                        Write-Text 'godot-tail.log' ($earlyTail + $earlySummary)
                        if ($heldLog.CanWrite) {
                            $heldLog.Seek(0, [IO.SeekOrigin]::End) | Out-Null
                            $bytes = $utf8.GetBytes($earlySummary); $heldLog.Write($bytes, 0, $bytes.Length); $heldLog.Flush()
                            Write-Text 'appended' 'appended'
                        } else { Write-Text 'pending-recovery' 'Early exit log is read-only.' }
                    }
                }
            } catch { Write-Text 'early-exit-error.txt' $_.Exception.Message }
        }
    }
} finally {
    if ($heldLog) { $heldLog.Dispose() }
    if ($target) { $target.Dispose() }
}
