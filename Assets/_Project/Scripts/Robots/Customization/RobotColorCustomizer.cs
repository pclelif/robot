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
            BoldRed = 1,         // Kırmızı (Rich Bold Red - Non-pastel)
            VividOrange = 2,     // Turuncu (Vivid Saturated Orange - Non-pastel)
            HoneyYellow = 3,     // Sarı (Bright Honey Yellow)
            KhakiGreen = 4,      // Yeşil (Tactical Olive Khaki Green)
            TechBlue = 5,        // Mavi (Crisp Tech Blue)
            RoyalPurple = 6,     // Mor (Deep Royal Purple)
            VibrantPink = 7,     // Pembe (Vibrant Coral Pink)
            EspressoBrown = 8,   // Kahverengi (Deep Rich Espresso Brown)
            WarmCream = 9        // Beyaz (Warm Cream White)
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

        [Header("Color Presets (Custom User Sequence: Siyah -> Kırmızı -> Turuncu -> Sarı -> Yeşil -> Mavi -> Mor -> Pembe -> Kahverengi -> Krem Beyazı)")]
        [SerializeField]
        private ColorPreset[] presets = new ColorPreset[]
        {
            new ColorPreset 
            { 
                theme = ColorTheme.StealthBlack, 
                name = "Karbon Siyah (Stealth Black)", 
                bodyColor = new Color(0.14f, 0.15f, 0.17f), // Deep matte black
                jointColor = new Color(0.08f, 0.08f, 0.10f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.BoldRed, 
                name = "Canlı Kırmızı (Bold Ruby Red)", 
                bodyColor = new Color(0.88f, 0.08f, 0.12f), // Saturated deep bold red (non-pastel)
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.VividOrange, 
                name = "Canlı Turuncu (Vivid Citrus Orange)", 
                bodyColor = new Color(0.96f, 0.42f, 0.02f), // Saturated vivid citrus orange (non-pastel)
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.HoneyYellow, 
                name = "Bal Sarısı (Bright Honey Yellow)", 
                bodyColor = new Color(0.96f, 0.78f, 0.15f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.KhakiGreen, 
                name = "Haki Yeşil (Tactical Olive Khaki Green)", 
                bodyColor = new Color(0.32f, 0.45f, 0.26f), // Tactical khaki / olive green
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.TechBlue, 
                name = "Çelik Mavi (Crisp Tech Blue)", 
                bodyColor = new Color(0.18f, 0.48f, 0.85f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.RoyalPurple, 
                name = "Asil Mor (Deep Royal Purple)", 
                bodyColor = new Color(0.55f, 0.20f, 0.78f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.VibrantPink, 
                name = "Canlı Pembe (Vibrant Coral Pink)", 
                bodyColor = new Color(0.95f, 0.40f, 0.58f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.EspressoBrown, 
                name = "Çikolata Kahve (Deep Espresso Brown)", 
                bodyColor = new Color(0.42f, 0.22f, 0.10f), 
                jointColor = new Color(0.12f, 0.12f, 0.14f)
            },
            new ColorPreset 
            { 
                theme = ColorTheme.WarmCream, 
                name = "Krem Beyazı (Warm Cream White)", 
                bodyColor = new Color(0.94f, 0.91f, 0.84f), // Warm cream white
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
