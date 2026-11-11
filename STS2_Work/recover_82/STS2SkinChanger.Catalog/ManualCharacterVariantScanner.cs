using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace STS2SkinChanger.Catalog;

internal static class ManualCharacterVariantScanner
{
	[CompilerGenerated]
	private sealed class _003CFindToggledFields_003Ed__4 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private object[] body;

		public object[] _003C_003E3__body;

		private int _003Ci_003E5__2;

		string IEnumerator<string>.Current
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
		public _003CFindToggledFields_003Ed__4(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_0188;
			}
			_003C_003E1__state = -1;
			_003Ci_003E5__2 = 0;
			goto IL_0198;
			IL_0188:
			_003Ci_003E5__2++;
			goto IL_0198;
			IL_0198:
			if (_003Ci_003E5__2 + 3 < body.Length)
			{
				string text = Op(body[_003Ci_003E5__2]);
				bool flag = ((text == "ldsfld" || text == "ldfld") ? true : false);
				if (flag && Text(Operand(body[_003Ci_003E5__2]), "FieldType") == "System.Boolean" && Op(body[_003Ci_003E5__2 + 1]) == "ldc.i4.0" && Op(body[_003Ci_003E5__2 + 2]) == "ceq" && Op(body[_003Ci_003E5__2 + 3]) == ((Op(body[_003Ci_003E5__2]) == "ldsfld") ? "stsfld" : "stfld") && Text(Operand(body[_003Ci_003E5__2 + 3]), "FullName") == Text(Operand(body[_003Ci_003E5__2]), "FullName"))
				{
					_003C_003E2__current = Text(Operand(body[_003Ci_003E5__2]), "FullName");
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0188;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CFindToggledFields_003Ed__4 _003CFindToggledFields_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CFindToggledFields_003Ed__ = this;
			}
			else
			{
				_003CFindToggledFields_003Ed__ = new _003CFindToggledFields_003Ed__4(0);
			}
			_003CFindToggledFields_003Ed__.body = _003C_003E3__body;
			return _003CFindToggledFields_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CWalkTypes_003Ed__11 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private object values;

		public object _003C_003E3__values;

		private IEnumerator<object> _003C_003E7__wrap1;

		private object _003Ctype_003E5__3;

		private IEnumerator<object> _003C_003E7__wrap3;

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
		public _003CWalkTypes_003Ed__11(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
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
			_003Ctype_003E5__3 = null;
			_003C_003E7__wrap3 = null;
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
					_003C_003E7__wrap1 = Items(values).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					_003C_003E7__wrap3 = WalkTypes(Get(_003Ctype_003E5__3, "NestedTypes")).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00cd;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00cd;
					}
					IL_00cd:
					if (_003C_003E7__wrap3.MoveNext())
					{
						object current = _003C_003E7__wrap3.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					_003Ctype_003E5__3 = null;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					_003Ctype_003E5__3 = _003C_003E7__wrap1.Current;
					_003C_003E2__current = _003Ctype_003E5__3;
					_003C_003E1__state = 1;
					return true;
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
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
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
			_003CWalkTypes_003Ed__11 _003CWalkTypes_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CWalkTypes_003Ed__ = this;
			}
			else
			{
				_003CWalkTypes_003Ed__ = new _003CWalkTypes_003Ed__11(0);
			}
			_003CWalkTypes_003Ed__.values = _003C_003E3__values;
			return _003CWalkTypes_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	private static readonly Dictionary<string, (long Length, DateTime Modified, ManualCharacterState[] States)> Cache = new Dictionary<string, (long, DateTime, ManualCharacterState[])>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<(Type, string), PropertyInfo?> Properties = new Dictionary<(Type, string), PropertyInfo>();

	internal static ManualCharacterState[] ScanAssembly(string path)
	{
		FileInfo fileInfo = new FileInfo(path);
		lock (Cache)
		{
			if (Cache.TryGetValue(path, out (long, DateTime, ManualCharacterState[]) value) && value.Item1 == fileInfo.Length && value.Item2 == fileInfo.LastWriteTimeUtc)
			{
				return value.Item3;
			}
			using FileStream fileStream = File.OpenRead(path);
			object obj = typeof(Harmony).Assembly.GetType("Mono.Cecil.AssemblyDefinition", throwOnError: true).GetMethod("ReadAssembly", new Type[1] { typeof(Stream) }).Invoke(null, new object[1] { fileStream });
			try
			{
				object[] array = WalkTypes(Get(Get(obj, "MainModule"), "Types")).ToArray();
				object[] array2 = (from m in array.SelectMany((object t) => Items(Get(t, "Methods")))
					where (bool)Get(m, "HasBody")
					select m).ToArray();
				Dictionary<object, object[]> bodies = array2.ToDictionary((object m) => m, Body);
				Dictionary<string, object[]> dictionary = array2.SelectMany((object method) => from field in FindToggledFields(bodies[method])
					select (Field: field, Method: method)).GroupBy<(string, object), string>(((string Field, object Method) pair) => pair.Field, StringComparer.Ordinal).ToDictionary<IGrouping<string, (string, object)>, string, object[]>((IGrouping<string, (string Field, object Method)> group) => group.Key, (IGrouping<string, (string Field, object Method)> group) => group.Select(((string Field, object Method) pair) => pair.Method).Distinct().ToArray(), StringComparer.Ordinal);
				List<ManualCharacterState> list = new List<ManualCharacterState>();
				object[] array3 = array;
				foreach (object type in array3)
				{
					foreach (object item in from f in Items(Get(type, "Fields"))
						where Text(f, "FieldType") == "System.Boolean"
						select f)
					{
						bool flag = (bool)Get(item, "IsStatic");
						string fieldId = Text(item, "FullName");
						if (!dictionary.TryGetValue(fieldId, out var value2))
						{
							continue;
						}
						object[] array4 = value2;
						foreach (object toggle in array4)
						{
							string saveMethod = null;
							string[] array5 = Array.Empty<string>();
							if (!flag)
							{
								if (Text(toggle, "Name") != "_Input" || Text(toggle, "DeclaringType") != Text(type, "FullName"))
								{
									continue;
								}
								object[] array6 = (from f in Items(Get(type, "Fields"))
									where Text(f, "FieldType") == "System.String[]"
									select f).ToArray();
								if (array6.Length != 1 || array2.FirstOrDefault((object m) => Text(m, "DeclaringType") == Text(type, "FullName") && Calls(bodies[toggle], m) && Reads(bodies[m], fieldId) && bodies[m].Any((object i) => Operand(i) is string text && text == "get_color") && bodies[m].Any((object i) => Operand(i) is string text2 && text2 == "set_color") && bodies[m].Any((object i) => Op(i) == "ldc.r4" && object.Equals(Operand(i), 0f)) && bodies[m].Any((object i) => Op(i) == "stfld" && Text(Operand(i), "Name") == "A" && Text(Operand(i), "DeclaringType") == "Godot.Color")) == null)
								{
									continue;
								}
								array5 = ReadArrayInitializer(array2, bodies, Text(array6[0], "FullName"));
								int num = array5.Length;
								if ((num > 256 || num == 0) ? true : false)
								{
									continue;
								}
							}
							else
							{
								if (!array2.Any((object m) => Calls(bodies[m], "Godot.BaseButton", "add_Pressed") && References(bodies[m], toggle)) || !array2.Any((object m) => Reads(bodies[m], fieldId) && Calls(bodies[m], "Godot.ShaderMaterial", "SetShaderParameter")))
								{
									continue;
								}
								object obj2 = array2.FirstOrDefault((object m) => (bool)Get(m, "IsStatic") && !Items(Get(m, "Parameters")).Any() && Reads(bodies[m], fieldId) && Calls(bodies[m], "Godot.ConfigFile", "Save") && Calls(bodies[m], "Godot.ConfigFile", "SetValue") && Calls(bodies[toggle], m) && Text(m, "DeclaringType") == Text(type, "FullName"));
								if (obj2 == null)
								{
									continue;
								}
								saveMethod = Text(obj2, "Name");
							}
							list.Add(new ManualCharacterState(Text(type, "FullName").Replace('/', '+'), Text(item, "Name"), flag, Text(toggle, "DeclaringType").Replace('/', '+'), Text(toggle, "Name"), saveMethod, array5));
							break;
						}
					}
				}
				ManualCharacterState[] array7 = list.DistinctBy((ManualCharacterState s) => s.Id).ToArray();
				Cache[path] = (fileInfo.Length, fileInfo.LastWriteTimeUtc, array7);
				return array7;
			}
			finally
			{
				((IDisposable)obj).Dispose();
			}
		}
	}

	internal static void Expand(IReadOnlyCollection<PckResourceIndex> indexes, IDictionary<string, SkinGroup> groups, IReadOnlySet<string> characterGroups)
	{
		foreach (PckResourceIndex index in indexes)
		{
			if (!index.Mod.HasDll)
			{
				continue;
			}
			(SkinGroup, SkinOption)[] array = groups.Values.Where((SkinGroup g) => characterGroups.Contains(g.Id)).SelectMany((SkinGroup g) => from o in g.Options
				where o.EffectiveProviderId == index.Mod.Id
				select (Group: g, Option: o)).ToArray();
			if (array.Length == 0 || !Directory.Exists(index.Mod.RootPath))
			{
				continue;
			}
			try
			{
				string text = SkinPackagePaths.Resolve(index.Mod.RootPath, index.Mod.ResourceNamespaceId, ".dll");
				string[] array2 = ((!File.Exists(text)) ? Directory.GetFiles(index.Mod.RootPath, "*.dll") : new string[1] { text });
				if (!File.Exists(text) && array2.Length != 1)
				{
					continue;
				}
				ManualCharacterState[] array3 = array2.SelectMany(ScanAssembly).DistinctBy((ManualCharacterState s) => s.Id).ToArray();
				if (array3.Length != 1 || array.Select(((SkinGroup Group, SkinOption Option) p) => p.Group.Id).Distinct().Count() != 1)
				{
					continue;
				}
				(SkinGroup, SkinOption)[] array4 = array;
				for (int i = 0; i < array4.Length; i++)
				{
					(SkinGroup, SkinOption) tuple = array4[i];
					SkinGroup item = tuple.Item1;
					SkinOption item2 = tuple.Item2;
					int index2 = item.Options.IndexOf(item2);
					item.Options.RemoveAt(index2);
					bool[] array5 = new bool[2] { false, true };
					foreach (bool flag in array5)
					{
						item.Options.Insert(index2++, item2 with
						{
							Id = (flag ? (item2.Id + "::manual:" + Uri.EscapeDataString(array3[0].Id) + ":1") : item2.Id),
							Name = CardSkinOptionNamingPolicy.Build(item2.Name, null, (!flag) ? 1 : 2, 2),
							ProviderId = item2.EffectiveProviderId,
							ManualCharacterVariant = new ManualCharacterVariant(array3[0], flag)
						});
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}

	[IteratorStateMachine(typeof(_003CFindToggledFields_003Ed__4))]
	private static IEnumerable<string> FindToggledFields(object[] body)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFindToggledFields_003Ed__4(-2)
		{
			_003C_003E3__body = body
		};
	}

	private static string[] ReadArrayInitializer(object[] methods, Dictionary<object, object[]> bodies, string field)
	{
		foreach (object item in methods.Where(delegate(object m)
		{
			string text = Text(m, "Name");
			return (text == ".ctor" || text == ".cctor") ? true : false;
		}))
		{
			object[] array = bodies[item];
			int num = Array.FindIndex(array, delegate(object i)
			{
				string text2 = Op(i);
				return ((text2 == "stfld" || text2 == "stsfld") ? true : false) && Text(Operand(i), "FullName") == field;
			});
			if (num < 0)
			{
				continue;
			}
			int num2 = Array.FindLastIndex(array, num, (object i) => Op(i) == "newarr" && Text(Operand(i), "FullName") == "System.String");
			if (num2 >= 0)
			{
				return (from i in array[(num2 + 1)..num]
					where Op(i) == "ldstr"
					select (string)Operand(i)).ToArray();
			}
		}
		return Array.Empty<string>();
	}

	private static bool Reads(object[] body, string field)
	{
		return body.Any(delegate(object i)
		{
			string text = Op(i);
			return ((text == "ldfld" || text == "ldsfld") ? true : false) && Text(Operand(i), "FullName") == field;
		});
	}

	private static bool Calls(object[] body, object method)
	{
		return body.Any(delegate(object i)
		{
			string text = Op(i);
			return ((text == "call" || text == "callvirt") ? true : false) && Text(Operand(i), "FullName") == Text(method, "FullName");
		});
	}

	private static bool Calls(object[] body, string type, string name)
	{
		return body.Any(delegate(object i)
		{
			string text = Op(i);
			bool flag = ((text == "call" || text == "callvirt") ? true : false);
			return flag && Text(Operand(i), "DeclaringType") == type && Text(Operand(i), "Name") == name;
		});
	}

	private static bool References(object[] body, object method)
	{
		return body.Any((object i) => Op(i) == "ldftn" && Text(Operand(i), "FullName") == Text(method, "FullName"));
	}

	private static object[] Body(object method)
	{
		return (from i in Items(Get(Get(method, "Body"), "Instructions"))
			where Op(i) != "nop"
			select i).ToArray();
	}

	[IteratorStateMachine(typeof(_003CWalkTypes_003Ed__11))]
	private static IEnumerable<object> WalkTypes(object values)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWalkTypes_003Ed__11(-2)
		{
			_003C_003E3__values = values
		};
	}

	private static object Get(object value, string name)
	{
		return Property(value, name);
	}

	private static object? Operand(object value)
	{
		return Get(value, "Operand");
	}

	private static string Op(object value)
	{
		return Text(value, "OpCode");
	}

	private static string Text(object? value, string name)
	{
		return Property(value, name)?.ToString() ?? string.Empty;
	}

	private static object? Property(object? value, string name)
	{
		if (value == null)
		{
			return null;
		}
		(Type, string name) key = (value.GetType(), name: name);
		if (!Properties.TryGetValue(key, out PropertyInfo value2))
		{
			value2 = (from candidate in key.Item1.GetProperties()
				where candidate.Name == name
				orderby candidate.DeclaringType == key.Item1 descending
				select candidate).FirstOrDefault();
			Properties[key] = value2;
		}
		return value2?.GetValue(value);
	}

	private static IEnumerable<object> Items(object value)
	{
		return ((IEnumerable)value).Cast<object>();
	}
}
