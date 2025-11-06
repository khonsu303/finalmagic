using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolBetweenPoints : MonoBehaviour
{
    [Header("Puntos (coordenadas)")]
    public Vector2 pointA = new Vector2(0f, 0f);   // Coordenada A (world o relativa)
    public Vector2 pointB = new Vector2(5f, 0f);   // Coordenada B (world o relativa)
    public bool useRelativeToStart = true;         // true = puntos relativos a la posici�n inicial; false = coordenadas world

    [Header("Movimiento")]
    public float speed = 2f;
    public float waitTime = 1.5f;                  // tiempo de espera al llegar a cada punto
    public float arriveThreshold = 0.05f;          // distancia para considerar "lleg�"

    [Header("Opciones")]
    public bool startAtA = true;                   // si empieza en A (si false empieza en B)
    public bool loop = true;                       // si true hace ciclos A->B->A->B...

    // Internos
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 worldA;
    private Vector2 worldB;
    private int targetIndex = 0;                   // 0 -> A, 1 -> B
    private float waitCounter = 0f;
    private bool waiting = false;
    private bool isFacingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // Calcular posiciones en world space
        if (useRelativeToStart)
        {
            Vector2 startPos = transform.position;
            worldA = startPos + pointA;
            worldB = startPos + pointB;
        }
        else
        {
            worldA = pointA;
            worldB = pointB;
        }

        targetIndex = startAtA ? 0 : 1;

        // opcional: si empiezas en B, colocarte exactamente all�
        Vector2 startPosToSet = targetIndex == 0 ? worldA : worldB;
        transform.position = startPosToSet;

        // Asegura que Rigidbody2D no rote por f�sica
        if (rb)
            rb.freezeRotation = true;
    }

    void Update()
    {
        // Manejo de espera sin mover f�sicamente (velocidad = 0)
        if (waiting)
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0f)
            {
                waiting = false;
            }
            // Mantener detenido visualmente
            if (anim) anim.SetBool("isWalking", false);
            return;
        }

        Vector2 target = (targetIndex == 0) ? worldA : worldB;
        Vector2 pos = transform.position;
        float dist = Vector2.Distance(pos, target);

        if (dist <= arriveThreshold)
        {
            // Lleg� al punto
            rb.linearVelocity = Vector2.zero;
            if (anim) anim.SetBool("isWalking", false);

            if (loop)
            {
                // espera y cambia objetivo
                waiting = true;
                waitCounter = waitTime;
                targetIndex = (targetIndex == 0) ? 1 : 0;
            }
            else
            {
                // Si no se hace loop, se queda en el punto y se detiene
                enabled = false; // desactiva el script si quieres que se quede est�tico
            }

            return;
        }

        // Mover hacia el target
        Vector2 direction = (target - pos).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        if (anim) anim.SetBool("isWalking", true);

        // Flip seg�n la velocidad en X
        if (rb.linearVelocity.x > 0.01f && !isFacingRight) Flip();
        else if (rb.linearVelocity.x < -0.01f && isFacingRight) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    // Dibuja los puntos en la escena para facilitar la edici�n
    private void OnDrawGizmosSelected()
    {
        Vector2 a = pointA;
        Vector2 b = pointB;
        if (useRelativeToStart && Application.isPlaying == false)
        {
            // mostrar relativos a la posici�n actual del objeto en edit mode
            a = (Vector2)transform.position + pointA;
            b = (Vector2)transform.position + pointB;
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(a, 0.12f);
        Gizmos.DrawWireSphere(b, 0.12f);
        Gizmos.DrawLine(a, b);
    }
}
