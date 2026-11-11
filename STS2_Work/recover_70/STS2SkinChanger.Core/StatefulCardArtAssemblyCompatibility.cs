using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class StatefulCardArtAssemblyCompatibility
{
	[CompilerGenerated]
	private sealed class _003CAllTypes_003Ed__4 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private object collection;

		public object _003C_003E3__collection;

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
		public _003CAllTypes_003Ed__4(int _003C_003E1__state)
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
					_003C_003E7__wrap1 = Items(collection).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					_003C_003E7__wrap3 = AllTypes(Get(_003Ctype_003E5__3, "NestedTypes")).GetEnumerator();
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
			_003CAllTypes_003Ed__4 _003CAllTypes_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CAllTypes_003Ed__ = this;
			}
			else
			{
				_003CAllTypes_003Ed__ = new _003CAllTypes_003Ed__4(0);
			}
			_003CAllTypes_003Ed__.collection = _003C_003E3__collection;
			return _003CAllTypes_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}
	}

	internal static MemoryStream? Rewrite(Stream input, StatefulCardArtContract? contract, out int changed)
	{
		changed = 0;
		if (contract == null)
		{
			return null;
		}
		long position = input.Position;
		input.Position = 0L;
		Type type = typeof(Harmony).Assembly.GetType("Mono.Cecil.AssemblyDefinition", throwOnError: true);
		object obj = type.GetMethod("ReadAssembly", new Type[1] { typeof(Stream) }).Invoke(null, new object[1] { input });
		try
		{
			string text = default(string);
			foreach (object item in AllTypes(Get(Get(obj, "MainModule"), "Types")))
			{
				foreach (object item2 in Items(Get(item, "Methods")).Where(delegate(object method)
				{
					object obj2 = Get(method, "HasBody");
					return obj2 is bool && (bool)obj2;
				}))
				{
					foreach (object item3 in Items(Get(Get(item2, "Body"), "Instructions")))
					{
						int num;
						if (Text(item3, "OpCode") == "ldstr")
						{
							text = Get(item3, "Operand") as string;
							num = ((text != null) ? 1 : 0);
						}
						else
						{
							num = 0;
						}
						bool flag = (byte)num != 0;
						if (flag)
						{
							bool flag2;
							switch (text)
							{
							case "artists":
							case "usernames":
							case "event_chatter":
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
							item3.GetType().GetProperty("Operand").SetValue(item3, contract.LocalizationPrefix + text);
							changed++;
						}
					}
				}
			}
			foreach (object item4 in Items(Get(Get(obj, "MainModule"), "Types")))
			{
				object[] array = Items(Get(item4, "Fields")).Where(delegate(object field)
				{
					object obj3 = Get(field, "IsStatic");
					if (obj3 is bool && (bool)obj3)
					{
						obj3 = Get(field, "IsLiteral");
						if (obj3 is bool)
						{
							return !(bool)obj3;
						}
						return false;
					}
					return false;
				}).ToArray();
				bool flag3 = array.Length == 1 && Text(array[0], "FieldType").StartsWith("BaseLib.Utils.AddedNode`2<", StringComparison.Ordinal);
				bool flag4 = Text(item4, "FullName") == contract.ArtType;
				object obj4 = Items(Get(item4, "Methods")).SingleOrDefault((object method) => Text(method, "Name") == ".cctor");
				if (obj4 == null)
				{
					continue;
				}
				object obj5 = Get(obj4, "Body");
				if (!flag3 && !flag4)
				{
					object[] array2 = Items(Get(obj5, "Instructions")).ToArray();
					foreach (object obj6 in array2)
					{
						object value = Get(obj6, "Operand");
						if (!(Text(obj6, "OpCode") != "newobj") && Text(value, "DeclaringType").StartsWith("BaseLib.Utils.AddedNode`2<", StringComparison.Ordinal))
						{
							object obj7 = obj5.GetType().GetMethod("GetILProcessor").Invoke(obj5, null);
							Type type2 = typeof(Harmony).Assembly.GetType("Mono.Cecil.Cil.OpCodes", throwOnError: true);
							Type type3 = obj6.GetType();
							MethodInfo methodInfo = type3.GetMethods().Single((MethodInfo method) => method.Name == "Create" && method.GetParameters().Length == 1);
							MethodInfo method2 = obj7.GetType().GetMethod("InsertAfter", new Type[2] { type3, type3 });
							int num2 = Items(Get(value, "Parameters")).Count();
							type3.GetProperty("OpCode").SetValue(obj6, type2.GetField((num2 == 0) ? "Ldnull" : "Pop").GetValue(null));
							type3.GetProperty("Operand").SetValue(obj6, null);
							object obj8 = obj6;
							for (int j = 1; j <= num2; j++)
							{
								object obj9 = methodInfo.Invoke(null, new object[1] { type2.GetField((j == num2) ? "Ldnull" : "Pop").GetValue(null) });
								method2.Invoke(obj7, new object[2] { obj8, obj9 });
								obj8 = obj9;
							}
							changed++;
						}
					}
				}
				else
				{
					string[] array3 = new string[3] { "Instructions", "Variables", "ExceptionHandlers" };
					foreach (string name in array3)
					{
						object obj10 = Get(obj5, name);
						obj10.GetType().GetMethod("Clear").Invoke(obj10, null);
					}
					object obj11 = obj5.GetType().GetMethod("GetILProcessor").Invoke(obj5, null);
					object value2 = typeof(Harmony).Assembly.GetType("Mono.Cecil.Cil.OpCodes", throwOnError: true).GetField("Ret").GetValue(null);
					obj11.GetType().GetMethods().Single((MethodInfo method) => method.Name == "Emit" && method.GetParameters().Length == 1)
						.Invoke(obj11, new object[1] { value2 });
					changed++;
				}
			}
			if (changed == 0)
			{
				throw new InvalidOperationException("Stateful card contract has no isolatable factory; keep original initializer disabled.");
			}
			MemoryStream memoryStream = new MemoryStream();
			try
			{
				type.GetMethod("Write", new Type[1] { typeof(Stream) }).Invoke(obj, new object[1] { memoryStream });
				memoryStream.Position = 0L;
				return memoryStream;
			}
			catch
			{
				memoryStream.Dispose();
				throw;
			}
		}
		finally
		{
			(obj as IDisposable)?.Dispose();
			input.Position = position;
		}
	}

	private static object? Get(object? value, string name)
	{
		return value?.GetType().GetProperties().FirstOrDefault((PropertyInfo property) => property.Name == name)?.GetValue(value);
	}

	private static string Text(object? value, string name)
	{
		return Get(value, name)?.ToString() ?? string.Empty;
	}

	private static IEnumerable<object> Items(object? value)
	{
		if (!(value is IEnumerable source))
		{
			return Array.Empty<object>();
		}
		return source.Cast<object>();
	}

	[IteratorStateMachine(typeof(_003CAllTypes_003Ed__4))]
	private static IEnumerable<object> AllTypes(object? collection)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CAllTypes_003Ed__4(-2)
		{
			_003C_003E3__collection = collection
		};
	}
}
