using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using ChenIronclad;
using ChenIronclad.ChenIroncladCode.core;
using Godot;

[assembly: AssemblyCompany("ChenIronclad")]
[assembly: AssemblyConfiguration("ExportRelease")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: AssemblyProduct("ChenIronclad")]
[assembly: AssemblyTitle("ChenIronclad")]
[assembly: AssemblyHasScripts(new Type[]
{
	typeof(ChenCardTrail),
	typeof(ChenCardTrailVfx),
	typeof(ChenCreatureVisuals),
	typeof(ChenMerchantCharacter),
	typeof(ChenRestSiteCharacter),
	typeof(ChenSelectionReticle),
	typeof(ChenIroncladInit)
})]
[assembly: AssemblyVersion("1.0.0.0")]
[module: RefSafetyRules(11)]
