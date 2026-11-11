using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class LauncherThemeConverter : JsonConverter<LauncherTheme>
{
	public override LauncherTheme Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		JsonTokenType tokenType = reader.TokenType;
		if ((tokenType == JsonTokenType.StartObject || tokenType == JsonTokenType.StartArray) ? true : false)
		{
			reader.Skip();
			return LauncherTheme.System;
		}
		if (reader.TokenType != JsonTokenType.String)
		{
			return LauncherTheme.System;
		}
		string @string = reader.GetString();
		LauncherTheme[] values = Enum.GetValues<LauncherTheme>();
		for (int i = 0; i < values.Length; i++)
		{
			LauncherTheme result = values[i];
			if (string.Equals(@string, result.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return result;
			}
		}
		return LauncherTheme.System;
	}

	public override void Write(Utf8JsonWriter writer, LauncherTheme value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
