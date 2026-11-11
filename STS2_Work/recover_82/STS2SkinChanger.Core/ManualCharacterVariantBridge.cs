using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

internal static class ManualCharacterVariantBridge
{
	private sealed class ScreenBinding(string group, string provider, ManualCharacterState state)
	{
		public readonly string GroupId = group;

		public readonly string ProviderId = provider;

		public readonly ManualCharacterState State = state;

		public long Generation;
	}

	private sealed class RestoreField(FieldInfo field, bool value) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				field.SetValue(null, value);
			}
		}
	}

	private static readonly Harmony Observer = new Harmony("Gurio.SkinChanger.manual_variants");

	private static readonly HashSet<MethodInfo> Observed = new HashSet<MethodInfo>();

	private static readonly ConditionalWeakTable<NCharacterSelectScreen, ScreenBinding> Screens = new ConditionalWeakTable<NCharacterSelectScreen, ScreenBinding>();

	private static readonly List<WeakReference<NCharacterSelectScreen>> ScreenRefs = new List<WeakReference<NCharacterSelectScreen>>();

	private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	internal static IDisposable? BeginScope(string groupId)
	{
		ManualCharacterVariant selectedManualCharacterVariant = SkinService.GetSelectedManualCharacterVariant(groupId);
		if ((object)selectedManualCharacterVariant != null && selectedManualCharacterVariant.State.IsStatic)
		{
			FieldInfo field = GetField(groupId, selectedManualCharacterVariant.State);
			if ((object)field != null)
			{
				bool value = (bool)field.GetValue(null);
				field.SetValue(null, selectedManualCharacterVariant.Value);
				return new RestoreField(field, value);
			}
		}
		return null;
	}

	internal static void ApplyLocalChoice(string groupId)
	{
		try
		{
			ApplyLocalChoiceCore(groupId);
		}
		catch (Exception ex)
		{
			ModLog.Warn("同步皮肤原配色设置失败：" + ex.GetBaseException().Message);
		}
	}

	private static void ApplyLocalChoiceCore(string groupId)
	{
		ManualCharacterVariant selectedManualCharacterVariant = SkinService.GetSelectedManualCharacterVariant(groupId);
		if ((object)selectedManualCharacterVariant == null || !selectedManualCharacterVariant.State.IsStatic)
		{
			return;
		}
		FieldInfo field = GetField(groupId, selectedManualCharacterVariant.State);
		if ((object)field != null && !object.Equals(field.GetValue(null), selectedManualCharacterVariant.Value))
		{
			field.SetValue(null, selectedManualCharacterVariant.Value);
			string saveMethod = selectedManualCharacterVariant.State.SaveMethod;
			if (saveMethod != null)
			{
				field.DeclaringType.GetMethod(saveMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes)?.Invoke(null, null);
			}
		}
	}

	private static FieldInfo? GetField(string groupId, ManualCharacterState state)
	{
		string selectedRuntimeProvider = SkinService.GetSelectedRuntimeProvider(groupId);
		if (selectedRuntimeProvider != null)
		{
			return ManagedSkinModLoader.GetActiveProviderAssembly(selectedRuntimeProvider)?.GetType(state.TypeName)?.GetField(state.FieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}
		return null;
	}

	internal static void BindScreen(NCharacterSelectScreen screen, string groupId, string providerId)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		ManualCharacterVariant selectedManualCharacterVariant = SkinService.GetSelectedManualCharacterVariant(groupId);
		if (selectedManualCharacterVariant == null)
		{
			return;
		}
		if (!Screens.TryGetValue(screen, out ScreenBinding _))
		{
			ScreenRefs.Add(new WeakReference<NCharacterSelectScreen>(screen));
		}
		Screens.Remove(screen);
		Screens.Add(screen, new ScreenBinding(groupId, providerId, selectedManualCharacterVariant.State));
		if (!selectedManualCharacterVariant.State.IsStatic)
		{
			return;
		}
		MethodInfo methodInfo = ManagedSkinModLoader.GetActiveProviderAssembly(providerId)?.GetType(selectedManualCharacterVariant.State.ToggleTypeName)?.GetMethod(selectedManualCharacterVariant.State.ToggleMethod, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes);
		if (methodInfo == null || !Observed.Add(methodInfo))
		{
			return;
		}
		try
		{
			Observer.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(typeof(ManualCharacterVariantBridge), "AfterToggle", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
		}
		catch
		{
			Observed.Remove(methodInfo);
			throw;
		}
	}

	private static void AfterToggle(MethodBase __originalMethod)
	{
		try
		{
			ObserveToggle(__originalMethod);
		}
		catch (Exception ex)
		{
			ModLog.Warn("同步作者皮肤开关失败：" + ex.GetBaseException().Message);
		}
	}

	private static void ObserveToggle(MethodBase __originalMethod)
	{
		ScreenRefs.RemoveAll((WeakReference<NCharacterSelectScreen> reference) => !reference.TryGetTarget(out NCharacterSelectScreen target) || !GodotObject.IsInstanceValid((GodotObject)(object)target));
		WeakReference<NCharacterSelectScreen>[] array = ScreenRefs.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGetTarget(out var target2) && Screens.TryGetValue(target2, out ScreenBinding value) && !(value.state.ToggleTypeName != __originalMethod.DeclaringType?.FullName) && !(value.state.ToggleMethod != __originalMethod.Name) && ManagedSkinModLoader.IsProviderAssemblyFor(value.provider, __originalMethod.DeclaringType.Assembly) && GetField(value.group, value.state)?.GetValue(null) is bool value2)
			{
				Request(target2, value, value2);
			}
		}
	}

	internal static void AfterSlotToggle(Control control, string groupId, string providerId, string stateId, bool value)
	{
		for (Node val = (Node)(object)control; val != null; val = val.GetParent())
		{
			NCharacterSelectScreen val2 = (NCharacterSelectScreen)(object)((val is NCharacterSelectScreen) ? val : null);
			if (val2 != null && Screens.TryGetValue(val2, out ScreenBinding value2))
			{
				if (value2.group == groupId && value2.provider == providerId && value2.state.Id == stateId)
				{
					Request(val2, value2, value);
				}
				break;
			}
		}
	}

	private static void Request(NCharacterSelectScreen screen, ScreenBinding binding, bool value)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		long generation = ++binding.Generation;
		Callable val = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)screen) && ((Node)screen).IsInsideTree() && ((CanvasItem)screen).IsVisibleInTree() && Screens.TryGetValue(screen, out ScreenBinding value2) && value2 == binding && generation == binding.Generation && !(SkinService.GetSelectedRuntimeProvider(binding.group) != binding.provider))
			{
				SkinOption skinOption = SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup group) => group.Id == binding.group)?.Options.FirstOrDefault(delegate(SkinOption candidate)
				{
					if (!candidate.IsComposition && candidate.EffectiveProviderId == binding.provider)
					{
						ManualCharacterVariant manualCharacterVariant = candidate.ManualCharacterVariant;
						if ((object)manualCharacterVariant != null && manualCharacterVariant.State.Id == binding.state.Id)
						{
							return manualCharacterVariant.Value == value;
						}
					}
					return false;
				});
				if (skinOption != null)
				{
					ContextualSkinControls.RequestManualVariantSelection(screen, binding.group, skinOption.Id);
				}
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}
}
