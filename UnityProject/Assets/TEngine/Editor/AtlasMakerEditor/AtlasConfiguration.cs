﻿namespace TEngine.Editor
{
#if UNITY_EDITOR
    using UnityEngine;
    using UnityEditor;

    [HybridCLR.Editor.Settings.FilePath("ProjectSettings/AtlasConfiguration.asset")]
    public class AtlasConfiguration : HybridCLR.Editor.Settings.ScriptableSingleton<AtlasConfiguration>
    {
        [Header("目录设置")]
        [Tooltip("生成的图集输出目录")]
        public string outputAtlasDir = "Assets/AssetArt/Atlas";

        [Tooltip("需要生成图集的UI根目录")]
        public string sourceAtlasRoot = "Assets/AssetRaw/UIRaw/Atlas";

        [Tooltip("不需要生成图集的UI目录")]
        public string excludeFolder = "Assets/AssetRaw/UIRaw/Raw";

        [Header("平台格式设置")]
        public TextureImporterFormat androidFormat = TextureImporterFormat.ASTC_6x6;

        public TextureImporterFormat iosFormat = TextureImporterFormat.ASTC_5x5;
        public TextureImporterFormat webglFormat = TextureImporterFormat.ASTC_6x6;

        [Header("PackingSetting")]
        public int padding = 2;

        public bool enableRotation = true;
        public int blockOffset = 1;
        public bool tightPacking = true;

        [Header("其他设置")]
        [Range(0, 100)]
        public int compressionQuality = 50;

        public bool autoGenerate = true;
        public bool enableLogging = true;
        public bool enableV2 = true;

        [Header("排除关键词")]
        public string[] excludeKeywords = { "_Delete", "_Temp" };
    }

#endif
}