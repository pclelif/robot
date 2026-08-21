using System;
using UnityEngine;

namespace Robot.Player.CameraControl
{
    /// <summary>Compatibility bridge for scenes created before the Cinemachine 3 camera controller.</summary>
    [Obsolete("Use ThirdPersonCameraController on a CinemachineCamera instead.")]
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private ThirdPersonCameraController controller;
        private void Awake()
        {
            if (controller == null) controller = GetComponent<ThirdPersonCameraController>();
        }
        public void SetTarget(Transform target)
        {
            if (controller != null) controller.SetTarget(target);
        }
    }
}
