using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movimiento general")]
    public float speed = 2f;
    public float jumpForce = 7f;
    public float waitTime = 2f;
    public float arriveThreshold = 0.1f;

    [Header("Patrulla")]
    public Vector2 pointA = new Vector2(0, 0);
    public Vector2 pointB = new Vector2(4, 0);
    public bool useRelativeToStart = true;
    public bool loop = true;

    [Header("Detección y ataque")]
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public int damage = 10;

    [Header("Salto y detección de suelo")]
    public Transform groundCheck;        // Punto bajo del enemigo
    public Transform obstacleCheck;      // Punto frontal del enemigo
    public float groundCheckDistance = 0.5f;
    public float obstacleCheckDistance = 0.5f;
    public LayerMask groundLayer;        // Capa del suelo

    [Header("Ataque cuerpo a cuerpo")]
    public Transform attackPoint;

    private Rigidbody2D rb;
    private Animator anim;

    private Vector2 worldA;
    private Vector2 worldB;
    private int targetIndex = 0;
    private bool waiting = false;
    private float waitCounter = 0f;
    private float lastAttackTime = 0f;

    private bool isFacingRight = true;
    private bool chasingPlayer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (useRelativeToStart)
        {
            Vector2 start = transform.position;
            worldA = start + pointA;
            worldB = start + pointB;
        }
        else
        {
            worldA = pointA;
            worldB = pointB;
        }

        targetIndex = 1;

        if (rb)
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
            Patrol();
        }
    }

    void Patrol()
    {
        Vector2 target = (targetIndex == 0) ? worldA : worldB;
        float dist = Vector2.Distance(transform.position, target);

        if (waiting)
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0f)
                waiting = false;

            StopMoving();
            return;
        }

        if (dist < arriveThreshold)
        {
            StopMoving();
            waiting = true;
            waitCounter = waitTime;
            targetIndex = (targetIndex == 0) ? 1 : 0;
        }
        else
        {
            Vector2 dir = (target - (Vector2)transform.position).normalized;
            rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);

            if (anim) anim.SetBool("isWalking", true);

            if (dir.x > 0 && !isFacingRight)
                Flip();
            else if (dir.x < 0 && isFacingRight)
                Flip();

            HandleJumpDetection();
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
        // Detectar si hay suelo adelante
        bool groundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        // Detectar obstáculo en frente
        bool obstacleAhead = Physics2D.Raycast(obstacleCheck.position, transform.right, obstacleCheckDistance, groundLayer);

        if (!groundAhead)
        {
            // Salto para evitar caer
            Jump();
        }
        else if (obstacleAhead)
        {
            // Salto para pasar obstáculo
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
