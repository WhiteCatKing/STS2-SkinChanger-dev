using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

internal static class CacheWriteNoticeTests
{
    internal static void Run()
    {
        var state = new CacheWriteNoticeState();
        var run = new object();
        state.SetRun(run);
        void Take(CacheWriteRejectionReason reason, bool first)
        {
            state.Report(reason);
            Require(state.TryTake(out var actual, out var show) && actual == reason && show == first,
                "Notice must retain status updates while deduplicating automatic banners by reason.");
        }
        Take(CacheWriteRejectionReason.Budget, true);
        Take(CacheWriteRejectionReason.Budget, false);
        Take(CacheWriteRejectionReason.DiskSpace, true);
        state.SetRun(run); // A directory/skin reload in the same run must not reset deduplication.
        Take(CacheWriteRejectionReason.Budget, false);
        state.SetRun(new object());
        Take(CacheWriteRejectionReason.Budget, true);
        Task.Run(() => state.Report(CacheWriteRejectionReason.DiskSpace)).GetAwaiter().GetResult();
        Require(state.TryTake(out var pending, out var fresh) && pending == CacheWriteRejectionReason.DiskSpace && fresh,
            "A background writer must safely queue a new reason for the UI thread.");
        Require(!state.TryTake(out _, out _), "Consumed failures must not create a per-frame warning loop.");

        var keys = new[] { ModText.CacheBudgetRejected, ModText.CacheDiskSpaceRejected, ModText.CacheWriteBlocked };
        Require(ModLocalization.SupportedLanguages.Count == 15, "Notice translation coverage changed.");
        var translations = (System.Collections.IDictionary)HarmonyLib.AccessTools.Field(typeof(ModLocalization),
            "CacheWriteTranslations").GetValue(null)!;
        Require(translations.Count == 15 && ModLocalization.SupportedLanguages.All(language => translations.Contains(language)),
            "Every published language must have its own cache notice translation.");
        foreach (var language in ModLocalization.SupportedLanguages)
        {
            var messages = keys.Select(key => ModLocalization.CacheWriteMessage(key, language)).ToArray();
            Require(messages.All(message => !string.IsNullOrWhiteSpace(message)) && messages.Distinct().Count() == 3,
                "Budget, disk and reserved-file refusals need separate translated explanations: " + language);
        }
        Require(ModLocalization.CacheWriteMessage(ModText.CacheBudgetRejected, "zhs").Contains("16 GiB") &&
                ModLocalization.CacheWriteMessage(ModText.CacheBudgetRejected, "unknown") ==
                ModLocalization.CacheWriteMessage(ModText.CacheBudgetRejected, "eng"), "Budget or language fallback missing.");

        var logging = AuditLogSilencer.Install();
        var root = Path.Combine(Path.GetTempPath(), "sc-notice-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var budget = new CacheWriteBudget([root], availableBytes: _ => 0);
            var called = false;
            try
            {
                SkinChangerPaths.WritePckOutput(budget, Path.Combine(root, "blocked.pck"), 1, () => called = true);
                throw new InvalidOperationException("Low disk write was not rejected.");
            }
            catch (CacheWriteRejectedException error)
            {
                Require(!called && error.Reason == CacheWriteRejectionReason.DiskSpace &&
                        error.Message == CacheWriteNotice.Report(error.Reason),
                    "The production writer must attach the localized UI message to the rollback exception.");
            }
        }
        finally
        {
            logging.UnpatchAll(logging.Id);
            Directory.Delete(root);
        }
        Console.WriteLine("Cache write notices passed: per-run reasons, reload retention, background queue, 15 languages and real writer rejection.");
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
