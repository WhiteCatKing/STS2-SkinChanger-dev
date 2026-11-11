using System;
using Godot;

namespace PengoTarot;

public static class NodeExtensions
{
	public static void OnReady(this Node node, Action action)
	{
		if (node.IsNodeReady())
		{
			action();
		}
		else
		{
			node.Ready += action;
		}
	}
}
