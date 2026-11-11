using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal sealed class WorkshopPagedChoice
{
	private string[] _ids = Array.Empty<string>();

	private string _value = "";

	private int _page;

	private readonly Func<string, string> _name;

	private readonly Action<string> _changed;

	private readonly Func<string> _allName;

	public OptionButton Picker { get; } = new OptionButton
	{
		FitToLongestItem = false,
		ClipText = true,
		CustomMinimumSize = new Vector2(215f, 42f),
		SizeFlagsHorizontal = (SizeFlags)3
	};

	public WorkshopPagedChoice(Func<string, string> name, Action<string> changed, Func<string>? allName = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		_name = name;
		_changed = changed;
		_allName = allName ?? ((Func<string>)(() => WorkshopText.Get(WorkshopTextKey.All)));
		ContextualSkinControls.ApplyGameTheme(Picker);
		Picker.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			Variant itemMetadata = Picker.GetItemMetadata((int)index);
			string text = ((Variant)(ref itemMetadata)).AsString();
			if ((text == "__previous_page__" || text == "__next_page__") ? true : false)
			{
				_page += ((!(text == "__previous_page__")) ? 1 : (-1));
				Populate();
				Callable val = Callable.From((Action)delegate
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)Picker) && ((Node)Picker).IsInsideTree() && ((CanvasItem)Picker).IsVisibleInTree())
					{
						Picker.ShowPopup();
					}
				});
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
			else
			{
				_value = text;
				_changed(text);
			}
		};
	}

	public void SetOptions(IEnumerable<string> ids, string value)
	{
		_ids = ids.Where((string id) => id.Length > 0).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		if (value.Length > 0 && !_ids.Contains<string>(value, StringComparer.OrdinalIgnoreCase))
		{
			string[] ids2 = _ids;
			int num = 0;
			string[] array = new string[1 + ids2.Length];
			ReadOnlySpan<string> readOnlySpan = new ReadOnlySpan<string>(ids2);
			readOnlySpan.CopyTo(new Span<string>(array).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			array[num] = value;
			_ids = array;
		}
		_value = value;
		_page = Math.Max(0, Array.FindIndex(_ids, (string id) => id.Equals(value, StringComparison.OrdinalIgnoreCase))) / 10;
		Populate();
	}

	private void Populate()
	{
		Picker.Clear();
		Add(_allName(), "");
		string[] array = WorkshopBrowserPolicy.Page(_ids, _page);
		foreach (string text2 in array)
		{
			Add(_name(text2), text2);
		}
		int num = Math.Max(1, (_ids.Length + 9) / 10);
		if (_page > 0)
		{
			Add("‹ " + WorkshopText.Get(WorkshopTextKey.Previous) + $"  {_page}/{num}", "__previous_page__");
		}
		if (_page + 1 < num)
		{
			Add(WorkshopText.Get(WorkshopTextKey.Next) + $"  {_page + 2}/{num} ›", "__next_page__");
		}
		((Button)Picker).Text = ((_value.Length == 0) ? _allName() : _name(_value));
		((Control)Picker).TooltipText = ((Button)Picker).Text;
		void Add(string text, string id)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			int itemCount = Picker.ItemCount;
			Picker.AddItem(text, -1);
			Picker.SetItemMetadata(itemCount, Variant.op_Implicit(id));
			Picker.GetPopup().SetItemTooltip(itemCount, text);
			if (id == _value)
			{
				Picker.Select(itemCount);
			}
		}
	}
}
