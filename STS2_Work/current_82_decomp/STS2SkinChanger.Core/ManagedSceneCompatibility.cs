using System.Collections.Generic;
using Godot;

namespace STS2SkinChanger.Core;

internal static class ManagedSceneCompatibility
{
	public static int CopyMissingUniqueNodes(Node? baselineRoot, Node? replacementRoot)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)baselineRoot) || !GodotObject.IsInstanceValid((GodotObject)(object)replacementRoot) || baselineRoot == replacementRoot)
		{
			return 0;
		}
		List<Node> list = new List<Node>();
		CollectUniqueNodes(baselineRoot, baselineRoot, list);
		list.Sort((Node left, Node right) => GetRelativeDepth(left, baselineRoot).CompareTo(GetRelativeDepth(right, baselineRoot)));
		int num = 0;
		foreach (Node item in list)
		{
			if (item == baselineRoot)
			{
				continue;
			}
			string text = ((object)item.Name).ToString();
			if (!string.IsNullOrWhiteSpace(text) && replacementRoot.GetNodeOrNull<Node>(NodePath.op_Implicit("%" + text)) == null)
			{
				Node val = item.Duplicate(15);
				if (val != null)
				{
					Node obj = FindDestinationParent(baselineRoot, replacementRoot, item.GetParent());
					val.Name = item.Name;
					obj.AddChild(val, false, (InternalMode)0);
					val.UniqueNameInOwner = true;
					SetOwnerRecursive(val, replacementRoot);
					num++;
				}
			}
		}
		return num;
	}

	private static void CollectUniqueNodes(Node node, Node owner, ICollection<Node> output)
	{
		if (node.UniqueNameInOwner && (node == owner || node.Owner == owner))
		{
			output.Add(node);
		}
		foreach (Node child in node.GetChildren(false))
		{
			CollectUniqueNodes(child, owner, output);
		}
	}

	private static int GetRelativeDepth(Node node, Node root)
	{
		int num = 0;
		Node val = node;
		while (val != null && val != root)
		{
			num++;
			val = val.GetParent();
		}
		return num;
	}

	private static Node FindDestinationParent(Node baselineRoot, Node replacementRoot, Node? baselineParent)
	{
		if (baselineParent == null || baselineParent == baselineRoot)
		{
			return replacementRoot;
		}
		Stack<string> stack = new Stack<string>();
		Node val = baselineParent;
		while (val != null && val != baselineRoot)
		{
			stack.Push(((object)val.Name).ToString());
			val = val.GetParent();
		}
		Node obj;
		if (stack.Count != 0)
		{
			obj = replacementRoot.GetNodeOrNull<Node>(NodePath.op_Implicit(string.Join('/', stack)));
			if (obj == null)
			{
				return replacementRoot;
			}
		}
		else
		{
			obj = replacementRoot;
		}
		return obj;
	}

	private static void SetOwnerRecursive(Node node, Node owner)
	{
		if (node != owner)
		{
			node.Owner = owner;
		}
		foreach (Node child in node.GetChildren(false))
		{
			SetOwnerRecursive(child, owner);
		}
	}
}
