using System;
using System.Collections.Generic;
using UnityEngine;

namespace Robot.Robots.Customization
{
    public class RobotColorCustomizer : MonoBehaviour
    {
        public enum ColorTheme
        {
            CeramicWhite = 0,
            PandaCyan = 1,
            PandaTerracotta = 2,
            PandaMeadowGreen = 3,
            PandaAmberGold = 4,
            PandaSlateDark = 5,
            PandaRoseCoral = 6
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
        [SerializeField] private ColorTheme activeTheme = ColorTheme.CeramicWhite;
        [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();

        [Header("Presets (Matched with Pandazole Low-Poly Palette)")]
        [SerializeField]
        private ColorPreset[] presets = new ColorPreset[]
        {
            new ColorPreset { theme = ColorTheme.CeramicWhite, name = "Panda Ceramic White", primaryColor = new Color(0.97f, 0.98f, 0.98f), emissiveColor = new Color(0f, 0.82f, 0.98f), uvOffset = Vector2.zero },
            new ColorPreset { theme = ColorTheme.PandaCyan, name = "Panda City Cyan", primaryColor = new Color(0.23f, 0.69f, 0.66f), emissiveColor = new Color(0f, 0.95f, 0.95f), uvOffset = new Vector2(0.205f, 0.03125f) },
            new ColorPreset { theme = ColorTheme.PandaTerracotta, name = "Panda Terracotta Orange", primaryColor = new Color(0.88f, 0.34f, 0.22f), emissiveColor = new Color(1f, 0.42f, 0.25f), uvOffset = new Vector2(0.41f, 0.0625f) },
            new ColorPreset { theme = ColorTheme.PandaMeadowGreen, name = "Panda Meadow Green", primaryColor = new Color(0.31f, 0.62f, 0.24f), emissiveColor = new Color(0.46f, 0.73f, 0.11f), uvOffset = new Vector2(0f, 0.09375f) },
            new ColorPreset { theme = ColorTheme.PandaAmberGold, name = "Panda Amber Gold", primaryColor = new Color(0.96f, 0.64f, 0.38f), emissiveColor = new Color(1f, 0.76f, 0.15f), uvOffset = new Vector2(0.205f, 0.125f) },
            new ColorPreset { theme = ColorTheme.PandaSlateDark, name = "Panda Industrial Slate", primaryColor = new Color(0.17f, 0.18f, 0.26f), emissiveColor = new Color(0f, 0.68f, 0.71f), uvOffset = new Vector2(0.41f, 0.15625f) },
            new ColorPreset { theme = ColorTheme.PandaRoseCoral, name = "Panda Rose Coral", primaryColor = new Color(0.90f, 0.22f, 0.27f), emissiveColor = new Color(1f, 0.3f, 0.43f), uvOffset = new Vector2(0f, 0.1875f) }
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
