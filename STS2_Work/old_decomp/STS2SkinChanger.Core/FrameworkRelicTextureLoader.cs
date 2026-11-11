using Godot;

namespace STS2SkinChanger.Core;

internal static class FrameworkRelicTextureLoader
{
	public static bool Apply(string path, ref Texture2D result)
	{
		Texture2D val = ResourceLoader.Load<Texture2D>(path, (string)null, (CacheMode)1);
		if (val == null)
		{
			ModLog.Warn("无法加载框架遗物图片：" + path);
			return true;
		}
		result = val;
		return false;
	}
}
