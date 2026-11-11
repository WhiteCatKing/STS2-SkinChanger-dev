using System;
using System.Collections.Generic;
using Godot;

namespace DesktopDeathSafety;

public static class TopBarPalette
{
	private const string Root = "res://joanofarcironcladhpwear/";

	private static readonly Dictionary<int, (ImageTexture, int, int, byte[][], byte[])> Pages = new Dictionary<int, (ImageTexture, int, int, byte[][], byte[])>();

	private static readonly HashSet<int> Missing = new HashSet<int>();

	private static Vector4 weights = new Vector4(1f, 0f, 0f, 0f);

	public static Texture2D Texture(int stage, bool selection)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		if (selection || Missing.Contains(stage))
		{
			return null;
		}
		if (Pages.TryGetValue(stage, out var value))
		{
			return (Texture2D)(object)value.Item1;
		}
		try
		{
			string text = $"s{stage}_run_icon";
			Texture2D obj = GD.Load<Texture2D>("res://joanofarcironcladhpwear/" + text + ".ctex");
			Image val = ((obj != null) ? obj.GetImage() : null);
			try
			{
				if (val == null || val.IsEmpty())
				{
					throw new InvalidOperationException("Original icon is missing.");
				}
				int width = val.GetWidth();
				int height = val.GetHeight();
				val.Convert((Format)5);
				byte[][] array = new byte[4][]
				{
					val.GetData(),
					null,
					null,
					null
				};
				for (int i = 1; i < array.Length; i++)
				{
					Image val2 = new Image();
					try
					{
						string text2 = "res://joanofarcironcladhpwear/" + $"portrait_palettes/{i}/{text}.png";
						if ((int)val2.LoadPngFromBuffer(FileAccess.GetFileAsBytes(text2)) != 0 || val2.GetWidth() != width || val2.GetHeight() != height)
						{
							throw new InvalidOperationException("Invalid color icon " + text2);
						}
						val2.Convert((Format)5);
						array[i] = val2.GetData();
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				byte[] array2 = new byte[array[0].Length];
				Blend(array, array2);
				Image val3 = Image.CreateFromData(width, height, false, (Format)5, array2);
				try
				{
					ImageTexture val4 = ImageTexture.CreateFromImage(val3);
					Pages.Add(stage, (val4, width, height, array, array2));
					return (Texture2D)(object)val4;
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			Missing.Add(stage);
			GD.PushWarning("Jeanne run portrait fallback: " + ex.GetBaseException().Message);
			return null;
		}
	}

	public static void Update(Vector4 next)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (weights == next)
		{
			return;
		}
		weights = next;
		foreach (var value in Pages.Values)
		{
			Blend(value.Item4, value.Item5);
			Image val = Image.CreateFromData(value.Item2, value.Item3, false, (Format)5, value.Item5);
			try
			{
				value.Item1.Update(val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private static void Blend(byte[][] colors, byte[] output)
	{
		byte[] array = colors[0];
		byte[] array2 = colors[1];
		byte[] array3 = colors[2];
		byte[] array4 = colors[3];
		for (int i = 0; i < output.Length; i++)
		{
			float num = (float)(int)array[i] * weights.X + (float)(int)array2[i] * weights.Y + (float)(int)array3[i] * weights.Z + (float)(int)array4[i] * weights.W;
			output[i] = (byte)Math.Clamp((int)(num + 0.5f), 0, 255);
		}
	}
}
