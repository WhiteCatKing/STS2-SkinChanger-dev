using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace STS2SkinChanger.Core;

internal static class NativeFrameworkAssemblyLoader
{
	private static readonly Dictionary<string, Assembly> Loaded = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

	public static Assembly? Find(string path)
	{
		return Loaded.GetValueOrDefault(Path.GetFullPath(path));
	}

	public static bool IsOriginalFrameworkPath(string path)
	{
		if (!FrameworkCompatibilityLayer.IsKnownFrameworkHost(Path.GetFileNameWithoutExtension(path)))
		{
			return false;
		}
		string fullPath = Path.GetFullPath(path);
		return ModManager.Mods.Any((Mod mod) => FrameworkCompatibilityLayer.IsKnownFrameworkHost(mod.manifest?.id) && Path.GetFullPath(Path.Combine(mod.path, mod.manifest.id + ".dll")).Equals(fullPath, StringComparison.OrdinalIgnoreCase));
	}

	public static bool TryLoadCompatible(AssemblyLoadContext context, string path, out Assembly? assembly)
	{
		path = Path.GetFullPath(path);
		assembly = Find(path);
		if (assembly != null)
		{
			return AssemblyLoadContext.GetLoadContext(assembly) == context;
		}
		string identity = AssemblyName.GetAssemblyName(path).FullName;
		if (context.Assemblies.Any((Assembly candidate) => candidate.FullName == identity))
		{
			return false;
		}
		ProviderCompatibilityResult providerCompatibilityResult = ProviderAssemblyCompatibility.PrepareForCurrentGame(path);
		ProviderCompatibilityReport report = providerCompatibilityResult.Report;
		if (report.UnresolvedReferences.Count > 0)
		{
			ModLog.Warn($"原管理器 {Path.GetFileName(path)} 尚有 {report.UnresolvedReferences.Count} 处静态游戏接口未解析：" + string.Join("；", report.UnresolvedReferences.Take(8)) + "；未猜测替代调用，不据此阻止游戏原加载流程。");
		}
		MemoryStream assembly2 = providerCompatibilityResult.Assembly;
		if (assembly2 == null)
		{
			if (report.Failure != null)
			{
				ModLog.Warn("原管理器游戏接口检查失败，保留游戏加载路径：" + report.Failure);
			}
			return false;
		}
		using (assembly2)
		{
			using MemoryStream assembly3 = PreserveEntryLocation(assembly2, path);
			assembly = context.LoadFromStream(assembly3);
		}
		Loaded.Add(path, assembly);
		ModLog.Info($"原管理器按正常加载流程桥接 {report.Changes.Count} 处已知游戏版本接口：{Path.GetFileName(path)}；" + "全部原功能保留，原 DLL/PCK 未修改。");
		return true;
	}

	private static MemoryStream PreserveEntryLocation(MemoryStream source, string path)
	{
		Assembly assembly = typeof(Harmony).Assembly;
		object obj = assembly.GetType("Mono.Cecil.AssemblyDefinition", throwOnError: true).GetMethod("ReadAssembly", new Type[1] { typeof(Stream) }).Invoke(null, new object[1] { source });
		try
		{
			object value2 = Property(obj, "MainModule");
			Type type = assembly.GetType("Mono.Cecil.Cil.OpCodes", throwOnError: true);
			Type type2 = assembly.GetType("Mono.Cecil.Cil.Instruction", throwOnError: true);
			MethodInfo methodInfo = type2.GetMethods().Single(delegate(MethodInfo method)
			{
				if (method.Name == "Create")
				{
					ParameterInfo[] parameters = method.GetParameters();
					if (parameters != null && parameters.Length == 2)
					{
						return parameters[1].ParameterType == typeof(string);
					}
				}
				return false;
			});
			foreach (object item in ((IEnumerable)Property(value2, "Types")).Cast<object>())
			{
				if (!((IEnumerable)Property(item, "CustomAttributes")).Cast<object>().Any((object attribute) => (string)Property(Property(attribute, "AttributeType"), "FullName") == "MegaCrit.Sts2.Core.Modding.ModInitializerAttribute"))
				{
					continue;
				}
				foreach (object item2 in ((IEnumerable)Property(item, "Methods")).Cast<object>())
				{
					if ((string)Property(item2, "Name") != ".cctor")
					{
						continue;
					}
					object obj2 = Property(item2, "Body");
					object[] array = ((IEnumerable)Property(obj2, "Instructions")).Cast<object>().ToArray();
					object obj3 = obj2.GetType().GetMethod("GetILProcessor").Invoke(obj2, null);
					for (int i = 3; i < array.Length; i++)
					{
						if (!(Property(array[i], "Operand")?.ToString() != "System.String System.Reflection.Assembly::get_Location()") && !(Property(array[i - 3], "Operand")?.ToString() != (string)Property(item, "FullName")))
						{
							array[i].GetType().GetProperty("OpCode").SetValue(array[i], type.GetField("Pop").GetValue(null));
							array[i].GetType().GetProperty("Operand").SetValue(array[i], null);
							object obj4 = methodInfo.Invoke(null, new object[2]
							{
								type.GetField("Ldstr").GetValue(null),
								path
							});
							obj3.GetType().GetMethod("InsertAfter", new Type[2] { type2, type2 }).Invoke(obj3, new object[2]
							{
								array[i],
								obj4
							});
						}
					}
				}
			}
			MemoryStream memoryStream = new MemoryStream();
			obj.GetType().GetMethod("Write", new Type[1] { typeof(Stream) }).Invoke(obj, new object[1] { memoryStream });
			memoryStream.Position = 0L;
			return memoryStream;
		}
		finally
		{
			((IDisposable)obj).Dispose();
		}
		static object Property(object value, string name)
		{
			return value.GetType().GetProperty(name).GetValue(value);
		}
	}
}
