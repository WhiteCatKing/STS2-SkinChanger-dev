using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal static class WorkshopCatalogPolicy
{
	public const string CommandId = "__workshop__";

	public const uint AppId = 2868840u;

	private static readonly HashSet<string> Kinds = new HashSet<string> { "character", "cards", "monster", "ancient", "merchant", "companion", "event" };

	public static WorkshopCatalogItem[] Parse(string json)
	{
		if (json.Length > 4194304)
		{
			throw new InvalidDataException("Workshop catalog too large.");
		}
		return (from item in JsonSerializer.Deserialize<WorkshopCatalogItem[]>(json, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				MaxDepth = 8
			}) ?? Array.Empty<WorkshopCatalogItem>()
			where item != null && item.Id != 0
			group item by item.Id into @group
			select new WorkshopCatalogItem(@group.Key, (from t in @group.SelectMany((WorkshopCatalogItem item) => item.Targets ?? Array.Empty<WorkshopTarget>())
				where t != null && Kinds.Contains(t.Kind) && !string.IsNullOrWhiteSpace(t.Target)
				select new WorkshopTarget(t.Kind, t.Target.ToLowerInvariant())).Distinct().ToArray(), @group.Any((WorkshopCatalogItem item) => item.RestartRequired)) into item
			where item.Targets.Length != 0
			select item).ToArray();
	}

	public static ulong[] FilterIds(string json, string kind, string target)
	{
		return (from item in Filter(Parse(json), kind, target)
			select item.Id).ToArray();
	}

	public static IEnumerable<WorkshopCatalogItem> Filter(IEnumerable<WorkshopCatalogItem> items, string kind, string target)
	{
		return items.Where((WorkshopCatalogItem item) => item.Targets.Any((WorkshopTarget t) => (kind.Length == 0 || t.Kind == kind) && (target.Length == 0 || t.Target.Equals(target, StringComparison.OrdinalIgnoreCase))));
	}

	public static bool IsSkinChoice(string id)
	{
		return !id.Equals("__workshop__", StringComparison.OrdinalIgnoreCase);
	}

	public static bool CanUseInstalledFiles(bool installed, bool needsUpdate, bool downloading, bool pending)
	{
		if (installed && !needsUpdate && !downloading)
		{
			return !pending;
		}
		return false;
	}

	public static bool IsSafeCover(byte[] data)
	{
		if (data.Length < 24 || data.Length > 2097152)
		{
			return false;
		}
		if (data.AsSpan(0, 8).SequenceEqual(new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 }) && data.AsSpan(12, 4).SequenceEqual("IHDR"u8))
		{
			return Size(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(16)), BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(20)));
		}
		if (data[0] != byte.MaxValue || data[1] != 216)
		{
			return false;
		}
		int i = 2;
		while (i + 4 < data.Length)
		{
			if (data[i++] != byte.MaxValue)
			{
				return false;
			}
			for (; i < data.Length && data[i] == byte.MaxValue; i++)
			{
			}
			if (i + 3 >= data.Length)
			{
				return false;
			}
			byte b = data[i++];
			if ((uint)(b - 217) <= 1u)
			{
				return false;
			}
			bool flag;
			switch (b)
			{
			case 1:
			case 208:
			case 209:
			case 210:
			case 211:
			case 212:
			case 213:
			case 214:
			case 215:
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				continue;
			}
			int num = (data[i] << 8) | data[i + 1];
			if (num < 2 || i + num > data.Length)
			{
				return false;
			}
			switch (b)
			{
			case 192:
			case 193:
			case 194:
			case 195:
			case 197:
			case 198:
			case 199:
			case 201:
			case 202:
			case 203:
			case 205:
			case 206:
			case 207:
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				if (num >= 7)
				{
					return Size((uint)((data[i + 5] << 8) | data[i + 6]), (uint)((data[i + 3] << 8) | data[i + 4]));
				}
				return false;
			}
			i += num;
		}
		return false;
		static bool Size(uint width, uint height)
		{
			if (width != 0 && width <= 4096 && height != 0 && height <= 4096)
			{
				return (ulong)((long)width * (long)height) <= 8388608uL;
			}
			return false;
		}
	}
}
