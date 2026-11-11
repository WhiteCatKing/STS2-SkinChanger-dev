using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STS2SkinChanger.Core;

internal sealed class WorkshopCarousel<T>(Func<string, CancellationToken, Task<T?>> load, Action<T?, int, int> present, Func<double> clock) where T : class
{
	private const double Interval = 2.0;

	private string[] _urls = Array.Empty<string>();

	private CancellationTokenSource? _lifetime;

	private Task<T?>? _pending;

	private int _nextIndex;

	private int _generation;

	private bool _presented;

	private double _due;

	public void Start(string[] urls, CancellationToken token)
	{
		Clear();
		if (urls.Length != 0 && !token.IsCancellationRequested)
		{
			_urls = urls.ToArray();
			_lifetime = CancellationTokenSource.CreateLinkedTokenSource(token);
			_pending = LoadSafe(_urls[0], _lifetime.Token);
		}
	}

	public void Tick(bool autoAdvance = true)
	{
		if (_lifetime == null || _lifetime.IsCancellationRequested)
		{
			return;
		}
		Task<T> pending = _pending;
		if (pending == null || !pending.IsCompleted || (_presented && (!autoAdvance || _003Cclock_003EP() < _due)))
		{
			return;
		}
		int generation = _generation;
		T result = pending.GetAwaiter().GetResult();
		if (result != null || !_presented)
		{
			_003Cpresent_003EP(result, _nextIndex, _urls.Length);
		}
		if (_generation == generation && _lifetime != null && !_lifetime.IsCancellationRequested)
		{
			_presented = true;
			_due = _003Cclock_003EP() + 2.0;
			_pending = null;
			if (_urls.Length > 1)
			{
				_nextIndex = (_nextIndex + 1) % _urls.Length;
				_pending = LoadSafe(_urls[_nextIndex], _lifetime.Token);
			}
		}
	}

	private async Task<T?> LoadSafe(string url, CancellationToken token)
	{
		try
		{
			return await _003Cload_003EP(url, token);
		}
		catch
		{
			return null;
		}
	}

	public void Clear()
	{
		_generation++;
		_lifetime?.Cancel();
		_lifetime?.Dispose();
		_lifetime = null;
		_pending = null;
		_urls = Array.Empty<string>();
		_nextIndex = 0;
		_presented = false;
		_due = 0.0;
	}
}
