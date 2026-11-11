using System;
using System.Collections.Generic;
using System.Linq;

namespace CznCore.Standalone;

internal sealed class TransportRegistration
{
	private readonly object _sync = new object();

	private readonly HashSet<Type> _installed = new HashSet<Type>();

	private readonly Dictionary<Type, Exception> _failed = new Dictionary<Type, Exception>();

	public void Ensure(IEnumerable<Type> types, Action<Type> install)
	{
		Exception first = null;
		IReadOnlyList<Type> readOnlyList = TryEnsure(types, install, delegate(Type _, Exception ex)
		{
			if (first == null)
			{
				first = ex;
			}
		});
		if (readOnlyList.Count != 0)
		{
			throw new InvalidOperationException("Core presentation patch setup failed: " + string.Join(", ", readOnlyList.Select((Type type) => type.Name)) + ". Restart before retrying.", first);
		}
	}

	public IReadOnlyList<Type> TryEnsure(IEnumerable<Type> types, Action<Type> install, Action<Type, Exception> reportFailure)
	{
		List<Type> list = null;
		lock (_sync)
		{
			foreach (Type type in types)
			{
				if (_installed.Contains(type))
				{
					continue;
				}
				if (_failed.TryGetValue(type, out Exception value))
				{
					(list ?? (list = new List<Type>())).Add(type);
					reportFailure(type, value);
					continue;
				}
				try
				{
					install(type);
					_installed.Add(type);
				}
				catch (Exception ex)
				{
					_failed.Add(type, ex);
					(list ?? (list = new List<Type>())).Add(type);
					reportFailure(type, ex);
				}
			}
		}
		IReadOnlyList<Type> readOnlyList = list;
		return readOnlyList ?? Array.Empty<Type>();
	}
}
