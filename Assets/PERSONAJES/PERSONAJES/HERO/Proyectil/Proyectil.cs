using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Stats")]
    public int damage = 10;          // Daño que inflige
    public float speed = 10f;        // Velocidad de desplazamiento
    public float range = 5f;         // Distancia máxima que puede recorrer
    public float lifeTime = 5f;      // Tiempo antes de desaparecer automáticamente

    private Vector3 startPos;
    private float direction = 1f;    // 1 = derecha, -1 = izquierda

    void Start()
    {
        startPos = transform.position;
        Destroy(gameObject, lifeTime); // Destruye automáticamente después de lifeTime
    }

    void Update()
    {
        // Mover proyectil según dirección
        transform.Translate(Vector2.right * speed * direction * Time.deltaTime);

        // Destruir si supera el rango máximo
        if (Vector3.Distance(startPos, transform.position) >= range)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Destruye si choca con suelo
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        // Infligir daño al jugador
        PlayerActions player = collision.GetComponentInParent<PlayerActions>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Infligir daño al enemigo
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemy = collision.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
    }


    // Asignar dirección del proyectil (1 = derecha, -1 = izquierda)
    public void SetDirection(float dir)
    {
        direction = dir;

        // Ajustar el sprite para mirar la dirección correcta
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }
}
