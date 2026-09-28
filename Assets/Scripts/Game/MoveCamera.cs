using UnityEngine;

namespace Game
{
    public class MoveCamera : MonoBehaviour
    {
        public Transform target;
        
        public Vector3 offset;
        private float cameraY;

        private void Start()
        {
            cameraY = transform.position.y;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float playerX = target.position.x;
            float playerZ = target.position.z;

            Vector3 targetPosition = new Vector3(playerX, cameraY, playerZ) + offset;

            transform.position = targetPosition;
        }
    }
}
