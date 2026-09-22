using UnityEngine;

namespace Game
{
    public class MoveCamera : MonoBehaviour
    {
        public Transform target;

        public Vector3 offset;
        float cameraZ;

        Vector3 velocity = Vector3.zero;

        public void Start()
        {
            cameraZ = transform.position.z;
        }

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float playerX = target.position.x;
            float playerY = target.position.y;

            var targetPosition = new Vector3(
                playerX,
                playerY,
                cameraZ
            ) + offset;

            transform.position = targetPosition;
        }
    }
}
