using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class LayeredBackgroundRuntime
{
	private const string ConfigureMethod = "configure_for_sts2";

	private const string BattlefieldCanvasPath = "BattlefieldCanvas";

	private const string SelectedLayersMeta = "czn_battlefield_selected_layers";

	private const int BattlefieldCanvasLayer = -1;

	private readonly HashSet<string> _reportedUnavailableSurfaces = new HashSet<string>(StringComparer.Ordinal);

	public bool TryGetPreloadPaths(BattlefieldPresentationProfile profile, out IReadOnlyList<string> paths)
	{
		return TryGetPreloadPaths("battlefield:" + profile.Id, profile.Id, profile.Background.ResourcePaths, out paths);
	}

	private bool TryGetPreloadPaths(string surfaceKey, string profileId, IEnumerable<string> resourcePaths, out IReadOnlyList<string> paths)
	{
		string[] array = resourcePaths.Distinct<string>(StringComparer.Ordinal).ToArray();
		string[] array2 = array.Where((string path) => !ResourceLoader.Exists(path, "")).ToArray();
		if (array2.Length == 0)
		{
			paths = array;
			return true;
		}
		if (_reportedUnavailableSurfaces.Add(surfaceKey))
		{
			Entry.Log.Warn("Layered background replacement resources are unavailable; keeping the STS2 background [profile=" + profileId + ", missing=" + string.Join(",", array2) + "].", 1);
		}
		paths = Array.Empty<string>();
		return false;
	}

	public bool TryCreate(BattlefieldPresentationProfile profile, Rng rng, out NCombatBackground background)
	{
		return TryCreate("battlefield:" + profile.Id, profile.Id, profile.Background.ShellScenePath, profile.Background.BgLayerGroups, profile.Background.FgLayers, profile.Background.ResourcePaths, rng, out background);
	}

	public bool TryMount(BattlefieldPresentationProfile profile, Rng rng, Node parent, out Control shell)
	{
		shell = null;
		Control shell2 = null;
		try
		{
			if (!TryBuildShell("battlefield:" + profile.Id, profile.Id, profile.Background.ShellScenePath, profile.Background.BgLayerGroups, profile.Background.FgLayers, profile.Background.ResourcePaths, rng, out shell2))
			{
				return false;
			}
			parent.AddChild((Node)(object)shell2, false, (InternalMode)0);
			shell = shell2;
			shell2 = null;
			return true;
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Layered background mount failed; keeping the STS2 " + $"background [profile={profile.Id}, error={ex.Message}].", 1);
			return false;
		}
		finally
		{
			if (shell2 != null && GodotObject.IsInstanceValid((GodotObject)(object)shell2))
			{
				((GodotObject)shell2).Free();
			}
		}
	}

	private bool TryCreate(string surfaceKey, string profileId, string shellScenePath, IReadOnlyList<IReadOnlyList<string>> bgLayerGroups, IReadOnlyList<string> fgLayers, IEnumerable<string> resourcePaths, Rng rng, out NCombatBackground background)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		background = null;
		NCombatBackground val = null;
		Control shell = null;
		try
		{
			if (!TryBuildShell(surfaceKey, profileId, shellScenePath, bgLayerGroups, fgLayers, resourcePaths, rng, out shell))
			{
				return false;
			}
			Control val2 = shell;
			val = new NCombatBackground
			{
				Name = StringName.op_Implicit("CznLayeredBackground"),
				MouseFilter = (MouseFilterEnum)2
			};
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			shell = null;
			background = val;
			val = null;
			return true;
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Layered background replacement failed; using the STS2 " + $"background [profile={profileId}, error={ex.Message}].", 1);
			return false;
		}
		finally
		{
			if (shell != null && GodotObject.IsInstanceValid((GodotObject)(object)shell))
			{
				((GodotObject)shell).Free();
			}
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((GodotObject)val).Free();
			}
		}
	}

	private bool TryBuildShell(string surfaceKey, string profileId, string shellScenePath, IReadOnlyList<IReadOnlyList<string>> bgLayerGroups, IReadOnlyList<string> fgLayers, IEnumerable<string> resourcePaths, Rng rng, out Control shell)
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		shell = null;
		Control val = null;
		try
		{
			List<string> list = new List<string>();
			if (!TryGetPreloadPaths(surfaceKey, profileId, resourcePaths, out IReadOnlyList<string> _))
			{
				return false;
			}
			val = PreloadManager.Cache.GetScene(shellScenePath).Instantiate<Control>((GenEditState)0);
			ValidateShell(val, bgLayerGroups.Count);
			for (int i = 0; i < bgLayerGroups.Count; i++)
			{
				string text = rng.NextItem<string>((IEnumerable<string>)bgLayerGroups[i]);
				if (string.IsNullOrWhiteSpace(text))
				{
					throw new InvalidOperationException($"Background layer group {i} has no selection.");
				}
				Control val2 = InstantiateAndConfigureLayer(text);
				((Node)val).GetNode(NodePath.op_Implicit($"{"BattlefieldCanvas"}/Layer_{i:D2}")).AddChild((Node)(object)val2, false, (InternalMode)0);
				list.Add(text);
			}
			if (fgLayers.Count > 0)
			{
				string text2 = rng.NextItem<string>((IEnumerable<string>)fgLayers);
				if (!string.IsNullOrWhiteSpace(text2))
				{
					Control val3 = InstantiateAndConfigureLayer(text2);
					((Node)val).GetNode(NodePath.op_Implicit("BattlefieldCanvas/Foreground")).AddChild((Node)(object)val3, false, (InternalMode)0);
					list.Add(text2);
				}
			}
			string text3 = string.Join(",", list);
			((GodotObject)val).SetMeta(StringName.op_Implicit("czn_battlefield_selected_layers"), Variant.From<string>(ref text3));
			Entry.Log.Info("Selected CZN battlefield layers " + $"[surface={surfaceKey}, profile={profileId}, " + "layers=" + text3 + "].", 1);
			shell = val;
			val = null;
			return true;
		}
		finally
		{
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((GodotObject)val).Free();
			}
		}
	}

	private static void ValidateShell(Control shell, int backgroundLayerCount)
	{
		CanvasLayer nodeOrNull = ((Node)shell).GetNodeOrNull<CanvasLayer>(NodePath.op_Implicit("BattlefieldCanvas"));
		if (nodeOrNull == null || nodeOrNull.Layer != -1)
		{
			throw new InvalidOperationException("Background shell has no isolated battlefield canvas.");
		}
		for (int i = 0; i < backgroundLayerCount; i++)
		{
			if (((Node)shell).GetNodeOrNull(NodePath.op_Implicit($"{"BattlefieldCanvas"}/Layer_{i:D2}")) == null)
			{
				throw new InvalidOperationException($"Background shell is missing Layer_{i:D2}.");
			}
		}
		if (((Node)shell).GetNodeOrNull(NodePath.op_Implicit("BattlefieldCanvas/Foreground")) == null)
		{
			throw new InvalidOperationException("Background shell is missing Foreground.");
		}
	}

	private static Control InstantiateAndConfigureLayer(string layerPath)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Invalid comparison between Unknown and I8
		Control val = PreloadManager.Cache.GetScene(layerPath).Instantiate<Control>((GenEditState)0);
		try
		{
			if (!((GodotObject)val).HasMethod(StringName.op_Implicit("configure_for_sts2")))
			{
				throw new InvalidOperationException("Background layer has no configure_for_sts2 contract: " + layerPath);
			}
			Variant val2 = ((GodotObject)val).Call(StringName.op_Implicit("configure_for_sts2"), Array.Empty<Variant>());
			if ((long)((Variant)(ref val2)).VariantType != 1 || !((Variant)(ref val2)).AsBool())
			{
				throw new InvalidOperationException("Background layer rejected its runtime contract: " + layerPath);
			}
			((CanvasItem)val).Visible = true;
			return val;
		}
		catch
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((GodotObject)val).Free();
			}
			throw;
		}
	}
}
