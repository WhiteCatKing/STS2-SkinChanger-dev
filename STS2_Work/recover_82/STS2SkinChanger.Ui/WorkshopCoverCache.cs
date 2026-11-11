using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class WorkshopCoverCache
{
	private static readonly WorkshopSessionCache<Texture2D?> Thumbnails = new WorkshopSessionCache<Texture2D>();

	private static readonly BoundedLruCache<string, Texture2D?> Previews = new BoundedLruCache<string, Texture2D>(24, StringComparer.Ordinal);

	private static readonly Dictionary<string, Task<Texture2D?>> PendingPreviews = new Dictionary<string, Task<Texture2D>>();

	public static Task<Texture2D?> Get(string url, CancellationToken token)
	{
		return Thumbnails.Get(url, () => Load(url), token);
	}

	public static Task<Texture2D?> GetPreview(string url, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (Previews.TryGetValue(url, out Texture2D value))
		{
			return Task.FromResult<Texture2D>(value);
		}
		if (!PendingPreviews.TryGetValue(url, out Task<Texture2D> value2))
		{
			value2 = LoadPreview(url);
			PendingPreviews[url] = value2;
			if (value2.IsCompleted)
			{
				PendingPreviews.Remove(url);
			}
		}
		return value2.WaitAsync(token);
	}

	private static async Task<Texture2D?> LoadPreview(string url)
	{
		try
		{
			Texture2D val = await Load(url, 640, 360);
			Previews.Set(url, val, out KeyValuePair<string, Texture2D> _);
			return val;
		}
		finally
		{
			PendingPreviews.Remove(url);
		}
	}

	private static async Task<Texture2D?> Load(string url, int width = 144, int height = 100)
	{
		try
		{
			byte[] array = await SkinWorkshopService.Cover(url, CancellationToken.None);
			if (array == null)
			{
				return null;
			}
			Image val = new Image();
			try
			{
				if ((int)((array[0] == 137) ? val.LoadPngFromBuffer(array) : val.LoadJpgFromBuffer(array)) != 0 || val.GetWidth() > 4096 || val.GetHeight() > 4096)
				{
					return null;
				}
				double num = Math.Min(1.0, Math.Min((double)width / (double)val.GetWidth(), (double)height / (double)val.GetHeight()));
				val.Resize(Math.Max(1, (int)((double)val.GetWidth() * num)), Math.Max(1, (int)((double)val.GetHeight() * num)), (Interpolation)4);
				return (Texture2D?)(object)ImageTexture.CreateFromImage(val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			ModLog.Info("工坊封面暂不可用，本次保留占位图：" + ex.Message);
			return null;
		}
	}
}
