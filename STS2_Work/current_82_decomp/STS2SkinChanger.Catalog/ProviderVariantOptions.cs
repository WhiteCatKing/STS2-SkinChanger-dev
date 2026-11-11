using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace STS2SkinChanger.Catalog;

internal static class ProviderVariantOptions
{
	[CompilerGenerated]
	private sealed class _003CFindVariants_003Ed__2 : IEnumerable<(string Suffix, string Path)>, IEnumerable, IEnumerator<(string Suffix, string Path)>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private (string Suffix, string Path) _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string groupId;

		public string _003C_003E3__groupId;

		private SkinOption option;

		public SkinOption _003C_003E3__option;

		private string _003Cprefix_003E5__2;

		private IEnumerator<KeyValuePair<string, ResourceAsset>> _003C_003E7__wrap2;

		(string Suffix, string Path) IEnumerator<(string, string)>.Current
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
		public _003CFindVariants_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Cprefix_003E5__2 = null;
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
					_003Cprefix_003E5__2 = "res://scenes/creature_visuals/" + groupId + "_";
					_003C_003E7__wrap2 = option.Assets.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap2.MoveNext())
				{
					string sourcePath = _003C_003E7__wrap2.Current.Value.SourcePath;
					if (sourcePath.StartsWith(_003Cprefix_003E5__2, StringComparison.OrdinalIgnoreCase) && sourcePath.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase))
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);
						int num = groupId.Length + 1;
						string text = fileNameWithoutExtension.Substring(num, fileNameWithoutExtension.Length - num);
						if (text.Length != 0 && !text.Contains('/'))
						{
							_003C_003E2__current = (Suffix: text, Path: sourcePath);
							_003C_003E1__state = 1;
							return true;
						}
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
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
		IEnumerator<(string Suffix, string Path)> IEnumerable<(string, string)>.GetEnumerator()
		{
			_003CFindVariants_003Ed__2 _003CFindVariants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CFindVariants_003Ed__ = this;
			}
			else
			{
				_003CFindVariants_003Ed__ = new _003CFindVariants_003Ed__2(0);
			}
			_003CFindVariants_003Ed__.option = _003C_003E3__option;
			_003CFindVariants_003Ed__.groupId = _003C_003E3__groupId;
			return _003CFindVariants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<(string, string)>)this).GetEnumerator();
		}
	}

	internal static void Expand(IDictionary<string, SkinGroup> groups)
	{
		foreach (SkinGroup value in groups.Values)
		{
			SkinOption[] array = value.Options.ToArray();
			foreach (SkinOption skinOption in array)
			{
				if (skinOption.Id.Contains("::variant:", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				KeyValuePair<string, ResourceAsset>? keyValuePair = FindScene(skinOption, value.Id);
				if (!keyValuePair.HasValue)
				{
					continue;
				}
				(string, string)[] array2 = FindVariants(skinOption, value.Id).ToArray();
				if (array2.Length != 0)
				{
					int index = value.Options.IndexOf(skinOption);
					value.Options.RemoveAt(index);
					value.Options.Insert(index++, skinOption with
					{
						Name = skinOption.Name + "_默认"
					});
					(string, string)[] array3 = array2;
					for (int j = 0; j < array3.Length; j++)
					{
						(string, string) tuple = array3[j];
						string item = tuple.Item1;
						string item2 = tuple.Item2;
						ResourceAsset resourceAsset = new ResourceAsset(item2);
						resourceAsset.AddFile(keyValuePair.Value.Value.Files[0].Archive, item2);
						Dictionary<string, ResourceAsset> assets = new Dictionary<string, ResourceAsset>(skinOption.Assets, StringComparer.Ordinal) { [keyValuePair.Value.Key] = resourceAsset };
						value.Options.Insert(index++, skinOption with
						{
							Id = skinOption.Id + "::variant:" + item,
							Name = skinOption.Name + "_" + Label(item),
							ProviderId = skinOption.EffectiveProviderId,
							Assets = assets
						});
					}
				}
			}
		}
	}

	private static KeyValuePair<string, ResourceAsset>? FindScene(SkinOption option, string stem)
	{
		foreach (KeyValuePair<string, ResourceAsset> asset in option.Assets)
		{
			string sourcePath = asset.Value.SourcePath;
			if (sourcePath.StartsWith("res://scenes/creature_visuals/", StringComparison.OrdinalIgnoreCase) && sourcePath.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) && Path.GetFileNameWithoutExtension(sourcePath).Equals(stem, StringComparison.OrdinalIgnoreCase))
			{
				return asset;
			}
		}
		return null;
	}

	[IteratorStateMachine(typeof(_003CFindVariants_003Ed__2))]
	private static IEnumerable<(string Suffix, string Path)> FindVariants(SkinOption option, string groupId)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFindVariants_003Ed__2(-2)
		{
			_003C_003E3__option = option,
			_003C_003E3__groupId = groupId
		};
	}

	private static string Label(string suffix)
	{
		return suffix.ToLowerInvariant() switch
		{
			"aggressor" => "侵略者", 
			"banneret" => "方旗骑士", 
			"templar" => "圣殿骑士", 
			"infernal" => "渊狱", 
			"default" => "默认皮肤", 
			_ => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(suffix.Replace('_', ' ')), 
		};
	}
}
