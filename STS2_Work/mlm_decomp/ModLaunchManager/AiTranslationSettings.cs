using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class AiTranslationSettings
{
	[SupportedOSPlatform("windows")]
	private static class WindowsAccount
	{
		private struct DataBlob
		{
			public int Size;

			public nint Data;
		}

		private const int UiForbidden = 1;

		[DllImport("crypt32.dll", SetLastError = true)]
		private static extern bool CryptProtectData(ref DataBlob input, nint description, ref DataBlob entropy, nint reserved, nint prompt, int flags, out DataBlob output);

		[DllImport("crypt32.dll", SetLastError = true)]
		private static extern bool CryptUnprotectData(ref DataBlob input, nint description, ref DataBlob entropy, nint reserved, nint prompt, int flags, out DataBlob output);

		[DllImport("kernel32.dll")]
		private static extern nint LocalFree(nint memory);

		internal static byte[] Protect(byte[] plain, byte[] binding)
		{
			return Transform(plain, binding, protect: true);
		}

		internal static byte[] Unprotect(byte[] protectedKey, byte[] binding)
		{
			return Transform(protectedKey, binding, protect: false);
		}

		private static byte[] Transform(byte[] input, byte[] binding, bool protect)
		{
			GCHandle gCHandle = GCHandle.Alloc(input, GCHandleType.Pinned);
			GCHandle gCHandle2 = GCHandle.Alloc(binding, GCHandleType.Pinned);
			DataBlob output = default(DataBlob);
			try
			{
				DataBlob dataBlob = default(DataBlob);
				dataBlob.Size = input.Length;
				dataBlob.Data = gCHandle.AddrOfPinnedObject();
				DataBlob input2 = dataBlob;
				dataBlob = default(DataBlob);
				dataBlob.Size = binding.Length;
				dataBlob.Data = gCHandle2.AddrOfPinnedObject();
				DataBlob entropy = dataBlob;
				if (!(protect ? CryptProtectData(ref input2, IntPtr.Zero, ref entropy, IntPtr.Zero, IntPtr.Zero, 1, out output) : CryptUnprotectData(ref input2, IntPtr.Zero, ref entropy, IntPtr.Zero, IntPtr.Zero, 1, out output)) || output.Data == IntPtr.Zero || output.Size <= 0)
				{
					throw new CryptographicException();
				}
				byte[] array = new byte[output.Size];
				Marshal.Copy(output.Data, array, 0, output.Size);
				return array;
			}
			finally
			{
				if (output.Data != IntPtr.Zero)
				{
					if (output.Size > 0)
					{
						Marshal.Copy(new byte[output.Size], 0, output.Data, output.Size);
					}
					LocalFree(output.Data);
				}
				gCHandle.Free();
				gCHandle2.Free();
			}
		}
	}

	private const byte WindowsAccountFormat = 2;

	private const byte UserFileFormat = 3;

	private const int MaxKeyLength = 4096;

	public string? Provider { get; set; }

	public string Endpoint { get; set; } = "https://api.openai.com/v1";

	public string Model { get; set; } = string.Empty;

	public string Notes { get; set; } = string.Empty;

	public string? StoredKey { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? EncryptedKey { get; set; }

	internal AiProviderPreset Preset => AiProviderPreset.ForId(Provider) ?? AiProviderPreset.For(AiProviderPreset.Infer(Endpoint));

	internal bool HasKey => !string.IsNullOrEmpty(StoredKey);

	internal bool NeedsKeyAgain
	{
		get
		{
			if (!HasKey)
			{
				return !string.IsNullOrEmpty(EncryptedKey);
			}
			return false;
		}
	}

	internal static string StoragePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ModLaunchManager", "private", "ai-translation.json");

	private byte[] Binding => Encoding.UTF8.GetBytes(CompletionEndpoint().AbsoluteUri + "\n" + Model);

	internal static AiTranslationSettings Load()
	{
		return Load(StoragePath);
	}

	internal static AiTranslationSettings LoadForLocale(string locale)
	{
		if (!File.Exists(StoragePath))
		{
			return ForNewPlayer(locale);
		}
		return Load();
	}

	internal static AiTranslationSettings ForNewPlayer(string locale)
	{
		AiProviderPreset aiProviderPreset = AiProviderPreset.For(AiProviderPreset.DefaultFor(locale));
		return new AiTranslationSettings
		{
			Provider = aiProviderPreset.Id,
			Endpoint = aiProviderPreset.Endpoint,
			Model = aiProviderPreset.DefaultModel
		};
	}

	internal static AiTranslationSettings Load(string path)
	{
		if (!File.Exists(path))
		{
			return new AiTranslationSettings();
		}
		try
		{
			return JsonSerializer.Deserialize<AiTranslationSettings>(PckLocalizationArchive.Utf8.GetString(ModTranslationFiles.ReadFile(path)).TrimStart('\ufeff')) ?? new AiTranslationSettings();
		}
		catch
		{
			return new AiTranslationSettings();
		}
	}

	internal void Save()
	{
		Save(StoragePath);
	}

	internal void Save(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!OperatingSystem.IsWindows())
		{
			Directory.CreateDirectory(directoryName, UnixFileMode.UserExecute | UnixFileMode.UserWrite | UnixFileMode.UserRead);
			File.SetUnixFileMode(directoryName, UnixFileMode.UserExecute | UnixFileMode.UserWrite | UnixFileMode.UserRead);
		}
		ModTranslationFiles.WriteAtomic(path, JsonSerializer.SerializeToUtf8Bytes(this));
		if (!OperatingSystem.IsWindows())
		{
			File.SetUnixFileMode(path, UnixFileMode.UserWrite | UnixFileMode.UserRead);
		}
	}

	internal Uri CompletionEndpoint()
	{
		if (!Uri.TryCreate((Preset.IsPreset ? Preset.Endpoint : Endpoint).Trim(), UriKind.Absolute, out Uri result) || result.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(result.UserInfo) || !string.IsNullOrEmpty(result.Query) || !string.IsNullOrEmpty(result.Fragment))
		{
			throw new TranslationFileException("endpoint");
		}
		string text = result.AbsoluteUri.TrimEnd('/');
		return new Uri(text.EndsWith("/chat/completions", StringComparison.Ordinal) ? text : (text + "/chat/completions"));
	}

	internal void Validate()
	{
		CompletionEndpoint();
		if (string.IsNullOrWhiteSpace(Model) || Model.Length > 200 || Model.Any(char.IsControl) || Notes.Length > 4000)
		{
			throw new TranslationFileException("settings");
		}
	}

	internal void StoreKey(string key)
	{
		Validate();
		key = key.Trim();
		if (key.Length == 0 || key.Length > 4096 || key.Any(char.IsControl))
		{
			throw new TranslationFileException("key");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(key);
		byte[] array = null;
		try
		{
			byte[] array4;
			if (!OperatingSystem.IsWindows())
			{
				byte b = 3;
				byte[] array2 = bytes;
				int num = 0;
				byte[] array3 = new byte[1 + array2.Length];
				array3[num] = b;
				num++;
				ReadOnlySpan<byte> readOnlySpan = new ReadOnlySpan<byte>(array2);
				readOnlySpan.CopyTo(new Span<byte>(array3).Slice(num, readOnlySpan.Length));
				num += readOnlySpan.Length;
				array4 = array3;
			}
			else
			{
				byte b = 2;
				byte[] array3 = WindowsAccount.Protect(bytes, Binding);
				int num = 0;
				byte[] array2 = new byte[1 + array3.Length];
				array2[num] = b;
				num++;
				ReadOnlySpan<byte> readOnlySpan = new ReadOnlySpan<byte>(array3);
				readOnlySpan.CopyTo(new Span<byte>(array2).Slice(num, readOnlySpan.Length));
				num += readOnlySpan.Length;
				array4 = array2;
			}
			array = array4;
			StoredKey = Convert.ToBase64String(array);
			EncryptedKey = null;
		}
		finally
		{
			CryptographicOperations.ZeroMemory(bytes);
			if (array != null)
			{
				CryptographicOperations.ZeroMemory(array);
			}
		}
	}

	internal byte[] ReadKey()
	{
		byte[] array = null;
		byte[] array2 = null;
		try
		{
			array = Convert.FromBase64String(StoredKey ?? "");
			if (array.Length < 2)
			{
				throw new CryptographicException();
			}
			byte b = array[0];
			byte[] array3;
			if (b != 2)
			{
				if (b != 3)
				{
					goto IL_006d;
				}
				array3 = array[1..];
			}
			else
			{
				if (!OperatingSystem.IsWindows())
				{
					goto IL_006d;
				}
				array3 = WindowsAccount.Unprotect(array[1..], Binding);
			}
			array2 = array3;
			if (array2.Length == 0 || array2.Length > 16384)
			{
				throw new CryptographicException();
			}
			return array2;
			IL_006d:
			throw new CryptographicException();
		}
		catch
		{
			if (array2 != null)
			{
				CryptographicOperations.ZeroMemory(array2);
			}
			throw new TranslationFileException("savedKey");
		}
		finally
		{
			if (array != null)
			{
				CryptographicOperations.ZeroMemory(array);
			}
		}
	}
}
