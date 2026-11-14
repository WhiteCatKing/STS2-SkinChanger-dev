# 一键同步：拉取远端 → 提交本地改动 → 推送
# 用法：  .\sync.ps1 "本次改动说明"
param([string]$Message = "同步")
Set-Location $PSScriptRoot
$env:GIT_TERMINAL_PROMPT = '0'
git -c http.proxy=http://127.0.0.1:7890 pull --rebase
$dirty = (git status --porcelain | Measure-Object).Count
if ($dirty -gt 0) {
    git add -A
    git commit -q -m $Message
    "已提交：$Message（$dirty 项）"
} else { "没有本地改动" }
git -c http.proxy=http://127.0.0.1:7890 push
"--- 状态 ---"
git log --oneline -3
git status -sb | Select-Object -First 1