using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Godot;

namespace JoanofArcIroncladHpwear;

internal static class RitsuSettingsLoader
{
	private static bool registered;

	private static bool watching;

	private static bool registering;

	private static bool retryQueued;

	private static Assembly? bridge;

	private static readonly object Sync = new object();

	private static bool Compatible(AssemblyName expected, AssemblyName actual)
	{
		if (expected.Name == actual.Name && actual.Version != null && expected.Version != null && actual.Version.Major == expected.Version.Major && actual.Version.Minor == expected.Version.Minor)
		{
			return actual.Version >= expected.Version;
		}
		return false;
	}

	private static Assembly? ResolveLoaded(AssemblyLoadContext context, AssemblyName name)
	{
		if (name.Name == typeof(RitsuSettingsLoader).Assembly.GetName().Name)
		{
			return typeof(RitsuSettingsLoader).Assembly;
		}
		string? name2 = name.Name;
		if (name2 == null || !name2.StartsWith("STS2-RitsuLib", StringComparison.Ordinal))
		{
			return null;
		}
		return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => Compatible(name, a.GetName()));
	}

	private static void Loaded(object? sender, AssemblyLoadEventArgs args)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		string? name = args.LoadedAssembly.GetName().Name;
		if (name == null || !name.StartsWith("STS2-RitsuLib", StringComparison.Ordinal))
		{
			return;
		}
		lock (Sync)
		{
			if (registered || retryQueued)
			{
				return;
			}
			retryQueued = true;
		}
		try
		{
			Callable val = Callable.From((Action)Retry);
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		catch
		{
			lock (Sync)
			{
				retryQueued = false;
			}
		}
	}

	private static void Retry()
	{
		lock (Sync)
		{
			retryQueued = false;
		}
		TryRegister();
	}

	internal static void TryRegister()
	{
		if (registered || registering)
		{
			return;
		}
		if (!watching)
		{
			AppDomain.CurrentDomain.AssemblyLoad += Loaded;
			watching = true;
		}
		registering = true;
		try
		{
			Assembly assembly = typeof(RitsuSettingsLoader).Assembly;
			AssemblyLoadContext assemblyLoadContext = AssemblyLoadContext.GetLoadContext(assembly) ?? AssemblyLoadContext.Default;
			using Stream stream = assembly.GetManifestResourceStream("Joan.RitsuDependencies") ?? throw new InvalidOperationException("Ritsu dependency manifest missing");
			using StreamReader streamReader = new StreamReader(stream);
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					AssemblyName assemblyName = new AssemblyName(text);
					Assembly assembly2 = ResolveLoaded(assemblyLoadContext, assemblyName) ?? assemblyLoadContext.LoadFromAssemblyName(new AssemblyName(assemblyName.Name));
					if (!Compatible(assemblyName, assembly2.GetName()))
					{
						throw new InvalidOperationException("Incompatible RitsuLib: " + assembly2.FullName);
					}
				}
			}
			assemblyLoadContext.Resolving -= ResolveLoaded;
			assemblyLoadContext.Resolving += ResolveLoaded;
			if (bridge == null)
			{
				using Stream assembly3 = assembly.GetManifestResourceStream("Joan.RitsuBridge") ?? throw new InvalidOperationException("Ritsu settings bridge missing");
				bridge = assemblyLoadContext.LoadFromStream(assembly3);
			}
			bridge.GetType("JoanofArcIroncladHpwear.SettingsBridge.RitsuSettingsBridge", throwOnError: true).GetMethod("Register", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);
			registered = true;
			AppDomain.CurrentDomain.AssemblyLoad -= Loaded;
			watching = false;
			ModEntry.TryInfo("RitsuLib 贞德血量换装设置已注册。");
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("RitsuLib 设置暂不可用，保留默认换装：" + ex.GetBaseException().Message);
		}
		finally
		{
			registering = false;
		}
	}
}
