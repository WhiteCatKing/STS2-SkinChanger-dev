using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

namespace STS2SkinChanger.Core;

internal static class ProviderSettingsControls
{
	private static readonly FieldInfo? CommandsField = AccessTools.Field(typeof(DevConsole), "_commands");

	private static readonly List<(WeakReference<DevConsole> Console, bool Debug)> Consoles = new List<(WeakReference<DevConsole>, bool)>();

	private static readonly List<AbstractConsoleCmd> Commands = new List<AbstractConsoleCmd>();

	internal static bool TryRegisterCommand(IDictionary<string, AbstractConsoleCmd> commands, AbstractConsoleCmd command, bool allowDebug)
	{
		string name = command.CmdName;
		if ((command.DebugOnly && !allowDebug) || string.IsNullOrWhiteSpace(name) || commands.Keys.Any((string key) => key.Equals(name, StringComparison.OrdinalIgnoreCase)))
		{
			return false;
		}
		commands.Add(name.ToLowerInvariant(), command);
		return true;
	}

	internal static void Add(AbstractConsoleCmd command)
	{
		if (Commands.Any((AbstractConsoleCmd existing) => ((object)existing).GetType() == ((object)command).GetType()))
		{
			return;
		}
		Commands.Add(command);
		Consoles.RemoveAll(((WeakReference<DevConsole> Console, bool Debug) entry) => !entry.Console.TryGetTarget(out DevConsole _));
		foreach (var console in Consoles)
		{
			if (console.Console.TryGetTarget(out DevConsole target2))
			{
				Register(target2, console.Debug, command);
			}
		}
		ModLog.Info("已接入原皮肤设置命令：" + command.CmdName + "；不重新注册提供者全局类型或资源。");
	}

	internal static void Attach(DevConsole console, bool allowDebug)
	{
		Consoles.RemoveAll(((WeakReference<DevConsole> Console, bool Debug) entry) => !entry.Console.TryGetTarget(out DevConsole target) || target == console);
		Consoles.Add((new WeakReference<DevConsole>(console), allowDebug));
		foreach (AbstractConsoleCmd command in Commands)
		{
			Register(console, allowDebug, command);
		}
	}

	private static void Register(DevConsole console, bool allowDebug, AbstractConsoleCmd command)
	{
		if (CommandsField?.GetValue(console) is IDictionary<string, AbstractConsoleCmd> commands)
		{
			TryRegisterCommand(commands, command, allowDebug);
		}
	}
}
