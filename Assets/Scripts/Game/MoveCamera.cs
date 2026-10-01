using UnityEngine;

namespace Game
{
    public class MoveCamera : MonoBehaviour
    {
        public Transform target;
        public float spriteHeight = 2.25f;
        public float followDistance = 10f;
        public float cameraPitch = 45f;

        private void LateUpdate()
        {
            if (target == null) return;
            
            Vector3 spriteCentre = target.position + Vector3.up * (spriteHeight * 0.5f);

            float pitchRadians = cameraPitch * Mathf.Deg2Rad;

            Vector3 cameraOffset = new Vector3(
                0f,
                followDistance * Mathf.Sin(pitchRadians),
                -followDistance * Mathf.Cos(pitchRadians)
            );

            transform.position = spriteCentre + cameraOffset;
            transform.LookAt(spriteCentre);
        }
    }
}
