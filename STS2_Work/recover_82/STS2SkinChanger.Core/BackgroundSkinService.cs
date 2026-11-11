using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Core;

internal static class BackgroundSkinService
{
	internal sealed class BackgroundActEntry
	{
		public string ActId { get; }

		public string Title { get; }

		public List<BackgroundOption> Options { get; private set; }

		public BackgroundActEntry(string actId, string title, List<BackgroundOption> options)
		{
			ActId = actId;
			Title = title;
			Options = options;
		}

		internal void ReplaceOptions(List<BackgroundOption> options)
		{
			Options = options;
		}
	}

	private sealed record ModBackgroundImage(string ModId, string ModName, string Path);

	private sealed record ModBackgroundShell(string ModId, string ModName, BackgroundPackShell Shell);

	private sealed record ModBackgroundScene(string ModId, string ModName, string Path);

	private sealed record ModBackgroundResources(IReadOnlyList<string> Titles, IReadOnlyList<ModBackgroundImage> Images, IReadOnlyList<ModBackgroundShell> Shells, IReadOnlyList<ModBackgroundScene> Scenes);

	[CompilerGenerated]
	private sealed class _003CDiscoverDeclaredSceneBackgrounds_003Ed__91 : IEnumerable<(string Source, string ScenePath, string SourceDisplay)>, IEnumerable, IEnumerator<(string Source, string ScenePath, string SourceDisplay)>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private (string Source, string ScenePath, string SourceDisplay) _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private HashSet<string> _003Cseen_003E5__2;

		private Assembly[] _003C_003E7__wrap2;

		private int _003C_003E7__wrap3;

		private string _003Csource_003E5__5;

		private string _003CsourceDisplay_003E5__6;

		private IEnumerator<Type> _003C_003E7__wrap6;

		private FieldInfo[] _003C_003E7__wrap7;

		private int _003C_003E7__wrap8;

		private string[] _003C_003E7__wrap9;

		private int _003C_003E7__wrap10;

		(string Source, string ScenePath, string SourceDisplay) IEnumerator<(string, string, string)>.Current
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
		public _003CDiscoverDeclaredSceneBackgrounds_003Ed__91(int _003C_003E1__state)
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
			_003Cseen_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003Csource_003E5__5 = null;
			_003CsourceDisplay_003E5__6 = null;
			_003C_003E7__wrap6 = null;
			_003C_003E7__wrap7 = null;
			_003C_003E7__wrap9 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -3;
					goto IL_0273;
				}
				_003C_003E1__state = -1;
				_003Cseen_003E5__2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				_003C_003E7__wrap2 = AppDomain.CurrentDomain.GetAssemblies();
				_003C_003E7__wrap3 = 0;
				goto IL_02fc;
				IL_029b:
				_003C_003E7__wrap8++;
				goto IL_02a9;
				IL_02fc:
				Assembly assembly;
				string location;
				if (_003C_003E7__wrap3 < _003C_003E7__wrap2.Length)
				{
					assembly = _003C_003E7__wrap2[_003C_003E7__wrap3];
					try
					{
						if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
						{
							location = assembly.Location;
							goto IL_0082;
						}
					}
					catch
					{
					}
					goto IL_02ee;
				}
				_003C_003E7__wrap2 = null;
				return false;
				IL_02c3:
				if (_003C_003E7__wrap6.MoveNext())
				{
					Type current = _003C_003E7__wrap6.Current;
					_003C_003E7__wrap7 = current.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					_003C_003E7__wrap8 = 0;
					goto IL_02a9;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap6 = null;
				_003Csource_003E5__5 = null;
				_003CsourceDisplay_003E5__6 = null;
				goto IL_02ee;
				IL_0273:
				_003C_003E7__wrap10++;
				goto IL_0281;
				IL_0281:
				if (_003C_003E7__wrap10 < _003C_003E7__wrap9.Length)
				{
					string text = _003C_003E7__wrap9[_003C_003E7__wrap10];
					if (!string.IsNullOrWhiteSpace(text) && text.StartsWith("res://", StringComparison.OrdinalIgnoreCase) && text.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) && _003Cseen_003E5__2.Add(text) && ResourceLoader.Exists(text, ""))
					{
						_003C_003E2__current = (Source: _003Csource_003E5__5, ScenePath: text, SourceDisplay: _003CsourceDisplay_003E5__6);
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_0273;
				}
				_003C_003E7__wrap9 = null;
				goto IL_029b;
				IL_02ee:
				_003C_003E7__wrap3++;
				goto IL_02fc;
				IL_0082:
				string text2 = Path.GetDirectoryName(location) ?? string.Empty;
				if ((object)assembly != typeof(BackgroundSkinService).Assembly && (text2.Contains("\\mods\\", StringComparison.OrdinalIgnoreCase) || text2.Contains("\\workshop\\content\\", StringComparison.OrdinalIgnoreCase)))
				{
					Type[] source;
					try
					{
						source = assembly.GetTypes();
					}
					catch (ReflectionTypeLoadException ex)
					{
						source = ex.Types.Where((Type type) => type != null).Cast<Type>().ToArray();
					}
					catch
					{
						goto IL_02ee;
					}
					_003Csource_003E5__5 = assembly.GetName().Name ?? "mod";
					_003CsourceDisplay_003E5__6 = Path.GetFileNameWithoutExtension(location);
					_003C_003E7__wrap6 = source.Where((Type type) => type.Name.Contains("Background", StringComparison.OrdinalIgnoreCase)).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_02c3;
				}
				goto IL_02ee;
				IL_02a9:
				if (_003C_003E7__wrap8 < _003C_003E7__wrap7.Length)
				{
					FieldInfo fieldInfo = _003C_003E7__wrap7[_003C_003E7__wrap8];
					if (!(fieldInfo.FieldType != typeof(string[])) && fieldInfo.GetValue(null) is string[] array)
					{
						_003C_003E7__wrap9 = array;
						_003C_003E7__wrap10 = 0;
						goto IL_0281;
					}
					goto IL_029b;
				}
				_003C_003E7__wrap7 = null;
				goto IL_02c3;
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
			if (_003C_003E7__wrap6 != null)
			{
				_003C_003E7__wrap6.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<(string Source, string ScenePath, string SourceDisplay)> IEnumerable<(string, string, string)>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CDiscoverDeclaredSceneBackgrounds_003Ed__91(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<(string, string, string)>)this).GetEnumerator();
		}
	}

	private const string GameSourceId = "game";

	private static readonly object Sync = new object();

	private static readonly string[] BaseActIds = new string[4] { "overgrowth", "underdocks", "hive", "glory" };

	private static readonly FieldInfo? RunStateField = AccessTools.Field(typeof(NRun), "_state") ?? AccessTools.Field(typeof(NRun), "_runState");

	private static readonly PropertyInfo? BackgroundProperty = AccessTools.Property(typeof(NCombatRoom), "Background");

	private static readonly PropertyInfo? HasCustomBackgroundProperty = AccessTools.Property(typeof(EncounterModel), "HasCustomBackground");

	private static readonly HashSet<string> ReportedFailures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static List<BackgroundActEntry>? _acts;

	private static List<BackgroundOption>? _backgrounds;

	private static Texture2D? _fakeMerchantEventImage;

	private static Dictionary<string, string> _providerActDefaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static string? _lastDefaultScenePath;

	private static string? _lastDefaultSource;

	private static string? _lastDefaultActId;

	private static readonly Dictionary<string, string> EnsuredDefaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static readonly string[] BackgroundImageExcludedSegments = new string[14]
	{
		"/animations/", "/events/", "/event/", "merchant", "ancient", "neow", "/ui/", "map_bgs", "/map/", "icon",
		"portrait", "card", "/rest/", "rest_site"
	};

	private const string FakeMerchantEncounterId = "fake_merchant_event_encounter";

	internal const string FakeMerchantEventBackgroundName = "假商人事件背景 / Fake Merchant Event";

	private const string FakeMerchantEventOptionToken = "fake_merchant_event";

	public static IReadOnlyList<BackgroundActEntry> Acts
	{
		get
		{
			lock (Sync)
			{
				return _acts ?? (_acts = SafeDiscoverActs());
			}
		}
	}

	public static string? CurrentActId
	{
		get
		{
			if (TryGetCurrentRunState(out IRunState runState))
			{
				try
				{
					return NormalizeActId(((AbstractModel)runState.Act).Id.Entry);
				}
				catch
				{
					return null;
				}
			}
			return null;
		}
	}

	public static BackgroundActEntry? CurrentAct
	{
		get
		{
			if (!TryGetCurrentRunState(out IRunState runState))
			{
				return null;
			}
			string actId;
			string title;
			try
			{
				actId = NormalizeActId(((AbstractModel)runState.Act).Id.Entry);
				title = ReadActTitle(runState.Act);
			}
			catch
			{
				return null;
			}
			return FindAct(actId) ?? new BackgroundActEntry(actId, title, CreateOptions(Backgrounds));
		}
	}

	public static IReadOnlyList<BackgroundActEntry> AllActs
	{
		get
		{
			List<BackgroundActEntry> list = Acts.ToList();
			BackgroundActEntry current = CurrentAct;
			if (current != null && list.All((BackgroundActEntry entry) => !entry.ActId.Equals(current.ActId, StringComparison.OrdinalIgnoreCase)))
			{
				list.Add(current);
			}
			return list;
		}
	}

	private static IReadOnlyList<BackgroundOption> Backgrounds
	{
		get
		{
			lock (Sync)
			{
				return _backgrounds ?? (_backgrounds = SafeDiscoverBackgrounds());
			}
		}
	}

	public static BackgroundActEntry? FindAct(string? actId)
	{
		if (string.IsNullOrWhiteSpace(actId))
		{
			return null;
		}
		string key = NormalizeActId(actId);
		return Acts.FirstOrDefault((BackgroundActEntry entry) => entry.ActId.Equals(key, StringComparison.OrdinalIgnoreCase));
	}

	public static string GetSelection(string actId)
	{
		string key = NormalizeActId(actId);
		if (!SkinService.Config.BackgroundSelections.TryGetValue(key, out string value) || string.IsNullOrWhiteSpace(value))
		{
			return "__follow__";
		}
		return value;
	}

	public static bool SetSelection(string actId, string optionId)
	{
		string text = NormalizeActId(actId);
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		Dictionary<string, string> backgroundSelections = SkinService.Config.BackgroundSelections;
		if (string.IsNullOrWhiteSpace(optionId) || optionId.Equals("__follow__", StringComparison.OrdinalIgnoreCase) || optionId.Equals("__mod_default__", StringComparison.OrdinalIgnoreCase))
		{
			if (optionId.Equals("__mod_default__", StringComparison.OrdinalIgnoreCase))
			{
				backgroundSelections[text] = optionId.Trim();
			}
			else
			{
				backgroundSelections.Remove(text);
			}
		}
		else
		{
			backgroundSelections[text] = optionId.Trim();
		}
		try
		{
			SkinService.Config.Save(SkinChangerPaths.Configuration("skin_changer.json"));
		}
		catch (Exception ex)
		{
			ModLog.Warn("保存背景选择失败：" + ex.GetBaseException().Message);
		}
		RefreshCurrentActIfNeeded(text);
		return true;
	}

	public static void ApplyToResult(EncounterModel encounter, ActModel act, Rng rng, ref NCombatBackground result)
	{
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string text = NormalizeActId(((AbstractModel)act).Id.Entry);
			string selection = GetSelection(text);
			if (IsFakeMerchantEncounter(encounter))
			{
				if (selection.Equals("__base__", StringComparison.OrdinalIgnoreCase))
				{
					if (!TryUseFakeMerchantVanillaBackground(ref result) && !TryUseFakeMerchantEventImageBackground(ref result))
					{
						ModLog.Info("假商人战斗背景来源=遭遇自带。");
					}
					return;
				}
				if (BackgroundSkinPolicy.IsFollowCategory(selection) || selection.Equals("__mod_default__", StringComparison.OrdinalIgnoreCase))
				{
					if (!TryUseFakeMerchantEventImageBackground(ref result))
					{
						ModLog.Info("假商人战斗背景来源=遭遇自带。");
					}
					return;
				}
			}
			BackgroundOption backgroundOption = null;
			if (!BackgroundSkinPolicy.IsFollowCategory(selection) && !selection.Equals("__mod_default__", StringComparison.OrdinalIgnoreCase))
			{
				backgroundOption = ((!selection.Equals("__base__", StringComparison.OrdinalIgnoreCase)) ? FindBackground(selection) : ResolveGameDefault(encounter, text));
			}
			else
			{
				if (!_providerActDefaults.TryGetValue(text, out string value))
				{
					return;
				}
				BackgroundOption backgroundOption2 = FindBackground(value);
				if ((object)backgroundOption2 == null || backgroundOption2.Source.Equals("game", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
				backgroundOption = backgroundOption2;
			}
			if (backgroundOption == null)
			{
				ReportOnce("resolve:" + text, ((AbstractModel)act).Id.Entry + " 背景选择 " + selection + " 已不存在或对应资源缺失，已保留原背景。");
				return;
			}
			NCombatBackground val = CreateOptionBackground(backgroundOption, rng);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				if (result != null && GodotObject.IsInstanceValid((GodotObject)(object)result) && result != val)
				{
					((GodotObject)result).Free();
				}
				((GodotObject)val).SetMeta(StringName.op_Implicit("sc_background_applied"), Variant.op_Implicit(true));
				result = val;
				ModLog.Info($"已按背景选择创建 {((AbstractModel)act).Id.Entry} 战斗背景：{backgroundOption.DisplayName}。");
			}
		}
		catch (Exception ex)
		{
			ReportOnce("create:" + ((AbstractModel)encounter).Id.Entry, "创建战斗背景失败，已保留原背景：" + ex.GetBaseException().Message);
		}
	}

	public static void ApplyToRoom(NCombatRoom room, IRunState state)
	{
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			ActModel act = state.Act;
			string text = NormalizeActId(((AbstractModel)act).Id.Entry);
			string selection = GetSelection(text);
			if (IsFakeMerchantCombat(room))
			{
				return;
			}
			BackgroundOption backgroundOption = null;
			if (BackgroundSkinPolicy.IsFollowCategory(selection) || selection.Equals("__mod_default__", StringComparison.OrdinalIgnoreCase))
			{
				if (!_providerActDefaults.TryGetValue(text, out string value))
				{
					return;
				}
				BackgroundOption backgroundOption2 = FindBackground(value);
				if ((object)backgroundOption2 == null || backgroundOption2.Source.Equals("game", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
				backgroundOption = backgroundOption2;
			}
			if (room.Background != null && GodotObject.IsInstanceValid((GodotObject)(object)room.Background) && ((GodotObject)room.Background).HasMeta(StringName.op_Implicit("sc_background_applied")))
			{
				return;
			}
			BackgroundOption backgroundOption3 = backgroundOption ?? ((!selection.Equals("__base__", StringComparison.OrdinalIgnoreCase)) ? FindBackground(selection) : (GameBackgroundExists(text) ? GameOption(text) : null));
			if (backgroundOption3 == null)
			{
				return;
			}
			Rng val = CreatePreviewRng();
			if (val == null)
			{
				return;
			}
			NCombatBackground val2 = CreateOptionBackground(backgroundOption3, val);
			if (val2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				return;
			}
			Control nodeOrNull = ((Node)room).GetNodeOrNull<Control>(NodePath.op_Implicit("%BgContainer"));
			NCombatBackground background = room.Background;
			try
			{
				BackgroundProperty?.SetValue(room, null);
			}
			catch
			{
			}
			if (background != null && GodotObject.IsInstanceValid((GodotObject)(object)background))
			{
				if (nodeOrNull != null)
				{
					((Node)nodeOrNull).RemoveChild((Node)(object)background);
				}
				((Node)background).QueueFree();
			}
			((GodotObject)val2).SetMeta(StringName.op_Implicit("sc_background_applied"), Variant.op_Implicit(true));
			try
			{
				BackgroundProperty?.SetValue(room, val2);
			}
			catch
			{
			}
			if (nodeOrNull != null)
			{
				((Node)nodeOrNull).AddChild((Node)(object)val2, false, (InternalMode)0);
			}
			ModLog.Info($"已按背景选择替换 {((AbstractModel)act).Id.Entry} 的房间背景：{backgroundOption3.DisplayName}。");
		}
		catch (Exception ex)
		{
			ReportOnce("room:" + ((AbstractModel)state.Act).Id.Entry, "替换房间背景失败，已保留原背景：" + ex.GetBaseException().Message);
		}
	}

	public static void EnsureActDefaultOption(string actId)
	{
		try
		{
			string token = NormalizeToken(actId);
			if (token.Length == 0)
			{
				return;
			}
			BackgroundOption backgroundOption = Backgrounds.FirstOrDefault(delegate(BackgroundOption option)
			{
				string text = NormalizeToken(option.Title);
				return text.Length > 0 && (text.StartsWith(token, StringComparison.Ordinal) || token.StartsWith(text, StringComparison.Ordinal));
			});
			if (backgroundOption != null)
			{
				EnsuredDefaults[actId] = backgroundOption.Id;
				return;
			}
			ActModel val = EnumerateActs().FirstOrDefault((Func<ActModel, bool>)((ActModel candidate) => NormalizeActId(((AbstractModel)candidate).Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase)));
			Rng val2 = CreatePreviewRng();
			if (val == null || val2 == null)
			{
				return;
			}
			BackgroundAssets val3;
			try
			{
				val3 = val.GenerateBackgroundAssets(val2);
			}
			catch
			{
				return;
			}
			string backgroundScenePath = val3.BackgroundScenePath;
			if (string.IsNullOrWhiteSpace(backgroundScenePath) || !backgroundScenePath.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			string text2 = Path.GetFileNameWithoutExtension(backgroundScenePath);
			if (text2.EndsWith("_background", StringComparison.OrdinalIgnoreCase))
			{
				string text3 = text2;
				int length = "_background".Length;
				text2 = text3.Substring(0, text3.Length - length);
			}
			List<string> list = ListLayersInDirectory(backgroundScenePath.Substring(0, backgroundScenePath.LastIndexOf('/')));
			string text4 = ((object)val).GetType().Assembly.GetName().Name ?? "act";
			string id = BackgroundSkinPolicy.ProviderOptionId(text4, text2);
			if (!(FindBackground(id) != null))
			{
				string text5 = text4 + " · " + BackgroundSkinPolicy.DisplayName(text2);
				BackgroundOption backgroundOption2 = new BackgroundOption(id, text5, text4, (list.Count > 0) ? text2 : backgroundScenePath, (list.Count > 0) ? new BackgroundPackShell(text2, backgroundScenePath, list) : null);
				lock (Sync)
				{
					_backgrounds?.Add(backgroundOption2);
				}
				EnsuredDefaults[actId] = backgroundOption2.Id;
				RefreshActOptions();
				ModLog.Info($"已将 {((AbstractModel)val).Id.Entry} 的默认背景加入可切换选项：{text5}。");
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("同步 " + actId + " 的默认背景选项失败，已跳过：" + ex.GetBaseException().Message);
		}
	}

	private static List<string> ListLayersInDirectory(string directory)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		try
		{
			DirAccess val = DirAccess.Open(directory + "/layers");
			try
			{
				if (val == null)
				{
					return list;
				}
				val.ListDirBegin();
				while (true)
				{
					string next = val.GetNext();
					if (string.IsNullOrEmpty(next))
					{
						break;
					}
					if (!next.StartsWith(".", StringComparison.Ordinal) && !val.CurrentIsDir() && SkinCatalog.IsScenePath(next, ".tscn"))
					{
						list.Add(SkinCatalog.StripRemap(directory + "/layers/" + next));
					}
				}
				val.ListDirEnd();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}

	public static BackgroundOption? ResolvePreviewOption(string actId, string selection)
	{
		if (BackgroundSkinPolicy.IsFollowCategory(selection) || selection.Equals("__mod_default__", StringComparison.OrdinalIgnoreCase))
		{
			return ActDefaultOption(actId);
		}
		if (selection.Equals("__base__", StringComparison.OrdinalIgnoreCase))
		{
			return ActDefaultOption(actId);
		}
		BackgroundOption backgroundOption = FindBackground(selection);
		if (backgroundOption == null)
		{
			BackgroundOption backgroundOption2 = ActDefaultOption(actId);
			if (backgroundOption2 != null)
			{
				ModLog.Info($"背景选择 {selection} 已失效，{actId} 改用默认背景：{backgroundOption2.DisplayName}。");
				return backgroundOption2;
			}
			return null;
		}
		if (backgroundOption.Source.Equals("game", StringComparison.OrdinalIgnoreCase) && !BaseActIds.Contains<string>(actId, StringComparer.OrdinalIgnoreCase))
		{
			BackgroundOption backgroundOption3 = ActDefaultOption(actId);
			if (backgroundOption3 != null)
			{
				return backgroundOption3;
			}
		}
		return backgroundOption;
	}

	public static Control? CreatePreview(BackgroundOption option)
	{
		Rng val = CreatePreviewRng();
		if (val == null)
		{
			ReportOnce("preview-rng", "无法创建背景预览所需的随机数生成器。");
			return null;
		}
		return (Control?)(object)CreateOptionBackground(option, val);
	}

	private static bool TryProbeBackground(BackgroundOption option)
	{
		try
		{
			Rng val = CreatePreviewRng();
			if (val == null)
			{
				return false;
			}
			NCombatBackground val2 = CreateOptionBackground(option, val);
			bool result = val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2) && HasLayerContent((Node)(object)val2);
			if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				((GodotObject)val2).Free();
			}
			return result;
		}
		catch (Exception ex)
		{
			ReportOnce("probe:" + option.Id, option.DisplayName + " 背景自检失败，已从列表移除：" + ex.GetBaseException().Message);
			return false;
		}
	}

	private static NCombatBackground? CreateOptionBackground(BackgroundOption option, Rng rng)
	{
		if (IsActStubTitle(option.Title))
		{
			BackgroundOption backgroundOption = FindBossSibling(option);
			if (backgroundOption != null)
			{
				option = backgroundOption;
			}
		}
		if (option.Source.Equals("game", StringComparison.OrdinalIgnoreCase))
		{
			return CreateGameBackground(option.Title, rng);
		}
		if (option.Pack != null)
		{
			return CreatePackBackground(option, rng);
		}
		if (!option.Title.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase))
		{
			return CreateImageBackground(option);
		}
		return CreateSceneBackground(option);
	}

	private static NCombatBackground? CreateImageBackground(BackgroundOption option)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		try
		{
			Texture2D val = (Texture2D)("res://images/events/fake_merchant.png".Equals(option.Title, StringComparison.OrdinalIgnoreCase) ? ((object)LoadFakeMerchantEventImage()) : ((object)PreloadManager.Cache.GetTexture2D(option.Title)));
			if (val == null)
			{
				ReportOnce("image:" + option.Id, option.DisplayName + " 的图片尚未挂载。");
				return null;
			}
			NCombatBackground val2 = new NCombatBackground
			{
				Name = StringName.op_Implicit("SkinChangerImageBackground"),
				MouseFilter = (MouseFilterEnum)2
			};
			TextureRect val3 = new TextureRect
			{
				Name = StringName.op_Implicit("BackgroundImage"),
				Texture = val,
				ExpandMode = (ExpandModeEnum)1,
				StretchMode = (StretchModeEnum)6,
				MouseFilter = (MouseFilterEnum)2,
				Position = new Vector2(-960f, -540f),
				Size = new Vector2(1920f, 1080f)
			};
			((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
			return val2;
		}
		catch (Exception ex)
		{
			ReportOnce("image:" + option.Id, option.DisplayName + " 背景图片创建失败：" + ex.GetBaseException().Message);
			return null;
		}
	}

	private static Texture2D? LoadFakeMerchantEventImage()
	{
		if (_fakeMerchantEventImage != null && GodotObject.IsInstanceValid((GodotObject)(object)_fakeMerchantEventImage))
		{
			return _fakeMerchantEventImage;
		}
		SkinService.EnsureFakeMerchantEventArtMounted();
		_fakeMerchantEventImage = ResourceLoader.Load<Texture2D>("res://images/events/fake_merchant.png", (string)null, (CacheMode)2) ?? PreloadManager.Cache.GetTexture2D("res://images/events/fake_merchant.png");
		return _fakeMerchantEventImage;
	}

	public static BackgroundOption? ResolveActDefaultFallback(string actId)
	{
		EnsureActDefaultOption(actId);
		string text = NormalizeToken(actId);
		if (text.Length == 0)
		{
			return null;
		}
		string targetBase = BaseTokenOf(text);
		BackgroundOption backgroundOption = Backgrounds.FirstOrDefault(delegate(BackgroundOption option)
		{
			string text2 = NormalizeToken(Path.GetFileNameWithoutExtension(option.Title));
			return text2.Length >= 6 && (text2.StartsWith(targetBase, StringComparison.Ordinal) || targetBase.StartsWith(text2, StringComparison.Ordinal));
		});
		if (backgroundOption != null)
		{
			return backgroundOption;
		}
		BackgroundOption result = null;
		double num = 0.0;
		foreach (BackgroundOption background in Backgrounds)
		{
			if (background.Source.Equals("game", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			string text3 = NormalizeToken(Path.GetFileNameWithoutExtension(background.Title));
			if (text3.Length != 0)
			{
				double num2 = Similarity(text, text3);
				if (num2 > num)
				{
					num = num2;
					result = background;
				}
			}
		}
		if (!(num >= 0.4))
		{
			return null;
		}
		return result;
	}

	private static BackgroundOption? ResolveEncounterImageDefault(ActModel act)
	{
		string[] array;
		try
		{
			array = (from encounter in act.AllEncounters.Take(16)
				select NormalizeToken(((AbstractModel)encounter).Id.Entry) into token
				where token.Length >= 6
				select token).ToArray();
		}
		catch
		{
			return null;
		}
		if (array.Length == 0)
		{
			return null;
		}
		BackgroundOption result = null;
		int num = 0;
		foreach (BackgroundOption background in Backgrounds)
		{
			if (background.Pack != null || !IsBackgroundImagePath(background.Title))
			{
				continue;
			}
			string text = NormalizeToken(background.Title);
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (text2.Length > num && text.Contains(text2, StringComparison.Ordinal))
				{
					result = background;
					num = text2.Length;
				}
			}
		}
		return result;
	}

	private static bool IsVanillaActBackground(string? scenePath, string actId)
	{
		if (string.IsNullOrWhiteSpace(scenePath))
		{
			return false;
		}
		string text = scenePath.Replace('\\', '/');
		string[] baseActIds = BaseActIds;
		foreach (string value in baseActIds)
		{
			if (!actId.Equals(value, StringComparison.OrdinalIgnoreCase) && text.Contains($"/scenes/backgrounds/{value}/{value}_background", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private static NCombatBackground? CreateRepresentativeEncounterBackground(ActModel act, Rng rng)
	{
		try
		{
			foreach (EncounterModel item in act.AllEncounters.Take(10))
			{
				if (item == null)
				{
					continue;
				}
				EncounterModel val;
				try
				{
					val = (((AbstractModel)item).IsMutable ? item : item.ToMutable());
				}
				catch
				{
					val = item;
				}
				try
				{
					NCombatBackground val2 = val.CreateBackground(act, rng);
					if (val2 != null && HasLayerContent((Node)(object)val2))
					{
						return val2;
					}
					if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
					{
						((Node)val2).QueueFree();
					}
				}
				catch
				{
				}
			}
		}
		catch (Exception ex)
		{
			ReportOnce("representative:" + ((AbstractModel)act).Id.Entry, "无法获取 " + ((AbstractModel)act).Id.Entry + " 的代表性战斗背景：" + ex.GetBaseException().Message);
		}
		return null;
	}

	private static bool HasVisibleLayerContent(Node background)
	{
		bool flag = false;
		foreach (Node child in background.GetChildren(false))
		{
			string text = ((object)child.Name).ToString();
			if (text.StartsWith("Layer_", StringComparison.OrdinalIgnoreCase) || text.Equals("Foreground", StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
				if (child.GetChildCount(false) > 0)
				{
					return true;
				}
			}
		}
		if (!flag)
		{
			return background.GetChildCount(false) > 0;
		}
		return false;
	}

	private static bool IsActStubTitle(string title)
	{
		string text = NormalizeToken(Path.GetFileNameWithoutExtension(title));
		if (!text.EndsWith("act", StringComparison.Ordinal))
		{
			return text.EndsWith("stage", StringComparison.Ordinal);
		}
		return true;
	}

	private static BackgroundOption? FindBossSibling(BackgroundOption option)
	{
		string token = NormalizeToken(Path.GetFileNameWithoutExtension(option.Title));
		string baseToken = BaseTokenOf(token);
		if (baseToken.Length < 6)
		{
			return null;
		}
		return Backgrounds.FirstOrDefault((BackgroundOption candidate) => !candidate.Id.Equals(option.Id, StringComparison.OrdinalIgnoreCase) && candidate.Source.Equals(option.Source, StringComparison.OrdinalIgnoreCase) && NormalizeToken(Path.GetFileNameWithoutExtension(candidate.Title)).StartsWith(baseToken, StringComparison.Ordinal) && (NormalizeToken(candidate.Title).Contains("boss", StringComparison.Ordinal) || NormalizeToken(candidate.Title).Contains("encounter", StringComparison.Ordinal)));
	}

	private static bool HasLayerContent(Node background)
	{
		foreach (Node child in background.GetChildren(false))
		{
			string text = ((object)child.Name).ToString();
			if (text.StartsWith("Layer_", StringComparison.OrdinalIgnoreCase) || text.Equals("Foreground", StringComparison.OrdinalIgnoreCase) || text.Equals("BattlefieldCanvas", StringComparison.OrdinalIgnoreCase))
			{
				if (child.GetChildCount(false) > 0)
				{
					return true;
				}
				continue;
			}
			return true;
		}
		return false;
	}

	private static string? ReadCustomBackgroundPath(object encounter)
	{
		string[] array = new string[3] { "CustomBackgroundScenePath", "CustomBackgroundPath", "BackgroundScenePath" };
		foreach (string name in array)
		{
			PropertyInfo property = encounter.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null || property.PropertyType != typeof(string) || property.GetIndexParameters().Length != 0)
			{
				continue;
			}
			try
			{
				if (property.GetValue(encounter) is string { Length: >0 } text)
				{
					return text;
				}
			}
			catch
			{
			}
		}
		return null;
	}

	private static BackgroundAssets? PrepCustomBackgroundAssets(EncounterModel encounter, ActModel act)
	{
		try
		{
			Rng val = CreatePreviewRng();
			if (val == null)
			{
				return null;
			}
			EncounterModel val2 = (EncounterModel)(((AbstractModel)encounter).IsMutable ? ((object)encounter) : ((object)encounter.ToMutable()));
			MethodInfo method = ((object)val2).GetType().GetMethod("PrepCustomBackground", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method == null)
			{
				return null;
			}
			method.Invoke(val2, new object[2] { act, val });
			Type type = ((object)val2).GetType();
			while (type != null)
			{
				object? obj = type.GetField("_customBackgroundAssets", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(val2);
				BackgroundAssets val3 = (BackgroundAssets)((obj is BackgroundAssets) ? obj : null);
				if (val3 != null && !string.IsNullOrWhiteSpace(val3.BackgroundScenePath))
				{
					return val3;
				}
				type = type.BaseType;
			}
		}
		catch
		{
		}
		return null;
	}

	private static BackgroundOption? RegisterPreparedOption(string actId, ActModel act, BackgroundAssets assets)
	{
		try
		{
			string text = assets.BackgroundScenePath.Replace('\\', '/');
			List<string> list = new List<string>(assets.BgLayers.Select((string path) => path.Replace('\\', '/')));
			if (!string.IsNullOrWhiteSpace(assets.FgLayer))
			{
				list.Add(assets.FgLayer.Replace('\\', '/'));
			}
			list = list.Where((string path) => !string.IsNullOrWhiteSpace(path)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
			if (list.Count == 0)
			{
				return null;
			}
			string text2 = Path.GetFileNameWithoutExtension(text);
			if (text2.EndsWith("_bg", StringComparison.OrdinalIgnoreCase))
			{
				string text3 = text2;
				int length = "_bg".Length;
				text2 = text3.Substring(0, text3.Length - length);
			}
			string text4 = ((object)act).GetType().Assembly.GetName().Name ?? "act-default";
			string id = BackgroundSkinPolicy.ProviderOptionId(text4, "prepared:" + text2);
			BackgroundOption backgroundOption = FindBackground(id);
			if (backgroundOption != null)
			{
				return backgroundOption;
			}
			BackgroundOption backgroundOption2 = new BackgroundOption(id, text4 + " · " + BackgroundSkinPolicy.DisplayName(text2), text4, text2, new BackgroundPackShell(text2, text, list));
			lock (Sync)
			{
				_backgrounds?.Add(backgroundOption2);
			}
			EnsuredDefaults[actId] = backgroundOption2.Id;
			RefreshActOptions();
			ModLog.Info($"已按 BaseLib 自定义遭遇注册 {((AbstractModel)act).Id.Entry} 的默认背景：{backgroundOption2.DisplayName}（图层 {list.Count}：" + string.Join(" | ", list.Take(4)) + "）。");
			return backgroundOption2;
		}
		catch
		{
			return null;
		}
	}

	private static BackgroundOption? RegisterScenePathOption(string actId, ActModel act, string scenePath)
	{
		try
		{
			scenePath = scenePath.Replace('\\', '/');
			string text = ((object)act).GetType().Assembly.GetName().Name ?? "act-default";
			string id = BackgroundSkinPolicy.ProviderOptionId(text, scenePath);
			BackgroundOption backgroundOption = FindBackground(id);
			if (backgroundOption != null)
			{
				return backgroundOption;
			}
			string text2 = Path.GetFileNameWithoutExtension(scenePath);
			if (text2.EndsWith("_bg", StringComparison.OrdinalIgnoreCase))
			{
				string text3 = text2;
				int length = "_bg".Length;
				text2 = text3.Substring(0, text3.Length - length);
			}
			BackgroundOption backgroundOption2 = new BackgroundOption(id, text + " · " + BackgroundSkinPolicy.DisplayName(text2), text, scenePath, null);
			lock (Sync)
			{
				_backgrounds?.Add(backgroundOption2);
			}
			EnsuredDefaults[actId] = backgroundOption2.Id;
			RefreshActOptions();
			ModLog.Info($"已按 BaseLib 遭遇场景登记 {((AbstractModel)act).Id.Entry} 的背景：{backgroundOption2.DisplayName}（{scenePath}）。");
			return backgroundOption2;
		}
		catch
		{
			return null;
		}
	}

	private static BackgroundOption? RegisterImageOption(string actId, ActModel act, string imagePath)
	{
		try
		{
			imagePath = imagePath.Replace('\\', '/');
			string text = ((object)act).GetType().Assembly.GetName().Name ?? "act-default";
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
			string id = BackgroundSkinPolicy.ProviderOptionId(text, imagePath);
			BackgroundOption backgroundOption = FindBackground(id);
			if (backgroundOption != null)
			{
				return backgroundOption;
			}
			BackgroundOption backgroundOption2 = new BackgroundOption(id, text + " · " + BackgroundSkinPolicy.DisplayName(fileNameWithoutExtension), text, imagePath, null);
			lock (Sync)
			{
				_backgrounds?.Add(backgroundOption2);
			}
			EnsuredDefaults[actId] = backgroundOption2.Id;
			RefreshActOptions();
			ModLog.Info($"已按 BaseLib 遭遇图层登记 {((AbstractModel)act).Id.Entry} 的背景：{backgroundOption2.DisplayName}。");
			return backgroundOption2;
		}
		catch
		{
			return null;
		}
	}

	private static string DescribeNode(Node node, int depth)
	{
		if (depth > 4)
		{
			return "...";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(StringName.op_Implicit(node.Name)).Append('(').Append(((object)node).GetType().Name)
			.Append(')');
		foreach (Node child in node.GetChildren(false))
		{
			stringBuilder.Append(" [").Append(DescribeNode(child, depth + 1)).Append(']');
		}
		return stringBuilder.ToString();
	}

	private static string SceneStem(string path)
	{
		string text = NormalizeToken(Path.GetFileNameWithoutExtension(path));
		if (text.EndsWith("bg", StringComparison.Ordinal) && text.Length > 2)
		{
			string text2 = text;
			text = text2.Substring(0, text2.Length - 2);
		}
		return text;
	}

	private static BackgroundOption? FindImageOptionForScene(string scenePath)
	{
		string stem = SceneStem(scenePath);
		if (stem.Length == 0)
		{
			return null;
		}
		return Backgrounds.FirstOrDefault((BackgroundOption option) => IsBackgroundImagePath(option.Title) && SceneStem(option.Title).Equals(stem, StringComparison.OrdinalIgnoreCase));
	}

	private static string BaseTokenOf(string token)
	{
		string[] array = new string[2] { "act", "stage" };
		foreach (string text in array)
		{
			if (token.Length > text.Length + 4 && token.EndsWith(text, StringComparison.Ordinal))
			{
				int length = text.Length;
				return token.Substring(0, token.Length - length);
			}
		}
		return token;
	}

	private static string NormalizeToken(string value)
	{
		return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}

	private static double Similarity(string left, string right)
	{
		if (left.Length == 0 || right.Length == 0)
		{
			return 0.0;
		}
		if (left.Contains(right, StringComparison.Ordinal) || right.Contains(left, StringComparison.Ordinal))
		{
			return 1.0;
		}
		int num = Levenshtein(left, right);
		return 1.0 - (double)num / (double)Math.Max(left.Length, right.Length);
	}

	private static int Levenshtein(string left, string right)
	{
		int[] array = new int[right.Length + 1];
		int[] array2 = new int[right.Length + 1];
		for (int i = 0; i <= right.Length; i++)
		{
			array[i] = i;
		}
		for (int j = 1; j <= left.Length; j++)
		{
			array2[0] = j;
			for (int k = 1; k <= right.Length; k++)
			{
				int num = ((left[j - 1] != right[k - 1]) ? 1 : 0);
				array2[k] = Math.Min(Math.Min(array2[k - 1] + 1, array[k] + 1), array[k - 1] + num);
			}
			int[] array3 = array2;
			array2 = array;
			array = array3;
		}
		return array[right.Length];
	}

	private static void RefreshActOptions()
	{
		lock (Sync)
		{
			if (_acts == null)
			{
				return;
			}
			foreach (BackgroundActEntry act in _acts)
			{
				act.ReplaceOptions(CreateOptions(Backgrounds));
			}
		}
	}

	public static bool RegisterLastDefaultAsOption()
	{
		string path = _lastDefaultScenePath;
		string source = _lastDefaultSource;
		bool flag = path?.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) ?? false;
		bool flag2 = path != null && IsBackgroundImagePath(path);
		if (string.IsNullOrWhiteSpace(path) || (!flag && !flag2))
		{
			return false;
		}
		try
		{
			string title = Path.GetFileNameWithoutExtension(path);
			if (flag && title.EndsWith("_background", StringComparison.OrdinalIgnoreCase))
			{
				string text = title;
				int length = "_background".Length;
				title = text.Substring(0, text.Length - length);
			}
			if (Backgrounds.Any((BackgroundOption option) => option.Title.Equals(path, StringComparison.OrdinalIgnoreCase) || (NormalizeToken(option.Title) == NormalizeToken(title) && option.Source.Equals(source, StringComparison.OrdinalIgnoreCase))))
			{
				return false;
			}
			List<string> list = ListLayersInDirectory(path.Substring(0, path.LastIndexOf('/')));
			string text2 = (string.IsNullOrWhiteSpace(source) ? "act-default" : source);
			BackgroundOption backgroundOption = new BackgroundOption(BackgroundSkinPolicy.ProviderOptionId(text2, title), text2 + " · " + BackgroundSkinPolicy.DisplayName(title), text2, (list.Count > 0) ? title : path, (list.Count > 0) ? new BackgroundPackShell(title, path, list) : null);
			lock (Sync)
			{
				_backgrounds?.Add(backgroundOption);
			}
			if (!string.IsNullOrWhiteSpace(_lastDefaultActId))
			{
				EnsuredDefaults[_lastDefaultActId] = backgroundOption.Id;
			}
			RefreshActOptions();
			ModLog.Info($"已将默认背景加入可切换选项：{backgroundOption.DisplayName}（{path}）。");
			return true;
		}
		catch (Exception ex)
		{
			ModLog.Warn("登记默认背景选项失败：" + ex.GetBaseException().Message);
			return false;
		}
	}

	public static Control? CreateActDefaultPreview(string actId)
	{
		_lastDefaultScenePath = null;
		_lastDefaultSource = null;
		_lastDefaultActId = actId;
		try
		{
			ActModel val = EnumerateActs().FirstOrDefault((Func<ActModel, bool>)((ActModel candidate) => NormalizeActId(((AbstractModel)candidate).Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase)));
			if (val == null)
			{
				return null;
			}
			Rng val2 = CreatePreviewRng();
			if (val2 == null)
			{
				return null;
			}
			BackgroundAssets val3 = val.GenerateBackgroundAssets(val2);
			_lastDefaultScenePath = val3.BackgroundScenePath;
			_lastDefaultSource = ((object)val).GetType().Assembly.GetName().Name;
			NCombatBackground val4 = NCombatBackground.Create(val3);
			bool flag = IsVanillaActBackground(val3.BackgroundScenePath, actId);
			if (val4 == null || !HasLayerContent((Node)(object)val4) || flag)
			{
				BackgroundOption backgroundOption = ResolveEncounterImageDefault(val);
				if (backgroundOption != null)
				{
					NCombatBackground val5 = CreateImageBackground(backgroundOption);
					if (val5 != null)
					{
						_lastDefaultScenePath = backgroundOption.Title;
						_lastDefaultSource = backgroundOption.Source;
						if (val4 != null && GodotObject.IsInstanceValid((GodotObject)(object)val4))
						{
							((Node)val4).QueueFree();
						}
						return (Control?)(object)val5;
					}
				}
				NCombatBackground val6 = CreateRepresentativeEncounterBackground(val, val2);
				if (val6 != null)
				{
					if (val4 != null && GodotObject.IsInstanceValid((GodotObject)(object)val4))
					{
						((Node)val4).QueueFree();
					}
					return (Control?)(object)val6;
				}
			}
			if (val4 != null && (!HasLayerContent((Node)(object)val4) || flag))
			{
				((Node)val4).QueueFree();
				return null;
			}
			return (Control?)(object)val4;
		}
		catch (Exception ex)
		{
			ReportOnce("act-default:" + actId, "无法创建 " + actId + " 的默认背景预览：" + ex.GetBaseException().Message);
			return null;
		}
	}

	private static NCombatBackground? CreateSceneBackground(BackgroundOption option, bool allowSiblingFallback = true)
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		try
		{
			PackedScene scene = PreloadManager.Cache.GetScene(option.Title.Replace('\\', '/'));
			if (scene == null)
			{
				ReportOnce("scene:" + option.Id, option.DisplayName + " 的背景场景尚未挂载，已保留原背景。");
				return null;
			}
			Node val = scene.Instantiate((GenEditState)0);
			Control val2 = (Control)(object)((val is Control) ? val : null);
			if (val2 == null)
			{
				Node2D val3 = (Node2D)(object)((val is Node2D) ? val : null);
				if (val3 != null)
				{
					Control val4 = new Control
					{
						Name = StringName.op_Implicit("SkinChangerSceneHost"),
						MouseFilter = (MouseFilterEnum)2,
						Position = new Vector2(-960f, -540f),
						Size = new Vector2(1920f, 1080f)
					};
					((Node)val4).AddChild((Node)(object)val3, false, (InternalMode)0);
					NCombatBackground val5 = new NCombatBackground
					{
						Name = StringName.op_Implicit("SkinChangerSceneBackground"),
						MouseFilter = (MouseFilterEnum)2
					};
					((Node)val5).AddChild((Node)(object)val4, false, (InternalMode)0);
					return val5;
				}
				if (GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					val.QueueFree();
				}
				return null;
			}
			NCombatBackground val6 = (NCombatBackground)(object)((val2 is NCombatBackground) ? val2 : null);
			if (val6 != null)
			{
				return val6;
			}
			NCombatBackground val7 = new NCombatBackground
			{
				Name = StringName.op_Implicit("SkinChangerSceneBackground"),
				MouseFilter = (MouseFilterEnum)2
			};
			while (((Node)val2).GetChildCount(false) > 0)
			{
				Node child = ((Node)val2).GetChild(0, false);
				((Node)val2).RemoveChild(child);
				((Node)val7).AddChild(child, false, (InternalMode)0);
			}
			((Node)val2).QueueFree();
			if (allowSiblingFallback && !HasVisibleLayerContent((Node)(object)val7))
			{
				BackgroundOption backgroundOption = FindBossSibling(option);
				if (backgroundOption != null)
				{
					((Node)val7).QueueFree();
					return CreateSceneBackground(backgroundOption, allowSiblingFallback: false);
				}
			}
			return val7;
		}
		catch (Exception ex)
		{
			ReportOnce("scene:" + option.Id, option.DisplayName + " 背景创建失败，已保留原背景：" + ex.GetBaseException().Message);
			return null;
		}
	}

	private static Rng? CreatePreviewRng()
	{
		Type typeFromHandle = typeof(Rng);
		ConstructorInfo[] constructors = typeFromHandle.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (ConstructorInfo constructorInfo in constructors)
		{
			ParameterInfo[] parameters = constructorInfo.GetParameters();
			if (parameters.Length != 1)
			{
				continue;
			}
			Type parameterType = parameters[0].ParameterType;
			object obj = ((parameterType == typeof(ulong)) ? ((object)20260814uL) : ((parameterType == typeof(uint)) ? ((object)20260814u) : ((parameterType == typeof(long)) ? ((object)20260814L) : ((parameterType == typeof(int)) ? ((object)20260814) : null))));
			if (obj != null)
			{
				try
				{
					object obj2 = constructorInfo.Invoke(new object[1] { obj });
					return (Rng?)((obj2 is Rng) ? obj2 : null);
				}
				catch
				{
				}
			}
		}
		object? obj4 = typeFromHandle.GetProperty("Chaotic", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
		return (Rng?)((obj4 is Rng) ? obj4 : null);
	}

	public static BackgroundOption? DefaultOptionFor(string actId)
	{
		return ActDefaultOption(actId);
	}

	public static void RunSelfCheck()
	{
		try
		{
			foreach (BackgroundActEntry act in Acts)
			{
				if (BaseActIds.Contains<string>(act.ActId, StringComparer.OrdinalIgnoreCase))
				{
					continue;
				}
				BackgroundOption backgroundOption = DefaultOptionFor(act.ActId);
				if (backgroundOption == null)
				{
					ModLog.Warn("背景自检失败 " + act.ActId + "：没有解析到默认背景。");
					continue;
				}
				Control val = CreatePreview(backgroundOption);
				bool flag = val != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && HasLayerContent((Node)(object)val);
				ModLog.Info($"背景自检 {act.ActId}：{(flag ? "通过" : "空白")} → {backgroundOption.Source} · {backgroundOption.Title}");
				if (!flag && val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					ModLog.Warn("背景自检节点 " + act.ActId + "：" + DescribeNode((Node)(object)val, 0));
				}
				if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					((Node)val).QueueFree();
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("背景自检异常：" + ex.GetBaseException().Message);
		}
	}

	private static BackgroundOption? ActDefaultOption(string actId)
	{
		if (_providerActDefaults.TryGetValue(actId, out string value))
		{
			BackgroundOption backgroundOption = FindBackground(value);
			if ((object)backgroundOption != null)
			{
				return backgroundOption;
			}
		}
		if (EnsuredDefaults.TryGetValue(actId, out string value2))
		{
			BackgroundOption backgroundOption2 = FindBackground(value2);
			if ((object)backgroundOption2 != null)
			{
				return backgroundOption2;
			}
		}
		if (BaseActIds.Contains<string>(actId, StringComparer.OrdinalIgnoreCase) && GameBackgroundExists(actId))
		{
			return FindBackground(BackgroundSkinPolicy.ProviderOptionId("game", actId));
		}
		string text = NormalizeToken(actId);
		string actBaseToken = BaseTokenOf(text);
		try
		{
			ActModel val = EnumerateActs().FirstOrDefault((Func<ActModel, bool>)((ActModel candidate) => NormalizeActId(((AbstractModel)candidate).Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase)));
			if (val != null)
			{
				foreach (EncounterModel item in val.AllEncounters.Concat(val.BossDiscoveryOrder).Take(24))
				{
					BackgroundAssets val2 = PrepCustomBackgroundAssets(item, val);
					if (val2 != null)
					{
						string text2 = new string[1] { val2.FgLayer }.Concat(val2.BgLayers).FirstOrDefault((string candidate) => !string.IsNullOrWhiteSpace(candidate) && candidate.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase));
						if (text2 != null)
						{
							BackgroundOption backgroundOption3 = FindImageOptionForScene(text2);
							if (backgroundOption3 != null)
							{
								return backgroundOption3;
							}
						}
						string text3 = val2.BgLayers.FirstOrDefault(IsBackgroundImagePath);
						if (text3 != null)
						{
							BackgroundOption backgroundOption4 = RegisterImageOption(actId, val, text3);
							if (backgroundOption4 != null)
							{
								return backgroundOption4;
							}
						}
						BackgroundOption backgroundOption5 = FindImageOptionForScene(val2.BackgroundScenePath);
						if (backgroundOption5 != null)
						{
							return backgroundOption5;
						}
						BackgroundOption backgroundOption6 = RegisterPreparedOption(actId, val, val2);
						if (backgroundOption6 != null)
						{
							return backgroundOption6;
						}
					}
					string path = ReadCustomBackgroundPath(item);
					if (!string.IsNullOrWhiteSpace(path))
					{
						BackgroundOption backgroundOption7 = FindImageOptionForScene(path);
						if (backgroundOption7 != null)
						{
							return backgroundOption7;
						}
					}
					if (!string.IsNullOrWhiteSpace(path) && !path.Contains("/images/background/", StringComparison.OrdinalIgnoreCase))
					{
						BackgroundOption backgroundOption8 = Backgrounds.FirstOrDefault((BackgroundOption option) => option.Title.Equals(path, StringComparison.OrdinalIgnoreCase));
						if (backgroundOption8 != null)
						{
							return backgroundOption8;
						}
					}
				}
			}
		}
		catch
		{
		}
		try
		{
			ActModel val3 = EnumerateActs().FirstOrDefault((Func<ActModel, bool>)((ActModel candidate) => NormalizeActId(((AbstractModel)candidate).Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase)));
			if (val3 != null)
			{
				string[] array = (from encounter in val3.AllEncounters.Concat(val3.BossDiscoveryOrder).Take(32)
					select NormalizeToken(((AbstractModel)encounter).Id.Entry) into candidate
					where candidate.Length >= 6 && (candidate.StartsWith(actBaseToken, StringComparison.Ordinal) || actBaseToken.StartsWith(candidate, StringComparison.Ordinal))
					select candidate).Distinct<string>(StringComparer.Ordinal).ToArray();
				BackgroundOption backgroundOption9 = null;
				int num = 0;
				foreach (BackgroundOption background in Backgrounds)
				{
					string text4 = NormalizeToken(background.Title);
					string[] array2 = array;
					foreach (string text5 in array2)
					{
						if (text5.Length > num && text4.Contains(text5, StringComparison.Ordinal))
						{
							backgroundOption9 = background;
							num = text5.Length;
						}
					}
				}
				if (backgroundOption9 != null)
				{
					return backgroundOption9;
				}
			}
		}
		catch
		{
		}
		string text6 = text;
		if (text6.Length < 6)
		{
			return null;
		}
		BackgroundOption backgroundOption10 = null;
		int num2 = 0;
		foreach (BackgroundOption background2 in Backgrounds)
		{
			string text7 = NormalizeToken(background2.Title);
			if (text7.Length >= 6 && (text7.StartsWith(text6, StringComparison.Ordinal) || text6.StartsWith(text7, StringComparison.Ordinal)) && text7.Length > num2)
			{
				backgroundOption10 = background2;
				num2 = text7.Length;
			}
		}
		string text8 = actBaseToken;
		string text9 = ((backgroundOption10 == null) ? "" : NormalizeToken(backgroundOption10.Title));
		bool flag = backgroundOption10 == null || (text9.StartsWith(text8, StringComparison.Ordinal) && (text9.EndsWith("act", StringComparison.Ordinal) || text9.EndsWith("stage", StringComparison.Ordinal)));
		if (text8.Length >= 6 && flag)
		{
			BackgroundOption backgroundOption11 = null;
			int num3 = 0;
			foreach (BackgroundOption background3 in Backgrounds)
			{
				string text10 = NormalizeToken(background3.Title);
				if (text10.Length > num3 && text10.StartsWith(text8, StringComparison.Ordinal) && (text10.Contains("boss", StringComparison.Ordinal) || text10.Contains("encounter", StringComparison.Ordinal)))
				{
					backgroundOption11 = background3;
					num3 = text10.Length;
				}
			}
			if (backgroundOption11 != null)
			{
				backgroundOption10 = backgroundOption11;
			}
			else
			{
				string value3 = null;
				try
				{
					value3 = ((object)EnumerateActs().FirstOrDefault((Func<ActModel, bool>)((ActModel candidate) => NormalizeActId(((AbstractModel)candidate).Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase))))?.GetType().Assembly.GetName().Name;
				}
				catch
				{
				}
				if (!string.IsNullOrWhiteSpace(value3))
				{
					BackgroundOption backgroundOption12 = null;
					int num4 = 0;
					BackgroundOption backgroundOption13 = null;
					int num5 = 0;
					foreach (BackgroundOption background4 in Backgrounds)
					{
						if (background4.Source.Equals(value3, StringComparison.OrdinalIgnoreCase))
						{
							string text11 = NormalizeToken(background4.Title);
							if (text11.StartsWith(text8, StringComparison.Ordinal) && text11.Length > num4)
							{
								backgroundOption12 = background4;
								num4 = text11.Length;
							}
							if (text11.Length > num5)
							{
								backgroundOption13 = background4;
								num5 = text11.Length;
							}
						}
					}
					backgroundOption10 = backgroundOption12 ?? backgroundOption13;
				}
			}
		}
		if (backgroundOption10 == null)
		{
			try
			{
				string assembly = ((object)EnumerateActs().FirstOrDefault((Func<ActModel, bool>)((ActModel candidate) => NormalizeActId(((AbstractModel)candidate).Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase))))?.GetType().Assembly.GetName().Name;
				if (!string.IsNullOrWhiteSpace(assembly))
				{
					backgroundOption10 = Backgrounds.FirstOrDefault((BackgroundOption option) => option.Source.Equals(assembly, StringComparison.OrdinalIgnoreCase));
				}
			}
			catch
			{
			}
		}
		ModLog.Info("地区默认背景 " + actId + " → " + ((backgroundOption10 == null) ? "未找到" : (backgroundOption10.Source + " · " + backgroundOption10.Title)));
		return backgroundOption10;
	}

	private static BackgroundOption? ResolveGameDefault(EncounterModel encounter, string actId)
	{
		try
		{
			object obj = HasCustomBackgroundProperty?.GetValue(encounter);
			if (obj is bool && (bool)obj)
			{
				string title = ((AbstractModel)encounter).Id.Entry.ToLowerInvariant();
				if (GameBackgroundExists(title))
				{
					return GameOption(title);
				}
			}
		}
		catch
		{
		}
		if (!GameBackgroundExists(actId))
		{
			return null;
		}
		return GameOption(actId);
	}

	private static BackgroundOption GameOption(string title)
	{
		return FindBackground(BackgroundSkinPolicy.ProviderOptionId("game", title)) ?? new BackgroundOption(BackgroundSkinPolicy.ProviderOptionId("game", title), BackgroundSkinPolicy.DisplayName(title), "game", title, null);
	}

	private static NCombatBackground? CreateGameBackground(string title, Rng rng)
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		try
		{
			string text = $"res://scenes/backgrounds/{title}/{title}_background.tscn";
			if (ResourceLoader.Exists(text, ""))
			{
				PackedScene scene = PreloadManager.Cache.GetScene(text);
				if (scene != null)
				{
					Control val = scene.Instantiate<Control>((GenEditState)0);
					List<string> layerPaths = ListLayerPaths(title);
					NCombatBackground val2 = (NCombatBackground)(object)((val is NCombatBackground) ? val : null);
					if (val2 != null)
					{
						FillLayers(val, (Node)(object)val, layerPaths, rng);
						return val2;
					}
					FillLayers(val, (Node)(((object)((Node)val).GetNodeOrNull<Node>(NodePath.op_Implicit("BattlefieldCanvas"))) ?? ((object)val)), layerPaths, rng);
					if (val.Size == Vector2.Zero)
					{
						val.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
					}
					NCombatBackground val3 = new NCombatBackground
					{
						Name = StringName.op_Implicit("SkinChangerGameBackground"),
						MouseFilter = (MouseFilterEnum)2
					};
					((Node)val3).AddChild((Node)(object)val, false, (InternalMode)0);
					return val3;
				}
			}
			return NCombatBackground.Create(new BackgroundAssets(title, rng));
		}
		catch (Exception ex)
		{
			ReportOnce("game:" + title, "游戏背景 " + title + " 无法创建，已保留原背景：" + ex.GetBaseException().Message);
			return null;
		}
	}

	private static List<string> ListLayerPaths(string title)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		try
		{
			DirAccess val = DirAccess.Open("res://scenes/backgrounds/" + title + "/layers");
			try
			{
				if (val == null)
				{
					return list;
				}
				val.ListDirBegin();
				while (true)
				{
					string next = val.GetNext();
					if (string.IsNullOrEmpty(next))
					{
						break;
					}
					if (!next.StartsWith(".", StringComparison.Ordinal) && !val.CurrentIsDir() && SkinCatalog.IsScenePath(next, ".tscn"))
					{
						list.Add(SkinCatalog.StripRemap("res://scenes/backgrounds/" + title + "/layers/" + next));
					}
				}
				val.ListDirEnd();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			ReportOnce("layers:" + title, "列出背景层失败：" + ex.GetBaseException().Message);
		}
		return list;
	}

	private static int FillLayers(Control shell, Node canvas, IEnumerable<string> layerPaths, Rng rng)
	{
		string[] source = layerPaths.ToArray();
		int num = 0;
		int num2 = 0;
		foreach (IGrouping<string, string> item in (from path in source
			where path.Contains("_bg_", StringComparison.OrdinalIgnoreCase)
			group path by LayerGroupKey(path, "_bg_")).OrderBy<IGrouping<string, string>, string>((IGrouping<string, string> group) => group.Key, StringComparer.Ordinal))
		{
			string text = rng.NextItem<string>((IEnumerable<string>)item.ToArray());
			Node val = canvas.GetNodeOrNull<Node>(NodePath.op_Implicit($"Layer_{num2:D2}")) ?? canvas.GetNodeOrNull<Node>(NodePath.op_Implicit("Layer_00"));
			num2++;
			if (!string.IsNullOrWhiteSpace(text) && val != null)
			{
				val.AddChild((Node)(object)InstantiateLayer(text), false, (InternalMode)0);
				num++;
			}
		}
		string text2 = source.FirstOrDefault((string path) => path.Contains("_fg_", StringComparison.OrdinalIgnoreCase));
		if (!string.IsNullOrWhiteSpace(text2))
		{
			Node val2 = canvas.GetNodeOrNull<Node>(NodePath.op_Implicit("Foreground")) ?? canvas.GetNodeOrNull<Node>(NodePath.op_Implicit("ForegroundLayer"));
			if (val2 != null)
			{
				val2.AddChild((Node)(object)InstantiateLayer(text2), false, (InternalMode)0);
				num++;
			}
		}
		return num;
	}

	private static NCombatBackground? CreatePackBackground(BackgroundOption option, Rng rng, bool allowSiblingFallback = true)
	{
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Expected O, but got Unknown
		BackgroundPackShell pack = option.Pack;
		if (pack == null)
		{
			return null;
		}
		try
		{
			ManagedSkinModLoader.EnsureProviderGodotScripts(option.Source);
			if (!ResourceLoader.Exists(pack.ShellPath, ""))
			{
				foreach (string item in SkinService.Catalog?.GetProviderResourcePackPaths(option.Source) ?? Array.Empty<string>())
				{
					if (File.Exists(item))
					{
						ProjectSettings.LoadResourcePack(item, false, 0);
					}
				}
			}
			PackedScene scene = PreloadManager.Cache.GetScene(pack.ShellPath);
			if (scene == null)
			{
				ReportOnce("pack:" + option.Id, option.DisplayName + " 的背景资源尚未挂载，已保留原背景。");
				return null;
			}
			Control val = scene.Instantiate<Control>((GenEditState)0);
			Node val2 = (Node)(((object)((Node)val).GetNodeOrNull<Node>(NodePath.op_Implicit("BattlefieldCanvas"))) ?? ((object)val));
			FillLayers(val, val2, pack.LayerPaths, rng);
			if (!((IEnumerable<Node>)val2.GetChildren(false)).Any((Node child) => ((object)child.Name).ToString().StartsWith("Layer_", StringComparison.OrdinalIgnoreCase) && child.GetChildCount(false) > 0) && allowSiblingFallback)
			{
				string baseToken = BaseTokenOf(NormalizeToken(option.Title));
				BackgroundOption backgroundOption = Backgrounds.FirstOrDefault((BackgroundOption candidate) => !candidate.Id.Equals(option.Id, StringComparison.OrdinalIgnoreCase) && candidate.Source.Equals(option.Source, StringComparison.OrdinalIgnoreCase) && candidate.Pack != null && NormalizeToken(candidate.Title).StartsWith(baseToken, StringComparison.Ordinal) && (NormalizeToken(candidate.Title).Contains("boss", StringComparison.Ordinal) || NormalizeToken(candidate.Title).Contains("encounter", StringComparison.Ordinal)));
				if (backgroundOption != null)
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)val))
					{
						((Node)val).QueueFree();
					}
					return CreatePackBackground(backgroundOption, rng, allowSiblingFallback: false);
				}
			}
			NCombatBackground val3 = new NCombatBackground
			{
				Name = StringName.op_Implicit("SkinChangerLayeredBackground"),
				MouseFilter = (MouseFilterEnum)2
			};
			((Node)val3).AddChild((Node)(object)val, false, (InternalMode)0);
			return val3;
		}
		catch (Exception ex)
		{
			ReportOnce("pack:" + option.Id, option.DisplayName + " 背景创建失败，已保留原背景：" + ex.GetBaseException().Message);
			return null;
		}
	}

	private static Control InstantiateLayer(string path)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I8
		Control val = PreloadManager.Cache.GetScene(path).Instantiate<Control>((GenEditState)0);
		if (((GodotObject)val).HasMethod(StringName.op_Implicit("configure_for_sts2")))
		{
			Variant val2 = ((GodotObject)val).Call(StringName.op_Implicit("configure_for_sts2"), Array.Empty<Variant>());
			if ((long)((Variant)(ref val2)).VariantType == 1 && !((Variant)(ref val2)).AsBool())
			{
				throw new InvalidOperationException("背景层拒绝了运行契约：" + path);
			}
		}
		((CanvasItem)val).Visible = true;
		return val;
	}

	private static string LayerGroupKey(string path, string marker)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		int num = fileNameWithoutExtension.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
		if (num < 0)
		{
			return fileNameWithoutExtension;
		}
		string text = fileNameWithoutExtension;
		int num2 = num + marker.Length;
		string text2 = text.Substring(num2, text.Length - num2);
		int num3 = text2.IndexOf('_');
		if (num3 <= 0)
		{
			return text2;
		}
		return text2.Substring(0, num3);
	}

	private static void RefreshCurrentActIfNeeded(string actId)
	{
		try
		{
			if (!TryGetCurrentRunState(out IRunState runState) || !NormalizeActId(((AbstractModel)runState.Act).Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			CombatManager instance = CombatManager.Instance;
			if (instance == null || !instance.IsInProgress)
			{
				return;
			}
			NCombatRoom instance2 = NCombatRoom.Instance;
			if (instance2 == null || instance2.Background == null)
			{
				return;
			}
			Control nodeOrNull = ((Node)instance2).GetNodeOrNull<Control>(NodePath.op_Implicit("%BgContainer"));
			NCombatBackground background = instance2.Background;
			try
			{
				BackgroundProperty?.SetValue(instance2, null);
			}
			catch
			{
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)background))
			{
				if (nodeOrNull != null)
				{
					((Node)nodeOrNull).RemoveChild((Node)(object)background);
				}
				((Node)background).QueueFree();
			}
			instance2.SetUpBackground(runState);
			ModLog.Info("已按新选择刷新当前战斗背景。");
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新当前战斗背景失败：" + ex.GetBaseException().Message);
		}
	}

	private static List<BackgroundActEntry> SafeDiscoverActs()
	{
		try
		{
			return DiscoverActs();
		}
		catch (Exception ex)
		{
			ModLog.Warn("枚举背景地区失败，已跳过背景功能：" + ex.GetBaseException().Message);
			return new List<BackgroundActEntry>();
		}
	}

	private static List<BackgroundOption> SafeDiscoverBackgrounds()
	{
		try
		{
			return DiscoverBackgrounds();
		}
		catch (Exception ex)
		{
			ModLog.Warn("扫描背景失败，已跳过背景功能：" + ex.GetBaseException().Message);
			return new List<BackgroundOption>();
		}
	}

	private static List<BackgroundActEntry> DiscoverActs()
	{
		IReadOnlyList<BackgroundOption> backgrounds = Backgrounds;
		Dictionary<string, BackgroundActEntry> entries = new Dictionary<string, BackgroundActEntry>(StringComparer.OrdinalIgnoreCase);
		List<string> list = new List<string>();
		foreach (ActModel item in EnumerateActs())
		{
			string text = NormalizeActId(((AbstractModel)item).Id.Entry);
			if (!entries.ContainsKey(text))
			{
				list.Add(text);
			}
			entries[text] = new BackgroundActEntry(text, ReadActTitle(item), CreateOptions(backgrounds));
		}
		string[] baseActIds = BaseActIds;
		foreach (string text2 in baseActIds)
		{
			if (!entries.ContainsKey(text2))
			{
				list.Add(text2);
				entries[text2] = new BackgroundActEntry(text2, text2, CreateOptions(backgrounds));
			}
		}
		return list.Select((string id) => entries[id]).ToList();
	}

	private static IEnumerable<ActModel> EnumerateActs()
	{
		return ActCatalog.All;
	}

	private static List<BackgroundOption> CreateOptions(IReadOnlyList<BackgroundOption> backgrounds)
	{
		List<BackgroundOption> list = new List<BackgroundOption>();
		list.Add(new BackgroundOption("__follow__", ModLocalization.Get(ModText.FollowCategory), string.Empty, string.Empty, null));
		list.Add(new BackgroundOption("__base__", ModLocalization.Get(ModText.GameDefault), "game", string.Empty, null));
		list.Add(new BackgroundOption("__mod_default__", ModLocalization.Get(ModText.BackgroundModDefault), string.Empty, string.Empty, null));
		list.AddRange(backgrounds);
		return list;
	}

	public static void Invalidate()
	{
		lock (Sync)
		{
			_acts = null;
			_backgrounds = null;
		}
	}

	private static List<BackgroundOption> DiscoverBackgrounds()
	{
		List<BackgroundOption> result = new List<BackgroundOption>();
		HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> actTitles = ReadActTitles();
		List<string> first = actTitles.Keys.ToList();
		AddFakeMerchantEventFromPack();
		List<string> list = ListGameBackgroundTitles();
		foreach (string item in first.Concat(list))
		{
			AddGame(item);
		}
		foreach (string item2 in list.OrderBy<string, string>((string title) => title, StringComparer.OrdinalIgnoreCase))
		{
			AddGame(item2);
		}
		ModBackgroundResources modBackgroundResources = DiscoverModBackgroundResources();
		foreach (string title in modBackgroundResources.Titles)
		{
			string text = NormalizeToken(title);
			string titleBase = BaseTokenOf(text);
			if ((!text.EndsWith("act", StringComparison.Ordinal) && !text.EndsWith("stage", StringComparison.Ordinal)) || !modBackgroundResources.Shells.Any((ModBackgroundShell shell) => NormalizeToken(shell.Shell.Title).StartsWith(titleBase, StringComparison.Ordinal) && (NormalizeToken(shell.Shell.Title).Contains("boss", StringComparison.Ordinal) || NormalizeToken(shell.Shell.Title).Contains("encounter", StringComparison.Ordinal))))
			{
				AddGame(title);
			}
		}
		foreach (ModBackgroundImage item3 in modBackgroundResources.Images.OrderBy<ModBackgroundImage, string>((ModBackgroundImage image) => image.ModName, StringComparer.OrdinalIgnoreCase).ThenBy<ModBackgroundImage, string>((ModBackgroundImage image) => image.Path, StringComparer.OrdinalIgnoreCase))
		{
			if (seen.Add("image::" + item3.Path))
			{
				result.Add(new BackgroundOption(BackgroundSkinPolicy.ProviderOptionId(item3.ModId, item3.Path), item3.ModName + " · " + BackgroundSkinPolicy.DisplayName(Path.GetFileNameWithoutExtension(item3.Path)), item3.ModId, item3.Path, null));
			}
		}
		foreach (ModBackgroundShell entry in modBackgroundResources.Shells.OrderBy<ModBackgroundShell, string>((ModBackgroundShell shell) => shell.ModName, StringComparer.OrdinalIgnoreCase).ThenBy<ModBackgroundShell, string>((ModBackgroundShell shell) => shell.Shell.Title, StringComparer.OrdinalIgnoreCase))
		{
			if (seen.Add(entry.ModId + "::" + entry.Shell.Title))
			{
				string text2 = NormalizeToken(entry.Shell.Title);
				string baseToken = BaseTokenOf(text2);
				if ((!text2.EndsWith("act", StringComparison.Ordinal) && !text2.EndsWith("stage", StringComparison.Ordinal)) || !modBackgroundResources.Shells.Any((ModBackgroundShell other) => other.ModId.Equals(entry.ModId, StringComparison.OrdinalIgnoreCase) && !other.Shell.Title.Equals(entry.Shell.Title, StringComparison.OrdinalIgnoreCase) && NormalizeToken(other.Shell.Title).StartsWith(baseToken, StringComparison.Ordinal) && (NormalizeToken(other.Shell.Title).Contains("boss", StringComparison.Ordinal) || NormalizeToken(other.Shell.Title).Contains("encounter", StringComparison.Ordinal))))
				{
					result.Add(new BackgroundOption(BackgroundSkinPolicy.ProviderOptionId(entry.ModId, entry.Shell.Title), entry.ModName + " · " + BackgroundSkinPolicy.DisplayName(entry.Shell.Title), entry.ModId, entry.Shell.Title, entry.Shell));
				}
			}
		}
		foreach (ModBackgroundScene item4 in modBackgroundResources.Scenes.OrderBy<ModBackgroundScene, string>((ModBackgroundScene scene) => scene.ModName, StringComparer.OrdinalIgnoreCase).ThenBy<ModBackgroundScene, string>((ModBackgroundScene scene) => scene.Path, StringComparer.OrdinalIgnoreCase))
		{
			string stem = SceneStem(item4.Path);
			ModBackgroundImage modBackgroundImage = modBackgroundResources.Images.FirstOrDefault((ModBackgroundImage candidate) => SceneStem(candidate.Path).Equals(stem, StringComparison.OrdinalIgnoreCase));
			if (!(modBackgroundImage == null) || !item4.Path.Contains("/images/background/", StringComparison.OrdinalIgnoreCase))
			{
				string text3 = modBackgroundImage?.Path ?? item4.Path;
				if (seen.Add("scene::" + text3))
				{
					result.Add(new BackgroundOption(BackgroundSkinPolicy.ProviderOptionId(item4.ModId, text3), item4.ModName + " · " + BackgroundSkinPolicy.DisplayName(Path.GetFileNameWithoutExtension(text3)), item4.ModId, text3, null));
				}
			}
		}
		(string, string, string)[] array = DiscoverDeclaredSceneBackgrounds().ToArray();
		(string, string, string)[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			(string Source, string ScenePath, string SourceDisplay) scene2 = array2[i];
			string id = BackgroundSkinPolicy.ProviderOptionId(scene2.Source, scene2.ScenePath);
			if (seen.Add("scene::" + scene2.ScenePath))
			{
				string text4 = NormalizeToken(Path.GetFileNameWithoutExtension(scene2.ScenePath));
				string sceneBase = BaseTokenOf(text4);
				if ((!text4.EndsWith("act", StringComparison.Ordinal) && !text4.EndsWith("stage", StringComparison.Ordinal)) || !array.Any(((string Source, string ScenePath, string SourceDisplay) other) => other.Source.Equals(scene2.Source, StringComparison.OrdinalIgnoreCase) && !other.ScenePath.Equals(scene2.ScenePath, StringComparison.OrdinalIgnoreCase) && NormalizeToken(Path.GetFileNameWithoutExtension(other.ScenePath)).StartsWith(sceneBase, StringComparison.Ordinal) && (NormalizeToken(other.ScenePath).Contains("boss", StringComparison.Ordinal) || NormalizeToken(other.ScenePath).Contains("encounter", StringComparison.Ordinal))))
				{
					result.Add(new BackgroundOption(id, scene2.SourceDisplay + " · " + BackgroundSkinPolicy.DisplayName(Path.GetFileNameWithoutExtension(scene2.ScenePath)), scene2.Source, scene2.ScenePath, null));
				}
			}
		}
		SkinCatalog catalog = SkinService.Catalog;
		if (catalog == null)
		{
			return result;
		}
		foreach (BackgroundPackCandidate item5 in catalog.FindBackgroundPackCandidates().OrderBy<BackgroundPackCandidate, string>((BackgroundPackCandidate candidate) => candidate.DisplayName, StringComparer.OrdinalIgnoreCase))
		{
			foreach (BackgroundPackShell item6 in item5.Shells.OrderBy<BackgroundPackShell, string>((BackgroundPackShell shell) => shell.Title, StringComparer.OrdinalIgnoreCase))
			{
				if (seen.Add(item5.ProviderId + "::" + item6.Title))
				{
					result.Add(new BackgroundOption(BackgroundSkinPolicy.ProviderOptionId(item5.ProviderId, item6.Title), item5.DisplayName + " · " + BackgroundSkinPolicy.DisplayName(item6.Title), item5.ProviderId, item6.Title, item6));
				}
			}
		}
		_providerActDefaults = DiscoverProviderActDefaults(result);
		ModLog.Info($"背景资源扫描完成：规范场景={modBackgroundResources.Titles.Count}，背景图={modBackgroundResources.Images.Count}，可选背景总数={result.Count}。");
		return result;
		void AddFakeMerchantEventFromPack()
		{
			string text5 = SkinService.Catalog?.FindFakeMerchantEventArtProvider();
			if (!string.IsNullOrWhiteSpace(text5) && seen.Add(text5 + "::fake_merchant_event"))
			{
				string text6 = "res://images/events/fake_merchant.png";
				if (!ResourceLoader.Exists(text6, ""))
				{
					ModLog.Info("假商人事件包没有可用的满屏立绘，背景列表保留游戏自带的帐篷场景。");
				}
				else
				{
					BackgroundOption backgroundOption = new BackgroundOption(BackgroundSkinPolicy.ProviderOptionId(text5, "fake_merchant_event"), "假商人事件背景 / Fake Merchant Event", text5, text6, null);
					if (!TryProbeBackground(backgroundOption))
					{
						ModLog.Warn("假商人事件背景 / Fake Merchant Event 无法渲染为背景，已从列表移除。");
					}
					else
					{
						result.Add(backgroundOption);
						ModLog.Info($"已把假商人事件背景图加入可切换列表：{"假商人事件背景 / Fake Merchant Event"}（来源={text5}，图={text6}）。");
					}
				}
			}
		}
		void AddGame(string title)
		{
			if (GameBackgroundExists(title) && seen.Add("game::" + title))
			{
				result.Add(new BackgroundOption(BackgroundSkinPolicy.ProviderOptionId("game", title), actTitles.TryGetValue(title, out string value) ? value : BackgroundSkinPolicy.DisplayName(title), "game", title, null));
			}
		}
	}

	private static ModBackgroundResources DiscoverModBackgroundResources()
	{
		List<string> list = new List<string>();
		List<ModBackgroundImage> list2 = new List<ModBackgroundImage>();
		List<ModBackgroundShell> list3 = new List<ModBackgroundShell>();
		List<ModBackgroundScene> list4 = new List<ModBackgroundScene>();
		foreach (Mod mod in ModManager.Mods)
		{
			string text;
			try
			{
				ModManifest manifest = mod.manifest;
				if (manifest == null || manifest.id == null || string.IsNullOrWhiteSpace(mod.path))
				{
					continue;
				}
				text = SkinPackagePaths.Resolve(mod.path, manifest.id, ".pck");
				goto IL_007d;
			}
			catch
			{
			}
			continue;
			IL_007d:
			if (string.IsNullOrWhiteSpace(text) || !File.Exists(text))
			{
				continue;
			}
			try
			{
				List<ModBackgroundImage> list5 = new List<ModBackgroundImage>();
				bool flag = false;
				using PckArchive pckArchive = PckArchive.Open(text);
				string[] array = pckArchive.Paths.ToArray();
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (SkinCatalog.IsScenePath(text2, "_background.tscn") && SkinCatalog.IsBattleBackgroundLocation(text2))
					{
						flag = true;
						string text3 = SkinCatalog.StripRemap(text2);
						string text4 = Path.GetFileNameWithoutExtension(text3);
						if (text4.EndsWith("_background", StringComparison.OrdinalIgnoreCase))
						{
							string text5 = text4;
							int length = "_background".Length;
							text4 = text5.Substring(0, text5.Length - length);
						}
						if (text3.StartsWith("res://scenes/backgrounds/", StringComparison.OrdinalIgnoreCase))
						{
							list.Add(text4);
						}
						string directory = text3.Substring(0, text3.LastIndexOf('/'));
						string[] array3 = array.Where((string candidate) => candidate.StartsWith(directory + "/layers/", StringComparison.OrdinalIgnoreCase) && SkinCatalog.IsScenePath(candidate, ".tscn")).Select(SkinCatalog.StripRemap).ToArray();
						if (array3.Length != 0)
						{
							list3.Add(new ModBackgroundShell(mod.manifest.id, mod.manifest.name ?? mod.manifest.id, new BackgroundPackShell(text4, text3, array3)));
						}
					}
					else if (SkinCatalog.IsScenePath(text2, ".tscn") && text2.Contains("/images/background/", StringComparison.OrdinalIgnoreCase) && !text2.Contains("/rest/", StringComparison.OrdinalIgnoreCase) && !text2.Contains("rest_site", StringComparison.OrdinalIgnoreCase))
					{
						list4.Add(new ModBackgroundScene(mod.manifest.id, mod.manifest.name ?? mod.manifest.id, SkinCatalog.StripRemap(text2)));
					}
					else
					{
						string text6 = ResolveBackgroundImageSource(text2);
						if (text6 != null)
						{
							list5.Add(new ModBackgroundImage(mod.manifest.id, mod.manifest.name ?? mod.manifest.id, text6));
						}
					}
				}
				if (!flag)
				{
					list2.AddRange(list5);
					continue;
				}
				list2.AddRange(list5.Where((ModBackgroundImage image) => !image.Path.Contains("/images/backgrounds/", StringComparison.OrdinalIgnoreCase)));
			}
			catch
			{
			}
		}
		return new ModBackgroundResources(list.Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray(), list2.DistinctBy<ModBackgroundImage, string>((ModBackgroundImage image) => image.Path, StringComparer.OrdinalIgnoreCase).ToArray(), list3.DistinctBy<ModBackgroundShell, string>((ModBackgroundShell entry) => entry.Shell.ShellPath, StringComparer.OrdinalIgnoreCase).ToArray(), list4.DistinctBy<ModBackgroundScene, string>((ModBackgroundScene scene) => scene.Path, StringComparer.OrdinalIgnoreCase).ToArray());
	}

	private static string? ResolveBackgroundImageSource(string path)
	{
		string text;
		if (!path.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
		{
			text = path;
		}
		else
		{
			int length = ".import".Length;
			text = path.Substring(0, path.Length - length);
		}
		string text2 = text;
		if (!IsBackgroundImagePath(text2))
		{
			return null;
		}
		return text2;
	}

	private static bool IsBackgroundImagePath(string path)
	{
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string text = path.ToLowerInvariant();
		if (!text.Contains("/images/backgrounds/", StringComparison.Ordinal) && !text.Contains("/images/background/", StringComparison.Ordinal))
		{
			return false;
		}
		string[] backgroundImageExcludedSegments = BackgroundImageExcludedSegments;
		foreach (string value in backgroundImageExcludedSegments)
		{
			if (text.Contains(value, StringComparison.Ordinal))
			{
				return false;
			}
		}
		return true;
	}

	[IteratorStateMachine(typeof(_003CDiscoverDeclaredSceneBackgrounds_003Ed__91))]
	private static IEnumerable<(string Source, string ScenePath, string SourceDisplay)> DiscoverDeclaredSceneBackgrounds()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDiscoverDeclaredSceneBackgrounds_003Ed__91(-2);
	}

	private static Dictionary<string, string> DiscoverProviderActDefaults(IReadOnlyList<BackgroundOption> backgrounds)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, bool> dictionary2 = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		SkinCatalog catalog = SkinService.Catalog;
		if (catalog == null)
		{
			return dictionary;
		}
		foreach (BackgroundPackCandidate item in catalog.FindBackgroundPackCandidates())
		{
			if (!ManagedSkinModLoader.TryGetProviderAssemblyForInspection(item.ProviderId, out Assembly assembly))
			{
				continue;
			}
			Type[] source;
			try
			{
				source = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				source = ex.Types.Where((Type type) => type != null).Cast<Type>().ToArray();
			}
			catch
			{
				continue;
			}
			foreach (Type item2 in source.Where((Type type) => type.Name.Contains("Profile", StringComparison.OrdinalIgnoreCase)))
			{
				PropertyInfo property = item2.GetProperty("Profiles", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (property == null || property.GetIndexParameters().Length != 0)
				{
					continue;
				}
				IEnumerable enumerable2;
				try
				{
					if (!(property.GetValue(null) is IEnumerable enumerable))
					{
						continue;
					}
					enumerable2 = enumerable;
					goto IL_011c;
				}
				catch
				{
				}
				continue;
				IL_011c:
				foreach (object item3 in enumerable2)
				{
					if (item3 == null)
					{
						continue;
					}
					Type type2 = item3.GetType();
					object obj3 = type2.GetProperty("Target", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(item3);
					string text = obj3?.GetType().GetProperty("ActId", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj3) as string;
					if (string.IsNullOrWhiteSpace(text))
					{
						continue;
					}
					object obj4 = type2.GetProperty("Battlefield", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(item3);
					string text2 = ((obj4 == null) ? null : ReadBackgroundShellName(obj4, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
					if (string.IsNullOrWhiteSpace(text2))
					{
						continue;
					}
					string optionId = BackgroundSkinPolicy.ProviderOptionId(item.ProviderId, text2);
					if (backgrounds.Any((BackgroundOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase)))
					{
						string key = NormalizeActId(text);
						bool flag = IsStandardCombatScope(obj3, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						if (!dictionary.ContainsKey(key) || (!dictionary2[key] && flag))
						{
							dictionary[key] = optionId;
							dictionary2[key] = flag;
						}
					}
				}
			}
		}
		return dictionary;
	}

	private static bool IsStandardCombatScope(object? target, BindingFlags flags)
	{
		try
		{
			object obj = target?.GetType().GetProperty("RoomScope", flags)?.GetValue(target);
			return obj != null && (obj.ToString()?.Equals("StandardCombat", StringComparison.OrdinalIgnoreCase) ?? false);
		}
		catch
		{
			return false;
		}
	}

	private static string? ReadBackgroundShellName(object battlefield, BindingFlags flags)
	{
		try
		{
			object obj = battlefield.GetType().GetProperty("Background", flags)?.GetValue(battlefield);
			string text = obj?.GetType().GetProperty("ShellScenePath", flags)?.GetValue(obj) as string;
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
			string result;
			if (!fileNameWithoutExtension.EndsWith("_background", StringComparison.OrdinalIgnoreCase))
			{
				result = fileNameWithoutExtension;
			}
			else
			{
				string text2 = fileNameWithoutExtension;
				int length = "_background".Length;
				result = text2.Substring(0, text2.Length - length);
			}
			return result;
		}
		catch
		{
			return null;
		}
	}

	private static BackgroundOption? FindBackground(string id)
	{
		BackgroundOption backgroundOption = Backgrounds.FirstOrDefault((BackgroundOption option) => option.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (backgroundOption != null)
		{
			return backgroundOption;
		}
		if (!BackgroundSkinPolicy.TryParseProviderOptionId(id, out string _, out string title))
		{
			return null;
		}
		return Backgrounds.FirstOrDefault((BackgroundOption option) => option.Title.Equals(title, StringComparison.OrdinalIgnoreCase) || option.Title.EndsWith(title, StringComparison.OrdinalIgnoreCase));
	}

	private static bool IsFakeMerchantEncounter(EncounterModel encounter)
	{
		try
		{
			return ((AbstractModel)encounter).Id.Entry.Equals("fake_merchant_event_encounter", StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private static bool IsFakeMerchantCombat(NCombatRoom room)
	{
		try
		{
			NCombatBackground background = room.Background;
			return background != null && GodotObject.IsInstanceValid((GodotObject)(object)background) && ((Node)background).SceneFilePath.Contains("fake_merchant", StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private static bool TryUseFakeMerchantEventImageBackground(ref NCombatBackground result)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string text = SkinService.Catalog?.FindFakeMerchantEventArtProvider();
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			string id = BackgroundSkinPolicy.ProviderOptionId(text, "fake_merchant_event");
			BackgroundOption backgroundOption = FindBackground(id) ?? new BackgroundOption(id, "假商人事件背景 / Fake Merchant Event", text, "res://images/events/fake_merchant.png", null);
			NCombatBackground val = CreateImageBackground(backgroundOption);
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				return false;
			}
			if (result != null && GodotObject.IsInstanceValid((GodotObject)(object)result) && result != val)
			{
				((GodotObject)result).Free();
			}
			((GodotObject)val).SetMeta(StringName.op_Implicit("sc_background_applied"), Variant.op_Implicit(true));
			result = val;
			ModLog.Info("假商人战斗背景来源=事件图（" + backgroundOption.DisplayName + "）。");
			return true;
		}
		catch (Exception ex)
		{
			ReportOnce("fake-merchant-event-image", "构建假商人事件背景失败，已保留遭遇自带背景：" + ex.GetBaseException().Message);
			return false;
		}
	}

	private static bool TryUseFakeMerchantVanillaBackground(ref NCombatBackground result)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string text = SkinService.TryGetIsolatedRuntimeResourcePath("fake_merchant_monster", "res://scenes/backgrounds/fake_merchant_event_encounter/fake_merchant_event_encounter_background.tscn");
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			NCombatBackground val = CreateSceneBackground(new BackgroundOption("sc_fake_merchant_vanilla", "game default", "game", text, null), allowSiblingFallback: false);
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				return false;
			}
			if (result != null && GodotObject.IsInstanceValid((GodotObject)(object)result) && result != val)
			{
				((GodotObject)result).Free();
			}
			((GodotObject)val).SetMeta(StringName.op_Implicit("sc_background_applied"), Variant.op_Implicit(true));
			result = val;
			ModLog.Info("假商人战斗背景来源=游戏默认（原版帐篷副本）。");
			return true;
		}
		catch (Exception ex)
		{
			ReportOnce("fake-merchant-vanilla", "构建假商人原版战斗背景失败，已保留遭遇自带背景：" + ex.GetBaseException().Message);
			return false;
		}
	}

	private static bool GameBackgroundExists(string title)
	{
		if (!string.IsNullOrWhiteSpace(title))
		{
			return ResourceLoader.Exists($"res://scenes/backgrounds/{title}/{title}_background.tscn", "");
		}
		return false;
	}

	private static List<string> ListGameBackgroundTitles()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		try
		{
			DirAccess val = DirAccess.Open("res://scenes/backgrounds");
			try
			{
				if (val == null)
				{
					return list;
				}
				val.ListDirBegin();
				while (true)
				{
					string next = val.GetNext();
					if (string.IsNullOrEmpty(next))
					{
						break;
					}
					if (!next.StartsWith(".", StringComparison.Ordinal) && val.CurrentIsDir() && GameBackgroundExists(next))
					{
						list.Add(next);
					}
				}
				val.ListDirEnd();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			ReportOnce("list-backgrounds", "列出游戏背景失败：" + ex.GetBaseException().Message);
		}
		return list;
	}

	private static Dictionary<string, string> ReadActTitles()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		try
		{
			foreach (ActModel item in EnumerateActs())
			{
				string key = NormalizeActId(((AbstractModel)item).Id.Entry);
				if (!dictionary.ContainsKey(key))
				{
					dictionary[key] = ReadActTitle(item);
				}
			}
		}
		catch (Exception ex)
		{
			ReportOnce("act-titles", "读取地区名称失败：" + ex.GetBaseException().Message);
		}
		return dictionary;
	}

	private static string ReadActTitle(ActModel act)
	{
		try
		{
			string formattedText = act.Title.GetFormattedText();
			return string.IsNullOrWhiteSpace(formattedText) ? ((AbstractModel)act).Id.Entry : formattedText;
		}
		catch
		{
			return ((AbstractModel)act).Id.Entry;
		}
	}

	private static bool TryGetCurrentRunState(out IRunState runState)
	{
		try
		{
			NRun instance = NRun.Instance;
			if (instance != null)
			{
				object? obj = RunStateField?.GetValue(instance);
				IRunState val = (IRunState)((obj is IRunState) ? obj : null);
				if (val != null)
				{
					runState = val;
					return true;
				}
			}
		}
		catch
		{
		}
		try
		{
			CombatManager instance2 = CombatManager.Instance;
			CombatState val2 = ((instance2 != null) ? instance2.DebugOnlyGetState() : null);
			if (val2 != null)
			{
				IRunState runState2 = val2.RunState;
				if (runState2 != null)
				{
					runState = runState2;
					return true;
				}
			}
		}
		catch
		{
		}
		runState = null;
		return false;
	}

	private static string NormalizeActId(string actId)
	{
		return actId.Trim().ToLowerInvariant();
	}

	private static void ReportOnce(string key, string message)
	{
		if (ReportedFailures.Add(key))
		{
			ModLog.Warn(message);
		}
	}
}
