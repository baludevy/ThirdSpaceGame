using UnityEngine;

public class MoveCamera : MonoBehaviour {
    public Transform target;

    Vector3 velocity = Vector3.zero;
    float cameraZ;

    public Vector3 offset;

    public void Start() {
        cameraZ = transform.position.z;
    }

    void LateUpdate() {
        if (target == null) {
            return;
        }

        float playerX = target.position.x;
        float playerY = target.position.y;

        Vector3 targetPosition = new Vector3(
            playerX,
            playerY,
            cameraZ
        ) + offset;
        
        transform.position = targetPosition;
    }
}