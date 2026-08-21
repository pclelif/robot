using System;
using System.Collections.Generic;
using UnityEngine;

namespace Robot.Robots.Customization
{
    public class RobotColorCustomizer : MonoBehaviour
    {
        public enum ColorTheme
        {
            SteelBlue = 0,       // Sleek Tech Blue
            StealthCharcoal = 1, // Deep Matte Black / Carbon
            CrimsonRed = 2,      // Rich Deep Red
            EmeraldGreen = 3,    // Tactical Forest Green
            CopperAmber = 4,     // Warm Burnt Copper
            RoyalViolet = 5,     // Deep Satin Purple
            VibrantPink = 6,     // Prominent Vibrant Pink
            HoneyYellow = 7,     // Rich Mustard Honey Yellow
            PearlWhite = 8,      // Pristine Pearl White
            MahoganyBrown = 9    // Warm Chocolate Mahogany Brown
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
        [SerializeField] private ColorTheme activeTheme = ColorTheme.SteelBlue;
        [SerializeField] private EyeColorMode eyeMode = EyeColorMode.SleekBlack;
        [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();

        [Header("Color Presets (10 Rich URP Shading Themes)")]
        [SerializeField]
        private ColorPreset[] presets = new ColorPreset[]
        {
            new ColorPreset 
            { 
                theme = ColorTheme.SteelBlue, 
                name = "Çelik Mavi (Tech Blue)", 
                bodyColor = new Color(0.18f, 0.48f, 0.85f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.StealthCharcoal, 
                name = "Karbon Siyah (Stealth Charcoal)", 
                bodyColor = new Color(0.18f, 0.19f, 0.22f), 
                jointColor = new Color(0.08f, 0.08f, 0.10f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.CrimsonRed, 
                name = "Koyu Kırmızı (Deep Crimson)", 
                bodyColor = new Color(0.82f, 0.15f, 0.18f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.EmeraldGreen, 
                name = "Askeri Yeşil (Tactical Green)", 
                bodyColor = new Color(0.20f, 0.65f, 0.32f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.CopperAmber, 
                name = "Bakır Turuncu (Warm Copper)", 
                bodyColor = new Color(0.88f, 0.42f, 0.15f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.RoyalViolet, 
                name = "Asil Mor (Deep Purple)", 
                bodyColor = new Color(0.55f, 0.20f, 0.78f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.VibrantPink, 
                name = "Canlı Pembe (Vibrant Coral Pink)", 
                bodyColor = new Color(0.95f, 0.28f, 0.55f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.HoneyYellow, 
                name = "Bal Sarısı (Mustard Honey Yellow)", 
                bodyColor = new Color(0.95f, 0.76f, 0.18f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.PearlWhite, 
                name = "İnci Beyazı (Pristine Pearl White)", 
                bodyColor = new Color(0.92f, 0.93f, 0.95f), 
                jointColor = new Color(0.14f, 0.14f, 0.16f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.MahoganyBrown, 
                name = "Maun Kahverengi (Mahogany Brown)", 
                bodyColor = new Color(0.55f, 0.32f, 0.18f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
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

                    if (matName.Contains("M_AtlasOffset") || matName.Contains("Offset")) // Main Robot Armor & Body (Head, Chest, Arms, Legs)
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
