using System;
using System.Collections.Generic;
using UnityEngine;

namespace Robot.Robots.Customization
{
    public class RobotColorCustomizer : MonoBehaviour
    {
        public enum ColorTheme
        {
            CrimsonRed = 0,      // Pure Striking Ruby Red (DEFAULT INITIAL THEME)
            PearlWhite = 1,      // Pristine Pearl White (Lightest)
            HoneyYellow = 2,     // Bright Honey Yellow
            CoralPink = 3,       // Bright Vibrant Coral Pink
            CopperAmber = 4,     // Pure Bright Tangerine Orange (Distinct from Red & Brown)
            EmeraldGreen = 5,    // Tactical Forest Green
            SteelBlue = 6,       // Crisp Tech Blue
            RoyalViolet = 7,     // Deep Royal Purple
            MahoganyBrown = 8,   // Deep Rich Chocolate Brown (Distinct from Red & Orange)
            StealthCharcoal = 9  // Deep Stealth Charcoal (Darkest)
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
        [SerializeField] private ColorTheme activeTheme = ColorTheme.CrimsonRed; // Starts Red as requested!
        [SerializeField] private EyeColorMode eyeMode = EyeColorMode.SleekBlack;
        [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();

        [Header("Color Presets (10 Distinct High-Contrast Themes)")]
        [SerializeField]
        private ColorPreset[] presets = new ColorPreset[]
        {
            new ColorPreset 
            { 
                theme = ColorTheme.CrimsonRed, 
                name = "Koyu Kırmızı (Pure Ruby Red)", 
                bodyColor = new Color(0.92f, 0.10f, 0.14f), // Striking Pure Ruby Red
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.PearlWhite, 
                name = "İnci Beyazı (Pristine Pearl White)", 
                bodyColor = new Color(0.94f, 0.95f, 0.96f), 
                jointColor = new Color(0.14f, 0.14f, 0.16f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.HoneyYellow, 
                name = "Bal Sarısı (Bright Honey Yellow)", 
                bodyColor = new Color(0.96f, 0.82f, 0.20f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.CoralPink, 
                name = "Mercan Pembesi (Vibrant Coral Pink)", 
                bodyColor = new Color(0.96f, 0.45f, 0.62f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.CopperAmber, 
                name = "Canlı Turuncu (Bright Tangerine Orange)", 
                bodyColor = new Color(0.98f, 0.52f, 0.05f), // Distinct Citrus Orange
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.EmeraldGreen, 
                name = "Askeri Yeşil (Tactical Emerald Green)", 
                bodyColor = new Color(0.22f, 0.68f, 0.35f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.SteelBlue, 
                name = "Çelik Mavi (Crisp Steel Blue)", 
                bodyColor = new Color(0.20f, 0.52f, 0.88f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.RoyalViolet, 
                name = "Asil Mor (Deep Royal Purple)", 
                bodyColor = new Color(0.58f, 0.24f, 0.82f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.MahoganyBrown, 
                name = "Çikolata Kahve (Deep Espresso Brown)", 
                bodyColor = new Color(0.42f, 0.22f, 0.10f), // Distinct Dark Espresso Chocolate
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.StealthCharcoal, 
                name = "Karbon Siyah (Stealth Charcoal Black)", 
                bodyColor = new Color(0.18f, 0.19f, 0.22f), 
                jointColor = new Color(0.08f, 0.08f, 0.10f)
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
