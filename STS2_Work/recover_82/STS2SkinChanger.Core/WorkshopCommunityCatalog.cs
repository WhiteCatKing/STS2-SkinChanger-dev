using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace STS2SkinChanger.Core;

internal sealed class WorkshopCommunityCatalog
{
	public WorkshopCommunityState State { get; private set; } = WorkshopCommunityState.Empty;

	public WorkshopCatalogItem[] Items => State.Entries.Select((WorkshopVerifiedSubmission e) => e.Item).ToArray();

	public async Task Replace(Func<Task<WorkshopCommunityState>> load, string cachePath, Action<string, Action>? cacheWriter = null)
	{
		WorkshopCommunityState state = await load();
		Validate(state);
		string json = JsonSerializer.Serialize(state);
		if (Encoding.UTF8.GetByteCount(json) > 33554432)
		{
			throw new InvalidDataException("Community catalog cache too large.");
		}
		await Task.Run(delegate
		{
			if (cacheWriter == null)
			{
				Write();
			}
			else
			{
				cacheWriter(cachePath, Write);
			}
		});
		State = state;
		void Write()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
			string text = cachePath + ".tmp";
			File.WriteAllText(text, json);
			File.Move(text, cachePath, overwrite: true);
		}
	}

	public async Task Restore(string cachePath)
	{
		if (File.Exists(cachePath))
		{
			if (new FileInfo(cachePath).Length > 33554432)
			{
				throw new InvalidDataException("Community catalog cache too large.");
			}
			WorkshopCommunityState workshopCommunityState = JsonSerializer.Deserialize<WorkshopCommunityState>(await File.ReadAllTextAsync(cachePath), new JsonSerializerOptions
			{
				MaxDepth = 16
			});
			Validate(workshopCommunityState);
			State = workshopCommunityState with
			{
				Issues = workshopCommunityState.Issues.Where(delegate(WorkshopCodeIssue i)
				{
					WorkshopCodeError error = i.Error;
					bool flag = (uint)(error - 1) <= 1u;
					return !flag;
				}).ToArray()
			};
		}
	}

	private static void Validate(WorkshopCommunityState? state)
	{
		if ((object)state == null || state.Format != 2 || state.Entries == null || state.Issues == null || state.Entries.Length > 20000 || state.Issues.Length > 10000 || state.Entries.Any(delegate(WorkshopVerifiedSubmission e)
		{
			int num;
			if (!(e == null))
			{
				string name = e.Name;
				if (name != null && name.Length <= 1024)
				{
					name = e.Group;
					if (name != null && name.Length == 24 && !(e.Item == null) && Hex(e.Group, 24))
					{
						num = ((!WorkshopSubmissionCodec.ValidItem(e.Item)) ? 1 : 0);
						goto IL_0062;
					}
				}
			}
			num = 1;
			goto IL_0062;
			IL_0062:
			bool flag = (byte)num != 0;
			bool flag2;
			if (!flag)
			{
				int? reply = e.Reply;
				if (reply.HasValue)
				{
					int valueOrDefault = reply.GetValueOrDefault();
					if (valueOrDefault < 0 || valueOrDefault > 30000)
					{
						flag2 = true;
						goto IL_0092;
					}
				}
				flag2 = false;
				goto IL_0092;
			}
			goto IL_0095;
			IL_0095:
			return flag;
			IL_0092:
			flag = flag2;
			goto IL_0095;
		}) || state.Issues.Any(delegate(WorkshopCodeIssue i)
		{
			int num2;
			if (!(i == null) && (Hex(i.Key, 12) || Hex(i.Key, 24)) && Enum.IsDefined(i.Error))
			{
				string name2 = i.Name;
				if (name2 != null && name2.Length <= 4096)
				{
					name2 = i.ActualName;
					if (name2 != null && name2.Length <= 1024 && i.Group != null)
					{
						num2 = ((i.Group.Length != 0 && !Hex(i.Group, 24)) ? 1 : 0);
						goto IL_00cd;
					}
				}
			}
			num2 = 1;
			goto IL_00cd;
			IL_00cd:
			bool flag3 = (byte)num2 != 0;
			if (!flag3)
			{
				int total = i.Total;
				bool flag4 = ((total < 0 || total > 128) ? true : false);
				flag3 = flag4;
			}
			if (!flag3)
			{
				int[] missing = i.Missing;
				if (missing != null && missing.Length <= 128 && !i.Missing.Any((int p) => p < 1 || p > i.Total))
				{
					WorkshopCodeSource[] sources = i.Sources;
					if (sources != null)
					{
						int total = sources.Length;
						if (total > 0 && total <= 10000)
						{
							return i.Sources.Any(delegate(WorkshopCodeSource s)
							{
								int num3;
								if (!(s == null))
								{
									string code = s.Code;
									if (code != null)
									{
										int length = code.Length;
										if (length > 0 && length <= 4096 && s.Url != null && WorkshopCodeDiagnostics.SafeSource(s.Url) && s.Page >= 1 && s.Reply >= 0)
										{
											num3 = ((s.Order < 0) ? 1 : 0);
											goto IL_0059;
										}
									}
								}
								num3 = 1;
								goto IL_0059;
								IL_0059:
								bool flag5 = (byte)num3 != 0;
								if (!flag5)
								{
									int length = s.Part;
									bool flag6 = ((length < 0 || length > 128) ? true : false);
									flag5 = flag6;
								}
								return flag5;
							});
						}
					}
				}
			}
			return true;
		}))
		{
			throw new InvalidDataException("Unverified or invalid community catalog cache.");
		}
	}

	private static bool Hex(string? value, int length)
	{
		if (value != null && value.Length == length)
		{
			return value.All(Uri.IsHexDigit);
		}
		return false;
	}
}
