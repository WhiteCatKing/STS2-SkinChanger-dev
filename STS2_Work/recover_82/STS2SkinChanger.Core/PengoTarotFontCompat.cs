using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;

namespace STS2SkinChanger.Core;

internal static class PengoTarotFontCompat
{
	private static readonly string[] ScreenTypeNames = new string[4] { "PengoTarot.BalatroEffect.NBalatroInspectScreen", "PengoTarot.BalatroEffect.NBalatroInspectEnchantScreen", "PengoTarot.ConfigFW.NPortableTarotPopup", "PengoTarot.UI.NMerchantTarot" };

	private static Harmony? _harmony;

	private static readonly HashSet<string> PatchedTypes = new HashSet<string>(StringComparer.Ordinal);

	private static bool _reported;

	public static void Install(Harmony harmony)
	{
		_harmony = harmony;
		TryInstall();
		AppDomain.CurrentDomain.AssemblyLoad += delegate
		{
			TryInstall();
		};
	}

	private static void TryInstall()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		if (_harmony == null)
		{
			return;
		}
		string[] screenTypeNames = ScreenTypeNames;
		foreach (string typeName in screenTypeNames)
		{
			if (PatchedTypes.Contains(typeName))
			{
				continue;
			}
			Type type = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
				select assembly.GetType(typeName, throwOnError: false)).FirstOrDefault((Type candidate) => candidate != null);
			if (type == null)
			{
				continue;
			}
			MethodInfo method = type.GetMethod("_Ready", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
			if (method == null)
			{
				PatchedTypes.Add(typeName);
				continue;
			}
			try
			{
				_harmony.Patch((MethodBase)method, (HarmonyMethod)null, new HarmonyMethod(typeof(PengoTarotFontCompat), "ApplyLocaleFontsPostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
				PatchedTypes.Add(typeName);
				if (!_reported)
				{
					_reported = true;
					ModLog.Info("Pengo's Tarot Enchantments 字体兼容已启用：它的卡牌检视界面会用中/日/韩字体重绘含 CJK 字符的文本，数字与英文仍用原像素字体。");
				}
			}
			catch (Exception ex)
			{
				PatchedTypes.Add(typeName);
				ModLog.Warn("Pengo's Tarot 字体兼容安装失败（" + type.Name + "）：" + ex.GetBaseException().Message);
			}
		}
	}

	private static void ApplyLocaleFontsPostfix(Node __instance)
	{
		try
		{
			ApplyToTree(__instance);
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)__instance))
			{
				return;
			}
			val.CreateTimer(0.15, true, false, false).Timeout += delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)__instance))
				{
					ApplyToTree(__instance);
				}
			};
		}
		catch (Exception ex)
		{
			ModLog.Warn("Pengo's Tarot 字体兼容应用失败：" + ex.GetBaseException().Message);
		}
	}

	private static void ApplyToTree(Node node)
	{
		Font val = LocaleSubstituteFont();
		if (val == null)
		{
			return;
		}
		ApplyToNode(node, val);
		foreach (Node child in node.GetChildren(false))
		{
			if (child != null)
			{
				Node node2 = child;
				ApplyToTree(node2, val);
			}
		}
	}

	private static void ApplyToTree(Node node, Font font)
	{
		ApplyToNode(node, font);
		foreach (Node child in node.GetChildren(false))
		{
			if (child != null)
			{
				Node node2 = child;
				ApplyToTree(node2, font);
			}
		}
	}

	private static void ApplyToNode(Node node, Font font)
	{
		Label val = (Label)(object)((node is Label) ? node : null);
		if (val == null)
		{
			RichTextLabel val2 = (RichTextLabel)(object)((node is RichTextLabel) ? node : null);
			if (val2 == null)
			{
				Button val3 = (Button)(object)((node is Button) ? node : null);
				if (val3 != null && ContainsCjk(val3.Text))
				{
					((Control)val3).AddThemeFontOverride(StringName.op_Implicit("font"), font);
				}
			}
			else if (ContainsCjk(val2.Text))
			{
				((Control)val2).AddThemeFontOverride(StringName.op_Implicit("font"), font);
			}
		}
		else if (ContainsCjk(val.Text))
		{
			((Control)val).AddThemeFontOverride(StringName.op_Implicit("font"), font);
		}
	}

	private static Font? LocaleSubstituteFont()
	{
		try
		{
			LocManager instance = LocManager.Instance;
			if (instance == null)
			{
				return null;
			}
			string language = instance.Language;
			if (string.IsNullOrWhiteSpace(language) || !FontManager.NeedsFontSubstitution(language))
			{
				return null;
			}
			return FontManager.GetSubstituteFont(language, (FontType)1);
		}
		catch
		{
			return null;
		}
	}

	private static bool ContainsCjk(string? text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		int num = 0;
		while (num < text.Length)
		{
			char c = text[num];
			if (c >= '豈')
			{
				if (c >= '\uff00')
				{
					if (c <= '\uffef')
					{
						goto IL_005e;
					}
				}
				else if (c <= '\ufaff')
				{
					goto IL_005e;
				}
			}
			else if (c >= '가')
			{
				if (c <= '\ud7af')
				{
					goto IL_005e;
				}
			}
			else if (c >= '⺀' && c <= '鿿')
			{
				goto IL_005e;
			}
			bool flag = false;
			goto IL_0064;
			IL_0064:
			if (flag)
			{
				return true;
			}
			num++;
			continue;
			IL_005e:
			flag = true;
			goto IL_0064;
		}
		return false;
	}
}
