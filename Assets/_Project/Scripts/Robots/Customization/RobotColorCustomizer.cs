using System;
using System.Collections.Generic;
using UnityEngine;

namespace Robot.Robots.Customization
{
    public class RobotColorCustomizer : MonoBehaviour
    {
        public enum ColorTheme
        {
            Default = 0,
            CrimsonRed = 1,
            CobaltBlue = 2,
            EmeraldGreen = 3,
            ElectricYellow = 4,
            StealthBlack = 5,
            CyberPurple = 6
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
        [SerializeField] private ColorTheme activeTheme = ColorTheme.Default;
        [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();

        [Header("Presets")]
        [SerializeField]
        private ColorPreset[] presets = new ColorPreset[]
        {
            new ColorPreset { theme = ColorTheme.Default, name = "Default White", primaryColor = Color.white, emissiveColor = new Color(0f, 0.8f, 1f), uvOffset = Vector2.zero },
            new ColorPreset { theme = ColorTheme.CrimsonRed, name = "Crimson Red", primaryColor = new Color(0.9f, 0.15f, 0.15f), emissiveColor = new Color(1f, 0.2f, 0f), uvOffset = new Vector2(0.205f, 0.03125f) },
            new ColorPreset { theme = ColorTheme.CobaltBlue, name = "Cobalt Blue", primaryColor = new Color(0.15f, 0.45f, 0.95f), emissiveColor = new Color(0f, 0.9f, 1f), uvOffset = new Vector2(0.41f, 0.0625f) },
            new ColorPreset { theme = ColorTheme.EmeraldGreen, name = "Emerald Green", primaryColor = new Color(0.15f, 0.85f, 0.35f), emissiveColor = new Color(0.2f, 1f, 0.4f), uvOffset = new Vector2(0f, 0.09375f) },
            new ColorPreset { theme = ColorTheme.ElectricYellow, name = "Electric Yellow", primaryColor = new Color(0.95f, 0.85f, 0.1f), emissiveColor = new Color(1f, 0.9f, 0.1f), uvOffset = new Vector2(0.205f, 0.125f) },
            new ColorPreset { theme = ColorTheme.StealthBlack, name = "Stealth Black", primaryColor = new Color(0.15f, 0.15f, 0.18f), emissiveColor = new Color(1f, 0.3f, 0f), uvOffset = new Vector2(0.41f, 0.15625f) },
            new ColorPreset { theme = ColorTheme.CyberPurple, name = "Cyber Purple", primaryColor = new Color(0.6f, 0.15f, 0.85f), emissiveColor = new Color(0.9f, 0.2f, 1f), uvOffset = new Vector2(0f, 0.1875f) }
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
