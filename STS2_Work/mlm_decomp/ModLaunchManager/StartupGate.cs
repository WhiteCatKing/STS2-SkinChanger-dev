using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;

namespace ModLaunchManager;

internal static class StartupGate
{
	private static readonly TaskCompletionSource<bool> Completion = new TaskCompletionSource<bool>();

	private static FieldInfo? _stateField;

	private static FieldInfo? _awaiterField;

	private static FieldInfo? _builderField;

	private static MethodInfo? _getTaskAwaiter;

	private static MethodInfo? _awaitUnsafeOnCompleted;

	private static int _armed;

	private static bool _enabled;

	internal static bool Install(Harmony harmony)
	{
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected O, but got Unknown
		try
		{
			Type type = AccessTools.Method(typeof(OneTimeInitialization), "ExecuteVeryEarly", (Type[])null, (Type[])null).GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;
			if (type == null)
			{
				throw new MissingMemberException("ExecuteVeryEarly has no async state machine");
			}
			_stateField = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SingleOrDefault((FieldInfo field) => field.FieldType == typeof(int) && field.Name.Contains("state", StringComparison.Ordinal));
			_awaiterField = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SingleOrDefault((FieldInfo field) => field.FieldType == typeof(TaskAwaiter));
			_builderField = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SingleOrDefault((FieldInfo field) => field.FieldType == typeof(AsyncTaskMethodBuilder));
			_getTaskAwaiter = AccessTools.Method(typeof(Task), "GetAwaiter", (Type[])null, (Type[])null);
			_awaitUnsafeOnCompleted = typeof(AsyncTaskMethodBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public).Single((MethodInfo method) => method.Name == "AwaitUnsafeOnCompleted" && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 2).MakeGenericMethod(typeof(TaskAwaiter), type);
			if (_stateField == null || _awaiterField == null || _builderField == null || _getTaskAwaiter == null)
			{
				throw new MissingMemberException("the ExecuteVeryEarly state-machine shape changed");
			}
			MethodInfo methodInfo = AccessTools.Method(type, "MoveNext", (Type[])null, (Type[])null);
			_enabled = true;
			harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(StartupGate), "Transpiler", (Type[])null, (Type[])null)), (HarmonyMethod)null);
			InstallEarlyInputGuards(harmony);
			return true;
		}
		catch (Exception ex)
		{
			_enabled = false;
			Completion.TrySetResult(result: true);
			GD.PushWarning($"{"[ModLaunchManager]"} startup-gate install failed: {ex.GetType().Name} {ex.Message}");
			return false;
		}
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Expected O, but got Unknown
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Expected O, but got Unknown
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Expected O, but got Unknown
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Expected O, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Expected O, but got Unknown
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Expected O, but got Unknown
		List<CodeInstruction> list = instructions.ToList();
		if (list.Count == 0 || _stateField == null || _awaiterField == null || _builderField == null || _getTaskAwaiter == null || _awaitUnsafeOnCompleted == null)
		{
			throw new InvalidOperationException("startup-gate metadata was not prepared");
		}
		Label label = generator.DefineLabel();
		list[0].labels.Add(label);
		List<CodeInstruction> list2 = new List<CodeInstruction>();
		list2.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
		list2.Add(new CodeInstruction(OpCodes.Ldfld, (object)_stateField));
		list2.Add(new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
		list2.Add(new CodeInstruction(OpCodes.Blt, (object)label));
		list2.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(StartupGate), "ShouldSuspend", (Type[])null, (Type[])null)));
		list2.Add(new CodeInstruction(OpCodes.Brfalse, (object)label));
		list2.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(StartupGate), "ArmOverlay", (Type[])null, (Type[])null)));
		list2.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
		list2.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(StartupGate), "GetGateTask", (Type[])null, (Type[])null)));
		list2.Add(new CodeInstruction(OpCodes.Callvirt, (object)_getTaskAwaiter));
		list2.Add(new CodeInstruction(OpCodes.Stfld, (object)_awaiterField));
		list2.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
		list2.Add(new CodeInstruction(OpCodes.Ldflda, (object)_builderField));
		list2.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
		list2.Add(new CodeInstruction(OpCodes.Ldflda, (object)_awaiterField));
		list2.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
		list2.Add(new CodeInstruction(OpCodes.Call, (object)_awaitUnsafeOnCompleted));
		list2.Add(new CodeInstruction(OpCodes.Ret, (object)null));
		list2.AddRange(list);
		return list2;
	}

	public static bool ShouldSuspend()
	{
		if (_enabled)
		{
			return !Completion.Task.IsCompleted;
		}
		return false;
	}

	public static Task GetGateTask()
	{
		return Completion.Task;
	}

	private static void InstallEarlyInputGuards(Harmony harmony)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		HarmonyMethod guard = new HarmonyMethod(AccessTools.Method(typeof(StartupGate), "AllowGameInput", (Type[])null, (Type[])null));
		PatchInputMethod(harmony, guard, "MegaCrit.Sts2.Core.Nodes.CommonUi.NControllerManager", "_Input");
		PatchInputMethod(harmony, guard, "MegaCrit.Sts2.Core.Nodes.CommonUi.NHotkeyManager", "_UnhandledInput");
		PatchInputMethod(harmony, guard, "MegaCrit.Sts2.Core.Nodes.CommonUi.NInputManager", "_UnhandledInput");
		PatchInputMethod(harmony, guard, "MegaCrit.Sts2.Core.Nodes.CommonUi.NInputManager", "_UnhandledKeyInput");
	}

	private static void PatchInputMethod(Harmony harmony, HarmonyMethod guard, string typeName, string methodName)
	{
		Type type = typeof(OneTimeInitialization).Assembly.GetType(typeName);
		MethodInfo methodInfo = ((type == null) ? null : AccessTools.Method(type, methodName, (Type[])null, (Type[])null));
		if (methodInfo == null)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} early-input guard target missing: {typeName}.{methodName}");
		}
		else
		{
			harmony.Patch((MethodBase)methodInfo, guard, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}
	}

	private static bool AllowGameInput()
	{
		return !ShouldSuspend();
	}

	public static void ArmOverlay()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (Interlocked.Exchange(ref _armed, 1) == 0)
		{
			GD.Print("[ModLaunchManager] startup is suspended before ExecuteEssential.");
			Callable val = Callable.From((Action)ManagerOverlay.Show);
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
	}

	internal static void Complete()
	{
		_enabled = false;
		Completion.TrySetResult(result: true);
		GD.Print("[ModLaunchManager] startup gate opened.");
	}

	internal static void FailOpen(string reason)
	{
		_enabled = false;
		Completion.TrySetResult(result: true);
		GD.PushWarning("[ModLaunchManager] fail-open: " + reason + ".");
	}
}
