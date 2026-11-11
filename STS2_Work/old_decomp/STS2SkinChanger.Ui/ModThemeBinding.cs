using System;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class ModThemeBinding : Node
{
	internal Action<ModThemeSettings> Refresh = delegate
	{
	};

	private bool _connected;

	public ModThemeBinding()
	{
		((Node)this).TreeEntered += Connect;
		((Node)this).TreeExiting += Disconnect;
	}

	private void Connect()
	{
		if (!_connected)
		{
			_connected = true;
			ModThemeRuntime.Session.Changed += Apply;
			Apply();
		}
	}

	private void Disconnect()
	{
		if (_connected)
		{
			_connected = false;
			ModThemeRuntime.Session.Changed -= Apply;
		}
	}

	internal void Apply()
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)this) || ((GodotObject)this).IsQueuedForDeletion())
		{
			return;
		}
		Node parent = ((Node)this).GetParent();
		if (parent != null && ((GodotObject)parent).IsQueuedForDeletion())
		{
			return;
		}
		try
		{
			Refresh(ModThemeRuntime.Current);
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新 Mod 主题失败：" + ex.GetBaseException().Message);
		}
	}
}
