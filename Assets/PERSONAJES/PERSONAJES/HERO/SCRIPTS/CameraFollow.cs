using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Jugador")]
    public Transform target;            // El jugador a seguir

    [Header("Movimiento")]
    public float smoothSpeed = 5f;      // Qué tan suave sigue la cámara
    public Vector2 offset = new Vector2(0, 1f); // Ajuste de posición (ej: un poco más arriba)

    [Header("Límites del mundo")]
    public bool useLimits = true;       // Activar o desactivar límites
    public float minX = -10f;           // Límite izquierdo
    public float maxX = 10f;            // Límite derecho
    public float minY = -2f;            // Límite inferior
    public float maxY = 5f;             // Límite superior

    private Vector3 targetPosition;

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("⚠️ CameraFollow: No hay jugador asignado como objetivo.");
            return;
        }

        // Posición deseada (jugador + offset)
        targetPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);

        // Aplicar límites del mundo
        if (useLimits)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }

        // Movimiento suave hacia el jugador
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    // Para visualizar los límites en el editor
    private void OnDrawGizmosSelected()
    {
        if (!useLimits) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(minX, minY, 0), new Vector3(minX, maxY, 0)); // Izquierda
        Gizmos.DrawLine(new Vector3(maxX, minY, 0), new Vector3(maxX, maxY, 0)); // Derecha
        Gizmos.DrawLine(new Vector3(minX, minY, 0), new Vector3(maxX, minY, 0)); // Abajo
        Gizmos.DrawLine(new Vector3(minX, maxY, 0), new Vector3(maxX, maxY, 0)); // Arriba
    }
}
