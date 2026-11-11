using System.Collections.Generic;
using Godot;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class EnemyNodeSet
{
	private static readonly List<Node> NoNodes = new List<Node>();

	private List<Node>? _spineSprites;

	private List<Node>? _particles;

	public IReadOnlyList<Node> SpineSprites => _spineSprites ?? NoNodes;

	public IReadOnlyList<Node> Particles => _particles ?? NoNodes;

	internal void AddSpineSprite(Node node)
	{
		(_spineSprites ?? (_spineSprites = new List<Node>())).Add(node);
	}

	internal void AddParticles(Node node)
	{
		(_particles ?? (_particles = new List<Node>())).Add(node);
	}
}
