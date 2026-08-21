using UnityEditor;
using UnityEngine;
using Robot.Player.Movement;
using Robot.Player.CameraControl;
using Robot.Robots.Customization;
using Robot.UI.HUD;

namespace Robot.Editor
{
    public static class SetupRobotPlayer
    {
        [MenuItem("Tools/Robot/Setup Active Scene Player & Camera")]
        public static void SetupPlayerInActiveScene()
        {
            // 1. Find or setup Player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("RobotPlayer");
            }

            if (player != null)
            {
                player.tag = "Player";

                // Setup CharacterController
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc == null)
                {
                    cc = player.AddComponent<CharacterController>();
                }
                cc.center = new Vector3(0f, 0.9f, 0f);
                cc.radius = 0.35f;
                cc.height = 1.8f;

                // Setup RobotMovementController
                RobotMovementController movement = player.GetComponent<RobotMovementController>();
                if (movement == null)
                {
                    movement = player.AddComponent<RobotMovementController>();
                }

                // Setup RobotColorCustomizer
                RobotColorCustomizer customizer = player.GetComponent<RobotColorCustomizer>();
                if (customizer == null)
                {
                    customizer = player.AddComponent<RobotColorCustomizer>();
                }

                // Setup Showcase UI
                RobotShowcaseUI showcaseUI = player.GetComponent<RobotShowcaseUI>();
                if (showcaseUI == null)
                {
                    showcaseUI = player.AddComponent<RobotShowcaseUI>();
                }

                Debug.Log($"[RobotSetup] Successfully configured player components on '{player.name}'.");
            }
            else
            {
                Debug.LogWarning("[RobotSetup] Could not find 'RobotPlayer' in active scene.");
            }

            // 2. Find or setup Main Camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                ThirdPersonCamera tpc = mainCam.GetComponent<ThirdPersonCamera>();
                if (tpc == null)
                {
                    tpc = mainCam.gameObject.AddComponent<ThirdPersonCamera>();
                }

                if (player != null)
                {
                    tpc.SetTarget(player.transform);
                }

                Debug.Log($"[RobotSetup] Successfully attached ThirdPersonCamera to '{mainCam.name}'.");
            }
        }
    }
}
