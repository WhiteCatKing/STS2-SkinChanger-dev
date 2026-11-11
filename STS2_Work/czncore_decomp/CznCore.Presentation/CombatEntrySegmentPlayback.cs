using System;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznCore.Presentation;

internal sealed class CombatEntrySegmentPlayback : IDisposable
{
	private readonly Node2D _spine;

	private readonly Callable _started;

	private readonly Callable _ended;

	private readonly Action _completed;

	private bool _disposed;

	public CombatEntrySegmentPlayback(Node2D spine, Node2D visuals, Vector2 target, string[] clips, Vector2[] offsets, Action completed)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		CombatEntrySegmentPlayback combatEntrySegmentPlayback = this;
		_spine = spine;
		_completed = completed;
		_started = Callable.From<GodotObject, GodotObject, GodotObject>((Action<GodotObject, GodotObject, GodotObject>)delegate(GodotObject _, GodotObject _, GodotObject nativeTrack)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			MegaTrackEntry val = new MegaTrackEntry(Variant.op_Implicit(nativeTrack));
			try
			{
				Variant val2 = nativeTrack.Call(StringName.op_Implicit("get_track_index"), Array.Empty<Variant>());
				if (((Variant)(ref val2)).AsInt32() == 0)
				{
					int num = Array.IndexOf(clips, val.GetAnimationName());
					if (num >= 0)
					{
						if (num < offsets.Length)
						{
							visuals.Position = target + offsets[num];
						}
					}
					else
					{
						combatEntrySegmentPlayback.Complete();
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		});
		_ended = Callable.From<GodotObject, GodotObject, GodotObject>((Action<GodotObject, GodotObject, GodotObject>)delegate(GodotObject _, GodotObject state, GodotObject nativeTrack)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			Variant val3 = nativeTrack.Call(StringName.op_Implicit("get_track_index"), Array.Empty<Variant>());
			if (((Variant)(ref val3)).AsInt32() != 0)
			{
				return;
			}
			Variant val4 = state.Call(StringName.op_Implicit("get_current"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0) });
			try
			{
				if (((Variant)(ref val4)).AsGodotObject() == null)
				{
					combatEntrySegmentPlayback.Complete();
				}
			}
			finally
			{
				((IDisposable)(Variant)(ref val4)/*cast due to .constrained prefix*/).Dispose();
			}
		});
		Error val5 = ((GodotObject)spine).Connect(StringName.op_Implicit("animation_started"), _started, 0u);
		if ((int)val5 != 0)
		{
			throw new InvalidOperationException("Cannot bind entry segments: " + ((object)(Error)(ref val5)/*cast due to .constrained prefix*/).ToString());
		}
		val5 = ((GodotObject)spine).Connect(StringName.op_Implicit("animation_ended"), _ended, 0u);
		if ((int)val5 != 0)
		{
			Dispose();
			throw new InvalidOperationException("Cannot bind entry cancellation: " + ((object)(Error)(ref val5)/*cast due to .constrained prefix*/).ToString());
		}
		((Node)spine).TreeExiting += Complete;
	}

	private void Complete()
	{
		if (!_disposed)
		{
			Dispose();
			_completed();
		}
	}

	public void Dispose()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_spine))
		{
			if (((GodotObject)_spine).IsConnected(StringName.op_Implicit("animation_started"), _started))
			{
				((GodotObject)_spine).Disconnect(StringName.op_Implicit("animation_started"), _started);
			}
			if (((GodotObject)_spine).IsConnected(StringName.op_Implicit("animation_ended"), _ended))
			{
				((GodotObject)_spine).Disconnect(StringName.op_Implicit("animation_ended"), _ended);
			}
			((Node)_spine).TreeExiting -= Complete;
		}
	}
}
