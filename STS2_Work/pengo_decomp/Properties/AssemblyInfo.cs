using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Godot;
using PengoTarot.BalatroEffect;
using PengoTarot.ConfigFW;
using PengoTarot.UI;

[assembly: AssemblyCompany("PengoTarot")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6efb5c4188108b94a76ac7442ec080d962465e0e")]
[assembly: AssemblyProduct("PengoTarot")]
[assembly: AssemblyTitle("PengoTarot")]
[assembly: AssemblyHasScripts(new Type[]
{
	typeof(NBalatroInspectEnchantScreen),
	typeof(NBalatroInspectScreen),
	typeof(NConfigFloatingWindow),
	typeof(NConfigFloatingWindowEntryButton),
	typeof(NPortableTarotPopup),
	typeof(NMerchantTarot)
})]
[assembly: AssemblyVersion("1.0.0.0")]
[module: RefSafetyRules(11)]
