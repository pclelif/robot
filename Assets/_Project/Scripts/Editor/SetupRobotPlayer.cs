using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Robot.Player.Movement;
using Robot.Player.CameraControl;
using Robot.Player;
using Robot.Input;
using Robot.Robots.Customization;
using Robot.UI.HUD;
using Unity.Cinemachine;

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

                // Input is intentionally the only component that knows Input System actions.
                PlayerInputReader input = player.GetComponent<PlayerInputReader>();
                if (input == null) input = player.AddComponent<PlayerInputReader>();
                input.Configure(AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
                    "Assets/_Project/Settings/Input/InputSystem_Actions.inputactions"));

                // Setup RobotMovementController
                RobotMovementController movement = player.GetComponent<RobotMovementController>();
                if (movement == null)
                {
                    movement = player.AddComponent<RobotMovementController>();
                }

                if (player.GetComponent<RobotAnimator>() == null)
                {
                    player.AddComponent<RobotAnimator>();
                }

                foreach (VirtualJoystick joystick in Object.FindObjectsByType<VirtualJoystick>(FindObjectsSortMode.None))
                {
                    joystick.Configure(input);
                    EditorUtility.SetDirty(joystick);
                }
                foreach (MobileTouchLook lookArea in Object.FindObjectsByType<MobileTouchLook>(FindObjectsSortMode.None))
                {
                    lookArea.Configure(input);
                    EditorUtility.SetDirty(lookArea);
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

            // 2. The physical camera owns the Cinemachine Brain. The virtual camera is separate.
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                if (mainCam.GetComponent<CinemachineBrain>() == null)
                {
                    mainCam.gameObject.AddComponent<CinemachineBrain>();
                }

                GameObject virtualCameraObject = GameObject.Find("RobotThirdPersonCamera");
                if (virtualCameraObject == null)
                {
                    virtualCameraObject = new GameObject("RobotThirdPersonCamera");
                }
                if (virtualCameraObject.GetComponent<CinemachineCamera>() == null)
                {
                    virtualCameraObject.AddComponent<CinemachineCamera>();
                }
                if (virtualCameraObject.GetComponent<CinemachineOrbitalFollow>() == null)
                {
                    virtualCameraObject.AddComponent<CinemachineOrbitalFollow>();
                }
                
                CinemachineDeoccluder deoccluder = virtualCameraObject.GetComponent<CinemachineDeoccluder>();
                if (deoccluder == null)
                {
                    deoccluder = virtualCameraObject.AddComponent<CinemachineDeoccluder>();
                }
                deoccluder.IgnoreTag = "Player";
                deoccluder.MinimumDistanceFromTarget = 0.8f;

                ThirdPersonCameraController tpc = virtualCameraObject.GetComponent<ThirdPersonCameraController>();
                if (tpc == null)
                {
                    tpc = virtualCameraObject.AddComponent<ThirdPersonCameraController>();
                }

                if (player != null)
                {
                    tpc.SetTarget(player.transform);
                    RobotMovementController rmc = player.GetComponent<RobotMovementController>();
                    if (rmc != null)
                    {
                        rmc.SetCameraTransform(mainCam.transform);
                    }
                }

                Debug.Log($"[RobotSetup] Successfully attached ThirdPersonCamera to '{mainCam.name}'.");
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
