using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed record GitHubInstallLedgerState(int Format, Dictionary<string, GitHubInstall> Installs, Dictionary<string, GitHubPendingOp>? Pending, Dictionary<string, string[]>? Deletes);
