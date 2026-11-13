using MegaCrit.Sts2.Core.Logging;

namespace STS2SkinChanger.Core;

internal static class ModLog
{
    private const string Prefix = "[SkinChanger] ";

    /// <summary>本次会话我们自己的 Info 输出总量上限（字符数），防止诊断日志把 godot.log 撑到几百 MB。</summary>
    private const long InfoBudgetCharacters = 24L * 1024 * 1024;

    private static long _infoCharacters;
    private static bool _budgetExhausted;

    public static void Info(string message)
    {
        if (_budgetExhausted)
        {
            return;
        }

        _infoCharacters += message.Length;
        if (_infoCharacters > InfoBudgetCharacters)
        {
            _budgetExhausted = true;
            Log.Warn(Prefix + $"Info 日志已达 {InfoBudgetCharacters / (1024 * 1024)} MB 上限，本会话后续 Info 不再输出（Warn/Error 仍然输出）。");
            return;
        }

        Log.Info(Prefix + message);
    }

    public static void Warn(string message) => Log.Warn(Prefix + message);

    public static void Error(string message) => Log.Error(Prefix + message);
}
