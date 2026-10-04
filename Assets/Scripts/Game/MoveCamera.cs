using UnityEngine;

namespace Game
{
    public class MoveCamera : MonoBehaviour
    {
        public Transform target;
        
        public Vector3 offset;
        private float cameraZ;

        private void Start()
        {
            TileManager.Instance.playerCamera = GetComponent<Camera>();
            cameraZ = transform.position.z;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float playerX = target.position.x;
            float playerY = target.position.y;

            Vector3 targetPosition = new Vector3(playerX, playerY, cameraZ) + offset;

            transform.position = targetPosition;
        }
    }
}
