using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 7f;
    public float jumpForce = 12f;
    public float fastFallMultiplier = 2f;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask whatIsGround;

    [Header("Rutina de Reposo")]
    public float idleRoutineTime = 7f; // tiempo antes de activar Idle1
    private float idleTimer;
    private bool isIdleRoutineActive = false;

    private Rigidbody2D rb;
    private float moveHorizontal;
    private bool isGrounded;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        idleTimer = idleRoutineTime;
    }

    void Update()
    {
        HandleInput();
        CheckGround();
        HandleJump();

        HandleIdleRoutine();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleFastFall();
    }

    // ========================= Funciones =========================

    private void HandleInput()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
    }

    private void HandleMovement()
    {
        Vector2 movement = new Vector2(moveHorizontal * speed, rb.linearVelocity.y);
        rb.linearVelocity = movement;

        // Voltear sprite
        if (moveHorizontal < 0) transform.localScale = new Vector3(-1, 1, 1);
        else if (moveHorizontal > 0) transform.localScale = new Vector3(1, 1, 1);

        // Actualizar animación de correr
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }
    }

  private void HandleJump()
{
    if (isGrounded && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")))
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (anim != null)
        {
            anim.SetBool("IsJumping", true);
        }

        ResetIdleTimer();
    }

    // Volver a Idle/Run al caer
    if (anim != null && isGrounded)
    {
        anim.SetBool("IsJumping", false);
    }
}

    private void HandleFastFall()
    {
        if (!isGrounded && rb.linearVelocity.y < 0 && Input.GetKey(KeyCode.S))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fastFallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
    }

    private void HandleIdleRoutine()
    {
        // Si se mueve o salta → reinicia temporizador
        if (Mathf.Abs(moveHorizontal) > 0.1f || Input.GetButtonDown("Jump"))
        {
            ResetIdleTimer();
            if (isIdleRoutineActive && anim != null)
            {
                anim.SetTrigger("ResetIdle");
                isIdleRoutineActive = false;
            }
        }
        else // jugador quieto
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0 && !isIdleRoutineActive && anim != null)
            {
                anim.SetTrigger("IdleTimer"); // disparar Idle1
                isIdleRoutineActive = true;
            }
        }
    }

    private void ResetIdleTimer()
    {
        idleTimer = idleRoutineTime;
    }

    // ========================= Debug =========================
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
