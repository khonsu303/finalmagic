using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AnimalFleeSimple : MonoBehaviour
{
    public float detectionRange = 5f;   // Distancia a la que detecta al jugador
    public float fleeSpeed = 5f;        // Velocidad de huida

    private Rigidbody2D rb;
    private bool isFleeing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("PLAYER");

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);

            if (dist <= detectionRange && !isFleeing)
            {
                // Moverse en dirección opuesta al jugador
                Vector2 dir = (transform.position - p.transform.position).normalized;
                rb.linearVelocity = new Vector2(dir.x * fleeSpeed, rb.linearVelocity.y);
                isFleeing = true;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

