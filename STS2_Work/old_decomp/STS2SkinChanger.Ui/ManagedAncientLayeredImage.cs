using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class ManagedAncientLayeredImage
{
	[CompilerGenerated]
	private sealed class _003CDescendantsAndSelf_003Ed__6 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Node _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private IEnumerator<Node> _003C_003E7__wrap2;

		Node IEnumerator<Node>.Current
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
		public _003CDescendantsAndSelf_003Ed__6(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 2)
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
					_003C_003E2__current = root;
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = root.GetChildren(false).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00d0;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00b6;
					}
					IL_00d0:
					if (_003C_003E7__wrap1.MoveNext())
					{
						Node current = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap2 = DescendantsAndSelf(current).GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00b6;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
					IL_00b6:
					if (_003C_003E7__wrap2.MoveNext())
					{
						Node current2 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current2;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_00d0;
				}
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
		IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
		{
			_003CDescendantsAndSelf_003Ed__6 _003CDescendantsAndSelf_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendantsAndSelf_003Ed__ = this;
			}
			else
			{
				_003CDescendantsAndSelf_003Ed__ = new _003CDescendantsAndSelf_003Ed__6(0);
			}
			_003CDescendantsAndSelf_003Ed__.root = _003C_003E3__root;
			return _003CDescendantsAndSelf_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	private const string CoverNodeName = "SkinChangerAncientBackgroundCover";

	private const string CharacterNodeName = "SkinChangerAncientCharacter";

	private const string CharacterMaskShaderCode = "shader_type canvas_item;\nuniform sampler2D mask_texture;\n\nvoid fragment() {\n    vec4 base_color = texture(TEXTURE, UV);\n    vec4 mask_color = texture(mask_texture, UV);\n    float mask_value = mask_color.a * dot(mask_color.rgb, vec3(0.299, 0.587, 0.114));\n    COLOR = vec4(base_color.rgb, base_color.a * mask_value);\n}";

	public static void TryApply(string? groupId, Node sceneRoot)
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(groupId))
		{
			return;
		}
		try
		{
			AncientLayeredImageTextures selectedAncientLayeredImageTextures = SkinService.GetSelectedAncientLayeredImageTextures(groupId);
			if (selectedAncientLayeredImageTextures == null)
			{
				return;
			}
			string placeholderMarker = groupId + "_placeholder";
			TextureRect val = DescendantsAndSelf(sceneRoot).OfType<TextureRect>().FirstOrDefault((Func<TextureRect, bool>)delegate(TextureRect textureRect)
			{
				Texture2D texture = textureRect.Texture;
				return texture != null && ((Resource)texture).ResourcePath.Contains(placeholderMarker, StringComparison.OrdinalIgnoreCase);
			});
			Node val2 = ((val != null) ? ((Node)val).GetParent() : null);
			if (val2 == null)
			{
				ModLog.Warn("先古图层皮肤 " + groupId + " 找不到原场景占位图。");
				return;
			}
			RemoveOldLayer(val2, "SkinChangerAncientBackgroundCover");
			RemoveOldLayer(val2, "SkinChangerAncientCharacter");
			int num = ((Node)val).GetIndex(false) + 1;
			if (selectedAncientLayeredImageTextures.BackgroundCover != null)
			{
				TextureRect val3 = DuplicateLayer(val, "SkinChangerAncientBackgroundCover", selectedAncientLayeredImageTextures.BackgroundCover);
				((CanvasItem)val3).Material = null;
				val2.AddChild((Node)(object)val3, false, (InternalMode)0);
				val2.MoveChild((Node)(object)val3, num++);
			}
			TextureRect val4 = DuplicateLayer(val, "SkinChangerAncientCharacter", selectedAncientLayeredImageTextures.Character);
			if (selectedAncientLayeredImageTextures.Mask != null)
			{
				Shader shader = new Shader
				{
					Code = "shader_type canvas_item;\nuniform sampler2D mask_texture;\n\nvoid fragment() {\n    vec4 base_color = texture(TEXTURE, UV);\n    vec4 mask_color = texture(mask_texture, UV);\n    float mask_value = mask_color.a * dot(mask_color.rgb, vec3(0.299, 0.587, 0.114));\n    COLOR = vec4(base_color.rgb, base_color.a * mask_value);\n}"
				};
				ShaderMaterial val5 = new ShaderMaterial
				{
					Shader = shader
				};
				val5.SetShaderParameter(StringName.op_Implicit("mask_texture"), Variant.op_Implicit((GodotObject)(object)selectedAncientLayeredImageTextures.Mask));
				((CanvasItem)val4).Material = (Material)(object)val5;
			}
			else
			{
				((CanvasItem)val4).Material = null;
			}
			val2.AddChild((Node)(object)val4, false, (InternalMode)0);
			val2.MoveChild((Node)(object)val4, num);
			ModLog.Info("已应用 " + groupId + " 的代码型先古图层皮肤。");
		}
		catch (Exception ex)
		{
			ModLog.Warn("应用 " + groupId + " 的先古图层皮肤失败：" + ex.Message);
		}
	}

	private static TextureRect DuplicateLayer(TextureRect source, string name, Texture2D texture)
	{
		Node obj = ((Node)source).Duplicate(15);
		Node obj2 = ((obj is TextureRect) ? obj : null) ?? throw new InvalidOperationException("无法复制先古场景占位图节点。");
		obj2.Name = StringName.op_Implicit(name);
		((TextureRect)obj2).Texture = texture;
		return (TextureRect)(object)obj2;
	}

	private static void RemoveOldLayer(Node parent, string name)
	{
		Node nodeOrNull = parent.GetNodeOrNull<Node>(NodePath.op_Implicit(name));
		if (nodeOrNull != null)
		{
			((GodotObject)nodeOrNull).Free();
		}
	}

	[IteratorStateMachine(typeof(_003CDescendantsAndSelf_003Ed__6))]
	private static IEnumerable<Node> DescendantsAndSelf(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendantsAndSelf_003Ed__6(-2)
		{
			_003C_003E3__root = root
		};
	}
}
