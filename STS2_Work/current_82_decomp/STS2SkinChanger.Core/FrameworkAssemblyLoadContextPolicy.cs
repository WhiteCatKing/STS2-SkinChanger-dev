using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace STS2SkinChanger.Core;

internal static class FrameworkAssemblyLoadContextPolicy
{
	public static Assembly LoadFromAssemblyPath(Assembly hostAssembly, string assemblyPath)
	{
		return (AssemblyLoadContext.GetLoadContext(hostAssembly) ?? AssemblyLoadContext.Default).LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));
	}
}
