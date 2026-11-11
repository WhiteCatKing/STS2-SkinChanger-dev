using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace STS2SkinChanger.Core;

internal static class WorkshopSubmissionCode
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass5_0
	{
		public string scanner;

		public string game;
	}

	public const string Prefix = "SCM1.";

	public const int MaxCodeLength = 1800;

	private const int MaxDecodedBytes = 131072;

	private static readonly HashSet<string> Kinds = new HashSet<string> { "character", "cards", "monster", "ancient", "merchant", "companion", "event" };

	private static readonly Regex Codes = new Regex("(?<![A-Za-z0-9_])SCM1\\.[A-Za-z0-9_-]+\\.[A-Fa-f0-9]{8}(?![A-Za-z0-9_])", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	public static string[] Encode(IEnumerable<WorkshopCatalogItem> items, string scanner, string game)
	{
		_003C_003Ec__DisplayClass5_0 CS_0024_003C_003E8__locals5 = new _003C_003Ec__DisplayClass5_0();
		CS_0024_003C_003E8__locals5.scanner = scanner;
		CS_0024_003C_003E8__locals5.game = game;
		WorkshopCatalogItem[] source = Normalize(items);
		List<string> list = new List<string>();
		List<WorkshopCatalogItem> list2 = new List<WorkshopCatalogItem>();
		foreach (WorkshopCatalogItem item in source.SelectMany(Split))
		{
			list2.Add(item);
			if (!Fits(list2.ToArray(), out var _))
			{
				list2.RemoveAt(list2.Count - 1);
				Fits(list2.ToArray(), out var code3);
				list.Add(code3);
				int num = 1;
				List<WorkshopCatalogItem> list3 = new List<WorkshopCatalogItem>(num);
				CollectionsMarshal.SetCount(list3, num);
				Span<WorkshopCatalogItem> span = CollectionsMarshal.AsSpan(list3);
				int index = 0;
				span[index] = item;
				list2 = list3;
			}
		}
		if (list2.Count > 0)
		{
			Fits(list2.ToArray(), out var code4);
			list.Add(code4);
		}
		return list.ToArray();
		bool Fits(WorkshopCatalogItem[] part, out string code)
		{
			code = Pack(new WorkshopSubmission(1, 2868840u, CS_0024_003C_003E8__locals5.scanner, CS_0024_003C_003E8__locals5.game, part)) ?? "";
			if (part.Length <= 64)
			{
				int length = code.Length;
				if (length > 0)
				{
					return length <= 1800;
				}
				return false;
			}
			return false;
		}
		[IteratorStateMachine(typeof(_003C_003Ec__DisplayClass5_0._003C_003CEncode_003Eg__Split_007C1_003Ed))]
		IEnumerable<WorkshopCatalogItem> Split(WorkshopCatalogItem item)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003C_003Ec__DisplayClass5_0._003C_003CEncode_003Eg__Split_007C1_003Ed(-2)
			{
				_003C_003E4__this = CS_0024_003C_003E8__locals5,
				_003C_003E3__item = item
			};
		}
	}

	private static string? Pack(WorkshopSubmission submission)
	{
		Validate(submission);
		byte[] array = JsonSerializer.SerializeToUtf8Bytes(submission);
		if (array.Length > 131072)
		{
			return null;
		}
		using MemoryStream memoryStream = new MemoryStream();
		using (BrotliStream brotliStream = new BrotliStream(memoryStream, CompressionLevel.Optimal, leaveOpen: true))
		{
			brotliStream.Write(array);
		}
		byte[] array2 = memoryStream.ToArray();
		return "SCM1." + Convert.ToBase64String(array2).TrimEnd('=').Replace('+', '-')
			.Replace('/', '_') + "." + Convert.ToHexString(SHA256.HashData(array2).AsSpan(0, 4));
	}

	public static WorkshopSubmission Decode(string code)
	{
		if (code.Length > 1800 || !code.StartsWith("SCM1.", StringComparison.Ordinal))
		{
			throw new InvalidDataException("Unknown or oversized submission code.");
		}
		string[] array = code.Split('.');
		if (array.Length != 3 || array[2].Length != 8)
		{
			throw new InvalidDataException("Invalid submission code.");
		}
		string text = array[1].Replace('-', '+').Replace('_', '/');
		byte[] array2 = Convert.FromBase64String(text.PadRight((text.Length + 3) / 4 * 4, '='));
		if (!Convert.ToHexString(SHA256.HashData(array2).AsSpan(0, 4)).Equals(array[2], StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidDataException("Submission checksum mismatch.");
		}
		using MemoryStream stream = new MemoryStream(array2);
		using BrotliStream brotliStream = new BrotliStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		byte[] array3 = new byte[4096];
		int num;
		while ((num = brotliStream.Read(array3)) > 0)
		{
			if (memoryStream.Length + num > 131072)
			{
				throw new InvalidDataException("Submission expansion limit.");
			}
			memoryStream.Write(array3, 0, num);
		}
		WorkshopSubmission workshopSubmission = JsonSerializer.Deserialize<WorkshopSubmission>(memoryStream.ToArray(), new JsonSerializerOptions
		{
			MaxDepth = 8
		}) ?? throw new InvalidDataException("Empty submission.");
		Validate(workshopSubmission);
		return workshopSubmission with
		{
			Items = Normalize(workshopSubmission.Items)
		};
	}

	public static WorkshopCatalogItem[] ReadPosts(IEnumerable<string> posts)
	{
		List<WorkshopCatalogItem> list = new List<WorkshopCatalogItem>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		int num = 0;
		foreach (string post in posts)
		{
			if (post.Length > 2097152)
			{
				throw new InvalidDataException("Discussion post too large.");
			}
			foreach (Match item in Codes.Matches(post))
			{
				if (item.Length > 1800 || !hashSet.Add(item.Value))
				{
					continue;
				}
				if (hashSet.Count > 10000)
				{
					throw new InvalidDataException("Too many submission codes.");
				}
				try
				{
					WorkshopCatalogItem[] items = Decode(item.Value).Items;
					num += items.Sum((WorkshopCatalogItem i) => i.Targets.Length);
					list.AddRange(items);
				}
				catch (Exception ex) when (((ex is InvalidDataException || ex is IOException || ex is JsonException || ex is FormatException || ex is ArgumentException) ? 1 : 0) != 0)
				{
				}
				if (list.Count > 20000 || num > 100000)
				{
					throw new InvalidDataException("Too many submitted Mods or targets.");
				}
			}
		}
		return Normalize(list);
	}

	public static WorkshopCatalogItem[] Merge(IEnumerable<WorkshopCatalogItem> builtIn, IEnumerable<WorkshopCatalogItem> community)
	{
		WorkshopCatalogItem[] array = builtIn.ToArray();
		HashSet<ulong> known = array.Select((WorkshopCatalogItem i) => i.Id).ToHashSet();
		return array.Concat(from i in Normalize(community)
			where !known.Contains(i.Id)
			select i).ToArray();
	}

	private static WorkshopCatalogItem[] Normalize(IEnumerable<WorkshopCatalogItem> items)
	{
		return (from i in items
			group i by i.Id into g
			orderby g.Key
			select new WorkshopCatalogItem(g.Key, (from t in g.SelectMany((WorkshopCatalogItem i) => i.Targets)
				select new WorkshopTarget(t.Kind, t.Target.ToLowerInvariant())).Distinct().OrderBy<WorkshopTarget, string>((WorkshopTarget t) => t.Kind, StringComparer.Ordinal).ThenBy<WorkshopTarget, string>((WorkshopTarget t) => t.Target, StringComparer.Ordinal)
				.ToArray(), g.Any((WorkshopCatalogItem i) => i.RestartRequired))).ToArray();
	}

	private static void Validate(WorkshopSubmission s)
	{
		if (s.Format == 1 && s.App == 2868840)
		{
			string scanner = s.Scanner;
			if (scanner != null)
			{
				int length = scanner.Length;
				if (length > 0 && length <= 32)
				{
					scanner = s.Game;
					if (scanner != null && scanner.Length <= 64)
					{
						WorkshopCatalogItem[] items = s.Items;
						if (items != null)
						{
							length = items.Length;
							if (length > 0 && length <= 65)
							{
								items = s.Items;
								length = 0;
								while (length < items.Length)
								{
									WorkshopCatalogItem workshopCatalogItem = items[length];
									if (!(workshopCatalogItem == null) && workshopCatalogItem.Id != 0L && workshopCatalogItem.Id != 3787302680u)
									{
										WorkshopTarget[] targets = workshopCatalogItem.Targets;
										if (targets != null)
										{
											int num = targets.Length;
											if (num > 0 && num <= 512)
											{
												targets = workshopCatalogItem.Targets;
												num = 0;
												while (num < targets.Length)
												{
													WorkshopTarget workshopTarget = targets[num];
													if (!(workshopTarget == null) && Kinds.Contains(workshopTarget.Kind))
													{
														scanner = workshopTarget.Target;
														if (scanner != null)
														{
															int length2 = scanner.Length;
															if (length2 > 0 && length2 <= 160 && !workshopTarget.Target.Any((char c) => char.IsControl(c) || "<>\"'\\/".Contains(c)))
															{
																num++;
																continue;
															}
														}
													}
													throw new InvalidDataException("Invalid classification target.");
												}
												length++;
												continue;
											}
										}
									}
									throw new InvalidDataException("Invalid submitted Mod.");
								}
								return;
							}
						}
					}
				}
			}
		}
		throw new InvalidDataException("Invalid submission schema.");
	}
}
