// 受保护兼容模块：贞德的每档调色页。启动时同身份 SFW 包可能只暴露 s1。
using System.Collections;
using System.Reflection;
using System.Text;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class StagePalettePages
{
    internal static void Ensure(ProviderSettingsVariantOptions.SettingsVariantContract selected, Type palette, string? skeletonPath)
    {
        if (!selected.AssemblyName.Equals("JoanofArcIroncladHpwear", StringComparison.OrdinalIgnoreCase)) return;
        var key = System.IO.Path.GetFileNameWithoutExtension(skeletonPath);
        if (key is not ("s1_charselect" or "s2_charselect" or "s3_charselect")) return;
        if (AccessTools.Field(palette, "Pages")?.GetValue(null) is not IDictionary pages || pages.Contains(key)) return;
        var pageType = palette.GetNestedType("Page", BindingFlags.NonPublic);
        if (pageType == null) throw new InvalidOperationException("提供者调色页结构不可用。");
        RegisterMissing(pages, pageType, key, () => BuildMaterial(selected, palette, key));
    }

    internal static bool RegisterMissing(IDictionary pages, Type pageType, string key, Func<ShaderMaterial> create)
    {
        if (pages.Contains(key)) return false;
        var page = Activator.CreateInstance(pageType, nonPublic: true)!;
        var keyField = AccessTools.Field(pageType, "Key") ?? throw new InvalidOperationException("调色页缺少 Key。");
        var materialField = AccessTools.Field(pageType, "Material") ?? throw new InvalidOperationException("调色页缺少 Material。");
        var material = create();
        keyField.SetValue(page, key);
        materialField.SetValue(page, material);
        pages.Add(key, page); // Native SetWeights/Tick now includes this stage in every variant.
        return true;
    }

    private static ShaderMaterial BuildMaterial(ProviderSettingsVariantOptions.SettingsVariantContract selected, Type palette, string key)
    {
        byte[] Read(string relative) => SkinService.Catalog?.ReadProviderFile("ironclad", selected.ProviderId, "palette/" + relative)
            ?? throw new InvalidOperationException("所选包缺少战损调色资源：" + relative);
        var files = new Dictionary<string, byte[]>
        {
            ["index.png"] = Read(key + "/index.png"), ["1.png"] = Read(key + "/1.png"),
            ["2.png"] = Read(key + "/2.png"), ["3.png"] = Read(key + "/3.png")
        };
        RuntimeLoadSafety.RequireHeadroom(RuntimeLoadSafety.EstimateTextureBytes(files));
        if (AccessTools.Field(palette, "_weights")?.GetValue(null) is not Vector4 weights)
            throw new InvalidOperationException("无法读取当前变体调色权重。");
        var material = new ShaderMaterial { Shader = new Shader { Code = Encoding.UTF8.GetString(Read("shader.txt")) } };
        try
        {
            foreach (var pair in files)
            {
                using var image = new Image();
                if (image.LoadPngFromBuffer(pair.Value) != Error.Ok) throw new InvalidDataException("战损调色图片无效：" + pair.Key);
                var texture = ImageTexture.CreateFromImage(image);
                using Variant parameter = texture;
                material.SetShaderParameter(pair.Key == "index.png" ? "pixel_indices" : "palette_" + pair.Key[0], parameter);
            }
            // Character-select pages are non-emissive in the author's BuildPage contract.
            using Variant weightsValue = weights;
            material.SetShaderParameter("appearance_weights", weightsValue);
            using Variant emission = 0f;
            material.SetShaderParameter("emission", emission);
            ModLog.Info("已从所选包补齐战损调色页：" + selected.ProviderId + "/" + key);
            return material;
        }
        catch
        {
            material.Dispose(); // Unpublished only: no live node/page can reference it yet.
            throw;
        }
    }
}
