using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace STS2SkinChanger.Core;

internal static class WorkshopSubmissionCodec
{
	private sealed record Part(ulong Id, int Format, string Group, int Index, int Total, string Body, WorkshopCodeSource Source);

	private sealed class CodeFailure(WorkshopCodeError error) : Exception()
	{
		internal WorkshopCodeError Error => error;
	}

	[CompilerGenerated]
	private sealed class _003CCodeStarts_003Ed__13 : IEnumerable<Match>, IEnumerable, IEnumerator<Match>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Match _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string line;

		public string _003C_003E3__line;

		private int _003Ccursor_003E5__2;

		private bool _003Cquoted_003E5__3;

		private IEnumerator _003C_003E7__wrap3;

		Match IEnumerator<Match>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCodeStarts_003Ed__13(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003Ccursor_003E5__2 = 0;
					_003Cquoted_003E5__3 = false;
					_003C_003E7__wrap3 = Starts.Matches(line).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap3.MoveNext())
				{
					Match match = (Match)_003C_003E7__wrap3.Current;
					while (_003Ccursor_003E5__2 < match.Index)
					{
						if ((line[_003Ccursor_003E5__2] == '\\') & _003Cquoted_003E5__3)
						{
							_003Ccursor_003E5__2 += 2;
							continue;
						}
						if (line[_003Ccursor_003E5__2] == '"')
						{
							_003Cquoted_003E5__3 = !_003Cquoted_003E5__3;
						}
						_003Ccursor_003E5__2++;
					}
					if (!_003Cquoted_003E5__3)
					{
						_003C_003E2__current = match;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap3 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap3 is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Match> IEnumerable<Match>.GetEnumerator()
		{
			_003CCodeStarts_003Ed__13 _003CCodeStarts_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CCodeStarts_003Ed__ = this;
			}
			else
			{
				_003CCodeStarts_003Ed__ = new _003CCodeStarts_003Ed__13(0);
			}
			_003CCodeStarts_003Ed__.line = _003C_003E3__line;
			return _003CCodeStarts_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Match>)this).GetEnumerator();
		}
	}

	internal const int MaxCodeLength = 1800;

	private const int MaxExpandedBytes = 131072;

	private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
	{
		Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
		MaxDepth = 8
	};

	private static readonly Regex Header = new Regex("^SCM2\\s+(?<name>\"(?:\\\\.|[^\"\\\\])*\")\\s+(?<id>[0-9]+)\\s+(?<group>[A-Fa-f0-9]{24})\\s+(?<part>[0-9]+)/(?<total>[0-9]+)\\s+(?<body>[A-Za-z0-9_-]+)\\.(?<sum>[A-Fa-f0-9]{8})$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private static readonly Regex Header3 = new Regex("^SCM3\\s+(?<id>[0-9]+)\\s+(?<group>[A-Fa-f0-9]{24})\\s+(?<part>[0-9]+)/(?<total>[0-9]+)\\s+(?<body>[A-Za-z0-9_-]+)\\.(?<sum>[A-Fa-f0-9]{8})$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private static readonly Regex Starts = new Regex("(?<!\\w)SCM\\d+\\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private static readonly Regex Recover = new Regex("^SCM[0-9]+\\s+(?:(?<name>\"(?:\\\\.|[^\"\\\\])*\")\\s+)?(?<id>[0-9]+)?(?:\\s+(?<group>[A-Fa-f0-9]{24})(?:\\s+(?<part>[0-9]+)/(?<total>[0-9]+))?)?", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	internal static string Fingerprint(string text)
	{
		return Hash(Encoding.UTF8.GetBytes(text), 12);
	}

	private static string Hash(byte[] bytes, int chars)
	{
		return Convert.ToHexString(SHA256.HashData(bytes)).Substring(0, chars);
	}

	internal static string[] Encode(WorkshopCatalogItem item, string scanner, string game)
	{
		WorkshopCodePayload workshopCodePayload = new WorkshopCodePayload(3, 2868840u, scanner, game, item);
		Validate(workshopCodePayload);
		byte[] array = JsonSerializer.SerializeToUtf8Bytes(workshopCodePayload, Json);
		if (array.Length > 131072)
		{
			throw new InvalidDataException("Submission too large.");
		}
		using MemoryStream memoryStream = new MemoryStream();
		using (BrotliStream brotliStream = new BrotliStream(memoryStream, CompressionLevel.Optimal, leaveOpen: true))
		{
			brotliStream.Write(array);
		}
		byte[] array2 = memoryStream.ToArray();
		string body = Convert.ToBase64String(array2).TrimEnd('=').Replace('+', '-')
			.Replace('/', '_');
		string header = $"SCM3 {item.Id} {Hash(array2, 24)} ";
		int size = 1800 - header.Length - 32;
		int total = (body.Length + size - 1) / size;
		if (total > 128)
		{
			throw new InvalidDataException("Too many fragments.");
		}
		return Enumerable.Range(0, total).Select(delegate(int index)
		{
			string text = header + $"{index + 1}/{total} " + body.Substring(index * size, Math.Min(size, body.Length - index * size));
			return text + "." + Hash(Encoding.UTF8.GetBytes(text), 8);
		}).ToArray();
	}

	internal static WorkshopCodeRead Read(IEnumerable<WorkshopCodePost> posts)
	{
		List<Part> list = new List<Part>();
		List<WorkshopCodeIssue> list2 = new List<WorkshopCodeIssue>();
		int num = 0;
		int num2 = 0;
		foreach (WorkshopCodePost post in posts)
		{
			string[] array = post.Text.Replace('\r', '\n').Split('\n');
			foreach (string text in array)
			{
				Match[] array2 = CodeStarts(text).ToArray();
				for (int k = 0; k < array2.Length; k++)
				{
					if (++num2 > 10000)
					{
						throw new InvalidDataException("Too many submission codes.");
					}
					int index = array2[k].Index;
					string text2 = text.Substring(index, ((k + 1 < array2.Length) ? array2[k + 1].Index : text.Length) - index).Trim();
					WorkshopCodeSource source = new WorkshopCodeSource(text2.Substring(0, Math.Min(text2.Length, 4096)), post.Url, post.Page, post.Reply, num++);
					if (text2.StartsWith("SCM1.", StringComparison.Ordinal))
					{
						WorkshopCatalogItem[] array3 = WorkshopSubmissionCode.ReadPosts(new _003C_003Ez__ReadOnlySingleElementList<string>(text2));
						if (array3.Length == 0)
						{
							list2.Add(Issue(source, WorkshopCodeError.Legacy, 0uL));
							continue;
						}
						WorkshopCatalogItem[] array4 = array3;
						foreach (WorkshopCatalogItem workshopCatalogItem in array4)
						{
							list2.Add(Issue(source, WorkshopCodeError.Legacy, workshopCatalogItem.Id));
						}
					}
					else
					{
						try
						{
							list.Add(Parse(text2, source));
						}
						catch (CodeFailure codeFailure)
						{
							list2.Add(RecoverIssue(source, codeFailure.Error));
						}
						catch (Exception ex) when (((ex is JsonException || ex is FormatException || ex is ArgumentException || ex is OverflowException) ? 1 : 0) != 0)
						{
							list2.Add(RecoverIssue(source, WorkshopCodeError.Format));
						}
					}
				}
			}
		}
		List<WorkshopCodeCandidate> list3 = new List<WorkshopCodeCandidate>();
		int num3 = 0;
		foreach (IGrouping<string, Part> item in from p in list
			group p by p.Group)
		{
			Part first = item.First();
			WorkshopCodeSource[] sources = item.Select((Part p) => p.Source).Distinct().ToArray();
			Part[] source2 = item.DistinctBy((Part p) => (Index: p.Index, Body: p.Body)).ToArray();
			int[] array5 = Enumerable.Range(1, first.Total).Except(source2.Select((Part p) => p.Index)).ToArray();
			WorkshopCodeError? workshopCodeError = ((item.Any((Part p) => p.Id != first.Id || p.Format != first.Format || p.Total != first.Total) || (from p in source2
				group p by p.Index).Any((IGrouping<int, Part> g) => g.Count() > 1)) ? new WorkshopCodeError?(WorkshopCodeError.Conflict) : ((array5.Length != 0) ? new WorkshopCodeError?(WorkshopCodeError.MissingParts) : ((WorkshopCodeError?)null)));
			if (!workshopCodeError.HasValue)
			{
				try
				{
					string text3 = string.Concat(from p in source2
						orderby p.Index
						select p.Body).Replace('-', '+').Replace('_', '/');
					byte[] array6 = Convert.FromBase64String(text3.PadRight((text3.Length + 3) / 4 * 4, '='));
					if (Hash(array6, 24) != first.Group)
					{
						throw new CodeFailure(WorkshopCodeError.Checksum);
					}
					using MemoryStream stream = new MemoryStream(array6);
					using BrotliStream brotliStream = new BrotliStream(stream, CompressionMode.Decompress);
					using MemoryStream memoryStream = new MemoryStream();
					byte[] array7 = new byte[4096];
					int num4;
					while ((num4 = brotliStream.Read(array7)) > 0)
					{
						if (memoryStream.Length + num4 > 131072)
						{
							throw new CodeFailure(WorkshopCodeError.TooLarge);
						}
						memoryStream.Write(array7, 0, num4);
					}
					num3 += (int)memoryStream.Length;
					WorkshopCodePayload workshopCodePayload = JsonSerializer.Deserialize<WorkshopCodePayload>(memoryStream.ToArray(), Json) ?? throw new CodeFailure(WorkshopCodeError.InvalidData);
					Validate(workshopCodePayload);
					if (workshopCodePayload.Item.Id != first.Id || workshopCodePayload.Format != first.Format)
					{
						throw new CodeFailure(WorkshopCodeError.Conflict);
					}
					list3.Add(new WorkshopCodeCandidate(workshopCodePayload, first.Group, sources));
				}
				catch (CodeFailure codeFailure2)
				{
					workshopCodeError = codeFailure2.Error;
				}
				catch (Exception ex2) when (((ex2 is InvalidDataException || ex2 is JsonException || ex2 is IOException || ex2 is FormatException || ex2 is ArgumentException) ? 1 : 0) != 0)
				{
					workshopCodeError = WorkshopCodeError.InvalidData;
				}
			}
			if (num3 > 33554432)
			{
				throw new InvalidDataException("Submission expansion budget exceeded.");
			}
			if (workshopCodeError.HasValue)
			{
				WorkshopCodeError valueOrDefault = workshopCodeError.GetValueOrDefault();
				list2.Add(new WorkshopCodeIssue(first.Group, first.Id, "", "", valueOrDefault, first.Group, first.Total, array5, sources));
			}
		}
		return new WorkshopCodeRead(list3.ToArray(), (from i in list2
			group i by (Key: i.Key, Id: i.Id, Error: i.Error) into g
			select g.First()with
			{
				Sources = g.SelectMany((WorkshopCodeIssue i) => i.Sources).Distinct().ToArray()
			}).ToArray());
	}

	[IteratorStateMachine(typeof(_003CCodeStarts_003Ed__13))]
	private static IEnumerable<Match> CodeStarts(string line)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCodeStarts_003Ed__13(-2)
		{
			_003C_003E3__line = line
		};
	}

	private static Part Parse(string raw, WorkshopCodeSource source)
	{
		if (raw.Length > 1800)
		{
			throw new CodeFailure(WorkshopCodeError.TooLarge);
		}
		int num = (raw.StartsWith("SCM3 ", StringComparison.Ordinal) ? 3 : (raw.StartsWith("SCM2 ", StringComparison.Ordinal) ? 2 : 0));
		Match match = (num switch
		{
			0 => throw new CodeFailure(WorkshopCodeError.Version), 
			3 => Header3, 
			_ => Header, 
		}).Match(raw);
		if (!match.Success)
		{
			throw new CodeFailure(WorkshopCodeError.Format);
		}
		ulong num2 = ulong.Parse(match.Groups["id"].Value);
		int num3 = int.Parse(match.Groups["part"].Value);
		int num4 = int.Parse(match.Groups["total"].Value);
		bool flag = ((num2 == 0L || num2 == 3787302680u) ? true : false);
		bool flag2 = flag;
		if (!flag2)
		{
			bool flag3 = ((num4 < 1 || num4 > 128) ? true : false);
			flag2 = flag3;
		}
		if (flag2 || num3 < 1 || num3 > num4)
		{
			throw new CodeFailure(WorkshopCodeError.Format);
		}
		if (Hash(Encoding.UTF8.GetBytes(raw.Substring(0, raw.LastIndexOf('.'))), 8) != match.Groups["sum"].Value.ToUpperInvariant())
		{
			throw new CodeFailure(WorkshopCodeError.Checksum);
		}
		return new Part(num2, num, match.Groups["group"].Value.ToUpperInvariant(), num3, num4, match.Groups["body"].Value, source with
		{
			Part = num3
		});
	}

	private static WorkshopCodeIssue RecoverIssue(WorkshopCodeSource source, WorkshopCodeError error)
	{
		Match match = Recover.Match(source.Code);
		string name = "";
		try
		{
			if (match.Groups["name"].Success)
			{
				name = JsonSerializer.Deserialize<string>(match.Groups["name"].Value) ?? "";
			}
		}
		catch (JsonException)
		{
		}
		ulong.TryParse(match.Groups["id"].Value, out var result);
		int.TryParse(match.Groups["part"].Value, out var result2);
		int.TryParse(match.Groups["total"].Value, out var result3);
		bool flag = ((result3 < 1 || result3 > 128) ? true : false);
		if (flag || result2 < 1 || result2 > result3)
		{
			result2 = (result3 = 0);
		}
		WorkshopCodeSource source2 = source with
		{
			Part = result2
		};
		return Issue(source2, error, result, name, match.Groups["group"].Value)with
		{
			Total = result3
		};
	}

	private static WorkshopCodeIssue Issue(WorkshopCodeSource source, WorkshopCodeError error, ulong id = 0uL, string name = "", string group = "")
	{
		return new WorkshopCodeIssue(Fingerprint(source.Code), id, name, "", error, group, 0, Array.Empty<int>(), new WorkshopCodeSource[1] { source });
	}

	private static void Validate(WorkshopCodePayload payload)
	{
		int format = payload.Format;
		if ((uint)(format - 2) > 1u)
		{
			throw new CodeFailure(WorkshopCodeError.Version);
		}
		if (payload.App != 2868840)
		{
			throw new CodeFailure(WorkshopCodeError.WrongGame);
		}
		string scanner = payload.Scanner;
		if (scanner != null)
		{
			format = scanner.Length;
			if (format > 0 && format <= 32)
			{
				scanner = payload.Game;
				if (scanner != null && scanner.Length <= 64 && ValidItem(payload.Item))
				{
					return;
				}
			}
		}
		throw new CodeFailure(WorkshopCodeError.InvalidData);
	}

	internal static bool ValidItem(WorkshopCatalogItem? item)
	{
		if ((object)item != null)
		{
			ulong id = item.Id;
			if (id != 0L && id != 3787302680u)
			{
				WorkshopTarget[] targets = item.Targets;
				if (targets != null)
				{
					int num = targets.Length;
					if (num > 0 && num <= 512)
					{
						return item.Targets.All(delegate(WorkshopTarget target)
						{
							bool flag = target != null;
							if (flag)
							{
								bool flag2;
								switch (target.Kind)
								{
								case "character":
								case "companion":
								case "cards":
								case "event":
								case "ancient":
								case "monster":
								case "merchant":
									flag2 = true;
									break;
								default:
									flag2 = false;
									break;
								}
								flag = flag2;
							}
							if (flag)
							{
								string target2 = target.Target;
								if (target2 != null)
								{
									int length = target2.Length;
									if (length > 0 && length <= 160)
									{
										return !target.Target.Any((char c) => char.IsControl(c) || "<>\"'\\/".Contains(c));
									}
								}
							}
							return false;
						});
					}
				}
			}
		}
		return false;
	}
}
