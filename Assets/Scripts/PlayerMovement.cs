using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {
    public float MovingSpeed = 300f;
    public float sprintMultiplier = 1.8f;
    Vector2 inputVector;
    private Vector2 previousInput;
    Rigidbody2D rb;
    bool isSprinting;


    private Animator anim;
    private SpriteRenderer sprite;

    private FacingDirection lastDirection = FacingDirection.Forward;

    [SerializeField] private float Stamina;

    public float stamina {
        get => Stamina;
        set => Stamina = Mathf.Clamp(value, 0f, 100f);
    }


    private enum FacingDirection {
        Forward,
        Back,
        Left,
        Right
    }


    bool staminareload;
    Coroutine reloadCoroutine;
    public Slider StaminaSlider;
    public TMP_Text StaminaText;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (isSprinting) {
            stamina -= 15f * Time.deltaTime;
            if (stamina <= 0) {
                isSprinting = false;
                if (reloadCoroutine != null)
                    StopCoroutine(reloadCoroutine);
                reloadCoroutine = StartCoroutine(SprintReload());
            }
        }

        if (Stamina < 100f && staminareload) {
            Stamina += 15f * Time.deltaTime;
        }
        else {
            staminareload = false;
        }


        if (StaminaText != null)
            StaminaSlider.value = stamina / 100f;

        if (StaminaText != null)
            StaminaText.text = stamina.ToString("0") + "%";
    }

    //asked
    public void OnMove(InputAction.CallbackContext context) {
        const float deadzone = 0.1f;

        previousInput = inputVector;
        inputVector = context.ReadValue<Vector2>();

        bool leftHeld = inputVector.x <= -deadzone;
        bool rightHeld = inputVector.x >= deadzone;
        bool downHeld = inputVector.y <= -deadzone;
        bool upHeld = inputVector.y >= deadzone;

        bool leftWasHeld = previousInput.x <= -deadzone;
        bool rightWasHeld = previousInput.x >= deadzone;
        bool downWasHeld = previousInput.y <= -deadzone;
        bool upWasHeld = previousInput.y >= deadzone;

        if (leftHeld && !leftWasHeld)
            lastDirection = FacingDirection.Left;

        if (rightHeld && !rightWasHeld)
            lastDirection = FacingDirection.Right;

        if (downHeld && !downWasHeld)
            lastDirection = FacingDirection.Forward;

        if (upHeld && !upWasHeld)
            lastDirection = FacingDirection.Back;

        if (!IsDirectionHeld(lastDirection, leftHeld, rightHeld, downHeld, upHeld)) {
            if (rightHeld)
                lastDirection = FacingDirection.Right;
            else if (leftHeld)
                lastDirection = FacingDirection.Left;
            else if (upHeld)
                lastDirection = FacingDirection.Back;
            else if (downHeld)
                lastDirection = FacingDirection.Forward;
        }

        anim.SetBool("Side", false);
        anim.SetBool("Back", false);
        anim.SetBool("Forward", false);

        if (!leftHeld && !rightHeld && !upHeld && !downHeld)
            return;

        switch (lastDirection) {
            case FacingDirection.Right:
                anim.SetBool("Side", true);
                sprite.flipX = true;
                break;

            case FacingDirection.Left:
                anim.SetBool("Side", true);
                sprite.flipX = false;
                break;

            case FacingDirection.Back:
                anim.SetBool("Back", true);
                break;

            case FacingDirection.Forward:
                anim.SetBool("Forward", true);
                break;
        }
    }

    private bool IsDirectionHeld(FacingDirection direction, bool leftHeld, bool rightHeld, bool downHeld, bool upHeld) {
        switch (direction) {
            case FacingDirection.Left:
                return leftHeld;

            case FacingDirection.Right:
                return rightHeld;

            case FacingDirection.Forward:
                return downHeld;

            case FacingDirection.Back:
                return upHeld;

            default:
                return false;
        }
    }

    public void OnSprinting(InputAction.CallbackContext context) {
        if (context.performed && Stamina > 0) {
            isSprinting = true;
            staminareload = false;
            if (reloadCoroutine != null) {
                StopCoroutine(reloadCoroutine);
            }
        }
        else if (context.canceled) {
            isSprinting = false;
            if (reloadCoroutine != null) {
                StartCoroutine(SprintReload());
            }

            reloadCoroutine = StartCoroutine(SprintReload());
        }
    }

    void FixedUpdate() {
        float currentSpeed = isSprinting ? MovingSpeed * sprintMultiplier : MovingSpeed;

        Vector2 movement = Vector2.zero;

        switch (lastDirection) {
            case FacingDirection.Right:
                if (inputVector.x > 0.1f)
                    movement = Vector2.right;
                break;

            case FacingDirection.Left:
                if (inputVector.x < -0.1f)
                    movement = Vector2.left;
                break;

            case FacingDirection.Back:
                if (inputVector.y > 0.1f)
                    movement = Vector2.up;
                break;

            case FacingDirection.Forward:
                if (inputVector.y < -0.1f)
                    movement = Vector2.down;
                break;
        }

        rb.linearVelocity = movement * currentSpeed * Time.deltaTime;
    }

    IEnumerator SprintReload() {
        yield return new WaitForSeconds(4f);
        staminareload = true;
        reloadCoroutine = null;
    }
}