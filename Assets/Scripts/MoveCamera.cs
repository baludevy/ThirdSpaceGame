using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.12f;

    Vector3 velocity = Vector3.zero;
    float cameraZ;
    
    public void Start()
    {
        cameraZ = transform.position.z;
    }

    void LateUpdate()
    {
        if(target == null)
        {
            return;
        }

    float playerX = target.position.x;
    float playerY = target.position.y;

    Vector3 targetPosition = new Vector3(
        playerX,
        playerY,
        cameraZ 
    );

    Vector3 currentPosition = transform.position;
    
    Vector3 nextposition = Vector3.SmoothDamp(
        currentPosition,
        targetPosition,
        ref velocity,
        smoothTime
    );

    transform.position = nextposition;  
    }
}
