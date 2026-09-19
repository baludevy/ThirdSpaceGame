using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {
    public float MovingSpeed = 300f;
    public float sprintMultiplier = 1.8f;
    Vector2 inputVector;
    Rigidbody2D rb;
    bool isSprinting;

    [SerializeField] private float Stamina;

    public float stamina {
        get { return Stamina; }
        set { Stamina = Mathf.Clamp(value, 0f, 100f); }
    }

    bool staminareload;
    Coroutine reloadCoroutine;
    public Slider StaminaSlider;
    public TMP_Text StaminaText;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
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
        inputVector = context.ReadValue<Vector2>();
        Animator anim = gameObject.GetComponent<Animator>();
        SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();

        if(inputVector.x >= 0.1f)
        {
            anim.SetBool("Side", true);
            anim.SetBool("Back", false);
            anim.SetBool("Forward", false);
            
            sprite.flipX = true;
        }
        else if(inputVector.x < 0)
        {
            anim.SetBool("Side", true);
            anim.SetBool("Back", false);
            anim.SetBool("Forward", false);
            
            sprite.flipX = false;
        }
        else if(inputVector.y < 0f)
        {
            anim.SetBool("Forward", true);
            anim.SetBool("Side", false);
            anim.SetBool("Back", false);
        }
        else if(inputVector.y >= 0.1f)
        {
            anim.SetBool("Back", true);
            anim.SetBool("Side", false);
            anim.SetBool("Forward", false);
        }
        else
        {
            anim.SetBool("Back", false);
            anim.SetBool("Side", false);
            anim.SetBool("Forward", false);
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

        rb.linearVelocity = inputVector * currentSpeed * Time.deltaTime;
        
        ClientSend.PlayerMove(rb.position);
    }

    IEnumerator SprintReload() {
        yield return new WaitForSeconds(4f);
        staminareload = true;
        reloadCoroutine = null;
    }
}