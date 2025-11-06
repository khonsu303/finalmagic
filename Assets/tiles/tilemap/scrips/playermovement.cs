using UnityEngine;

public class PlatformerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;
    public float jumpCooldown = 0.2f;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool jumpPressed;
    private BoxCollider2D playerCollider;
    private bool isJumping;
    private float jumpStartTime;
    private float lastJumpTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();

        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Default");
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        bool canJump = !isJumping && (Time.time - lastJumpTime) >= jumpCooldown;

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            jumpPressed = true;
        }

        // Solo actualizar parámetros continuos en Update
        if (animator != null)
        {
            bool isMoving = Mathf.Abs(horizontalInput) > 0.1f;
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        }

        if (spriteRenderer != null && Mathf.Abs(horizontalInput) > 0.1f)
        {
            spriteRenderer.flipX = horizontalInput < 0;
        }
    }

    void FixedUpdate()
    {
        CheckGrounded();

        // MOVIDO: La lógica del trigger ahora está aquí
        if (jumpPressed && isGrounded && !isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            jumpStartTime = Time.time;
            lastJumpTime = Time.time;

            // ACTIVAR TRIGGER EN FIXEDUPDATE
            if (animator != null)
            {
                animator.SetTrigger("Jump");
                Debug.Log("🎯 TRIGGER JUMP ACTIVADO - Grounded: " + isGrounded);
            }

            Debug.Log("🚀 SALTO INICIADO");
        }

        if (isJumping && isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            isJumping = false;
            Debug.Log("🏁 SALTO TERMINADO - Tocó suelo");
        }

        if (isJumping && (Time.time - jumpStartTime) > 1.0f)
        {
            isJumping = false;
            Debug.Log("⏰ Salto terminado por tiempo");
        }

        jumpPressed = false;
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void CheckGrounded()
    {
        if (playerCollider == null) return;

        Bounds bounds = playerCollider.bounds;
        Vector2[] rayOrigins = new Vector2[3]
        {
            new Vector2(bounds.center.x, bounds.min.y),
            new Vector2(bounds.min.x + 0.1f, bounds.min.y),
            new Vector2(bounds.max.x - 0.1f, bounds.min.y)
        };

        bool hitDetected = false;
        float rayLength = groundCheckDistance;

        foreach (Vector2 origin in rayOrigins)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundLayer);
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                hitDetected = true;
                break;
            }
        }

        bool isFallingOrStable = rb.linearVelocity.y <= 0.1f;
        isGrounded = hitDetected && isFallingOrStable;

        // Debug visual
        Color debugColor = isGrounded ? Color.green : Color.red;
        if (isJumping) debugColor = Color.blue;

        foreach (Vector2 origin in rayOrigins)
        {
            Debug.DrawRay(origin, Vector2.down * rayLength, debugColor);
        }
    }
}