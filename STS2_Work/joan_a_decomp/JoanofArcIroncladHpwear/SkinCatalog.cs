using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace JoanofArcIroncladHpwear;

public static class SkinCatalog
{
	[CompilerGenerated]
	private sealed class _003CAllResources_003Ed__17 : IEnumerable<(string Group, int Stage)>, IEnumerable, IEnumerator<(string Group, int Stage)>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private (string Group, int Stage) _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string[] _003C_003E7__wrap1;

		private int _003C_003E7__wrap2;

		(string Group, int Stage) IEnumerator<(string, int)>.Current
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
		public _003CAllResources_003Ed__17(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E7__wrap1 = BattleGroups;
				_003C_003E7__wrap2 = 0;
				goto IL_006f;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E7__wrap2++;
				goto IL_006f;
			case 2:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_006f:
				if (_003C_003E7__wrap2 < _003C_003E7__wrap1.Length)
				{
					string item = _003C_003E7__wrap1[_003C_003E7__wrap2];
					_003C_003E2__current = (Group: item, Stage: 1);
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E7__wrap1 = null;
				_003C_003E2__current = (Group: "charselect", Stage: 1);
				_003C_003E1__state = 2;
				return true;
			}
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
		IEnumerator<(string Group, int Stage)> IEnumerable<(string, int)>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CAllResources_003Ed__17(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<(string, int)>)this).GetEnumerator();
		}
	}

	public const string ModId = "JoanofArcIroncladHpwear";

	public const string Root = "res://joanofarcironcladhpwear/";

	public const string TargetCharacter = "ironclad";

	public const string CombatGroup = "ironclad";

	public const string ShopGroup = "ironclad_shop";

	public const string RestGroup = "restsite";

	public const string CharSelectGroup = "charselect";

	public const int CharSelectMaxStage = 1;

	public static readonly string[] BattleGroups = new string[3] { "ironclad", "ironclad_shop", "restsite" };

	public static string Stem(string group, int stage)
	{
		return $"s{stage}_{group}";
	}

	public static string DataPath(string group, int stage)
	{
		return "res://joanofarcironcladhpwear/" + Stem(group, stage) + "_data.tres";
	}

	public static string SkelPath(string group, int stage)
	{
		return "res://joanofarcironcladhpwear/" + Stem(group, stage) + ".spskel";
	}

	public static string AtlasPath(string group, int stage)
	{
		return "res://joanofarcironcladhpwear/" + Stem(group, stage) + ".spatlas";
	}

	public static string TexturePath(string group, int stage)
	{
		return "res://joanofarcironcladhpwear/" + Stem(group, stage) + ".ctex";
	}

	public static bool IsOurPath(string? resourcePath)
	{
		if (string.IsNullOrEmpty(resourcePath))
		{
			return false;
		}
		if (resourcePath.StartsWith("res://joanofarcironcladhpwear/", StringComparison.Ordinal))
		{
			return true;
		}
		if (resourcePath.StartsWith("res://sts2_skin_runtime/", StringComparison.Ordinal) && resourcePath.Contains("/joanofarcironcladhpwear/", StringComparison.Ordinal))
		{
			return !resourcePath.Contains("/../", StringComparison.Ordinal);
		}
		return false;
	}

	public static int StageFromPath(string group, string? resourcePath)
	{
		for (int i = 1; i <= MaxStageFor(group); i++)
		{
			if (IsOurPath(resourcePath) && resourcePath.EndsWith("/" + Stem(group, i) + ".spskel", StringComparison.Ordinal))
			{
				return i;
			}
		}
		return 0;
	}

	public static int MaxStageFor(string group)
	{
		return 1;
	}

	[IteratorStateMachine(typeof(_003CAllResources_003Ed__17))]
	public static IEnumerable<(string Group, int Stage)> AllResources()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CAllResources_003Ed__17(-2);
	}
}
