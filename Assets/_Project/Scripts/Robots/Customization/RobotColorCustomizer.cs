using System;
using System.Collections.Generic;
using UnityEngine;

namespace Robot.Robots.Customization
{
    public class RobotColorCustomizer : MonoBehaviour
    {
        public enum ColorTheme
        {
            Red = 0,
            Orange = 1,
            Yellow = 2,
            Green = 3,
            Blue = 4,
            Purple = 5,
            Pink = 6
        }

        [Serializable]
        public struct ColorPreset
        {
            public ColorTheme theme;
            public string name;
            public Color primaryColor;
            public Color emissiveColor;
            public Vector2 uvOffset;
        }

        [Header("Configuration")]
        [SerializeField] private ColorTheme activeTheme = ColorTheme.Red;
        [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();

        [Header("Presets (Pandazole-Harmonized Multiplayer Colors)")]
        [SerializeField]
        private ColorPreset[] presets = new ColorPreset[]
        {
            new ColorPreset { theme = ColorTheme.Red, name = "Pandazole Kırmızı (Terracotta Red)", primaryColor = new Color(0.85f, 0.22f, 0.23f), emissiveColor = new Color(1.0f, 0.36f, 0.37f), uvOffset = Vector2.zero },
            new ColorPreset { theme = ColorTheme.Orange, name = "Pandazole Turuncu (Warm Amber)", primaryColor = new Color(0.91f, 0.44f, 0.32f), emissiveColor = new Color(0.96f, 0.64f, 0.38f), uvOffset = new Vector2(0.205f, 0.03125f) },
            new ColorPreset { theme = ColorTheme.Yellow, name = "Pandazole Sarı (Mustard Gold)", primaryColor = new Color(0.91f, 0.77f, 0.41f), emissiveColor = new Color(0.96f, 0.85f, 0.45f), uvOffset = new Vector2(0.41f, 0.0625f) },
            new ColorPreset { theme = ColorTheme.Green, name = "Pandazole Yeşil (Meadow Green)", primaryColor = new Color(0.31f, 0.62f, 0.35f), emissiveColor = new Color(0.46f, 0.78f, 0.40f), uvOffset = new Vector2(0f, 0.09375f) },
            new ColorPreset { theme = ColorTheme.Blue, name = "Pandazole Mavi (Coastal Teal/Blue)", primaryColor = new Color(0.20f, 0.58f, 0.68f), emissiveColor = new Color(0.30f, 0.78f, 0.88f), uvOffset = new Vector2(0.205f, 0.125f) },
            new ColorPreset { theme = ColorTheme.Purple, name = "Pandazole Mor (Lowpoly Berry)", primaryColor = new Color(0.55f, 0.25f, 0.68f), emissiveColor = new Color(0.71f, 0.38f, 0.85f), uvOffset = new Vector2(0.41f, 0.15625f) },
            new ColorPreset { theme = ColorTheme.Pink, name = "Pandazole Pembe (Coral Rose)", primaryColor = new Color(0.92f, 0.42f, 0.55f), emissiveColor = new Color(1.0f, 0.55f, 0.68f), uvOffset = new Vector2(0f, 0.1875f) }
        };

        private MaterialPropertyBlock propBlock;
        private static readonly int BaseColorHash = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorHash = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorHash = Shader.PropertyToID("_EmissionColor");
        private static readonly int UvOffsetHash = Shader.PropertyToID("_UV_Offset");

        public ColorTheme ActiveTheme => activeTheme;

        private void Awake()
        {
            propBlock = new MaterialPropertyBlock();
            if (targetRenderers.Count == 0)
            {
                GetComponentsInChildren(true, targetRenderers);
            }
        }

        private void Start()
        {
            ApplyTheme(activeTheme);
        }

        public void ApplyTheme(ColorTheme theme)
        {
            activeTheme = theme;
            ColorPreset preset = GetPreset(theme);

            foreach (Renderer ren in targetRenderers)
            {
                if (ren == null) continue;

                ren.GetPropertyBlock(propBlock);
                propBlock.SetColor(BaseColorHash, preset.primaryColor);
                propBlock.SetColor(ColorHash, preset.primaryColor);
                propBlock.SetColor(EmissionColorHash, preset.emissiveColor);
                propBlock.SetVector(UvOffsetHash, preset.uvOffset);
                ren.SetPropertyBlock(propBlock);

                // Also update instances if standard material property offsets are used
                foreach (Material mat in ren.materials)
                {
                    if (mat.HasProperty("_UV_Offset"))
                    {
                        mat.SetVector("_UV_Offset", preset.uvOffset);
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
