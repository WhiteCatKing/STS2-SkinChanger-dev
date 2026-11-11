using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class ProviderGameplayContentPolicy
{
	[CompilerGenerated]
	private sealed class _003CEnumerateAssemblies_003Ed__3 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string rootPath;

		public string _003C_003E3__rootPath;

		private string providerId;

		public string _003C_003E3__providerId;

		private IEnumerator<string> _003C_003E7__wrap1;

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
		public _003CEnumerateAssemblies_003Ed__3(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
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
				{
					_003C_003E1__state = -1;
					string text;
					try
					{
						text = SkinPackagePaths.Resolve(rootPath, providerId, ".dll");
					}
					catch
					{
						text = string.Empty;
					}
					if (!string.IsNullOrWhiteSpace(text) && File.Exists(text))
					{
						_003C_003E2__current = text;
						_003C_003E1__state = 1;
						return true;
					}
					if (!Directory.Exists(rootPath))
					{
						return false;
					}
					_003C_003E7__wrap1 = Directory.EnumerateFiles(rootPath, "*.dll", SearchOption.TopDirectoryOnly).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				case 1:
					_003C_003E1__state = -1;
					return false;
				case 2:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					string current = _003C_003E7__wrap1.Current;
					_003C_003E2__current = current;
					_003C_003E1__state = 2;
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

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CEnumerateAssemblies_003Ed__3 _003CEnumerateAssemblies_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateAssemblies_003Ed__ = this;
			}
			else
			{
				_003CEnumerateAssemblies_003Ed__ = new _003CEnumerateAssemblies_003Ed__3(0);
			}
			_003CEnumerateAssemblies_003Ed__.rootPath = _003C_003E3__rootPath;
			_003CEnumerateAssemblies_003Ed__.providerId = _003C_003E3__providerId;
			return _003CEnumerateAssemblies_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	private static readonly HashSet<string> ContentModelBaseNames = new HashSet<string>(StringComparer.Ordinal)
	{
		"CardPoolModel", "CardModel", "CharacterModel", "MonsterModel", "RelicModel", "RelicPoolModel", "PotionModel", "PotionPoolModel", "EventModel", "ActModel",
		"EncounterModel", "OrbModel", "EnchantmentModel", "AfflictionModel", "ModifierModel"
	};

	private static readonly Dictionary<string, bool> Cache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

	public static bool DefinesNewContent(string? rootPath, string providerId)
	{
		if (string.IsNullOrWhiteSpace(rootPath) || string.IsNullOrWhiteSpace(providerId))
		{
			return false;
		}
		string key = rootPath + "\n" + providerId;
		lock (Cache)
		{
			if (Cache.TryGetValue(key, out var value))
			{
				return value;
			}
		}
		bool flag = false;
		try
		{
			foreach (string item in EnumerateAssemblies(rootPath, providerId))
			{
				if (ScanAssembly(item))
				{
					flag = true;
					break;
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("检查皮肤包是否新增游戏内容失败，本次按外观包处理：" + ex.GetBaseException().Message);
		}
		lock (Cache)
		{
			Cache[key] = flag;
			return flag;
		}
	}

	[IteratorStateMachine(typeof(_003CEnumerateAssemblies_003Ed__3))]
	private static IEnumerable<string> EnumerateAssemblies(string rootPath, string providerId)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateAssemblies_003Ed__3(-2)
		{
			_003C_003E3__rootPath = rootPath,
			_003C_003E3__providerId = providerId
		};
	}

	private static bool ScanAssembly(string assemblyPath)
	{
		using FileStream peStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
		if (!pEReader.HasMetadata)
		{
			return false;
		}
		MetadataReader metadataReader = pEReader.GetMetadataReader();
		foreach (TypeDefinitionHandle typeDefinition in metadataReader.TypeDefinitions)
		{
			string text = BaseTypeName(metadataReader, metadataReader.GetTypeDefinition(typeDefinition).BaseType);
			if (text != null && ContentModelBaseNames.Contains(text))
			{
				return true;
			}
		}
		return false;
	}

	private static string? BaseTypeName(MetadataReader reader, EntityHandle handle)
	{
		try
		{
			return handle.Kind switch
			{
				HandleKind.TypeDefinition => reader.GetString(reader.GetTypeDefinition((TypeDefinitionHandle)handle).Name), 
				HandleKind.TypeReference => reader.GetString(reader.GetTypeReference((TypeReferenceHandle)handle).Name), 
				HandleKind.TypeSpecification => (reader.GetBlobReader(reader.GetTypeSpecification((TypeSpecificationHandle)handle).Signature).Length > 0) ? string.Empty : null, 
				_ => null, 
			};
		}
		catch
		{
			return null;
		}
	}
}
