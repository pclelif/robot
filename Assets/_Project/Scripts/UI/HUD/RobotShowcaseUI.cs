using UnityEngine;
using Robot.Robots.Customization;
using Robot.Player.Movement;

namespace Robot.UI.HUD
{
    /// <summary>
    /// Test UI script that lets you press Play in Unity and interactively test movement and color themes.
    /// </summary>
    public class RobotShowcaseUI : MonoBehaviour
    {
        [SerializeField] private RobotColorCustomizer colorCustomizer;
        [SerializeField] private KeyCode changeColorKey = KeyCode.C;

        private void Start()
        {
            if (colorCustomizer == null)
            {
                colorCustomizer = FindFirstObjectByType<RobotColorCustomizer>();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(changeColorKey))
            {
                if (colorCustomizer != null)
                {
                    colorCustomizer.NextTheme();
                }
            }
        }

        private void OnGUI()
        {
            // Simple On-Screen Test Panel
            GUILayout.BeginArea(new Rect(20, 20, 320, 280), GUI.skin.box);
            
            GUILayout.Label("<b>🤖 ROBOT TEST & GÖSTERİM</b>", GetHeaderStyle());
            GUILayout.Space(10);

            GUILayout.Label("🎮 <b>WASD / Ok Tuşları:</b> Hareket Et");
            GUILayout.Label("🏃 <b>Shift:</b> Koş");
            GUILayout.Label("🎥 <b>Fare Hareketi:</b> Kamera Döndür");
            GUILayout.Label("🎨 <b>'C' Tuşu:</b> Renk Değiştir");
            GUILayout.Space(15);

            string activeThemeName = colorCustomizer != null ? colorCustomizer.ActiveTheme.ToString() : "N/A";
            GUILayout.Label($"<b>Aktif Renk:</b> <color=cyan>{activeThemeName}</color>", GetBodyStyle());
            GUILayout.Space(10);

            if (GUILayout.Button("🎨 RENK DEĞİŞTİR (C)", GUILayout.Height(40)))
            {
                if (colorCustomizer != null)
                {
                    colorCustomizer.NextTheme();
                }
            }

            GUILayout.EndArea();
        }

        private GUIStyle GetHeaderStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 16;
            style.alignment = TextAnchor.MiddleCenter;
            style.richText = true;
            return style;
        }

        private GUIStyle GetBodyStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 13;
            style.richText = true;
            return style;
        }
    }
}
