using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class LauncherBackground
{
	internal readonly record struct Stamp(long Length, long WriteTicks);

	internal sealed record Result(TextureRect Node, string Path, Stamp Stamp, Stats Stats, float GaussianSigmaPixels, Texture2D BlurredTexture, Texture2D ArtworkTexture)
	{
		internal void ApplyAppearance(UiPalette palette)
		{
			Node.Texture = (palette.IsLiquidGlass ? ArtworkTexture : BlurredTexture);
		}

		internal void Release()
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)Node) && ((Node)Node).GetParent() == null && !((GodotObject)Node).IsQueuedForDeletion())
			{
				((Node)Node).QueueFree();
			}
			((GodotObject)BlurredTexture).Dispose();
			((GodotObject)ArtworkTexture).Dispose();
		}
	}

	internal readonly record struct Stats(float MeanLuminance, float Complexity)
	{
		internal float LegibilityDemand(UiPalette palette)
		{
			return Math.Clamp((palette.IsDark ? MeanLuminance : (1f - MeanLuminance)) * 0.55f + Complexity * 0.45f, 0f, 1f);
		}

		internal Color ReadabilityVeil(UiPalette palette)
		{
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			if (palette.IsLiquidGlass)
			{
				if (!palette.IsDark)
				{
					return new Color(1f, 1f, 1f, 0.12f);
				}
				return new Color(0f, 0f, 0f, 0.22f);
			}
			Color result = (Color)(palette.IsDark ? new Color(0f, 0f, 0f, 1f) : palette.Canvas);
			if (palette.IsDark)
			{
				result.A = Math.Clamp(0.28f + MeanLuminance * 0.28f + Complexity * 0.08f, 0.34f, 0.62f);
			}
			else
			{
				float num = LegibilityDemand(palette);
				result.A = Math.Clamp(0.1f + num * num * 0.4f, 0.12f, 0.5f);
			}
			return result;
		}
	}

	private readonly record struct Analysis(float MeanLuminance, float Complexity, float GaussianSigmaPixels);

	private readonly record struct CacheHeader(Stats Stats, float GaussianSigmaPixels, float BlurSigma, int Width, int Height);

	private sealed class Job
	{
		internal string Path { get; }

		internal Stamp Stamp { get; }

		internal string? CacheFolder { get; }

		internal Action<Result?> Done { get; set; }

		internal CancellationTokenSource Cancellation { get; } = new CancellationTokenSource();

		internal Stopwatch Clock { get; } = Stopwatch.StartNew();

		internal Prepared? Prepared { get; set; }

		internal string? Failure { get; set; }

		internal long WorkerMilliseconds { get; set; }

		internal Job(string path, Stamp stamp, string? cacheFolder, Action<Result?> done)
		{
			Path = path;
			Stamp = stamp;
			CacheFolder = cacheFolder;
			Done = done;
		}
	}

	private sealed class Prepared : IDisposable
	{
		internal Stats Stats { get; }

		internal float GaussianSigmaPixels { get; }

		internal Image Blurred { get; }

		internal Image Artwork { get; }

		internal int Width { get; }

		internal int Height { get; }

		internal bool FromCache { get; }

		internal string CacheNote { get; }

		internal Prepared(Stats stats, float gaussianSigmaPixels, Image blurred, Image artwork, bool fromCache, string? cacheNote)
		{
			Stats = stats;
			GaussianSigmaPixels = gaussianSigmaPixels;
			Blurred = blurred;
			Artwork = artwork;
			Width = blurred.GetWidth();
			Height = blurred.GetHeight();
			FromCache = fromCache;
			CacheNote = cacheNote ?? string.Empty;
		}

		public void Dispose()
		{
			((GodotObject)Blurred).Dispose();
			((GodotObject)Artwork).Dispose();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSearchFolders_003Ed__34 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string root;

		public string _003C_003E3__root;

		string IEnumerator<string>.Current
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
		public _003CSearchFolders_003Ed__34(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = Path.Combine(root, "backgrounds");
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = root;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CSearchFolders_003Ed__34 _003CSearchFolders_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CSearchFolders_003Ed__ = this;
			}
			else
			{
				_003CSearchFolders_003Ed__ = new _003CSearchFolders_003Ed__34(0);
			}
			_003CSearchFolders_003Ed__.root = _003C_003E3__root;
			return _003CSearchFolders_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	internal const string AutoSelection = "auto";

	internal const string NoSelection = "none";

	internal const string ImageFolderName = "backgrounds";

	private const int SampleColumns = 48;

	private const int SampleRows = 27;

	private const int MaximumBlurDimension = 1280;

	private const int GaussianBoxPasses = 3;

	private const int CacheEntries = 10;

	private const string CacheExtension = ".mlmbg";

	private const int CacheVersion = 1;

	private static readonly byte[] CacheMagic = "MLMBG"u8.ToArray();

	private const int SettleRetries = 3;

	private static readonly TimeSpan SettleDelay = TimeSpan.FromMilliseconds(700L, 0L);

	private static readonly string[] PreferredNames = new string[8] { "background.png", "background.jpg", "background.jpeg", "background.webp", "launcher_background.png", "launcher_background.jpg", "launcher_background.jpeg", "launcher_background.webp" };

	private static readonly HashSet<string> SupportedExtensions = new HashSet<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[4] { ".png", ".jpg", ".jpeg", ".webp" }), StringComparer.OrdinalIgnoreCase);

	private static FileSystemWatcher? _watcher;

	private static SceneTree? _tree;

	private static Action? _onChanged;

	private static long _lastChangeUtcTicks;

	private static int _dirty;

	private static int _retriesLeft;

	private static readonly object JobGate = new object();

	private static Job? _current;

	private static Job? _queued;

	private static Job? _finished;

	private static bool _workerRunning;

	private static SceneTree? _deliveryTree;

	internal static string? ImageFolder
	{
		get
		{
			string text = ResolveModRoot();
			if (text == null)
			{
				return null;
			}
			return Path.Combine(text, "backgrounds");
		}
	}

	internal static bool Loading => _current != null;

	internal static string? EnsureImageFolder(out string? error)
	{
		error = null;
		string imageFolder = ImageFolder;
		if (imageFolder == null)
		{
			return null;
		}
		try
		{
			Directory.CreateDirectory(imageFolder);
			return imageFolder;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			GD.PushWarning($"{"[ModLaunchManager]"} could not create {imageFolder}: {ex.Message}");
			return null;
		}
	}

	internal static IReadOnlyList<string> AvailableImages()
	{
		string text = ResolveModRoot();
		if (text == null)
		{
			return Array.Empty<string>();
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in SearchFolders(text))
		{
			foreach (string item2 in ImageNamesIn(item))
			{
				hashSet.Add(item2);
			}
		}
		return hashSet.OrderBy<string, string>((string name) => name, StringComparer.OrdinalIgnoreCase).ToArray();
	}

	[IteratorStateMachine(typeof(_003CSearchFolders_003Ed__34))]
	private static IEnumerable<string> SearchFolders(string root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSearchFolders_003Ed__34(-2)
		{
			_003C_003E3__root = root
		};
	}

	private static IEnumerable<string> ImageNamesIn(string folder)
	{
		try
		{
			return (from path in Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
				where SupportedExtensions.Contains(Path.GetExtension(path))
				select path).Select(Path.GetFileName).OfType<string>().ToArray();
		}
		catch (DirectoryNotFoundException)
		{
			return Array.Empty<string>();
		}
		catch (Exception ex2)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not scan {folder}: {ex2.Message}");
			return Array.Empty<string>();
		}
	}

	private static string? ResolveImagePath(string root, string name)
	{
		foreach (string item in SearchFolders(root))
		{
			string text = Path.Combine(item, name);
			if (File.Exists(text))
			{
				return text;
			}
		}
		return null;
	}

	internal static string ActiveSelection()
	{
		string backgroundPreference = MlmLocManager.BackgroundPreference;
		if (string.Equals(backgroundPreference, "none", StringComparison.OrdinalIgnoreCase))
		{
			return "none";
		}
		string text = SelectedFileName(backgroundPreference);
		if (text.Length <= 0 || !AvailableImages().Contains<string>(text, StringComparer.OrdinalIgnoreCase))
		{
			return "auto";
		}
		return text;
	}

	internal static string? SelectedPath()
	{
		string text = ResolveModRoot();
		if (text == null)
		{
			return null;
		}
		string backgroundPreference = MlmLocManager.BackgroundPreference;
		if (string.Equals(backgroundPreference, "none", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		string text2 = SelectedFileName(backgroundPreference);
		if (text2.Length > 0)
		{
			string text3 = ResolveImagePath(text, text2);
			if (text3 != null)
			{
				return text3;
			}
		}
		return FindImage(text);
	}

	internal static bool NeedsReload(Result? applied)
	{
		string text = SelectedPath();
		Job current = _current;
		if (current != null)
		{
			if (text != null)
			{
				return !IsSameFile(current.Path, current.Stamp, text);
			}
			return true;
		}
		if (text == null)
		{
			return applied != null;
		}
		if (!(applied == null))
		{
			return !IsSameFile(applied.Path, applied.Stamp, text);
		}
		return true;
	}

	internal static bool Shows(Result? applied, string path)
	{
		if (applied != null)
		{
			return IsSameFile(applied.Path, applied.Stamp, path);
		}
		return false;
	}

	private static bool IsSameFile(string shownPath, Stamp shownStamp, string path)
	{
		if (string.Equals(shownPath, path, StringComparison.OrdinalIgnoreCase))
		{
			return shownStamp == StampOf(path);
		}
		return false;
	}

	internal static void Watch(SceneTree tree, Action onChanged)
	{
		StopWatching();
		string text = ResolveModRoot();
		if (text != null && Directory.Exists(text))
		{
			try
			{
				_watcher = new FileSystemWatcher(text)
				{
					IncludeSubdirectories = true,
					Filter = "*",
					NotifyFilter = (NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite),
					EnableRaisingEvents = true
				};
			}
			catch (Exception ex)
			{
				_watcher = null;
				GD.PushWarning("[ModLaunchManager] could not watch launcher backgrounds: " + ex.Message);
				return;
			}
			_watcher.Created += OnFileChanged;
			_watcher.Changed += OnFileChanged;
			_watcher.Deleted += OnFileChanged;
			_watcher.Renamed += OnFileRenamed;
			_watcher.Error += OnWatcherError;
			_tree = tree;
			_onChanged = onChanged;
			Volatile.Write(ref _dirty, 0);
			tree.ProcessFrame += Poll;
			GD.Print("[ModLaunchManager] watching launcher backgrounds in " + text + ".");
		}
	}

	internal static void StopWatching()
	{
		if (_tree != null)
		{
			_tree.ProcessFrame -= Poll;
		}
		_tree = null;
		if (_watcher != null)
		{
			_watcher.EnableRaisingEvents = false;
			_watcher.Created -= OnFileChanged;
			_watcher.Changed -= OnFileChanged;
			_watcher.Deleted -= OnFileChanged;
			_watcher.Renamed -= OnFileRenamed;
			_watcher.Error -= OnWatcherError;
			_watcher.Dispose();
			_watcher = null;
		}
		_onChanged = null;
		Volatile.Write(ref _dirty, 0);
	}

	private static void OnFileChanged(object sender, FileSystemEventArgs args)
	{
		if (IsRelevantChange(args.Name))
		{
			MarkDirty();
		}
	}

	private static void OnFileRenamed(object sender, RenamedEventArgs args)
	{
		if (IsRelevantChange(args.Name) || IsRelevantChange(args.OldName))
		{
			MarkDirty();
		}
	}

	private static void OnWatcherError(object sender, ErrorEventArgs args)
	{
		MarkDirty();
	}

	private static bool IsRelevantChange(string? name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			if (!SupportedExtensions.Contains(Path.GetExtension(name)))
			{
				return name.StartsWith("backgrounds", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}

	private static void MarkDirty()
	{
		Volatile.Write(ref _retriesLeft, 3);
		Schedule();
	}

	internal static void RetryLater()
	{
		int num = Volatile.Read(in _retriesLeft);
		if (_watcher != null && num > 0)
		{
			Volatile.Write(ref _retriesLeft, num - 1);
			Schedule();
		}
	}

	private static void Schedule()
	{
		Volatile.Write(ref _lastChangeUtcTicks, DateTime.UtcNow.Ticks);
		Volatile.Write(ref _dirty, 1);
	}

	private static void Poll()
	{
		if (Volatile.Read(in _dirty) != 0 && DateTime.UtcNow.Ticks - Volatile.Read(in _lastChangeUtcTicks) >= SettleDelay.Ticks)
		{
			Volatile.Write(ref _dirty, 0);
			_onChanged?.Invoke();
		}
	}

	private static Stamp StampOf(string path)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			return new Stamp(fileInfo.Length, fileInfo.LastWriteTimeUtc.Ticks);
		}
		catch
		{
			return default(Stamp);
		}
	}

	private static string SelectedFileName(string selection)
	{
		if (string.IsNullOrWhiteSpace(selection) || string.Equals(selection, "auto", StringComparison.OrdinalIgnoreCase) || string.Equals(selection, "none", StringComparison.OrdinalIgnoreCase))
		{
			return string.Empty;
		}
		string text = selection.Trim();
		string fileName = Path.GetFileName(text);
		if (!string.Equals(fileName, text, StringComparison.Ordinal) || !SupportedExtensions.Contains(Path.GetExtension(fileName)))
		{
			return string.Empty;
		}
		return fileName;
	}

	internal static void Load(string path, Action<Result?> done)
	{
		Stamp stamp = StampOf(path);
		Job current = _current;
		if (current != null && current.Stamp == stamp && string.Equals(current.Path, path, StringComparison.OrdinalIgnoreCase))
		{
			current.Done = done;
			return;
		}
		CancelLoad();
		Job queued = (_current = new Job(path, stamp, CacheFolder(), done));
		lock (JobGate)
		{
			_queued = queued;
			if (!_workerRunning)
			{
				_workerRunning = true;
				Thread thread = new Thread(Work)
				{
					Name = "ModLaunchManager background",
					IsBackground = true
				};
				try
				{
					thread.Priority = ThreadPriority.BelowNormal;
				}
				catch
				{
				}
				thread.Start();
			}
		}
		if (_deliveryTree == null)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				val.ProcessFrame += Deliver;
				_deliveryTree = val;
			}
		}
	}

	internal static void CancelLoad()
	{
		Job current = _current;
		_current = null;
		StopDelivery();
		if (current == null)
		{
			return;
		}
		current.Cancellation.Cancel();
		lock (JobGate)
		{
			if (_queued == current)
			{
				_queued = null;
			}
			_finished?.Prepared?.Dispose();
			_finished = null;
		}
	}

	internal static Stats? CachedStats(string path)
	{
		Stamp stamp = StampOf(path);
		if (!(stamp == default(Stamp)))
		{
			string text = CacheFolder();
			if (text != null)
			{
				try
				{
					string path2 = CacheFile(text, path);
					if (!File.Exists(path2))
					{
						return null;
					}
					using FileStream input = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read);
					using BinaryReader reader = new BinaryReader(input);
					return ReadCacheHeader(reader, path, stamp)?.Stats;
				}
				catch
				{
					return null;
				}
			}
		}
		return null;
	}

	private static void StopDelivery()
	{
		if (_deliveryTree != null && GodotObject.IsInstanceValid((GodotObject)(object)_deliveryTree))
		{
			_deliveryTree.ProcessFrame -= Deliver;
		}
		_deliveryTree = null;
	}

	private static void Deliver()
	{
		Job finished;
		lock (JobGate)
		{
			finished = _finished;
			_finished = null;
		}
		if (finished == null)
		{
			return;
		}
		if (finished != _current)
		{
			finished.Prepared?.Dispose();
			return;
		}
		_current = null;
		StopDelivery();
		Stopwatch stopwatch = Stopwatch.StartNew();
		Result result = null;
		Prepared prepared = finished.Prepared;
		if (prepared != null)
		{
			try
			{
				result = Present(finished, prepared);
			}
			catch (Exception ex)
			{
				GD.PushWarning($"{"[ModLaunchManager]"} launcher background failed: {ex.GetType().Name} {ex.Message}");
			}
			finally
			{
				prepared.Dispose();
			}
		}
		else
		{
			GD.PushWarning("[ModLaunchManager] " + (finished.Failure ?? "launcher background failed."));
		}
		long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
		try
		{
			finished.Done(result);
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} launcher background could not be shown: {value}");
		}
		if (result != null)
		{
			Prepared prepared2 = finished.Prepared;
			if (prepared2 != null)
			{
				GD.Print($"{"[ModLaunchManager]"} custom launcher background: {finished.Path}; luminance={result.Stats.MeanLuminance:F3}, complexity={result.Stats.Complexity:F3}, gaussianSigma={result.GaussianSigmaPixels:F1}px, raster={prepared2.Width}x{prepared2.Height}, source={(prepared2.FromCache ? "cache" : "file")}, workerMs={finished.WorkerMilliseconds}, uploadMs={elapsedMilliseconds}, gameThreadMs={stopwatch.ElapsedMilliseconds}, shownAfterMs={finished.Clock.ElapsedMilliseconds}{prepared2.CacheNote}.");
			}
		}
	}

	private static Result Present(Job job, Prepared prepared)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		ImageTexture val = ImageTexture.CreateFromImage(prepared.Blurred);
		ImageTexture artworkTexture = ImageTexture.CreateFromImage(prepared.Artwork);
		Result result = new Result(new TextureRect
		{
			Name = StringName.op_Implicit("CustomLauncherBackground"),
			Texture = (Texture2D)(object)val,
			ExpandMode = (ExpandModeEnum)1,
			StretchMode = (StretchModeEnum)6,
			MouseFilter = (MouseFilterEnum)2
		}, job.Path, job.Stamp, prepared.Stats, prepared.GaussianSigmaPixels, (Texture2D)(object)val, (Texture2D)(object)artworkTexture);
		result.ApplyAppearance(UiKit.Palette);
		return result;
	}

	private static void Work()
	{
		while (true)
		{
			Job queued;
			lock (JobGate)
			{
				if (_queued == null)
				{
					_workerRunning = false;
					break;
				}
				queued = _queued;
				_queued = null;
			}
			CancellationToken token = queued.Cancellation.Token;
			try
			{
				if (!token.IsCancellationRequested)
				{
					queued.Prepared = Prepare(queued, token);
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex2)
			{
				queued.Failure = ((ex2 is InvalidDataException) ? ("launcher background could not be loaded: " + ex2.Message + ".") : ("launcher background failed: " + ex2.GetType().Name + " " + ex2.Message));
			}
			lock (JobGate)
			{
				if (token.IsCancellationRequested)
				{
					queued.Prepared?.Dispose();
					queued.Prepared = null;
				}
				else
				{
					queued.WorkerMilliseconds = queued.Clock.ElapsedMilliseconds;
					_finished = queued;
				}
			}
		}
	}

	private static Prepared Prepare(Job job, CancellationToken token)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		string note = null;
		bool flag = job.CacheFolder != null && job.Stamp != default(Stamp);
		if (flag)
		{
			Prepared prepared = ReadCache(job, token, out note);
			if (prepared != null)
			{
				return prepared;
			}
		}
		Image val = new Image();
		try
		{
			Error val2 = Decode(val, job.Path);
			if ((int)val2 != 0 || val.IsEmpty())
			{
				throw new InvalidDataException($"{job.Path} ({val2})");
			}
			token.ThrowIfCancellationRequested();
			Analysis analysis = Analyze(val);
			float num = Math.Max(0.8f, analysis.GaussianSigmaPixels * Downscale(val));
			if (flag)
			{
				note = WriteCache(job, analysis, num, val) ?? note;
			}
			token.ThrowIfCancellationRequested();
			Image val3 = Blur(val, num);
			val3.GenerateMipmaps(false);
			return new Prepared(new Stats(analysis.MeanLuminance, analysis.Complexity), analysis.GaussianSigmaPixels, val3, val, fromCache: false, note);
		}
		catch
		{
			((GodotObject)val).Dispose();
			throw;
		}
	}

	private static Error Decode(Image image, string path)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		byte[] array;
		try
		{
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			array = new byte[fileStream.Length];
			fileStream.ReadExactly(array);
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			return (Error)12;
		}
		switch (Path.GetExtension(path).ToLowerInvariant())
		{
		case ".png":
			return image.LoadPngFromBuffer(array);
		case ".jpg":
		case ".jpeg":
			return image.LoadJpgFromBuffer(array);
		case ".webp":
			return image.LoadWebpFromBuffer(array);
		default:
			return image.Load(path);
		}
	}

	private static string? CacheFolder()
	{
		try
		{
			string text = ProjectSettings.GlobalizePath("user://");
			return string.IsNullOrWhiteSpace(text) ? null : Path.Combine(Path.GetFullPath(text), "ModLaunchManager", "cache", "backgrounds");
		}
		catch
		{
			return null;
		}
	}

	private static string CacheFile(string folder, string path)
	{
		byte[] inArray = SHA256.HashData(Encoding.UTF8.GetBytes(Path.GetFullPath(path).ToUpperInvariant()));
		return Path.Combine(folder, Convert.ToHexString(inArray, 0, 16) + ".mlmbg");
	}

	private static string? WriteCache(Job job, Analysis analysis, float blurSigma, Image raster)
	{
		string cacheFolder = job.CacheFolder;
		string text = CacheFile(cacheFolder, job.Path);
		string text2 = $"{text}.{Guid.NewGuid():N}.tmp";
		try
		{
			Directory.CreateDirectory(cacheFolder);
			byte[] data = raster.GetData();
			using (FileStream fileStream = new FileStream(text2, FileMode.CreateNew, FileAccess.Write, FileShare.None))
			{
				using BinaryWriter binaryWriter = new BinaryWriter(fileStream);
				binaryWriter.Write(CacheMagic);
				binaryWriter.Write(1);
				binaryWriter.Write(job.Path);
				binaryWriter.Write(job.Stamp.Length);
				binaryWriter.Write(job.Stamp.WriteTicks);
				binaryWriter.Write(analysis.MeanLuminance);
				binaryWriter.Write(analysis.Complexity);
				binaryWriter.Write(analysis.GaussianSigmaPixels);
				binaryWriter.Write(blurSigma);
				binaryWriter.Write(raster.GetWidth());
				binaryWriter.Write(raster.GetHeight());
				binaryWriter.Write(data.Length);
				binaryWriter.Flush();
				using ZLibStream zLibStream = new ZLibStream(fileStream, CompressionLevel.Fastest, leaveOpen: true);
				zLibStream.Write(data);
			}
			File.Move(text2, text, overwrite: true);
			PruneCache(cacheFolder, text);
			return null;
		}
		catch (Exception ex)
		{
			try
			{
				File.Delete(text2);
			}
			catch
			{
			}
			return $", cache not written ({ex.GetType().Name} {ex.Message})";
		}
	}

	private static Prepared? ReadCache(Job job, CancellationToken token, out string? note)
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		note = null;
		string path = CacheFile(job.CacheFolder, job.Path);
		if (!File.Exists(path))
		{
			return null;
		}
		Image val = null;
		try
		{
			CacheHeader cacheHeader2;
			byte[] array;
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using BinaryReader reader = new BinaryReader(fileStream);
				CacheHeader? cacheHeader = ReadCacheHeader(reader, job.Path, job.Stamp);
				if (!cacheHeader.HasValue)
				{
					return null;
				}
				CacheHeader valueOrDefault = cacheHeader.GetValueOrDefault();
				cacheHeader2 = valueOrDefault;
				array = new byte[cacheHeader2.Width * cacheHeader2.Height * 4];
				using ZLibStream zLibStream = new ZLibStream(fileStream, CompressionMode.Decompress, leaveOpen: true);
				zLibStream.ReadExactly(array);
			}
			token.ThrowIfCancellationRequested();
			val = Image.CreateFromData(cacheHeader2.Width, cacheHeader2.Height, false, (Format)5, array);
			Image val2 = Blur(val, cacheHeader2.BlurSigma);
			val2.GenerateMipmaps(false);
			try
			{
				File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
			}
			catch
			{
			}
			return new Prepared(cacheHeader2.Stats, cacheHeader2.GaussianSigmaPixels, val2, val, fromCache: true, null);
		}
		catch (OperationCanceledException)
		{
			if (val != null)
			{
				((GodotObject)val).Dispose();
			}
			throw;
		}
		catch (Exception ex2)
		{
			if (val != null)
			{
				((GodotObject)val).Dispose();
			}
			note = ", cache entry unreadable (" + ex2.GetType().Name + ")";
			try
			{
				File.Delete(path);
			}
			catch
			{
			}
			return null;
		}
	}

	private static CacheHeader? ReadCacheHeader(BinaryReader reader, string path, Stamp stamp)
	{
		if (!reader.ReadBytes(CacheMagic.Length).AsSpan().SequenceEqual(CacheMagic) || reader.ReadInt32() != 1 || !string.Equals(reader.ReadString(), path, StringComparison.OrdinalIgnoreCase) || reader.ReadInt64() != stamp.Length || reader.ReadInt64() != stamp.WriteTicks)
		{
			return null;
		}
		Stats stats = new Stats(reader.ReadSingle(), reader.ReadSingle());
		float num = reader.ReadSingle();
		float num2 = reader.ReadSingle();
		int num3 = reader.ReadInt32();
		int num4 = reader.ReadInt32();
		int num5 = reader.ReadInt32();
		if (num3 <= 0 || num3 > 1280 || num4 <= 0 || num4 > 1280 || num5 != num3 * num4 * 4 || !float.IsFinite(stats.MeanLuminance) || !float.IsFinite(stats.Complexity) || !float.IsFinite(num) || !float.IsFinite(num2) || !(num2 > 0f))
		{
			return null;
		}
		return new CacheHeader(stats, num, num2, num3, num4);
	}

	private static void PruneCache(string folder, string keep)
	{
		try
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(folder);
			foreach (FileInfo item in (from entry in directoryInfo.GetFiles("*.mlmbg")
				where !string.Equals(entry.FullName, keep, StringComparison.OrdinalIgnoreCase)
				orderby entry.LastWriteTimeUtc descending
				select entry).Skip(9))
			{
				item.Delete();
			}
			foreach (FileInfo item2 in from entry in directoryInfo.GetFiles("*.tmp")
				where entry.LastWriteTimeUtc < DateTime.UtcNow.AddMinutes(-10.0)
				select entry)
			{
				item2.Delete();
			}
		}
		catch
		{
		}
	}

	internal static string? ResolveModRoot()
	{
		string location = typeof(ModLaunchManagerMod).Assembly.Location;
		if (!string.IsNullOrWhiteSpace(location))
		{
			string directoryName = Path.GetDirectoryName(location);
			if (directoryName != null && Directory.Exists(directoryName))
			{
				return directoryName;
			}
		}
		return ((IEnumerable<Mod>)ModManager.Mods).FirstOrDefault((Func<Mod, bool>)((Mod mod) => string.Equals(mod.manifest?.id, "ModLaunchManager", StringComparison.Ordinal)))?.path;
	}

	private static string? FindImage(string root)
	{
		string[] preferredNames = PreferredNames;
		foreach (string name in preferredNames)
		{
			string text = ResolveImagePath(root, name);
			if (text != null)
			{
				return text;
			}
		}
		IReadOnlyList<string> readOnlyList = AvailableImages();
		if (readOnlyList.Count <= 0)
		{
			return null;
		}
		return ResolveImagePath(root, readOnlyList[0]);
	}

	private static Analysis Analyze(Image image)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		int width = image.GetWidth();
		int height = image.GetHeight();
		int num = Math.Min(48, width);
		int num2 = Math.Min(27, height);
		float[] array = new float[num * num2];
		float num3 = 0f;
		for (int i = 0; i < num2; i++)
		{
			int num4 = ((num2 != 1) ? (i * (height - 1) / (num2 - 1)) : 0);
			for (int j = 0; j < num; j++)
			{
				int num5 = ((num != 1) ? (j * (width - 1) / (num - 1)) : 0);
				Color pixel = image.GetPixel(num5, num4);
				num3 += (array[i * num + j] = 0.2126f * pixel.R + 0.7152f * pixel.G + 0.0722f * pixel.B);
			}
		}
		float num6 = num3 / (float)array.Length;
		float num7 = 0f;
		float num8 = 0f;
		int num9 = 0;
		for (int k = 0; k < num2; k++)
		{
			for (int l = 0; l < num; l++)
			{
				int num10 = k * num + l;
				float num11 = array[num10] - num6;
				num7 += num11 * num11;
				if (l + 1 < num)
				{
					num8 += Math.Abs(array[num10] - array[num10 + 1]);
					num9++;
				}
				if (k + 1 < num2)
				{
					num8 += Math.Abs(array[num10] - array[num10 + num]);
					num9++;
				}
			}
		}
		float num12 = MathF.Sqrt(num7 / (float)array.Length);
		float num13 = ((num9 == 0) ? 0f : (num8 / (float)num9));
		float num14 = Math.Clamp(num12 * 1.45f + num13 * 3.2f, 0f, 1f);
		float value = (float)Math.Min(width, height) * (0.0065f + num14 * 0.0145f);
		value = Math.Clamp(value, 4f, 32f);
		return new Analysis(num6, num14, value);
	}

	private static float Downscale(Image source)
	{
		int width = source.GetWidth();
		int height = source.GetHeight();
		float num = Math.Min(1f, 1280f / (float)Math.Max(width, height));
		int num2 = Math.Max(1, (int)MathF.Round((float)width * num));
		int num3 = Math.Max(1, (int)MathF.Round((float)height * num));
		if (num2 != width || num3 != height)
		{
			source.Resize(num2, num3, (Interpolation)4);
		}
		source.Convert((Format)5);
		return num;
	}

	private static Image Blur(Image raster, float sigma)
	{
		int width = raster.GetWidth();
		int height = raster.GetHeight();
		int[] array = GaussianBoxSizes(sigma, 3);
		byte[] array2 = raster.GetData();
		byte[] array3 = new byte[array2.Length];
		byte[] array4 = new byte[array2.Length];
		int[] array5 = array;
		for (int i = 0; i < array5.Length; i++)
		{
			int radius = (array5[i] - 1) / 2;
			BoxBlurHorizontal(array2, array3, width, height, radius);
			BoxBlurVertical(array3, array4, width, height, radius);
			byte[] array6 = array4;
			array4 = array2;
			array2 = array6;
		}
		return Image.CreateFromData(width, height, false, (Format)5, array2);
	}

	private static int[] GaussianBoxSizes(float sigma, int passes)
	{
		int num = (int)MathF.Floor(MathF.Sqrt(12f * sigma * sigma / (float)passes + 1f));
		if (num % 2 == 0)
		{
			num--;
		}
		num = Math.Max(1, num);
		int num2 = num + 2;
		int num3 = Math.Clamp((int)MathF.Round((12f * sigma * sigma - (float)(passes * num * num) - 4f * (float)passes * (float)num - 3f * (float)passes) / (-4f * (float)num - 4f)), 0, passes);
		int[] array = new int[passes];
		for (int i = 0; i < passes; i++)
		{
			array[i] = ((i < num3) ? num : num2);
		}
		return array;
	}

	private static void BoxBlurHorizontal(byte[] source, byte[] destination, int width, int height, int radius)
	{
		int diameter = radius * 2 + 1;
		Parallel.For(0, height, delegate(int y)
		{
			int num = y * width * 4;
			for (int i = 0; i < 4; i++)
			{
				int num2 = source[num + i];
				int num3 = source[num + (width - 1) * 4 + i];
				int num4 = (radius + 1) * num2;
				for (int j = 1; j <= radius; j++)
				{
					num4 += source[num + Math.Min(j, width - 1) * 4 + i];
				}
				for (int k = 0; k < width; k++)
				{
					destination[num + k * 4 + i] = (byte)((num4 + diameter / 2) / diameter);
					int num5 = ((k - radius < 0) ? num2 : source[num + (k - radius) * 4 + i]);
					int num6 = ((k + radius + 1 >= width) ? num3 : source[num + (k + radius + 1) * 4 + i]);
					num4 += num6 - num5;
				}
			}
		});
	}

	private static void BoxBlurVertical(byte[] source, byte[] destination, int width, int height, int radius)
	{
		int diameter = radius * 2 + 1;
		int rowStride = width * 4;
		Parallel.For(0, width, delegate(int x)
		{
			int num = x * 4;
			for (int i = 0; i < 4; i++)
			{
				int num2 = source[num + i];
				int num3 = source[(height - 1) * rowStride + num + i];
				int num4 = (radius + 1) * num2;
				for (int j = 1; j <= radius; j++)
				{
					num4 += source[Math.Min(j, height - 1) * rowStride + num + i];
				}
				for (int k = 0; k < height; k++)
				{
					int num5 = k * rowStride + num + i;
					destination[num5] = (byte)((num4 + diameter / 2) / diameter);
					int num6 = ((k - radius < 0) ? num2 : source[(k - radius) * rowStride + num + i]);
					int num7 = ((k + radius + 1 >= height) ? num3 : source[(k + radius + 1) * rowStride + num + i]);
					num4 += num7 - num6;
				}
			}
		});
	}
}
