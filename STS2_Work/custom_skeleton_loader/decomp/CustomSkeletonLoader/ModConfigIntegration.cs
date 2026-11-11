using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using ModConfig;

namespace CustomSkeletonLoader;

internal static class ModConfigIntegration
{
	internal class ParsedSkeletonData
	{
		public string TresPath { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;

		public string AtlasPath { get; set; } = string.Empty;

		public string SkelPath { get; set; } = string.Empty;

		public string AtlasUid { get; set; } = string.Empty;

		public string SkelUid { get; set; } = string.Empty;

		public bool IsValid => !string.IsNullOrEmpty(AtlasPath) && !string.IsNullOrEmpty(SkelPath);
	}

	internal static class TresFileParser
	{
		private static readonly Regex AtlasPattern = new Regex("\\[ext_resource\\s+type=\"SpineAtlasResource\"[^\\]]*uid=\"(uid://[^\"]+)\"[^\\]]*path=\"(res://[^\"]+\\.atlas)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex SkelPattern = new Regex("\\[ext_resource\\s+type=\"SpineSkeletonFileResource\"[^\\]]*uid=\"(uid://[^\"]+)\"[^\\]]*path=\"(res://[^\"]+\\.skel)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		internal static ParsedSkeletonData Parse(string tresPath, string content)
		{
			ParsedSkeletonData parsedSkeletonData = new ParsedSkeletonData
			{
				TresPath = tresPath
			};
			Match match = AtlasPattern.Match(content);
			if (match.Success)
			{
				parsedSkeletonData.AtlasUid = match.Groups[1].Value;
				parsedSkeletonData.AtlasPath = match.Groups[2].Value;
			}
			Match match2 = SkelPattern.Match(content);
			if (match2.Success)
			{
				parsedSkeletonData.SkelUid = match2.Groups[1].Value;
				parsedSkeletonData.SkelPath = match2.Groups[2].Value;
			}
			if (!string.IsNullOrEmpty(parsedSkeletonData.AtlasPath))
			{
				parsedSkeletonData.Name = Path.GetFileNameWithoutExtension(parsedSkeletonData.AtlasPath);
			}
			return parsedSkeletonData;
		}
	}

	private const string ModId = "CustomSkeletonLoader";

	internal const string KeyEnableDebugLogs = "enable_debug_logs";

	internal const string KeyExportSkeletons = "export_skeletons";

	private static readonly Regex AtlasImagePattern = new Regex("image_file:\\s*(.+\\.png)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static volatile bool _isExporting = false;

	private static int _pngCallCount = 0;

	private static Dictionary<string, string>? _pckFileCache;

	internal static bool IsDebugLogEnabled()
	{
		try
		{
			return ModConfigApi.GetValue<bool>("CustomSkeletonLoader", "enable_debug_logs");
		}
		catch
		{
			return true;
		}
	}

	internal static void Register()
	{
		try
		{
			ModConfigApi.Register("CustomSkeletonLoader", "CustomSkeletonLoader", new Dictionary<string, string>
			{
				["en"] = "CustomSkeletonLoader",
				["zhs"] = "自定义骨骼加载器"
			}, (ConfigEntry[])(object)new ConfigEntry[2]
			{
				CreateDebugLogEntry(),
				CreateExportButtonEntry()
			});
			GD.Print("[CustomSkeletonLoader] ModConfig registered successfully");
		}
		catch (Exception value)
		{
			GD.PrintErr($"[CustomSkeletonLoader] Failed to register ModConfig: {value}");
		}
	}

	private static void CreateUnifiedCopy(string exportDir)
	{
		try
		{
			int num = 0;
			int num2 = 0;
			string[] directories = Directory.GetDirectories(exportDir);
			foreach (string text in directories)
			{
				string fileName = Path.GetFileName(text);
				if (fileName.Equals("unified", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				string[] files = Directory.GetFiles(text, "*.png");
				if (files.Length == 0)
				{
					continue;
				}
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[0]);
				string text2 = Path.Combine(text, "unified");
				HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				string[] files2 = Directory.GetFiles(text, "*.atlas");
				foreach (string path in files2)
				{
					string atlasContent = File.ReadAllText(path);
					List<string> list = ExtractPngNamesFromAtlas(atlasContent);
					foreach (string item in list)
					{
						hashSet.Add(item);
					}
				}
				if (hashSet.Count == 0)
				{
					string[] array = files;
					foreach (string path2 in array)
					{
						hashSet.Add(Path.GetFileName(path2));
					}
				}
				int num3 = 0;
				string[] files3 = Directory.GetFiles(text, "*.atlas");
				if (files3.Length > 1)
				{
					GD.Print($"[CustomSkeletonLoader] Skip: {fileName} has {files3.Length} atlas files, skipping unified/");
					continue;
				}
				Directory.CreateDirectory(text2);
				string[] array2 = files3;
				foreach (string sourceFileName in array2)
				{
					string text3 = Path.Combine(text2, fileNameWithoutExtension + ".atlas");
					File.Copy(sourceFileName, text3, overwrite: true);
					UpdateAtlasPngReferences(text3, hashSet);
					num3++;
				}
				string[] files4 = Directory.GetFiles(text, "*.skel");
				foreach (string sourceFileName2 in files4)
				{
					string destFileName = Path.Combine(text2, fileNameWithoutExtension + ".skel");
					File.Copy(sourceFileName2, destFileName, overwrite: true);
					num3++;
				}
				foreach (string item2 in hashSet)
				{
					string text4 = Path.Combine(text, item2);
					if (File.Exists(text4))
					{
						File.Copy(text4, Path.Combine(text2, item2), overwrite: true);
						num3++;
					}
				}
				num++;
				num2 += num3;
			}
			GD.Print($"[CustomSkeletonLoader] Unified copy: {num2} files in {num} skeleton dirs (embedded unified/)");
		}
		catch (Exception ex)
		{
			GD.PrintErr("[CustomSkeletonLoader] Unified copy error: " + ex.Message);
		}
	}

	private static void UpdateAtlasPngReferences(string atlasPath, HashSet<string> validPngNames)
	{
		try
		{
			string text = File.ReadAllText(atlasPath);
			string[] array = text.Split('\n');
			List<string> list = new List<string>();
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string text3 = text2.Trim();
				if (text3.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				{
					list.Add(validPngNames.Contains(text3) ? text3 : text2);
				}
				else
				{
					list.Add(text2);
				}
			}
			File.WriteAllText(atlasPath, string.Join("\n", list));
		}
		catch (Exception ex)
		{
			GD.PrintErr("[CustomSkeletonLoader] Failed to update atlas references for " + atlasPath + ": " + ex.Message);
		}
	}

	internal static void ExportAllSkeletons()
	{
		if (_isExporting)
		{
			GD.Print("[CustomSkeletonLoader] Export already in progress, skipping...");
			return;
		}
		_isExporting = true;
		try
		{
			GD.Print("[CustomSkeletonLoader] Starting skeleton export (runtime PCK read)...");
			Stopwatch stopwatch = Stopwatch.StartNew();
			_pngCallCount = 0;
			string exportRootPath = SkeletonRegistry.ExportRootPath;
			Directory.CreateDirectory(exportRootPath);
			string[] knownSkeletonTresPaths = GetKnownSkeletonTresPaths();
			GD.Print($"[CustomSkeletonLoader] Known skeleton .tres count: {knownSkeletonTresPaths.Length}");
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < knownSkeletonTresPaths.Length; i++)
			{
				string text = knownSkeletonTresPaths[i];
				try
				{
					byte[] array = LoadResBytes(text);
					if (array == null || array.Length == 0)
					{
						num2++;
						continue;
					}
					string @string = Encoding.UTF8.GetString(array);
					ParsedSkeletonData parsedSkeletonData = TresFileParser.Parse(text, @string);
					if (!parsedSkeletonData.IsValid)
					{
						num3++;
						continue;
					}
					if (hashSet.Contains(parsedSkeletonData.Name))
					{
						num3++;
						continue;
					}
					hashSet.Add(parsedSkeletonData.Name);
					if (ExportFromPck(parsedSkeletonData, exportRootPath))
					{
						num++;
					}
					else
					{
						num2++;
					}
				}
				catch (Exception value)
				{
					GD.PrintErr($"[CustomSkeletonLoader] Error processing {text}:\n{value}");
					num2++;
				}
				if ((i + 1) % 25 == 0 || i + 1 == knownSkeletonTresPaths.Length)
				{
					GD.Print($"[CustomSkeletonLoader] Progress: {i + 1}/{knownSkeletonTresPaths.Length} (ok={num} fail={num2} skip={num3})");
				}
			}
			stopwatch.Stop();
			GD.Print("[CustomSkeletonLoader] ═════ EXPORT COMPLETE ═════");
			GD.Print($"[CustomSkeletonLoader] Total .tres      : {knownSkeletonTresPaths.Length}");
			GD.Print($"[CustomSkeletonLoader] Success           : {num}");
			GD.Print($"[CustomSkeletonLoader] Failed             : {num2}");
			GD.Print($"[CustomSkeletonLoader] Skipped(dup/invalid): {num3}");
			GD.Print($"[CustomSkeletonLoader] Time elapsed       : {stopwatch.Elapsed.TotalSeconds:F2}s");
			GD.Print("[CustomSkeletonLoader] Output directory   : " + exportRootPath);
			MergeSharedSkeletons(exportRootPath);
			CreateUnifiedCopy(exportRootPath);
		}
		finally
		{
			_pckFileCache = null;
			_isExporting = false;
		}
	}

	private static void MergeSharedSkeletons(string exportDir)
	{
		try
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] directories = Directory.GetDirectories(exportDir);
			foreach (string text in directories)
			{
				string[] files = Directory.GetFiles(text, "*.skel");
				foreach (string path in files)
				{
					using MD5 mD = MD5.Create();
					byte[] inArray = mD.ComputeHash(File.ReadAllBytes(path));
					string key = Convert.ToHexString(inArray);
					if (!dictionary.ContainsKey(key))
					{
						dictionary[key] = text;
						continue;
					}
					string path2 = dictionary[key];
					string[] files2 = Directory.GetFiles(text);
					foreach (string text2 in files2)
					{
						string text3 = Path.Combine(path2, Path.GetFileName(text2));
						if (!File.Exists(text3))
						{
							File.Copy(text2, text3);
						}
					}
					if (Directory.GetFiles(text).Length == 0)
					{
						Directory.Delete(text);
					}
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("[CustomSkeletonLoader] Merge error: " + ex.Message);
		}
	}

	private static string[] GetKnownSkeletonTresPaths()
	{
		return new string[152]
		{
			"res://animations/backgrounds/ceremonial_beast/bg_ceremonial_beast_bottom_skel_data.tres", "res://animations/backgrounds/ceremonial_beast/bg_ceremonial_beast_top_skel_data.tres", "res://animations/backgrounds/fake_merchant_room/hand/fake_merchant_hand_skel_data.tres", "res://animations/backgrounds/mainmenu/bottom/main_menu_bottom_skel_data.tres", "res://animations/backgrounds/mainmenu/logo/main_menu_logo_skel_data.tres", "res://animations/backgrounds/mainmenu/top/main_menu_top_skel_data.tres", "res://animations/backgrounds/merchant_room/hand/merchant_hand_skel_data.tres", "res://animations/backgrounds/tezcatara/tezcatara_skel_data_resource.tres", "res://animations/backgrounds/treasure_room/chest_room_act_1_skel_data.tres", "res://animations/backgrounds/treasure_room/chest_room_act_2_skel_data.tres",
			"res://animations/backgrounds/treasure_room/chest_room_act_3_skel_data.tres", "res://animations/character_select/defect/characterselect_defect_skel_data.tres", "res://animations/character_select/ironclad/characterselect_ironclad_skel_data.tres", "res://animations/character_select/necrobinder/characterselect_necrobinder_skel_data.tres", "res://animations/character_select/regent/characterselect_regent_skel_data.tres", "res://animations/character_select/silent/characterselect_silent_skel_data.tres", "res://animations/characters/defect/defect_skel_data.tres", "res://animations/characters/ironclad/ironclad_skel_data.tres", "res://animations/characters/necrobinder/necrobinder_skel_data.tres", "res://animations/characters/regent/regent_skel_data.tres",
			"res://animations/characters/regent/regent_weapon_skel_data.tres", "res://animations/characters/silent/silent_skel_data.tres", "res://animations/map/ceremonial_beast_boss/ceremonial_beast_boss_node_skel_data.tres", "res://animations/map/queen_boss/queen_boss_node_skel_data.tres", "res://animations/map/the_insatiable_boss/the_insatiable_boss_node_skel_data.tres", "res://animations/merchant/defect/defect_merchant_skel_data.tres", "res://animations/merchant/ironclad/ironclad_merchant_skel_data.tres", "res://animations/merchant/necrobinder/necrobinder_merchant_skel_data.tres", "res://animations/merchant/regent/regent_merchant_skel_data.tres", "res://animations/merchant/silent/silent_merchant_skel_data.tres",
			"res://animations/monsters/amalgam/amalgam_skel_data.tres", "res://animations/monsters/architect/architect_skel_data.tres", "res://animations/monsters/assassin_ruby_raider/assassin_ruby_raider_skel_data.tres", "res://animations/monsters/axe_ruby_raider/axe_ruby_raider_skel_data.tres", "res://animations/monsters/axebot/axebot_skel_data.tres", "res://animations/monsters/battleworn_dummy/battleworn_dummy_skel_data.tres", "res://animations/monsters/bowlbug/bowlbug_skel_data.tres", "res://animations/monsters/brute_ruby_raider/brute_ruby_raider_skel_data.tres", "res://animations/monsters/byrdonis/byrdonis_skel_data.tres", "res://animations/monsters/byrdpip/byrdpip_skel_data.tres",
			"res://animations/monsters/ceremonial_beast/ceremonial_beast_skel_data.tres", "res://animations/monsters/chomper/chomper_skel_data.tres", "res://animations/monsters/corpse_slug/corpse_slug_skel_data.tres", "res://animations/monsters/crossbow_ruby_raider/crossbow_ruby_raider_skel_data.tres", "res://animations/monsters/cubex_construct/cubex_construct_skel_data.tres", "res://animations/monsters/cultists/cultists_skel_data.tres", "res://animations/monsters/decimillipede/decimillipede_back_skel_data.tres", "res://animations/monsters/decimillipede/decimillipede_front_skel_data.tres", "res://animations/monsters/decimillipede/decimillipede_middle_skel_data.tres", "res://animations/monsters/decimillipede/rockstone_skel_data.tres",
			"res://animations/monsters/devoted_sculptor/devoted_sculptor_skel_data.tres", "res://animations/monsters/entomancer/entomancer_skel_data.tres", "res://animations/monsters/exoskeleton/exoskeleton_skel_data.tres", "res://animations/monsters/eye_with_teeth/eye_with_teeth_skel_data.tres", "res://animations/monsters/fabricator/fabricator_skel_data.tres", "res://animations/monsters/fat_gremlin/fat_gremlin_skel_data.tres", "res://animations/monsters/flail_knight/flail_knight_skel_data.tres", "res://animations/monsters/flying_mushrooms/flying_mushrooms_skel_data.tres", "res://animations/monsters/fogmog/fogmog_skel_data.tres", "res://animations/monsters/fossil_stalker/fossil_stalker_skel_data.tres",
			"res://animations/monsters/frog_knight/frog_knight_skel_data.tres", "res://animations/monsters/fuzzy_wurm_crawler/fuzzy_wurm_crawler_skel_data.tres", "res://animations/monsters/gas_bomb/gas_bomb_skel_data.tres", "res://animations/monsters/gremlin_merc/gremlin_merc_skel_data.tres", "res://animations/monsters/guardbot/guardbot_skel_data.tres", "res://animations/monsters/haunted_ship/haunted_ship_skel_data.tres", "res://animations/monsters/hunter_killer/hunter_killer_skel_data.tres", "res://animations/monsters/infested_guardian/infested_guardian_skel_data.tres", "res://animations/monsters/infested_prism/infested_prism_skel_data.tres", "res://animations/monsters/infested_purifier/infested_purifier_skel_data.tres",
			"res://animations/monsters/inklet/inklet_skeleton_data.tres", "res://animations/monsters/kaiser_crab/kaiser_crab_skeleton_data.tres", "res://animations/monsters/kin_follower/kin_follower_skel_data.tres", "res://animations/monsters/kin_priest/kin_priest_skel_data.tres", "res://animations/monsters/knowledge_demon/knowledge_demon_skel_data.tres", "res://animations/monsters/lagavulin_matriarch/lagavulin_matriarch_skel_data.tres", "res://animations/monsters/leaf_slime_m/leaf_slime_m_skel_data.tres", "res://animations/monsters/leaf_slime_s/leaf_slime_s_skel_data.tres", "res://animations/monsters/living_shield/living_shield_skel_data.tres", "res://animations/monsters/living_smog/living_smog_skel_data.tres",
			"res://animations/monsters/louse_progenitor/louse_progenitor_skel_data.tres", "res://animations/monsters/magi_knight/magi_knight_skel_data.tres", "res://animations/monsters/mawler/mawler_skel_data.tres", "res://animations/monsters/mecha_knight/mecha_knight_skel_data.tres", "res://animations/monsters/myte/myte_skel_data.tres", "res://animations/monsters/nibbit/nibbit_skel_data.tres", "res://animations/monsters/noisebot/noisebot_skel_data.tres", "res://animations/monsters/orb_head/orb_head_skel_data.tres", "res://animations/monsters/osty/osty_skel_data.tres", "res://animations/monsters/ovicopter/ovicopter_skel_data.tres",
			"res://animations/monsters/owl_magistrate/owl_magistrate_skel_data.tres", "res://animations/monsters/paels_legion/paels_legion_skel_data.tres", "res://animations/monsters/parafright/parafright_skel_data.tres", "res://animations/monsters/phantasmal_gardener/phantasmal_gardener_skel_data.tres", "res://animations/monsters/phrog_parasite/phrog_parasite_skel_data.tres", "res://animations/monsters/punch_construct/punch_construct_skel_data.tres", "res://animations/monsters/queen/queen_skel_data.tres", "res://animations/monsters/scroll_of_biting/scroll_of_biting_skel_data.tres", "res://animations/monsters/seapunk/seapunk_skel_data.tres", "res://animations/monsters/sewer_clam/sewer_clam_skel_data.tres",
			"res://animations/monsters/shrinker_beetle/shrinker_beetle_skel_data.tres", "res://animations/monsters/slimed_berserker/slimed_berserker_skel_data.tres", "res://animations/monsters/slithering_strangler/slithering_strangler_skel_data.tres", "res://animations/monsters/sludge_spinner/sludge_spinner_skel_data.tres", "res://animations/monsters/slumbering_beetle/slumbering_beetle_skel_data.tres", "res://animations/monsters/snapping_jaxfruit/snapping_jaxfruit_skel_data.tres", "res://animations/monsters/sneaky_gremlin/sneaky_gremlin_skel_data.tres", "res://animations/monsters/soul_fysh/soul_fysh_skel_data.tres", "res://animations/monsters/soul_nexus/soulnexus_skel_data.tres", "res://animations/monsters/spectral_knight/spectral_knight_skel_data.tres",
			"res://animations/monsters/spiny_toad/spiny_toad_skel_data.tres", "res://animations/monsters/terror_eel/terror_eel_skel_data.tres", "res://animations/monsters/test_subject/test_subject_skel_data.tres", "res://animations/monsters/the_forgotten/the_forgotten_skel_data.tres", "res://animations/monsters/the_insatiable/the_insatiable_skel_data.tres", "res://animations/monsters/the_lost/the_lost_skel_data.tres", "res://animations/monsters/the_obscura/the_obscura_skel_data.tres", "res://animations/monsters/thieving_hopper/thieving_hopper_skel_data.tres", "res://animations/monsters/toadpole/toadpole_skel_data.tres", "res://animations/monsters/tough_egg/tough_egg_skel_data.tres",
			"res://animations/monsters/tunneler/tunneler_skel_data.tres", "res://animations/monsters/turret_operator/turret_operator_skel_data.tres", "res://animations/monsters/twig_slime_m/twig_slime_m_skel_data.tres", "res://animations/monsters/twig_slime_s/twig_slime_s_skel_data.tres", "res://animations/monsters/two_tailed_rat/two_tailed_rat_skel_data.tres", "res://animations/monsters/vantom/vantom_skel_data.tres", "res://animations/monsters/vine_shambler/vine_shambler_skel_data.tres", "res://animations/monsters/waterfall_giant/waterfall_giant_skeleton_data.tres", "res://animations/monsters/wriggler/wriggler_skel_data.tres", "res://animations/monsters/zapbot/zapbot_skel_data.tres",
			"res://animations/rest_site/defect/rest_site_defect_skel_data.tres", "res://animations/rest_site/ironclad/rest_site_ironclad_skel_data.tres", "res://animations/rest_site/necrobinder/rest_site_necrobinder_skel_data.tres", "res://animations/rest_site/necrobinder/rest_site_osty_skel_data.tres", "res://animations/rest_site/regent/rest_site_regent_skel_data.tres", "res://animations/rest_site/silent/rest_site_silent_skel_data.tres", "res://animations/ui/combat/orbs/dark_orb_skel_data.tres", "res://animations/ui/combat/orbs/frost_orb_skel_data.tres", "res://animations/ui/combat/orbs/glass_orb_skel_data.tres", "res://animations/ui/combat/orbs/lightning_orb_skel_data.tres",
			"res://animations/ui/combat/orbs/plasma_orb_skel_data.tres", "res://animations/logo/logo_megacrit_animate_skel_data.tres", "res://animations/vfx/vfx_kaiser_crab_boss_explosion/infested_boss_explosion_skel_data.tres", "res://animations/vfx/vfx_laser/laser_skel_data.tres", "res://animations/vfx/vfx_mecha_knight_shield/mecha_knight_shield_skel_data.tres", "res://animations/vfx/vfx_scratch/vfx_scratch_skel_data.tres", "res://animations/vfx/vfx_sovereign_blade/sovereign_blade_skel_data.tres", "res://animations/vfx/vfx_vine_shamber_vines/vine_shambler_vines_back_skel_data.tres", "res://animations/vfx/vfx_vine_shamber_vines/vine_shambler_vines_skel_data.tres", "res://animations/monsters/bygone_effigy/bygone_effigy_skel.tres",
			"res://animations/monsters/stabbot/stabbot_spine_data.tres", "res://animations/monsters/tracker_ruby_raider/tracker_ruby_raider.tres"
		};
	}

	private static bool ExportFromPck(ParsedSkeletonData parsed, string exportDir)
	{
		try
		{
			string text = Path.Combine(exportDir, parsed.Name);
			Directory.CreateDirectory(text);
			bool result = false;
			string text2 = Path.Combine(text, parsed.Name + ".spatlas");
			if (FindAndCopy(parsed.AtlasPath, text, ".spatlas"))
			{
				result = true;
				if (_pngCallCount <= 5)
				{
					GD.Print($"[CustomSkeletonLoader] [PNG-ENTRY-{_pngCallCount}] Calling ExportPngFromSpatlas for {parsed.Name}, exists={File.Exists(text2)}");
				}
				ExportPngFromSpatlas(text2, text);
				ConvertSpatlasToAtlas(text);
			}
			if (FindAndCopy(parsed.SkelPath, text, ".spskel"))
			{
				result = true;
				ConvertSpskelToSkel(text);
			}
			return result;
		}
		catch (Exception value)
		{
			GD.PrintErr($"[CustomSkeletonLoader] Failed to export '{parsed.Name}': {value}");
			return false;
		}
	}

	private static void ConvertSpatlasToAtlas(string targetDir)
	{
		try
		{
			string[] files = Directory.GetFiles(targetDir, "*.spatlas");
			foreach (string path in files)
			{
				string text = File.ReadAllText(path);
				int num = text.IndexOf("\"atlas_data\":\"");
				if (num < 0)
				{
					File.Delete(path);
					continue;
				}
				num += 14;
				int num2 = text.IndexOf("\",\"", num);
				if (num2 < 0)
				{
					num2 = text.IndexOf("\"}", num);
				}
				if (num2 < 0)
				{
					File.Delete(path);
					continue;
				}
				string contents = text.Substring(num, num2 - num).Replace("\\n", "\n");
				string path2 = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(path) + ".atlas");
				File.WriteAllText(path2, contents);
				File.Delete(path);
			}
		}
		catch (Exception ex)
		{
			GD.Print("[CustomSkeletonLoader][WARN] Failed to convert spatlas to atlas: " + ex.Message);
		}
	}

	private static void ConvertSpskelToSkel(string targetDir)
	{
		try
		{
			string[] files = Directory.GetFiles(targetDir, "*.spskel");
			foreach (string text in files)
			{
				string destFileName = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(text) + ".skel");
				File.Copy(text, destFileName, overwrite: true);
				File.Delete(text);
			}
		}
		catch (Exception ex)
		{
			GD.Print("[CustomSkeletonLoader][WARN] Failed to convert spskel to skel: " + ex.Message);
		}
	}

	private static List<string> ExtractPngNamesFromAtlas(string atlasContent)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrEmpty(atlasContent))
		{
			return hashSet.ToList();
		}
		foreach (Match item in AtlasImagePattern.Matches(atlasContent))
		{
			if (item.Groups.Count > 1)
			{
				hashSet.Add(item.Groups[1].Value.Trim());
			}
		}
		string[] array = atlasContent.Split('\n');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string text2 = text.Trim();
			if (text2.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(text2) && !text2.Contains(':'))
			{
				hashSet.Add(text2);
			}
		}
		return hashSet.ToList();
	}

	private static void ExportPngFromSpatlas(string spatlasPath, string targetDir)
	{
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		_pngCallCount++;
		bool flag = _pngCallCount <= 5;
		if (flag)
		{
			GD.Print($"[CustomSkeletonLoader] [PNG-START-{_pngCallCount}] path={spatlasPath}");
		}
		try
		{
			if (!File.Exists(spatlasPath))
			{
				if (flag)
				{
					GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-SKIP] file not found");
				}
				return;
			}
			byte[] array = File.ReadAllBytes(spatlasPath);
			if (flag)
			{
				GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-STEP1] ReadAllBytes OK, size={array.Length} bytes");
			}
			string @string = Encoding.UTF8.GetString(array);
			int num = @string.IndexOf("\"atlas_data\":\"");
			if (num < 0)
			{
				if (flag)
				{
					GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-STEP2-FAIL] no atlas_data field found");
				}
				return;
			}
			num += 14;
			int num2 = @string.IndexOf("\",\"", num);
			if (num2 < 0)
			{
				num2 = @string.IndexOf("\"}", num);
			}
			if (num2 < 0)
			{
				if (flag)
				{
					GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-STEP2-FAIL] cannot find atlas_data end");
				}
				return;
			}
			string text = @string.Substring(num, num2 - num);
			string atlasContent = text.Replace("\\n", "\n");
			List<string> list = ExtractPngNamesFromAtlas(atlasContent);
			if (flag)
			{
				GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-STEP2] extracted {list.Count} PNG names: [{string.Join(", ", list)}]");
			}
			if (list.Count == 0)
			{
				if (flag)
				{
					GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-SKIP] no valid png names found");
				}
				return;
			}
			if (_pckFileCache == null)
			{
				_pckFileCache = BuildPckFileCache();
			}
			if (_pckFileCache == null)
			{
				if (flag)
				{
					GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-STEP3-FAIL] PCK cache is null");
				}
				return;
			}
			int num3 = 0;
			int num4 = 0;
			foreach (string item in list)
			{
				try
				{
					if (!_pckFileCache.TryGetValue(item, out string value))
					{
						if (flag)
						{
							GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-STEP3] '{item}' NOT in PCK cache");
						}
						num4++;
						continue;
					}
					if (flag)
					{
						GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-STEP3] cache hit: {value}");
					}
					CompressedTexture2D val = ResourceLoader.Load<CompressedTexture2D>(value, (string)null, (CacheMode)1);
					if (val == null)
					{
						if (flag)
						{
							GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-STEP4] Load NULL for {item}");
						}
						num4++;
						continue;
					}
					Image image = ((Texture2D)val).GetImage();
					if (image == null)
					{
						if (flag)
						{
							GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-STEP5] GetImage NULL for {item}");
						}
						num4++;
						continue;
					}
					string text2 = Path.Combine(targetDir, item);
					image.SavePng(text2);
					if (flag)
					{
						GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-STEP6] SavePng OK: {text2}");
					}
					num3++;
				}
				catch (Exception ex)
				{
					if (flag)
					{
						GD.PrintErr($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-ERR] Failed to export {item}: {ex.Message}");
					}
					num4++;
				}
			}
			if (flag)
			{
				GD.Print($"[CustomSkeletonLoader] [PNG-{_pngCallCount}-DONE] total={list.Count} success={num3} fail={num4}");
			}
		}
		catch (Exception value2)
		{
			if (_pngCallCount <= 5)
			{
				GD.PrintErr($"[CustomSkeletonLoader] [PNG-ERR-{_pngCallCount}] {value2}");
			}
		}
	}

	private static bool FindAndCopy(string originalResPath, string targetDir, string ext)
	{
		if (_pckFileCache == null)
		{
			_pckFileCache = BuildPckFileCache();
		}
		if (_pckFileCache == null || _pckFileCache.Count == 0)
		{
			return false;
		}
		string fileName = Path.GetFileName(originalResPath);
		if (_pckFileCache.TryGetValue(fileName, out string value))
		{
			byte[] array = LoadResBytes(value);
			if (array != null && array.Length != 0)
			{
				string path = Path.GetFileNameWithoutExtension(fileName) + ext;
				File.WriteAllBytes(Path.Combine(targetDir, path), array);
				return true;
			}
		}
		return false;
	}

	private static Dictionary<string, string>? BuildPckFileCache()
	{
		GD.Print("[CustomSkeletonLoader] Building PCK file cache from res://.godot/imported/...");
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		try
		{
			DirAccess val = DirAccess.Open("res://.godot/imported/");
			try
			{
				if (val == null)
				{
					GD.PrintErr("[CustomSkeletonLoader] Cannot open res://.godot/imported/");
					return null;
				}
				ScanImportedDir(val, "res://.godot/imported/", dictionary, 0);
				GD.Print($"[CustomSkeletonLoader] PCK file cache built: {dictionary.Count} files");
				return dictionary;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception value)
		{
			GD.PrintErr($"[CustomSkeletonLoader] BuildPckFileCache error: {value}");
			return null;
		}
	}

	private static void ScanImportedDir(DirAccess da, string currentDir, Dictionary<string, string> dict, int depth)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (depth > 5)
		{
			return;
		}
		da.ListDirBegin();
		string next = da.GetNext();
		while (!string.IsNullOrEmpty(next))
		{
			if (next == "." || next == "..")
			{
				next = da.GetNext();
				continue;
			}
			string text = (currentDir.EndsWith("/") ? (currentDir + next) : (currentDir + "/" + next));
			if (da.CurrentIsDir())
			{
				DirAccess val = DirAccess.Open(text);
				try
				{
					if (val != null)
					{
						ScanImportedDir(val, text, dict, depth + 1);
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else if (next.EndsWith(".spatlas") || next.EndsWith(".spskel") || next.EndsWith(".png") || next.EndsWith(".ctex"))
			{
				string key = (next.Contains('-') ? next.Substring(0, next.LastIndexOf('-')) : next);
				if (!dict.ContainsKey(key))
				{
					dict[key] = text;
				}
			}
			next = da.GetNext();
		}
		da.ListDirEnd();
	}

	private static byte[]? LoadResBytes(string resourcePath)
	{
		try
		{
			byte[] fileAsBytes = FileAccess.GetFileAsBytes(resourcePath);
			if (fileAsBytes != null && fileAsBytes.Length != 0)
			{
				return fileAsBytes;
			}
		}
		catch
		{
		}
		FileAccess val = FileAccess.Open(resourcePath, (ModeFlags)1);
		try
		{
			if (val != null)
			{
				long length = (long)val.GetLength();
				if (length > 0 && length < int.MaxValue)
				{
					return val.GetBuffer((long)(int)length);
				}
			}
			if (resourcePath.StartsWith("res://"))
			{
				string path = ProjectSettings.GlobalizePath(resourcePath);
				if (File.Exists(path))
				{
					return File.ReadAllBytes(path);
				}
			}
			if (File.Exists(resourcePath))
			{
				return File.ReadAllBytes(resourcePath);
			}
			return null;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static ConfigEntry CreateDebugLogEntry()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		return new ConfigEntry
		{
			Key = "enable_debug_logs",
			Type = (ConfigType)0,
			DefaultValue = true,
			Label = "Enable Debug Logs",
			Labels = new Dictionary<string, string>
			{
				["en"] = "Enable Debug Logs",
				["zhs"] = "启用调试日志"
			},
			Description = "Output detailed debug information to the game log.",
			Descriptions = new Dictionary<string, string>
			{
				["en"] = "Output debug info.",
				["zhs"] = "输出调试信息。"
			}
		};
	}

	private static ConfigEntry CreateExportButtonEntry()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		return new ConfigEntry
		{
			Key = "export_skeletons",
			Type = (ConfigType)7,
			DefaultValue = false,
			Label = "Export All Skeletons",
			Labels = new Dictionary<string, string>
			{
				["en"] = "Export All Skeletons",
				["zhs"] = "导出所有骨骼"
			},
			Description = "Scans all Spine atlas/skel/png from game and exports them.",
			Descriptions = new Dictionary<string, string>
			{
				["en"] = "Exports all skeletons.",
				["zhs"] = "导出全部骨骼。"
			},
			ButtonText = "Export",
			ButtonTexts = new Dictionary<string, string>
			{
				["en"] = "Export",
				["zhs"] = "导出"
			},
			OnChanged = delegate
			{
				ExportAllSkeletons();
			}
		};
	}
}
