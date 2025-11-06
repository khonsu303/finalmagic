using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Movimiento")]
    public float speedThreshold = 0.1f; // Umbral para considerar que camina/corre

    [Header("Vida y ataques")]
    public int vida = 100;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (vida <= 0)
        {
            anim.SetTrigger("Die");
            return; // Detiene cualquier otra animación
        }

        HandleMovementAnim();
        HandleJumpAnim();
        HandleAttackInput();
    }

    // ========================= Movimiento =========================
    private void HandleMovementAnim()
    {
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        anim.SetFloat("Speed", horizontalSpeed);
    }

    private void HandleJumpAnim()
    {
        bool isJumping = Mathf.Abs(rb.linearVelocity.y) > 0.1f;
        anim.SetBool("IsJumping", isJumping);
    }

    // ========================= Ataques =========================
    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PlayAttack(0); // Ataque 1
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            PlayAttack(1); // Ataque 2
        }
        else if (Input.GetMouseButtonDown(1)) // Click derecho
        {
            PlayAttack(2); // Ataque 3
        }
    }

    public void PlayAttack(int attackType)
    {
        if (vida <= 0) return; // No puede atacar si está muerto
        anim.SetTrigger("Attack" + attackType); // Attack0, Attack1, Attack2
    }

    // ========================= Idle rutinario =========================
    public void StartIdleRoutine()
    {
        anim.SetTrigger("IdleTimer"); // Leer libro
    }

    public void ResetIdleRoutine()
    {
        anim.SetTrigger("ResetIdle"); // Vuelve al Idle normal
    }

    // ========================= Daño y Muerte =========================
    public void RecibirDaño(int daño)
    {
        vida -= daño;
        if (vida <= 0)
        {
            vida = 0;
            anim.SetTrigger("Die");
        }
    }

    public void Die()
    {
        anim.SetTrigger("Die");
    }
}
