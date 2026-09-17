using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float MovingSpeed = 300f;
    public float sprintMultiplier = 1.8f;
    Vector2 inputVector;
    Rigidbody2D rb;
    bool isSprinting;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
    }
    public void OnSprinting(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }
    }
    void FixedUpdate()
    {
        float currentSpeed = isSprinting ? MovingSpeed * sprintMultiplier : MovingSpeed;

        rb.linearVelocity = inputVector * currentSpeed * Time.deltaTime;
    }
}