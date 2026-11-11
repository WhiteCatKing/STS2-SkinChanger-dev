using System;
using System.Linq;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

internal static class FrameworkCreatureSceneFactory
{
	private const string FactoryTypeName = "BaseLib.Utils.NodeFactories.NodeFactory`1";

	public static NCreatureVisuals Create(PackedScene scene)
	{
		ArgumentNullException.ThrowIfNull(scene, "scene");
		Node val = scene.Instantiate((GenEditState)0);
		NCreatureVisuals val2 = (NCreatureVisuals)(object)((val is NCreatureVisuals) ? val : null);
		if (val2 != null)
		{
			return val2;
		}
		((GodotObject)val).Free();
		Type type2 = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
			select FrameworkCardVisualGuard.TryGetType(assembly, "BaseLib.Utils.NodeFactories.NodeFactory`1")).FirstOrDefault((Type type) => type != null)?.MakeGenericType(typeof(NCreatureVisuals)) ?? throw new InvalidOperationException("所选框架皮肤需要 BaseLib 的 NCreatureVisuals 场景工厂，但当前未找到该工厂。");
		MethodInfo methodInfo = type2.GetMethod("CreateFromScene", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(PackedScene) }, null) ?? throw new MissingMethodException(type2.FullName, "CreateFromScene(PackedScene)");
		try
		{
			object? obj = methodInfo.Invoke(null, new object[1] { scene });
			return (NCreatureVisuals)(((obj is NCreatureVisuals) ? obj : null) ?? throw new InvalidOperationException("BaseLib 场景工厂没有返回 NCreatureVisuals。"));
		}
		catch (TargetInvocationException ex) when (ex.InnerException != null)
		{
			throw new InvalidOperationException("BaseLib 无法把所选框架皮肤场景转换为战斗角色节点。", ex.InnerException);
		}
	}
}
