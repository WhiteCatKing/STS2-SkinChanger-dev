using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace STS2SkinChanger.Core;

internal static class ModPrivateDependencyResolver
{
	private static readonly object Sync = new object();

	private static readonly ConditionalWeakTable<Assembly, object> NativeProbes = new ConditionalWeakTable<Assembly, object>();

	private static readonly HashSet<string> Reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static Action<string>? _info;

	private static Action<string>? _warn;

	private static bool _installed;

	public static void Install(Action<string>? info = null, Action<string>? warn = null)
	{
		lock (Sync)
		{
			if (!_installed)
			{
				_installed = true;
				_info = info;
				_warn = warn;
				AppDomain.CurrentDomain.AssemblyResolve += Resolve;
			}
		}
	}

	private static Assembly? Resolve(object? sender, ResolveEventArgs args)
	{
		try
		{
			Assembly requestingAssembly = args.RequestingAssembly;
			if (requestingAssembly == null)
			{
				return null;
			}
			string requested = new AssemblyName(args.Name).Name;
			string text = ModPrivateDependencyPolicy.CandidatePath(requestingAssembly.Location, requested, AppContext.BaseDirectory, RuntimeEnvironment.GetRuntimeDirectory(), File.Exists);
			if (text == null)
			{
				return null;
			}
			AssemblyLoadContext assemblyLoadContext = AssemblyLoadContext.GetLoadContext(requestingAssembly) ?? AssemblyLoadContext.Default;
			Assembly assembly2 = assemblyLoadContext.Assemblies.FirstOrDefault((Assembly assembly) => string.Equals(assembly.GetName().Name, requested, StringComparison.OrdinalIgnoreCase));
			if (assembly2 != null)
			{
				return assembly2;
			}
			Assembly assembly3 = assemblyLoadContext.LoadFromAssemblyPath(text);
			AttachNativeProbe(assembly3, Path.GetDirectoryName(text));
			Report(requestingAssembly, assembly3);
			return assembly3;
		}
		catch (Exception ex)
		{
			_warn?.Invoke("按 Mod 自身目录解析随包依赖失败：" + ex.GetBaseException().Message);
			return null;
		}
	}

	private static void AttachNativeProbe(Assembly assembly, string directory)
	{
		if (NativeProbes.TryGetValue(assembly, out object _))
		{
			return;
		}
		try
		{
			NativeLibrary.SetDllImportResolver(assembly, (string name, Assembly _, DllImportSearchPath? _) => TryLoadNativeProbe(directory, name));
			NativeProbes.Add(assembly, new object());
		}
		catch (Exception ex)
		{
			_warn?.Invoke("无法为 " + assembly.GetName().Name + " 挂载随包原生依赖探测：" + ex.GetBaseException().Message);
		}
	}

	internal static nint TryLoadNativeProbe(string directory, string libraryName)
	{
		if (string.IsNullOrWhiteSpace(libraryName))
		{
			return IntPtr.Zero;
		}
		string text = Path.Combine(directory, libraryName + ".dll");
		if (!File.Exists(text))
		{
			return IntPtr.Zero;
		}
		try
		{
			return NativeLibrary.Load(text);
		}
		catch (Exception ex)
		{
			_warn?.Invoke("随包原生依赖 " + text + " 加载失败：" + ex.GetBaseException().Message);
			return IntPtr.Zero;
		}
	}

	private static void Report(Assembly requesting, Assembly resolved)
	{
		string item = requesting.GetName().Name + " -> " + resolved.GetName().Name;
		lock (Sync)
		{
			if (!Reported.Add(item))
			{
				return;
			}
		}
		_info?.Invoke($"已按 {requesting.GetName().Name} 自己的目录解析随包依赖 {resolved.GetName().Name}；" + "游戏加载器只探测游戏目录，这类依赖原本无法加载。");
	}
}
