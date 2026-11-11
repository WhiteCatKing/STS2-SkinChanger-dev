using System;

namespace STS2SkinChanger.Core;

internal sealed record ActionFailure(string Stage, Exception Exception);
