using UnityEngine;
using Robot.Robots.Customization;
using Robot.Player.Movement;

namespace Robot.UI.HUD
{
    /// <summary>
    /// Interactive Test UI script for Play Mode testing of movement and color themes.
    /// </summary>
    public class RobotShowcaseUI : MonoBehaviour
    {
        [SerializeField] private RobotColorCustomizer colorCustomizer;
        [SerializeField] private KeyCode changeColorKey = KeyCode.C;

        private GUIStyle headerStyle;
        private GUIStyle bodyStyle;

        private void Start()
        {
            EnsureCustomizerReference();
        }

        private void EnsureCustomizerReference()
        {
            if (colorCustomizer == null)
            {
                colorCustomizer = GetComponent<RobotColorCustomizer>();
                if (colorCustomizer == null)
                {
                    colorCustomizer = FindFirstObjectByType<RobotColorCustomizer>();
                }
            }
        }

        private void Update()
        {
            if (IsChangeColorKeyPressed())
            {
                TriggerNextColor();
            }
        }

        public void TriggerNextColor()
        {
            EnsureCustomizerReference();
            if (colorCustomizer != null)
            {
                colorCustomizer.NextTheme();
            }
            else
            {
                Debug.LogWarning("[RobotShowcaseUI] RobotColorCustomizer component not found!");
            }
        }

        private bool IsChangeColorKeyPressed()
        {
            // 1. Direct legacy KeyCode.C check
            try
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.C)) return true;
            }
            catch { }

            // 2. New Input System check
#if ENABLE_INPUT_SYSTEM
            try
            {
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                if (keyboard != null && keyboard.cKey.wasPressedThisFrame) return true;
            }
            catch { }
#endif

            return false;
        }

        private void OnGUI()
        {
            // On-Screen Control & Status Box
            GUILayout.BeginArea(new Rect(20, 20, 340, 260), GUI.skin.box);
            
            GUILayout.Label("<b>🤖 ROBOT TEST & RENK KONTROLÜ</b>", GetHeaderStyle());
            GUILayout.Space(8);

            GUILayout.Label("🎮 <b>WASD / Ok Tuşları:</b> Yürü");
            GUILayout.Label("🏃 <b>Shift:</b> Koş");
            GUILayout.Label("🎥 <b>Fare (Sağ Tık / Sürükle):</b> Kamera Döndür");
            GUILayout.Label("🎨 <b>'C' Tuşu:</b> Renk Değiştir");
            GUILayout.Space(12);

            string activeThemeName = colorCustomizer != null ? colorCustomizer.ActiveThemeName : "Yükleniyor...";
            GUILayout.Label($"<b>Aktif Renk Teması:</b> <color=cyan>{activeThemeName}</color>", GetBodyStyle());
            GUILayout.Space(10);

            if (GUILayout.Button("🎨 RENK DEĞİŞTİR ('C' Tuşu)", GUILayout.Height(45)))
            {
                TriggerNextColor();
            }

            GUILayout.EndArea();
        }

        private GUIStyle GetHeaderStyle()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    alignment = TextAnchor.MiddleCenter,
                    richText = true
                };
            }
            return headerStyle;
        }

        private GUIStyle GetBodyStyle()
        {
            if (bodyStyle == null)
            {
                bodyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    richText = true
                };
            }
            return bodyStyle;
        }
    }
}
