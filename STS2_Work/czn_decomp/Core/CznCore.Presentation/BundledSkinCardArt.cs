using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using CznCore.Settings;
using Godot;

namespace CznCore.Presentation;

internal sealed class BundledSkinCardArt
{
	internal sealed class Document
	{
		[JsonPropertyName("entries")]
		public List<Entry> Entries { get; set; } = new List<Entry>();
	}

	internal sealed class Entry
	{
		[JsonPropertyName("cardId")]
		public string CardId { get; set; } = "";

		[JsonPropertyName("image")]
		public string Image { get; set; } = "";

		[JsonPropertyName("ancientImage")]
		public string AncientImage { get; set; } = "";
	}

	private readonly string _modId;

	private readonly CharacterSkinProfile _profile;

	private readonly Action<string, int> _warn;

	private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(StringComparer.Ordinal);

	private readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);

	private readonly SettingsLocalizationStore _text;

	private bool _enabled = true;

	private string ConfigPath => "user://CznModConfig/" + _modId + "/card_art.cfg";

	internal string Owner => _profile.ProfileId;

	internal bool Enabled
	{
		get
		{
			if (_enabled)
			{
				return CharacterAppearanceCatalog.IsActive(_profile);
			}
			return false;
		}
	}

	public BundledSkinCardArt(string modId, CharacterSkinProfile profile, Action<string, int> warn)
	{
		_modId = modId;
		_profile = profile;
		_warn = warn;
		_text = new SettingsLocalizationStore("res://" + modId + "/localization/{0}/bundled_card_art.json", warn);
	}

	public void Register()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I8
		ConfigFile val = new ConfigFile();
		try
		{
			Error val2 = val.Load(ConfigPath);
			if ((int)val2 == 0)
			{
				_enabled = ReadEnabled(val);
			}
			else if ((long)val2 != 7)
			{
				_warn("Card-art settings load failed: " + ((object)(Error)(ref val2)/*cast due to .constrained prefix*/).ToString(), 2);
			}
			LoadEntries(FileAccess.GetFileAsString("res://" + _modId + "/card_replacements.json"));
			BundledSkinCardArtService.Register(this);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal static bool ReadEnabled(ConfigFile config)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I8
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (config.HasSectionKey("card_art", "selected_variant"))
		{
			Variant val = default(Variant);
			val = config.GetValue("card_art", "selected_variant", val);
			return ((Variant)(ref val)).AsString() != "base";
		}
		Variant value = config.GetValue("card_art", "enabled", Variant.op_Implicit(true));
		if ((long)((Variant)(ref value)).VariantType == 1)
		{
			return ((Variant)(ref value)).AsBool();
		}
		return true;
	}

	internal void LoadEntries(string json)
	{
		Document? obj = JsonSerializer.Deserialize<Document>(json) ?? throw new InvalidOperationException("Missing card-art document");
		_entries.Clear();
		foreach (Entry entry in obj.Entries)
		{
			if (string.IsNullOrWhiteSpace(entry.CardId) || !entry.Image.StartsWith("res://" + _modId + "/card_art/", StringComparison.Ordinal) || (!string.IsNullOrEmpty(entry.AncientImage) && !entry.AncientImage.StartsWith("res://" + _modId + "/card_art/", StringComparison.Ordinal)))
			{
				throw new InvalidOperationException("Invalid card-art mapping: " + entry.CardId);
			}
			_entries.Add(entry.CardId, entry);
		}
	}

	public void Unregister()
	{
		BundledSkinCardArtService.Unregister(this);
	}

	internal bool Contains(string cardId)
	{
		return _entries.ContainsKey(cardId);
	}

	internal void Warn(string message)
	{
		_warn(message, 2);
	}

	internal Texture2D? Texture(string cardId, bool ancient)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (!_entries.TryGetValue(cardId, out Entry value))
		{
			return null;
		}
		string text = ((ancient && !string.IsNullOrEmpty(value.AncientImage)) ? value.AncientImage : value.Image);
		if (_textures.TryGetValue(text, out Texture2D value2))
		{
			return value2;
		}
		if (!FileAccess.FileExists(text))
		{
			Texture2D val = (ResourceLoader.Exists(text, "") ? ResourceLoader.Load<Texture2D>(text, (string)null, (CacheMode)1) : null);
			if (val != null)
			{
				_textures.Add(text, val);
			}
			else
			{
				_warn("Card-art texture missing: " + text, 2);
			}
			return val;
		}
		Image val2 = new Image();
		try
		{
			Error val3 = val2.Load(text);
			if ((int)val3 != 0)
			{
				_warn("Card-art PNG load failed: " + text + " / " + ((object)(Error)(ref val3)/*cast due to .constrained prefix*/).ToString(), 2);
				return null;
			}
			ImageTexture val4 = ImageTexture.CreateFromImage(val2);
			_textures.Add(text, (Texture2D)(object)val4);
			return (Texture2D?)(object)val4;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public IReadOnlyList<SettingsUiExtensionGroup> CreateCharacterExtensions()
	{
		return new _003C_003Ez__ReadOnlySingleElementList<SettingsUiExtensionGroup>(new SettingsUiExtensionGroup(_modId + ".CardArt", "visual", 400, () => _text.Get("header", "Card Art"), delegate
		{
			SetEnabled(value: true);
		}, new _003C_003Ez__ReadOnlySingleElementList<SettingsUiExtensionControl>(new SettingsUiExtensionToggle("card_art_variant", 100, () => _text.Get("enabled", "Use bundled character card art"), () => _text.Get("description", "Use the card images included with this skin. Turn off to use the current base card art."), () => _enabled, SetEnabled, () => true))));
	}

	private void SetEnabled(bool value)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I8
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Invalid comparison between Unknown and I8
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (_enabled == value)
		{
			return;
		}
		_enabled = value;
		Error val = DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath("user://CznModConfig/" + _modId));
		if ((int)val == 0 || (long)val == 32)
		{
			ConfigFile val2 = new ConfigFile();
			try
			{
				val2.SetValue("card_art", "selected_variant", Variant.op_Implicit(value ? Owner : "base"));
				val = val2.Save(ConfigPath);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		if ((int)val != 0 && (long)val != 32)
		{
			_warn("Card-art settings save failed: " + ((object)(Error)(ref val)/*cast due to .constrained prefix*/).ToString(), 2);
		}
		BundledSkinCardArtService.Refresh();
	}
}
