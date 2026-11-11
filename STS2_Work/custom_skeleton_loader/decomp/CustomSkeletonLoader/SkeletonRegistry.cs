using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace CustomSkeletonLoader;

internal static class SkeletonRegistry
{
	internal class SkeletonInfo
	{
		public string Name { get; set; } = string.Empty;

		public string TresPath { get; set; } = string.Empty;

		public DateTime InterceptTime { get; set; }
	}

	private static readonly ConcurrentDictionary<string, SkeletonInfo> _registry = new ConcurrentDictionary<string, SkeletonInfo>(StringComparer.OrdinalIgnoreCase);

	internal static readonly string ExportRootPath = Path.Combine(SkeletonLoader.SkeletonsPath, "Spine_resource");

	internal static int Count => _registry.Count;

	internal static void Record(string name, string tresPath, Resource? resourceObj = null)
	{
		if (!string.IsNullOrEmpty(name))
		{
			SkeletonInfo addValue = new SkeletonInfo
			{
				Name = name,
				TresPath = tresPath,
				InterceptTime = DateTime.Now
			};
			_registry.AddOrUpdate(name, addValue, (string _, SkeletonInfo existing) => existing);
		}
	}

	internal static IEnumerable<SkeletonInfo> GetAll()
	{
		return _registry.Values;
	}

	internal static void Clear()
	{
		_registry.Clear();
	}
}
