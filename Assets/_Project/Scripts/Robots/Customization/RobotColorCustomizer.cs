using System;
using System.Collections.Generic;
using UnityEngine;

namespace Robot.Robots.Customization
{
    public class RobotColorCustomizer : MonoBehaviour
    {
        public enum ColorTheme
        {
            StealthBlack = 0,    // Siyah (DEFAULT INITIAL THEME)
            CrimsonRed = 1,      // Kırmızı (Harmonized Satin Crimson)
            SunsetOrange = 2,    // Turuncu (Harmonized Sunset Orange)
            GoldYellow = 3,      // Sarı (Harmonized Gold Yellow)
            SageGreen = 4,       // Yeşil (Harmonized Sage Haki Green)
            CobaltBlue = 5,      // Mavi (Harmonized Cobalt Blue)
            VelvetPurple = 6,    // Mor (Harmonized Velvet Purple)
            CoralPink = 7,       // Pembe (Harmonized Coral Rose Pink)
            MochaBrown = 8,      // Kahverengi (Harmonized Mocha Brown)
            IvoryCream = 9       // Krem Beyazı (Harmonized Ivory Cream)
        }

        public enum EyeColorMode
        {
            SleekBlack = 0,      // Sleek Glossy Black Eyes
            MatchTheme = 1       // Match Robot Body Theme Color
        }

        [Serializable]
        public struct ColorPreset
        {
            public ColorTheme theme;
            public string name;
            public Color bodyColor;
            public Color jointColor;
        }

        [Header("Configuration")]
        [SerializeField] private ColorTheme activeTheme = ColorTheme.StealthBlack; // Starts Black as requested!
        [SerializeField] private EyeColorMode eyeMode = EyeColorMode.SleekBlack;
        [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();

        [Header("Color Presets (Harmonized Satin Palette - 65% Saturation / 75% Value Scale)")]
        [SerializeField]
        private ColorPreset[] presets = new ColorPreset[]
        {
            new ColorPreset 
            { 
                theme = ColorTheme.StealthBlack, 
                name = "Karbon Siyah (Stealth Black)", 
                bodyColor = new Color(0.16f, 0.17f, 0.20f), // Sleek matte carbon
                jointColor = new Color(0.09f, 0.09f, 0.11f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.CrimsonRed, 
                name = "Koyu Kırmızı (Harmonized Satin Crimson)", 
                bodyColor = new Color(0.82f, 0.22f, 0.24f), // Harmonized Satin Crimson
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.SunsetOrange, 
                name = "Canlı Turuncu (Harmonized Sunset Orange)", 
                bodyColor = new Color(0.88f, 0.42f, 0.18f), // Harmonized Sunset Amber Orange
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.GoldYellow, 
                name = "Bal Sarısı (Harmonized Gold Yellow)", 
                bodyColor = new Color(0.88f, 0.68f, 0.20f), // Harmonized Mustard Gold
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.SageGreen, 
                name = "Haki Yeşil (Harmonized Sage Khaki Green)", 
                bodyColor = new Color(0.32f, 0.55f, 0.35f), // Harmonized Sage Khaki
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.CobaltBlue, 
                name = "Çelik Mavi (Harmonized Cobalt Blue)", 
                bodyColor = new Color(0.20f, 0.48f, 0.78f), // Harmonized Ocean Cobalt
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.VelvetPurple, 
                name = "Asil Mor (Harmonized Velvet Purple)", 
                bodyColor = new Color(0.52f, 0.28f, 0.75f), // Harmonized Velvet Purple
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.CoralPink, 
                name = "Canlı Pembe (Harmonized Coral Rose Pink)", 
                bodyColor = new Color(0.88f, 0.35f, 0.52f), // Harmonized Coral Rose
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.MochaBrown, 
                name = "Çikolata Kahve (Harmonized Mocha Brown)", 
                bodyColor = new Color(0.48f, 0.30f, 0.20f), // Harmonized Mocha Brown
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.IvoryCream, 
                name = "Krem Beyazı (Harmonized Ivory Cream)", 
                bodyColor = new Color(0.90f, 0.86f, 0.78f), // Harmonized Warm Ivory Cream
                jointColor = new Color(0.14f, 0.14f, 0.16f)
            }
        };

        private static readonly int BaseColorHash = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorHash = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorHash = Shader.PropertyToID("_EmissionColor");
        private static readonly int SmoothnessHash = Shader.PropertyToID("_Smoothness");
        private static readonly int MetallicHash = Shader.PropertyToID("_Metallic");

        public ColorTheme ActiveTheme => activeTheme;

        private void Awake()
        {
            if (targetRenderers.Count == 0)
            {
                GetComponentsInChildren(true, targetRenderers);
            }
        }

        private void Start()
        {
            EnsureUrpMaterials();
            ApplyTheme(activeTheme);
        }

        private void EnsureUrpMaterials()
        {
            Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLitShader == null) urpLitShader = Shader.Find("Standard");

            if (urpLitShader == null) return;

            foreach (Renderer ren in targetRenderers)
            {
                if (ren == null) continue;

                Material[] mats = ren.materials; // Access instance materials
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null && mats[i].shader != urpLitShader)
                    {
                        mats[i].shader = urpLitShader;
                    }
                }
            }
        }

        public void ApplyTheme(ColorTheme theme)
        {
            activeTheme = theme;
            ColorPreset preset = GetPreset(theme);

            Color eyesColor = eyeMode == EyeColorMode.SleekBlack ? new Color(0.04f, 0.04f, 0.05f) : preset.bodyColor;

            foreach (Renderer ren in targetRenderers)
            {
                if (ren == null) continue;

                Material[] mats = ren.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Material mat = mats[i];
                    if (mat == null) continue;

                    string matName = mat.name;

                    if (matName.Contains("M_AtlasOffset") || matName.Contains("Offset")) // Main Robot Armor & Body
                    {
                        mat.SetColor(BaseColorHash, preset.bodyColor);
                        mat.SetColor(ColorHash, preset.bodyColor);
                        if (mat.HasProperty(SmoothnessHash)) mat.SetFloat(SmoothnessHash, 0.35f);
                        if (mat.HasProperty(MetallicHash)) mat.SetFloat(MetallicHash, 0.15f);
                    }
                    else if (matName.Contains("M_AtlasBase") || matName.Contains("Base")) // Joint Connections
                    {
                        mat.SetColor(BaseColorHash, preset.jointColor);
                        mat.SetColor(ColorHash, preset.jointColor);
                        if (mat.HasProperty(SmoothnessHash)) mat.SetFloat(SmoothnessHash, 0.5f);
                        if (mat.HasProperty(MetallicHash)) mat.SetFloat(MetallicHash, 0.7f);
                    }
                    else if (matName.Contains("M_AtlasEmissive") || matName.Contains("Emissive")) // Eyes (Sleek Black)
                    {
                        mat.SetColor(BaseColorHash, eyesColor);
                        mat.SetColor(ColorHash, eyesColor);
                        if (mat.HasProperty(EmissionColorHash)) mat.SetColor(EmissionColorHash, Color.black);
                        if (mat.HasProperty(SmoothnessHash)) mat.SetFloat(SmoothnessHash, 0.9f);
                    }
                    else
                    {
                        mat.SetColor(BaseColorHash, preset.bodyColor);
                        mat.SetColor(ColorHash, preset.bodyColor);
                    }
                }
            }
        }

        public void NextTheme()
        {
            int totalThemes = Enum.GetValues(typeof(ColorTheme)).Length;
            int nextIndex = ((int)activeTheme + 1) % totalThemes;
            ApplyTheme((ColorTheme)nextIndex);
        }

        private ColorPreset GetPreset(ColorTheme theme)
        {
            foreach (var p in presets)
            {
                if (p.theme == theme) return p;
            }
            return presets[0];
        }
    }
}
