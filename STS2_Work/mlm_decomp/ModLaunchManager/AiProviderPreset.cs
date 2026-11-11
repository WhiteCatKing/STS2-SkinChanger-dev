using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ModLaunchManager;

internal sealed record AiProviderPreset(AiTranslationProvider Provider, string Id, string Endpoint, string DefaultModel, string? KeyPage, int Concurrency)
{
	internal bool IsPreset => Provider != AiTranslationProvider.Custom;

	internal static readonly AiProviderPreset[] All = new AiProviderPreset[3]
	{
		new AiProviderPreset(AiTranslationProvider.DeepSeek, "deepseek", "https://api.deepseek.com", "deepseek-flash", "https://platform.deepseek.com/api_keys", 4),
		new AiProviderPreset(AiTranslationProvider.OpenAI, "openai", "https://api.openai.com/v1", "gpt-6-luna", "https://platform.openai.com/api-keys", 4),
		new AiProviderPreset(AiTranslationProvider.Custom, "custom", "", "", null, 1)
	};

	private static readonly Regex OpenAiReasoningModel = new Regex("^(o\\d|gpt-[5-9])(?!.*chat)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	internal static AiProviderPreset For(AiTranslationProvider provider)
	{
		return All.First((AiProviderPreset p) => p.Provider == provider);
	}

	internal static AiProviderPreset? ForId(string? id)
	{
		return All.FirstOrDefault((AiProviderPreset p) => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));
	}

	internal static AiTranslationProvider DefaultFor(string locale)
	{
		if (!locale.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
		{
			return AiTranslationProvider.OpenAI;
		}
		return AiTranslationProvider.DeepSeek;
	}

	internal static AiTranslationProvider Infer(string endpoint)
	{
		if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out Uri result))
		{
			return AiTranslationProvider.Custom;
		}
		AiProviderPreset[] all = All;
		foreach (AiProviderPreset aiProviderPreset in all)
		{
			if (aiProviderPreset.IsPreset && string.Equals(result.Host, new Uri(aiProviderPreset.Endpoint).Host, StringComparison.OrdinalIgnoreCase))
			{
				return aiProviderPreset.Provider;
			}
		}
		return AiTranslationProvider.Custom;
	}

	internal void AddRequestOptions(JsonObject body, string model, int maxOutputTokens, bool customThinkingSwitch)
	{
		switch (Provider)
		{
		case AiTranslationProvider.Custom:
			if (customThinkingSwitch)
			{
				body["thinking"] = new JsonObject((JsonNodeOptions?)null) { ["type"] = "disabled" };
			}
			break;
		case AiTranslationProvider.DeepSeek:
			body["response_format"] = new JsonObject((JsonNodeOptions?)null) { ["type"] = "json_object" };
			body["thinking"] = new JsonObject((JsonNodeOptions?)null) { ["type"] = "disabled" };
			body["max_tokens"] = maxOutputTokens;
			break;
		case AiTranslationProvider.OpenAI:
		{
			body["response_format"] = new JsonObject((JsonNodeOptions?)null) { ["type"] = "json_object" };
			bool flag = OpenAiReasoningModel.IsMatch(model);
			if (flag)
			{
				body["reasoning_effort"] = "low";
			}
			body["max_completion_tokens"] = maxOutputTokens + (flag ? 8192 : 0);
			break;
		}
		}
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Provider = ");
		builder.Append(Provider.ToString());
		builder.Append(", Id = ");
		builder.Append((object?)Id);
		builder.Append(", Endpoint = ");
		builder.Append((object?)Endpoint);
		builder.Append(", DefaultModel = ");
		builder.Append((object?)DefaultModel);
		builder.Append(", KeyPage = ");
		builder.Append((object?)KeyPage);
		builder.Append(", Concurrency = ");
		builder.Append(Concurrency.ToString());
		return true;
	}

	[CompilerGenerated]
	private AiProviderPreset(AiProviderPreset original)
	{
		Provider = original.Provider;
		Id = original.Id;
		Endpoint = original.Endpoint;
		DefaultModel = original.DefaultModel;
		KeyPage = original.KeyPage;
		Concurrency = original.Concurrency;
	}
}
