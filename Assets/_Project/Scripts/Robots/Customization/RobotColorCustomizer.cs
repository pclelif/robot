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

        [Header("Presets (Vibrant Multiplayer Colors)")]
        [SerializeField]
        private ColorPreset[] presets = new ColorPreset[]
        {
            new ColorPreset { theme = ColorTheme.Red, name = "Kırmızı (Neon Red)", primaryColor = new Color(0.95f, 0.10f, 0.15f), emissiveColor = new Color(1.0f, 0.20f, 0.25f), uvOffset = Vector2.zero },
            new ColorPreset { theme = ColorTheme.Orange, name = "Turuncu (Sunset Orange)", primaryColor = new Color(1.0f, 0.45f, 0.0f), emissiveColor = new Color(1.0f, 0.55f, 0.1f), uvOffset = new Vector2(0.205f, 0.03125f) },
            new ColorPreset { theme = ColorTheme.Yellow, name = "Sarı (Cyber Yellow)", primaryColor = new Color(1.0f, 0.85f, 0.0f), emissiveColor = new Color(1.0f, 0.93f, 0.1f), uvOffset = new Vector2(0.41f, 0.0625f) },
            new ColorPreset { theme = ColorTheme.Green, name = "Yeşil (Electric Green)", primaryColor = new Color(0.10f, 0.85f, 0.25f), emissiveColor = new Color(0.20f, 1.0f, 0.35f), uvOffset = new Vector2(0f, 0.09375f) },
            new ColorPreset { theme = ColorTheme.Blue, name = "Mavi (Cobalt Blue)", primaryColor = new Color(0.05f, 0.50f, 1.0f), emissiveColor = new Color(0.0f, 0.75f, 1.0f), uvOffset = new Vector2(0.205f, 0.125f) },
            new ColorPreset { theme = ColorTheme.Purple, name = "Mor (Deep Purple)", primaryColor = new Color(0.65f, 0.10f, 0.95f), emissiveColor = new Color(0.75f, 0.20f, 1.0f), uvOffset = new Vector2(0.41f, 0.15625f) },
            new ColorPreset { theme = ColorTheme.Pink, name = "Pembe (Hot Pink)", primaryColor = new Color(1.0f, 0.20f, 0.65f), emissiveColor = new Color(1.0f, 0.40f, 0.80f), uvOffset = new Vector2(0f, 0.1875f) }
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
