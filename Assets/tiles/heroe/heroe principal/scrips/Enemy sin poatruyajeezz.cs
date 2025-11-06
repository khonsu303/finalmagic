using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaseAI : MonoBehaviour
{
    [Header("Movimiento general")]
    public float speed = 4f;
    public float jumpForce = 7f;

    [Header("Detección y ataque")]
    public float detectionRange = 6f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public int damage = 10;

    [Header("Salto y detección de suelo")]
    public Transform groundCheck;
    public Transform obstacleCheck;
    public float groundCheckDistance = 0.5f;
    public float obstacleCheckDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Ataque cuerpo a cuerpo")]
    public Transform attackPoint;

    private Rigidbody2D rb;
    private Animator anim;

    private bool isFacingRight = true;
    private bool chasingPlayer = false;
    private float lastAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("PLAYER");

        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = p.transform;
            }
        }

        // Detectar si debe perseguir o detenerse
        if (closestPlayer != null && closestDistance <= detectionRange)
            chasingPlayer = true;
        else if (closestDistance > detectionRange * 1.2f)
            chasingPlayer = false;

        if (chasingPlayer && closestPlayer != null)
        {
            if (closestDistance > attackRange)
                MoveTowardsPlayer(closestPlayer);
            else
                Attack();
        }
        else
        {
            StopMoving();
        }
    }

    void MoveTowardsPlayer(Transform targetPlayer)
    {
        Vector2 dir = (targetPlayer.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);

        if (anim) anim.SetBool("isWalking", true);

        if (dir.x > 0 && !isFacingRight)
            Flip();
        else if (dir.x < 0 && isFacingRight)
            Flip();

        HandleJumpDetection();
    }

    void HandleJumpDetection()
    {
        bool groundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        bool obstacleAhead = Physics2D.Raycast(obstacleCheck.position, transform.right, obstacleCheckDistance, groundLayer);

        if (!groundAhead)
        {
            Jump();
        }
        else if (obstacleAhead)
        {
            Jump();
        }
    }

    void Jump()
    {
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (anim) anim.SetTrigger("jump");
        }
    }

    bool IsGrounded()
    {
        return Physics2D.Raycast(groundCheck.position, Vector2.down, 0.2f, groundLayer);
    }

    void Attack()
    {
        StopMoving();
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            if (anim) anim.SetTrigger("attack");
            lastAttackTime = Time.time;
        }
    }

    public void DealDamage()
    {
        if (attackPoint == null) attackPoint = transform;

        GameObject[] players = GameObject.FindGameObjectsWithTag("PLAYER");

        foreach (GameObject p in players)
        {
            float distance = Vector2.Distance(attackPoint.position, p.transform.position);
            if (distance <= attackRange)
            {
                PlayerActions playerHealth = p.GetComponent<PlayerActions>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"💀 Enemigo inflige {damage} de daño a {p.name}");
                }
            }
        }
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (anim) anim.SetBool("isWalking", false);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }

        if (obstacleCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(obstacleCheck.position, obstacleCheck.position + transform.right * obstacleCheckDistance);
        }
    }
}
