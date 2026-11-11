using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace ModLaunchManager;

internal static class RuntimeTypeInitialization
{
	private sealed class Probe
	{
		static Probe()
		{
			_probeRuns++;
		}
	}

	private sealed class FailedProbe
	{
		static FailedProbe()
		{
			throw new InvalidOperationException("Initialization-state self-check");
		}
	}

	private static int _probeRuns;

	private static readonly Func<Type, bool>? ReadInitialized = CreateReader();

	internal static bool CanReadStatics(Type type)
	{
		if (type.ContainsGenericParameters || type.HasElementType || type.IsGenericParameter || type.IsFunctionPointer)
		{
			return false;
		}
		if (type.TypeInitializer == null)
		{
			return true;
		}
		try
		{
			return ReadInitialized?.Invoke(type) ?? false;
		}
		catch
		{
			return false;
		}
	}

	private static Func<Type, bool>? CreateReader()
	{
		try
		{
			if (Environment.Version.Major != 9 || typeof(object).Assembly.GetName().Name != "System.Private.CoreLib")
			{
				return null;
			}
			Assembly assembly = typeof(object).Assembly;
			Type type2 = assembly.GetType("System.Runtime.CompilerServices.MethodTable");
			Type type3 = assembly.GetType("System.Runtime.CompilerServices.MethodTableAuxiliaryData");
			if ((object)type2 == null || !type2.IsExplicitLayout || (object)type3 == null || !type3.IsExplicitLayout)
			{
				return null;
			}
			FieldInfo field = type2.GetField("AuxiliaryData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo field2 = type3.GetField("Flags", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)field == null || !field.FieldType.IsPointer || field.FieldType.GetElementType() != type3 || field2?.FieldType != typeof(uint))
			{
				return null;
			}
			int? dataOffset = field.GetCustomAttribute<FieldOffsetAttribute>()?.Value;
			int? flagsOffset = field2.GetCustomAttribute<FieldOffsetAttribute>()?.Value;
			if (!dataOffset.HasValue || dataOffset.GetValueOrDefault() < 0 || !flagsOffset.HasValue || flagsOffset.GetValueOrDefault() < 0)
			{
				return null;
			}
			if (Read(typeof(Probe)) || Read(typeof(FailedProbe)) || _probeRuns != 0)
			{
				return null;
			}
			RuntimeHelpers.RunClassConstructor(typeof(Probe).TypeHandle);
			if (!Read(typeof(Probe)) || _probeRuns != 1)
			{
				return null;
			}
			try
			{
				RuntimeHelpers.RunClassConstructor(typeof(FailedProbe).TypeHandle);
			}
			catch (TypeInitializationException)
			{
			}
			if (Read(typeof(FailedProbe)))
			{
				return null;
			}
			return Read;
			bool Read(Type type)
			{
				if (type.GetType() != typeof(object).GetType() || type.HasElementType || type.ContainsGenericParameters || type.IsGenericParameter || type.IsFunctionPointer)
				{
					return false;
				}
				try
				{
					nint num = Marshal.ReadIntPtr(type.TypeHandle.Value, dataOffset.Value);
					if (num == 0)
					{
						return false;
					}
					int num2 = Marshal.ReadInt32(num, flagsOffset.Value);
					Thread.MemoryBarrier();
					return (num2 & 1) != 0;
				}
				finally
				{
					GC.KeepAlive(type);
				}
			}
		}
		catch
		{
			return null;
		}
	}
}
