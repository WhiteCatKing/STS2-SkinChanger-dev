using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznCore.Presentation;

internal sealed class CombatSpineAnimationAvailabilityCache
{
	private sealed class SpineAnimationCache
	{
		private readonly MegaSprite _sprite;

		private readonly Dictionary<string, bool> _animations = new Dictionary<string, bool>(StringComparer.Ordinal);

		public SpineAnimationCache(Node spineNode)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			_sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)spineNode));
		}

		public bool HasAnimation(string animationName, Action<string>? warn)
		{
			if (_animations.TryGetValue(animationName, out var value))
			{
				return value;
			}
			bool flag;
			try
			{
				flag = _sprite.HasAnimation(animationName);
			}
			catch (Exception ex)
			{
				warn?.Invoke("Spine animation lookup failed: " + animationName + " " + SkinLog.FormatException(ex));
				flag = false;
			}
			if (!flag)
			{
				warn?.Invoke("Missing Spine animation: " + animationName);
			}
			_animations[animationName] = flag;
			return flag;
		}
	}

	private readonly ConditionalWeakTable<Node, SpineAnimationCache> _caches = new ConditionalWeakTable<Node, SpineAnimationCache>();

	public bool HasAnimation(Node? spineNode, string? animationName, Action<string>? warn = null)
	{
		if (spineNode == null || string.IsNullOrEmpty(animationName) || !GodotObject.IsInstanceValid((GodotObject)(object)spineNode))
		{
			return false;
		}
		if (!_caches.TryGetValue(spineNode, out SpineAnimationCache value))
		{
			try
			{
				value = new SpineAnimationCache(spineNode);
				_caches.Add(spineNode, value);
			}
			catch (Exception ex)
			{
				warn?.Invoke("Spine animation cache init failed: " + SkinLog.FormatException(ex));
				return false;
			}
		}
		return value.HasAnimation(animationName, warn);
	}

	public void Clear()
	{
		_caches.Clear();
	}
}
