using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace STS2SkinChanger.Core;

internal static class ProviderAssemblyCompatibility
{
	[CompilerGenerated]
	private sealed class _003CEnumerateTypeAndNested_003Ed__21 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private object type;

		public object _003C_003E3__type;

		private IEnumerator<object> _003C_003E7__wrap1;

		private IEnumerator<object> _003C_003E7__wrap2;

		object IEnumerator<object>.Current
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
		public _003CEnumerateTypeAndNested_003Ed__21(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 2)
			{
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E2__current = type;
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = Enumerate(GetRequiredProperty(type, "NestedTypes")).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00d9;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00bf;
					}
					IL_00d9:
					if (_003C_003E7__wrap1.MoveNext())
					{
						object current = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap2 = EnumerateTypeAndNested(current).GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00bf;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
					IL_00bf:
					if (_003C_003E7__wrap2.MoveNext())
					{
						object current2 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current2;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_00d9;
				}
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
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			_003CEnumerateTypeAndNested_003Ed__21 _003CEnumerateTypeAndNested_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateTypeAndNested_003Ed__ = this;
			}
			else
			{
				_003CEnumerateTypeAndNested_003Ed__ = new _003CEnumerateTypeAndNested_003Ed__21(0);
			}
			_003CEnumerateTypeAndNested_003Ed__.type = _003C_003E3__type;
			return _003CEnumerateTypeAndNested_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateTypes_003Ed__20 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private object module;

		public object _003C_003E3__module;

		private IEnumerator<object> _003C_003E7__wrap1;

		private IEnumerator<object> _003C_003E7__wrap2;

		object IEnumerator<object>.Current
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
		public _003CEnumerateTypes_003Ed__20(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 1)
			{
				try
				{
					if (num == -4 || num == 1)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E7__wrap2 = null;
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
					_003C_003E1__state = -4;
					goto IL_0093;
				}
				_003C_003E1__state = -1;
				_003C_003E7__wrap1 = Enumerate(GetRequiredProperty(module, "Types")).GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_00ad;
				IL_0093:
				if (_003C_003E7__wrap2.MoveNext())
				{
					object current = _003C_003E7__wrap2.Current;
					_003C_003E2__current = current;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally2();
				_003C_003E7__wrap2 = null;
				goto IL_00ad;
				IL_00ad:
				if (_003C_003E7__wrap1.MoveNext())
				{
					object current2 = _003C_003E7__wrap1.Current;
					_003C_003E7__wrap2 = EnumerateTypeAndNested(current2).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_0093;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
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
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			_003CEnumerateTypes_003Ed__20 _003CEnumerateTypes_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateTypes_003Ed__ = this;
			}
			else
			{
				_003CEnumerateTypes_003Ed__ = new _003CEnumerateTypes_003Ed__20(0);
			}
			_003CEnumerateTypes_003Ed__.module = _003C_003E3__module;
			return _003CEnumerateTypes_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	private const string MegaAnimationStateTypeName = "MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState";

	private const string SpineAnimationAccessTypeName = "MegaCrit.Sts2.Core.Bindings.MegaSpine.SpineAnimationAccess";

	private const string MegaSpineBindingTypeName = "MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSpineBinding";

	private static readonly IReadOnlyDictionary<string, string> MegaAnimationMethodAliases = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["AddAnimation"] = "AddAnimationTracked",
		["AddAnimationTracked"] = "AddAnimation"
	};

	private static readonly Assembly GameAssembly = typeof(MegaSprite).Assembly;

	public static ProviderRuntimeAudit AuditForCurrentGame(string assemblyPath)
	{
		if (string.IsNullOrWhiteSpace(assemblyPath) || !File.Exists(assemblyPath))
		{
			return new ProviderRuntimeAudit(CanRun: false, 0, "皮肤 DLL 不存在");
		}
		try
		{
			ProviderCompatibilityResult providerCompatibilityResult = PrepareForCurrentGame(assemblyPath);
			if (providerCompatibilityResult.Report.Failure != null)
			{
				providerCompatibilityResult.Assembly?.Dispose();
				return new ProviderRuntimeAudit(CanRun: false, 0, providerCompatibilityResult.Report.Failure);
			}
			AssemblyLoadContext context = new AssemblyLoadContext("sc-provider-audit", isCollectible: true);
			string providerDirectory = Path.GetDirectoryName(Path.GetFullPath(assemblyPath));
			if (!string.IsNullOrWhiteSpace(providerDirectory))
			{
				context.Resolving += (AssemblyLoadContext _, AssemblyName name) => ResolvePrivateDependency(context, providerDirectory, name);
			}
			try
			{
				MemoryStream assembly = providerCompatibilityResult.Assembly;
				Assembly assembly2;
				if (assembly != null)
				{
					using (assembly)
					{
						assembly2 = context.LoadFromStream(assembly);
					}
				}
				else
				{
					using MemoryStream assembly3 = new MemoryStream(File.ReadAllBytes(assemblyPath), writable: false);
					assembly2 = context.LoadFromStream(assembly3);
				}
				Type[] types;
				try
				{
					types = assembly2.GetTypes();
				}
				catch (ReflectionTypeLoadException ex)
				{
					return new ProviderRuntimeAudit(CanRun: false, 0, string.Join("；", from loader in ex.LoaderExceptions.Take(3)
						select loader?.GetBaseException().Message ?? "未知类型加载失败"));
				}
				int num = 0;
				foreach (var (text, methodInfo) in types.SelectMany((Type type) => from method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
					select (Owner: type.FullName ?? type.Name, Method: method)).Concat<(string, MethodInfo)>(from method in assembly2.ManifestModule.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
					select (Owner: "<Module>", Method: method)))
				{
					if (!methodInfo.ContainsGenericParameters && methodInfo.GetMethodBody() != null)
					{
						try
						{
							RuntimeHelpers.PrepareMethod(methodInfo.MethodHandle);
							num++;
						}
						catch (Exception ex2)
						{
							return new ProviderRuntimeAudit(CanRun: false, num, text + "." + methodInfo.Name + " 无法在当前游戏编译：" + ex2.GetBaseException().Message);
						}
					}
				}
				return new ProviderRuntimeAudit(CanRun: true, num, string.Empty);
			}
			finally
			{
				context.Unload();
			}
		}
		catch (Exception ex3)
		{
			return new ProviderRuntimeAudit(CanRun: false, 0, ex3.GetBaseException().Message);
		}
	}

	private static Assembly? ResolvePrivateDependency(AssemblyLoadContext context, string providerDirectory, AssemblyName name)
	{
		if (string.IsNullOrWhiteSpace(name.Name))
		{
			return null;
		}
		string text = Path.Combine(providerDirectory, name.Name + ".dll");
		if (!File.Exists(text))
		{
			return null;
		}
		try
		{
			return context.LoadFromAssemblyPath(text);
		}
		catch (Exception ex) when (((ex is BadImageFormatException || ex is FileLoadException) ? 1 : 0) != 0)
		{
			return null;
		}
	}

	public static bool TryRewriteForCurrentGame(string assemblyPath, out MemoryStream? rewrittenAssembly, out int rewrittenCalls, out string? failure)
	{
		ProviderCompatibilityResult providerCompatibilityResult = PrepareForCurrentGame(assemblyPath);
		rewrittenAssembly = providerCompatibilityResult.Assembly;
		rewrittenCalls = providerCompatibilityResult.Report.Changes.Count;
		failure = providerCompatibilityResult.Report.Failure;
		return rewrittenAssembly != null;
	}

	private static MethodInfo[] FindRuntimeMegaAnimationMethods()
	{
		return (from name in new string[2] { "MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState", "MegaCrit.Sts2.Core.Bindings.MegaSpine.SpineAnimationAccess" }
			select GameAssembly.GetType(name) into type
			where type != null
			select type).SelectMany((Type type) => type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)).ToArray();
	}

	private static bool TryRewriteRuntimeDrift(string assemblyPath, IReadOnlyList<MethodInfo> runtimeMethods, ProviderCompatibilityReport report, out MemoryStream? rewrittenAssembly, out int rewrittenCalls, out string? failure)
	{
		rewrittenAssembly = null;
		rewrittenCalls = 0;
		failure = null;
		Assembly assembly = typeof(Harmony).Assembly;
		Type type = assembly.GetType("Mono.Cecil.AssemblyDefinition");
		Type type2 = assembly.GetType("Mono.Cecil.MethodReference");
		Type type3 = assembly.GetType("Mono.Cecil.Cil.Instruction");
		Type type4 = assembly.GetType("Mono.Cecil.Cil.OpCodes");
		if (type == null || type2 == null || type3 == null || type4 == null)
		{
			failure = "当前 Harmony 未提供兼容重写器所需的 IL 元数据接口";
			return false;
		}
		MethodInfo method2 = type.GetMethod("ReadAssembly", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(Stream) }, null);
		MethodInfo method3 = type.GetMethod("Write", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(Stream) }, null);
		MethodInfo methodInfo = type3.GetMethods(BindingFlags.Static | BindingFlags.Public).SingleOrDefault((MethodInfo method) => method.Name == "Create" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType.FullName == "Mono.Cecil.Cil.OpCode");
		object obj = type4.GetField("Pop", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
		object obj2 = type4.GetField("Nop", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
		if (method2 == null || method3 == null || methodInfo == null || obj == null || obj2 == null)
		{
			failure = "当前 Harmony 的 IL 元数据接口与兼容重写器不匹配";
			return false;
		}
		using MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(assemblyPath), writable: false);
		object obj3 = method2.Invoke(null, new object[1] { memoryStream });
		if (obj3 == null)
		{
			failure = "无法读取皮肤 DLL 元数据";
			return false;
		}
		try
		{
			object requiredProperty = GetRequiredProperty(obj3, "MainModule");
			RewriteKnownTypeMoves(requiredProperty, report);
			MethodInfo methodInfo2 = requiredProperty.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Single((MethodInfo method) => method.Name == "ImportReference" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(MethodBase));
			Dictionary<MethodInfo, object> dictionary = new Dictionary<MethodInfo, object>();
			Dictionary<object, object> dictionary2 = new Dictionary<object, object>();
			foreach (object item in EnumerateTypes(requiredProperty))
			{
				object[] array = Enumerate(GetRequiredProperty(item, "Methods")).ToArray();
				foreach (object obj4 in array)
				{
					if (!(bool)GetRequiredProperty(obj4, "HasBody"))
					{
						continue;
					}
					object requiredProperty2 = GetRequiredProperty(obj4, "Body");
					int num = rewrittenCalls;
					object[] array2 = Enumerate(GetRequiredProperty(requiredProperty2, "Instructions")).ToArray();
					object obj5 = requiredProperty2.GetType().GetMethod("GetILProcessor", BindingFlags.Instance | BindingFlags.Public)?.Invoke(requiredProperty2, null);
					if (obj5 == null)
					{
						throw new InvalidOperationException("无法取得皮肤 DLL 的 IL 编辑器");
					}
					for (int j = 0; j < array2.Length; j++)
					{
						object obj6 = array2[j];
						PropertyInfo property = obj6.GetType().GetProperty("Operand", BindingFlags.Instance | BindingFlags.Public);
						object obj7 = property?.GetValue(obj6);
						bool flag = obj7 == null;
						if (!flag)
						{
							string instructionOpCodeName = GetInstructionOpCodeName(obj6);
							bool flag2 = ((instructionOpCodeName == "call" || instructionOpCodeName == "callvirt") ? true : false);
							flag = !flag2;
						}
						if (flag || !type2.IsInstanceOfType(obj7) || (j > 0 && GetRequiredProperty(GetRequiredProperty(array2[j - 1], "OpCode"), "OpCodeType").ToString() == "Prefix"))
						{
							continue;
						}
						if (TryRewriteTaskReturn(requiredProperty, obj6, obj7, report))
						{
							rewrittenCalls++;
							continue;
						}
						if (TryRewriteUnsupportedDispose(obj4, requiredProperty2, array2, j, obj7, requiredProperty, assembly, type4))
						{
							rewrittenCalls++;
							report.Changes.Add($"Spine wrapper cleanup: {obj7}");
							continue;
						}
						MethodInfo runtimeMethod = FindMatchingRuntimeMethod(obj7, runtimeMethods);
						if (runtimeMethod == null)
						{
							continue;
						}
						string text = runtimeMethod.ReturnType.FullName ?? runtimeMethod.ReturnType.Name;
						string providerReturnName = GetTypeFullName(GetRequiredProperty(obj7, "ReturnType"));
						string a = GetRequiredProperty(obj7, "Name") as string;
						if (providerReturnName == text && string.Equals(a, runtimeMethod.Name, StringComparison.Ordinal))
						{
							continue;
						}
						int num2 = rewrittenCalls;
						try
						{
							if (!dictionary.TryGetValue(runtimeMethod, out var value))
							{
								value = methodInfo2.Invoke(requiredProperty, new object[1] { runtimeMethod }) ?? throw new InvalidOperationException("无法导入当前游戏的动画接口");
								dictionary[runtimeMethod] = value;
							}
							if (providerReturnName == text)
							{
								property.SetValue(obj6, value);
								rewrittenCalls++;
								continue;
							}
							if (providerReturnName == typeof(void).FullName && runtimeMethod.ReturnType != typeof(void))
							{
								property.SetValue(obj6, value);
								object obj8 = methodInfo.Invoke(null, new object[1] { obj }) ?? throw new InvalidOperationException("无法生成返回值清理指令");
								InvokeProcessor(obj5, "InsertAfter", obj6, obj8);
								rewrittenCalls++;
								continue;
							}
							if (runtimeMethod.ReturnType == typeof(void) && TryReplaceFollowingPopWithNop(requiredProperty2, array2, obj6, obj2))
							{
								property.SetValue(obj6, value);
								rewrittenCalls++;
								continue;
							}
							string instructionOpCodeName = GetInstructionOpCodeName(obj6);
							flag = ((instructionOpCodeName == "call" || instructionOpCodeName == "callvirt") ? true : false);
							if (flag && runtimeMethod.DeclaringType?.FullName == "MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState" && runtimeMethod.Name == "SetAnimation" && runtimeMethod.ReturnType == typeof(void) && providerReturnName == "MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaTrackEntry" && (from parameter in runtimeMethod.GetParameters()
								select parameter.ParameterType).SequenceEqual(new Type[3]
							{
								typeof(string),
								typeof(bool),
								typeof(int)
							}))
							{
								MethodInfo methodInfo3 = runtimeMethods.SingleOrDefault((MethodInfo candidate) => candidate.Name == "GetCurrent" && candidate.DeclaringType == runtimeMethod.DeclaringType && candidate.ReturnType.FullName == providerReturnName && (from parameter in candidate.GetParameters()
									select parameter.ParameterType).SequenceEqual(new Type[1] { typeof(int) }));
								if (!(methodInfo3 == null))
								{
									if (!dictionary2.TryGetValue(item, out var value2))
									{
										object getCurrent = methodInfo2.Invoke(requiredProperty, new object[1] { methodInfo3 });
										value2 = CreateTrackedSetAnimationAdapter(assembly, item, value, getCurrent, type4);
										dictionary2.Add(item, value2);
									}
									SetInstruction(obj6, type4.GetField("Call").GetValue(null), value2);
									rewrittenCalls++;
								}
							}
							else
							{
								flag = runtimeMethod.DeclaringType?.FullName == "MegaCrit.Sts2.Core.Bindings.MegaSpine.SpineAnimationAccess" && runtimeMethod.ReturnType == typeof(void) && providerReturnName == "MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaTrackEntry";
								if (flag)
								{
									instructionOpCodeName = runtimeMethod.Name;
									bool flag2 = ((instructionOpCodeName == "SetAnimation" || instructionOpCodeName == "AddAnimation") ? true : false);
									flag = flag2;
								}
								if (flag)
								{
									object operand = CreateAccessTrackAdapter(requiredProperty, item, runtimeMethod, assembly, type4);
									SetInstruction(obj6, type4.GetField("Call").GetValue(null), operand);
									rewrittenCalls++;
								}
							}
						}
						finally
						{
							if (rewrittenCalls != num2)
							{
								report.Changes.Add($"Animation signature: {obj7} -> {runtimeMethod.DeclaringType?.FullName}.{runtimeMethod.Name}");
							}
						}
					}
					if (rewrittenCalls != num)
					{
						ExpandShortBranches(requiredProperty2, type4);
					}
				}
			}
			if (report.Changes.Count == 0)
			{
				AuditGameReferences(requiredProperty, report);
				return false;
			}
			rewrittenAssembly = new MemoryStream();
			method3.Invoke(obj3, new object[1] { rewrittenAssembly });
			rewrittenAssembly.Position = 0L;
			object obj9 = method2.Invoke(null, new object[1] { rewrittenAssembly });
			try
			{
				AuditGameReferences(GetRequiredProperty(obj9, "MainModule"), report);
			}
			finally
			{
				(obj9 as IDisposable)?.Dispose();
			}
			rewrittenAssembly.Position = 0L;
			return true;
		}
		finally
		{
			(obj3 as IDisposable)?.Dispose();
		}
	}

	private static object CreateTrackedSetAnimationAdapter(Assembly cecilAssembly, object ownerType, object setAnimation, object getCurrent, Type opCodesType)
	{
		Type type = cecilAssembly.GetType("Mono.Cecil.MethodDefinition", throwOnError: true);
		Type type2 = cecilAssembly.GetType("Mono.Cecil.MethodAttributes", throwOnError: true);
		Type type3 = cecilAssembly.GetType("Mono.Cecil.ParameterDefinition", throwOnError: true);
		Type instructionType = cecilAssembly.GetType("Mono.Cecil.Cil.Instruction", throwOnError: true);
		object requiredProperty = GetRequiredProperty(ownerType, "Methods");
		HashSet<string> hashSet = (from method in Enumerate(requiredProperty)
			select (string)GetRequiredProperty(method, "Name")).ToHashSet<string>(StringComparer.Ordinal);
		string text = "__SkinChanger_SetAnimationTracked";
		int num = 1;
		while (hashSet.Contains(text))
		{
			text = "__SkinChanger_SetAnimationTracked_" + num;
			num++;
		}
		object obj = Activator.CreateInstance(type, text, Enum.Parse(type2, "Private, Static, HideBySig"), GetRequiredProperty(getCurrent, "ReturnType"));
		object requiredProperty2 = GetRequiredProperty(obj, "Parameters");
		foreach (object item in new object[1] { GetRequiredProperty(setAnimation, "DeclaringType") }.Concat(from parameter in Enumerate(GetRequiredProperty(setAnimation, "Parameters"))
			select GetRequiredProperty(parameter, "ParameterType")))
		{
			InvokeProcessor(requiredProperty2, "Add", Activator.CreateInstance(type3, item));
		}
		InvokeProcessor(requiredProperty, "Add", obj);
		object requiredProperty3 = GetRequiredProperty(obj, "Body");
		object processor = requiredProperty3.GetType().GetMethod("GetILProcessor").Invoke(requiredProperty3, null);
		Emit("Ldarg_0");
		Emit("Ldarg_1");
		Emit("Ldarg_2");
		Emit("Ldarg_3");
		Emit("Callvirt", setAnimation);
		Emit("Ldarg_0");
		Emit("Ldarg_3");
		Emit("Callvirt", getCurrent);
		Emit("Ret");
		return obj;
		void Emit(string opCodeName, object? operand = null)
		{
			object value = opCodesType.GetField(opCodeName).GetValue(null);
			object[] arguments = ((operand != null) ? new object[2] { value, operand } : new object[1] { value });
			MethodInfo methodInfo = instructionType.GetMethods(BindingFlags.Static | BindingFlags.Public).Single((MethodInfo candidate) => candidate.Name == "Create" && candidate.GetParameters().Length == arguments.Length && candidate.GetParameters().Select((ParameterInfo parameter, int index) => parameter.ParameterType.IsInstanceOfType(arguments[index])).All((bool matches) => matches));
			InvokeProcessor(processor, "Append", methodInfo.Invoke(null, arguments));
		}
	}

	private static bool TryRewriteUnsupportedDispose(object method, object body, IReadOnlyList<object> instructions, int instructionIndex, object methodReference, object module, Assembly cecilAssembly, Type opCodesType)
	{
		if (!string.Equals(GetRequiredProperty(methodReference, "Name") as string, "Dispose", StringComparison.Ordinal) || !(bool)GetRequiredProperty(methodReference, "HasThis") || GetTypeFullName(GetRequiredProperty(methodReference, "ReturnType")) != "System.Void" || Enumerate(GetRequiredProperty(methodReference, "Parameters")).Any())
		{
			return false;
		}
		int num = instructionIndex - 1;
		if (num >= 0 && string.Equals(GetInstructionOpCodeName(instructions[num]), "constrained.", StringComparison.Ordinal))
		{
			return false;
		}
		object requiredProperty = GetRequiredProperty(methodReference, "DeclaringType");
		string typeFullName = GetTypeFullName(requiredProperty);
		Type type = GameAssembly.GetType("MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSpineBinding");
		if (type == null)
		{
			return false;
		}
		Type type2 = ((num < 0) ? null : ResolveLoadedGameType(method, body, instructions[num]));
		if (type2 == null)
		{
			Type type3 = (IsGameTypeReference(requiredProperty) ? GameAssembly.GetType(typeFullName) : null);
			if (GetInstructionOpCodeName(instructions[instructionIndex]) != "call" || type3 == null || !type.IsAssignableFrom(type3) || HasInstanceDispose(type3))
			{
				return false;
			}
			goto IL_01f4;
		}
		string text = (string)GetRequiredProperty(GetRequiredProperty(requiredProperty, "Scope"), "Name");
		bool flag = typeFullName == typeof(IDisposable).FullName;
		if (flag)
		{
			bool flag2;
			switch (text)
			{
			case "System.Private.CoreLib":
			case "System.Runtime":
			case "mscorlib":
			case "netstandard":
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		if (flag)
		{
			goto IL_01d5;
		}
		if (IsGameTypeReference(requiredProperty))
		{
			Type type4 = GameAssembly.GetType(typeFullName);
			if ((object)type4 != null && type.IsAssignableFrom(type4))
			{
				goto IL_01d5;
			}
		}
		goto IL_01f2;
		IL_01f2:
		return false;
		IL_01d5:
		if (!type.IsAssignableFrom(type2) || typeof(IDisposable).IsAssignableFrom(type2))
		{
			goto IL_01f2;
		}
		goto IL_01f4;
		IL_01f4:
		MethodInfo methodInfo = type.GetProperty("BoundObject")?.GetMethod;
		MethodInfo methodInfo2 = methodInfo?.ReturnType.GetMethod("Dispose", Type.EmptyTypes);
		if (methodInfo == null || methodInfo2 == null)
		{
			return false;
		}
		object obj = NewAdapter(module, GetRequiredProperty(method, "DeclaringType"), "DisposeSpine", Import(module, typeof(void)), new _003C_003Ez__ReadOnlySingleElementList<object>(Import(module, type)), cecilAssembly);
		Emit(obj, "Ldarg_0", cecilAssembly, opCodesType);
		Emit(obj, "Callvirt", cecilAssembly, opCodesType, Import(module, methodInfo));
		Emit(obj, "Callvirt", cecilAssembly, opCodesType, Import(module, methodInfo2));
		Emit(obj, "Ret", cecilAssembly, opCodesType);
		SetInstruction(instructions[instructionIndex], opCodesType.GetField("Call").GetValue(null), obj);
		return true;
	}

	private static Type? ResolveLoadedGameType(object method, object body, object instruction)
	{
		object obj = TryGetLoadedValueType(method, body, instruction);
		if (obj == null || !IsGameTypeReference(obj))
		{
			return null;
		}
		return GameAssembly.GetType(GetTypeFullName(obj));
	}

	private static bool HasInstanceDispose(Type type)
	{
		return type.GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null;
	}

	private static object? TryGetLoadedValueType(object method, object body, object instruction)
	{
		string instructionOpCodeName = GetInstructionOpCodeName(instruction);
		if (instructionOpCodeName.StartsWith("ldloca", StringComparison.Ordinal) || instructionOpCodeName.StartsWith("ldarga", StringComparison.Ordinal) || instructionOpCodeName == "ldflda")
		{
			return null;
		}
		object obj = instruction.GetType().GetProperty("Operand", BindingFlags.Instance | BindingFlags.Public)?.GetValue(instruction);
		if (instructionOpCodeName.StartsWith("ldloc", StringComparison.Ordinal))
		{
			object obj2 = obj ?? TryGetIndexedItem(GetRequiredProperty(body, "Variables"), TryParseShortFormIndex(instructionOpCodeName, "ldloc."));
			if (obj2 != null)
			{
				return GetRequiredProperty(obj2, "VariableType");
			}
			return null;
		}
		if (instructionOpCodeName.StartsWith("ldarg", StringComparison.Ordinal))
		{
			object obj3 = obj;
			if (obj3 == null)
			{
				int num = TryParseShortFormIndex(instructionOpCodeName, "ldarg.");
				bool flag = (bool)GetRequiredProperty(method, "HasThis");
				if (num == 0 && flag)
				{
					return GetRequiredProperty(method, "DeclaringType");
				}
				if (num >= 0)
				{
					obj3 = TryGetIndexedItem(GetRequiredProperty(method, "Parameters"), num - (flag ? 1 : 0));
				}
			}
			if (obj3 != null)
			{
				return GetRequiredProperty(obj3, "ParameterType");
			}
			return null;
		}
		if ((instructionOpCodeName.StartsWith("call", StringComparison.Ordinal) || instructionOpCodeName == "newobj") && obj != null)
		{
			if (!(instructionOpCodeName == "newobj"))
			{
				return GetRequiredProperty(obj, "ReturnType");
			}
			return GetRequiredProperty(obj, "DeclaringType");
		}
		if (instructionOpCodeName.StartsWith("ldfld", StringComparison.Ordinal) && obj != null)
		{
			return GetRequiredProperty(obj, "FieldType");
		}
		return null;
	}

	private static int TryParseShortFormIndex(string opCodeName, string prefix)
	{
		if (opCodeName.StartsWith(prefix, StringComparison.Ordinal))
		{
			int length = prefix.Length;
			if (int.TryParse(opCodeName.Substring(length, opCodeName.Length - length), out var result))
			{
				return result;
			}
		}
		return -1;
	}

	private static object? TryGetIndexedItem(object collection, int index)
	{
		if (index >= 0)
		{
			return Enumerate(collection).ElementAtOrDefault(index);
		}
		return null;
	}

	private static string GetInstructionOpCodeName(object instruction)
	{
		return (GetRequiredProperty(GetRequiredProperty(instruction, "OpCode"), "Name") as string) ?? string.Empty;
	}

	private static void SetInstruction(object instruction, object opCode, object? operand)
	{
		instruction.GetType().GetProperty("OpCode", BindingFlags.Instance | BindingFlags.Public).SetValue(instruction, opCode);
		instruction.GetType().GetProperty("Operand", BindingFlags.Instance | BindingFlags.Public)?.SetValue(instruction, operand);
	}

	private static MethodInfo? FindMatchingRuntimeMethod(object methodReference, IReadOnlyList<MethodInfo> runtimeMethods)
	{
		object requiredProperty = GetRequiredProperty(methodReference, "DeclaringType");
		string declaringName = GetTypeFullName(requiredProperty);
		bool flag = !IsGameTypeReference(requiredProperty) || !(bool)GetRequiredProperty(methodReference, "HasThis") || (bool)GetRequiredProperty(methodReference, "HasGenericParameters") || (bool)GetRequiredProperty(methodReference, "IsGenericInstance");
		bool flag2;
		if (!flag)
		{
			string text = declaringName;
			flag2 = ((text == "MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState" || text == "MegaCrit.Sts2.Core.Bindings.MegaSpine.SpineAnimationAccess") ? true : false);
			flag = !flag2;
		}
		if (flag)
		{
			return null;
		}
		string methodName = GetRequiredProperty(methodReference, "Name") as string;
		string providerReturnName = GetTypeFullName(GetRequiredProperty(methodReference, "ReturnType"));
		switch (methodName)
		{
		case "SetAnimation":
		case "AddAnimation":
		case "AddAnimationTracked":
		case "AddEmptyAnimation":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		flag2 = !flag;
		if (!flag2)
		{
			string text = providerReturnName;
			bool flag3 = ((text == "System.Void" || text == "MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaTrackEntry") ? true : false);
			flag2 = !flag3;
		}
		if (flag2)
		{
			return null;
		}
		runtimeMethods = runtimeMethods.Where((MethodInfo method) => method.DeclaringType?.FullName == declaringName && !method.IsStatic).ToArray();
		string[] parameterNames = (from parameter in Enumerate(GetRequiredProperty(methodReference, "Parameters"))
			select GetTypeFullName(GetRequiredProperty(parameter, "ParameterType"))).ToArray();
		string[] array;
		switch (methodName)
		{
		case "SetAnimation":
			array = new string[3] { "System.String", "System.Boolean", "System.Int32" };
			break;
		case "AddAnimation":
		case "AddAnimationTracked":
			array = new string[4] { "System.String", "System.Single", "System.Boolean", "System.Int32" };
			break;
		case "AddEmptyAnimation":
			array = new string[1] { "System.Int32" };
			break;
		default:
			array = Array.Empty<string>();
			break;
		}
		string[] second = array;
		if (!parameterNames.SequenceEqual<string>(second, StringComparer.Ordinal))
		{
			return null;
		}
		MethodInfo[] source = runtimeMethods.Where((MethodInfo method) => string.Equals(method.Name, methodName, StringComparison.Ordinal) && (from parameter in method.GetParameters()
			select parameter.ParameterType.FullName ?? parameter.ParameterType.Name).SequenceEqual<string>(parameterNames, StringComparer.Ordinal)).ToArray();
		MethodInfo methodInfo = source.SingleOrDefault((MethodInfo method) => string.Equals(method.ReturnType.FullName ?? method.ReturnType.Name, providerReturnName, StringComparison.Ordinal));
		if (methodInfo != null)
		{
			return methodInfo;
		}
		if (methodName != null && MegaAnimationMethodAliases.TryGetValue(methodName, out string aliasName))
		{
			MethodInfo methodInfo2 = runtimeMethods.SingleOrDefault((MethodInfo method) => string.Equals(method.Name, aliasName, StringComparison.Ordinal) && string.Equals(method.ReturnType.FullName ?? method.ReturnType.Name, providerReturnName, StringComparison.Ordinal) && (from parameter in method.GetParameters()
				select parameter.ParameterType.FullName ?? parameter.ParameterType.Name).SequenceEqual<string>(parameterNames, StringComparer.Ordinal));
			if (methodInfo2 != null)
			{
				return methodInfo2;
			}
		}
		return source.SingleOrDefault();
	}

	private static bool TryReplaceFollowingPopWithNop(object body, IReadOnlyList<object> instructions, object callInstruction, object nopOpCode)
	{
		int num = -1;
		for (int i = 0; i < instructions.Count; i++)
		{
			if (instructions[i] == callInstruction)
			{
				num = i;
				break;
			}
		}
		if (num < 0 || num + 1 >= instructions.Count)
		{
			return false;
		}
		object next = instructions[num + 1];
		if (instructions.Any(delegate(object item)
		{
			object obj = item.GetType().GetProperty("Operand")?.GetValue(item);
			return obj != null && (obj == next || (obj is IEnumerable source && source.Cast<object>().Contains(next)));
		}) || Enumerate(GetRequiredProperty(body, "ExceptionHandlers")).Any((object handler) => new string[5] { "TryStart", "TryEnd", "HandlerStart", "HandlerEnd", "FilterStart" }.Any((string name) => handler.GetType().GetProperty(name)?.GetValue(handler) == next)))
		{
			return false;
		}
		PropertyInfo property = next.GetType().GetProperty("OpCode", BindingFlags.Instance | BindingFlags.Public);
		object obj2 = property?.GetValue(next);
		if (!string.Equals(obj2?.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj2) as string, "pop", StringComparison.Ordinal))
		{
			return false;
		}
		property.SetValue(next, nopOpCode);
		next.GetType().GetProperty("Operand", BindingFlags.Instance | BindingFlags.Public)?.SetValue(next, null);
		return true;
	}

	[IteratorStateMachine(typeof(_003CEnumerateTypes_003Ed__20))]
	private static IEnumerable<object> EnumerateTypes(object module)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateTypes_003Ed__20(-2)
		{
			_003C_003E3__module = module
		};
	}

	[IteratorStateMachine(typeof(_003CEnumerateTypeAndNested_003Ed__21))]
	private static IEnumerable<object> EnumerateTypeAndNested(object type)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateTypeAndNested_003Ed__21(-2)
		{
			_003C_003E3__type = type
		};
	}

	private static IEnumerable<object> Enumerate(object value)
	{
		if (!(value is IEnumerable source))
		{
			throw new InvalidOperationException(value.GetType().FullName + " 不是可枚举的元数据集合");
		}
		return source.Cast<object>();
	}

	private static object GetRequiredProperty(object value, string name)
	{
		Type valueType = value.GetType();
		return (from candidate in valueType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			where candidate.Name == name && candidate.GetIndexParameters().Length == 0
			orderby (!(candidate.DeclaringType == valueType)) ? 1 : 0
			select candidate).FirstOrDefault()?.GetValue(value) ?? throw new MissingMemberException(value.GetType().FullName, name);
	}

	private static string GetTypeFullName(object typeReference)
	{
		return (GetRequiredProperty(typeReference, "FullName") as string) ?? string.Empty;
	}

	private static void InvokeProcessor(object processor, string methodName, params object[] arguments)
	{
		processor.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Single((MethodInfo candidate) => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length && candidate.GetParameters().Select((ParameterInfo parameter, int index) => parameter.ParameterType.IsInstanceOfType(arguments[index])).All((bool matches) => matches))
			.Invoke(processor, arguments);
	}

	public static ProviderCompatibilityResult PrepareForCurrentGame(string assemblyPath)
	{
		ProviderCompatibilityReport providerCompatibilityReport = new ProviderCompatibilityReport
		{
			Target = $"{GameAssembly.GetName().Name}/{GameAssembly.ManifestModule.ModuleVersionId}"
		};
		MemoryStream rewrittenAssembly = null;
		try
		{
			TryRewriteRuntimeDrift(assemblyPath, FindRuntimeMegaAnimationMethods(), providerCompatibilityReport, out rewrittenAssembly, out int _, out string failure);
			providerCompatibilityReport.Failure = failure;
		}
		catch (Exception ex)
		{
			providerCompatibilityReport.Failure = ex.GetBaseException().Message;
		}
		if (providerCompatibilityReport.Failure != null)
		{
			rewrittenAssembly?.Dispose();
			rewrittenAssembly = null;
			providerCompatibilityReport.Changes.Clear();
		}
		return new ProviderCompatibilityResult(rewrittenAssembly, providerCompatibilityReport);
	}

	private static bool IsGameTypeReference(object type)
	{
		object requiredProperty = GetRequiredProperty(type, "Scope");
		if (requiredProperty.GetType().FullName == "Mono.Cecil.AssemblyNameReference")
		{
			return (string)GetRequiredProperty(requiredProperty, "Name") == GameAssembly.GetName().Name;
		}
		return false;
	}

	private static object[] References(object module, string method)
	{
		return Enumerate(module.GetType().GetMethod(method, Type.EmptyTypes).Invoke(module, null)).ToArray();
	}

	private static void RewriteKnownTypeMoves(object module, ProviderCompatibilityReport report)
	{
		object[] array = References(module, "GetTypeReferences");
		foreach (object obj in array)
		{
			if (!IsGameTypeReference(obj))
			{
				continue;
			}
			string typeFullName = GetTypeFullName(obj);
			if (GameAssembly.GetType(typeFullName) != null)
			{
				continue;
			}
			string text = (string)GetRequiredProperty(obj, "Name");
			string text2 = (string)GetRequiredProperty(obj, "Namespace");
			bool flag = ((text == "NPowerAppliedBuffVfx" || text == "NPowerAppliedDebuffVfx") ? true : false);
			bool flag2 = !flag;
			if (!flag2)
			{
				bool flag3 = ((text2 == "MegaCrit.Sts2.Core.Nodes.Vfx" || text2 == "MegaCrit.Sts2.Core.Nodes.Vfx.Ui") ? true : false);
				flag2 = !flag3;
			}
			if (!flag2)
			{
				string text3 = (text2.EndsWith(".Ui", StringComparison.Ordinal) ? "MegaCrit.Sts2.Core.Nodes.Vfx" : "MegaCrit.Sts2.Core.Nodes.Vfx.Ui");
				if (!(GameAssembly.GetType(text3 + "." + text) == null))
				{
					obj.GetType().GetProperty("Namespace").SetValue(obj, text3);
					report.Changes.Add("Type relocation: " + typeFullName + " -> " + GetTypeFullName(obj));
				}
			}
		}
	}

	private static void AuditGameReferences(object module, ProviderCompatibilityReport report)
	{
		SortedSet<string> sortedSet = new SortedSet<string>(StringComparer.Ordinal);
		Dictionary<string, Type?> types = new Dictionary<string, Type>(StringComparer.Ordinal);
		object[] array = References(module, "GetTypeReferences");
		foreach (object obj in array)
		{
			if (IsGameTypeReference(obj) && Resolve(obj) == null)
			{
				sortedSet.Add("Type: " + GetTypeFullName(obj));
			}
		}
		array = References(module, "GetMemberReferences");
		foreach (object member in array)
		{
			object requiredProperty = GetRequiredProperty(member, "DeclaringType");
			if (!IsGameTypeReference(requiredProperty))
			{
				continue;
			}
			if (!GetTypeFullName(requiredProperty).Contains('<') && !(bool)GetRequiredProperty(member, "ContainsGenericParameter"))
			{
				object obj2 = member.GetType().GetProperty("HasGenericParameters")?.GetValue(member);
				if (!(obj2 is bool) || !(bool)obj2)
				{
					Type type = Resolve(requiredProperty);
					if (type == null)
					{
						continue;
					}
					string name = (string)GetRequiredProperty(member, "Name");
					bool flag;
					if (member.GetType().GetProperty("ReturnType") != null)
					{
						string[] parameters = (from parameter in Enumerate(GetRequiredProperty(member, "Parameters"))
							select GetTypeFullName(GetRequiredProperty(parameter, "ParameterType"))).ToArray();
						string returns = GetTypeFullName(GetRequiredProperty(member, "ReturnType"));
						bool hasThis = (bool)GetRequiredProperty(member, "HasThis");
						string text = name;
						IEnumerable<MethodBase> source;
						if ((!(text == ".ctor") && !(text == ".cctor")) || 1 == 0)
						{
							IEnumerable<MethodBase> enumerable = from method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
								where method.Name == name
								select method;
							source = enumerable;
						}
						else
						{
							IEnumerable<MethodBase> enumerable = type.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
							source = enumerable;
						}
						flag = source.Any((MethodBase method) => !method.IsStatic == hasThis && !method.IsGenericMethod && RuntimeTypeName((method is MethodInfo methodInfo) ? methodInfo.ReturnType : typeof(void)) == returns && (from p in method.GetParameters()
							select RuntimeTypeName(p.ParameterType)).SequenceEqual(parameters));
					}
					else
					{
						flag = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Any((FieldInfo field) => field.Name == name && RuntimeTypeName(field.FieldType) == GetTypeFullName(GetRequiredProperty(member, "FieldType")));
					}
					if (!flag)
					{
						sortedSet.Add(member.ToString());
					}
					continue;
				}
			}
			report.UncheckedGenericReferences++;
		}
		report.UnresolvedReferences.AddRange(sortedSet);
		Type? Resolve(object reference)
		{
			string text2 = GetTypeFullName(reference).Replace('/', '+');
			if (!types.TryGetValue(text2, out Type value))
			{
				value = (types[text2] = GameAssembly.GetType(text2));
			}
			return value;
		}
	}

	private static string RuntimeTypeName(Type type)
	{
		if (type.IsByRef)
		{
			return RuntimeTypeName(type.GetElementType()) + "&";
		}
		if (type.IsArray)
		{
			return RuntimeTypeName(type.GetElementType()) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
		}
		if (type.IsConstructedGenericType)
		{
			return type.GetGenericTypeDefinition().FullName.Replace('+', '/') + "<" + string.Join(",", type.GetGenericArguments().Select(RuntimeTypeName)) + ">";
		}
		return (type.FullName ?? type.Name).Replace('+', '/');
	}

	private static object Import(object module, object value)
	{
		Type type = ((value is Type) ? typeof(Type) : typeof(MethodBase));
		return module.GetType().GetMethod("ImportReference", new Type[1] { type }).Invoke(module, new object[1] { value });
	}

	private static bool TryRewriteTaskReturn(object module, object instruction, object member, ProviderCompatibilityReport report)
	{
		object requiredProperty = GetRequiredProperty(member, "DeclaringType");
		if (!IsGameTypeReference(requiredProperty) || GetInstructionOpCodeName(instruction) != "call" || GetTypeFullName(requiredProperty) != "MegaCrit.Sts2.Core.Commands.CardCmd" || (string)GetRequiredProperty(member, "Name") != "Exhaust" || (bool)GetRequiredProperty(member, "HasThis") || (bool)GetRequiredProperty(member, "HasGenericParameters") || (bool)GetRequiredProperty(member, "IsGenericInstance") || GetTypeFullName(GetRequiredProperty(member, "ReturnType")) != typeof(Task).FullName)
		{
			return false;
		}
		string[] parameters = (from parameter in Enumerate(GetRequiredProperty(member, "Parameters"))
			select GetTypeFullName(GetRequiredProperty(parameter, "ParameterType"))).ToArray();
		if (!parameters.SequenceEqual(new string[4] { "MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext", "MegaCrit.Sts2.Core.Models.CardModel", "System.Boolean", "System.Boolean" }))
		{
			return false;
		}
		MethodInfo methodInfo = GameAssembly.GetType(GetTypeFullName(requiredProperty))?.GetMethods(BindingFlags.Static | BindingFlags.Public).SingleOrDefault((MethodInfo method) => method.Name == "Exhaust" && !method.IsGenericMethod && (from p in method.GetParameters()
			select RuntimeTypeName(p.ParameterType)).SequenceEqual(parameters));
		if (methodInfo == null || !methodInfo.ReturnType.IsConstructedGenericType || methodInfo.ReturnType.GetGenericTypeDefinition() != typeof(Task<>))
		{
			return false;
		}
		instruction.GetType().GetProperty("Operand").SetValue(instruction, Import(module, methodInfo));
		report.Changes.Add($"Task return: {member} -> {RuntimeTypeName(methodInfo.ReturnType)}; original Task identity retained");
		return true;
	}

	private static object NewAdapter(object module, object owner, string name, object returns, IEnumerable<object> parameterTypes, Assembly cecil)
	{
		object requiredProperty = GetRequiredProperty(owner, "Methods");
		HashSet<string> hashSet = (from m in Enumerate(requiredProperty)
			select (string)GetRequiredProperty(m, "Name")).ToHashSet();
		string text = "__SkinChanger_" + name;
		name = text;
		int num = 1;
		while (hashSet.Contains(name))
		{
			name = text + "_" + num;
			num++;
		}
		object obj = Activator.CreateInstance(cecil.GetType("Mono.Cecil.MethodDefinition"), name, Enum.Parse(cecil.GetType("Mono.Cecil.MethodAttributes"), "Private, Static, HideBySig"), returns);
		foreach (object parameterType in parameterTypes)
		{
			InvokeProcessor(GetRequiredProperty(obj, "Parameters"), "Add", Activator.CreateInstance(cecil.GetType("Mono.Cecil.ParameterDefinition"), parameterType));
		}
		InvokeProcessor(requiredProperty, "Add", obj);
		return obj;
	}

	private static object Instruction(string code, Assembly cecil, Type opCodes, object? operand = null)
	{
		object value = opCodes.GetField(code).GetValue(null);
		object[] arguments = ((operand != null) ? new object[2] { value, operand } : new object[1] { value });
		return cecil.GetType("Mono.Cecil.Cil.Instruction").GetMethods().Single((MethodInfo m) => m.Name == "Create" && m.GetParameters().Length == arguments.Length && m.GetParameters().Select((ParameterInfo p, int i) => p.ParameterType.IsInstanceOfType(arguments[i])).All((bool x) => x))
			.Invoke(null, arguments);
	}

	private static object Emit(object method, string code, Assembly cecil, Type opCodes, object? operand = null)
	{
		object requiredProperty = GetRequiredProperty(method, "Body");
		object? processor = requiredProperty.GetType().GetMethod("GetILProcessor").Invoke(requiredProperty, null);
		object obj = Instruction(code, cecil, opCodes, operand);
		InvokeProcessor(processor, "Append", obj);
		return obj;
	}

	private static object CreateAccessTrackAdapter(object module, object owner, MethodInfo runtime, Assembly cecil, Type opCodes)
	{
		Type declaringType = runtime.DeclaringType;
		MethodInfo method = declaringType.GetMethod("GetAnimationState");
		Type returnType = method.ReturnType;
		MethodInfo method2 = returnType.GetMethod((runtime.Name == "AddAnimation") ? "AddAnimationTracked" : "SetAnimation", (from p in runtime.GetParameters()
			select p.ParameterType).ToArray());
		object obj = Import(module, returnType.GetMethod("GetCurrent", new Type[1] { typeof(int) }).ReturnType);
		object obj2 = Activator.CreateInstance(cecil.GetType("Mono.Cecil.ByReferenceType"), Import(module, declaringType));
		object adapter = NewAdapter(module, owner, runtime.Name + "AccessTracked", obj, new object[1] { obj2 }.Concat(from p in runtime.GetParameters()
			select Import(module, p.ParameterType)), cecil);
		object body = GetRequiredProperty(adapter, "Body");
		body.GetType().GetProperty("InitLocals").SetValue(body, true);
		object operand2 = Local(Import(module, returnType));
		object operand3 = Local(obj);
		object obj3 = Instruction("Ldloc", cecil, opCodes, operand3);
		E("Ldarg_0");
		E("Call", Import(module, method));
		E("Stloc", operand2);
		E("Ldloc", operand2);
		E("Brfalse", obj3);
		object item = E("Ldloc", operand2);
		object[] array = Enumerate(GetRequiredProperty(adapter, "Parameters")).ToArray();
		for (int i = 1; i < array.Length; i++)
		{
			E("Ldarg", array[i]);
		}
		E("Callvirt", Import(module, method2));
		if (method2.ReturnType == typeof(void))
		{
			E("Ldloc", operand2);
			E("Ldarg", array[^1]);
			E("Callvirt", Import(module, returnType.GetMethod("GetCurrent", new Type[1] { typeof(int) })));
		}
		E("Stloc", operand3);
		E("Leave", obj3);
		MethodInfo value = returnType.GetMethod("Dispose", Type.EmptyTypes) ?? throw new MissingMethodException(returnType.FullName, "Dispose");
		object item2 = E("Ldloc", operand2);
		E("Callvirt", Import(module, value));
		E("Endfinally");
		InvokeProcessor(body.GetType().GetMethod("GetILProcessor").Invoke(body, null), "Append", obj3);
		E("Ret");
		object obj4 = Activator.CreateInstance(cecil.GetType("Mono.Cecil.Cil.ExceptionHandler"), Enum.Parse(cecil.GetType("Mono.Cecil.Cil.ExceptionHandlerType"), "Finally"));
		(string, object)[] array2 = new(string, object)[4]
		{
			("TryStart", item),
			("TryEnd", item2),
			("HandlerStart", item2),
			("HandlerEnd", obj3)
		};
		for (int j = 0; j < array2.Length; j++)
		{
			var (name, value2) = array2[j];
			obj4.GetType().GetProperty(name).SetValue(obj4, value2);
		}
		InvokeProcessor(GetRequiredProperty(body, "ExceptionHandlers"), "Add", obj4);
		return adapter;
		object E(string code, object? operand = null)
		{
			return Emit(adapter, code, cecil, opCodes, operand);
		}
		object Local(object type)
		{
			object obj5 = Activator.CreateInstance(cecil.GetType("Mono.Cecil.Cil.VariableDefinition"), type);
			InvokeProcessor(GetRequiredProperty(body, "Variables"), "Add", obj5);
			return obj5;
		}
	}

	private static void ExpandShortBranches(object body, Type opCodes)
	{
		Dictionary<string, object> dictionary = (from field in opCodes.GetFields(BindingFlags.Static | BindingFlags.Public)
			select field.GetValue(null)).ToDictionary<object, string>((object op) => (string)GetRequiredProperty(op, "Name"), StringComparer.Ordinal);
		foreach (object item in Enumerate(GetRequiredProperty(body, "Instructions")))
		{
			object requiredProperty = GetRequiredProperty(item, "OpCode");
			if (!(GetRequiredProperty(requiredProperty, "OperandType").ToString() != "ShortInlineBrTarget"))
			{
				string text = (string)GetRequiredProperty(requiredProperty, "Name");
				PropertyInfo? property = item.GetType().GetProperty("OpCode");
				string text2 = text;
				property.SetValue(item, dictionary[text2.Substring(0, text2.Length - 2)]);
			}
		}
	}
}
