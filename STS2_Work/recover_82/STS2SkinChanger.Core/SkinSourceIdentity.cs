using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace STS2SkinChanger.Core;

internal static class SkinSourceIdentity
{
	public const ulong GitHubMask = 9223372036854775808uL;

	public static ulong GitHubId(string owner, string repo)
	{
		string s = owner.Trim().ToLowerInvariant() + "/" + repo.Trim().ToLowerInvariant();
		return (BinaryPrimitives.ReadUInt64BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes(s))) & 0x7FFFFFFFFFFFFFFFL) | 0x8000000000000000uL;
	}

	public static bool IsGitHub(ulong id)
	{
		return (id & 0x8000000000000000uL) != 0;
	}

	public static bool IsSteam(ulong id)
	{
		if (id != 0L)
		{
			return (id & 0x8000000000000000uL) == 0;
		}
		return false;
	}
}
