using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layer;   // La capa del fondo
        [Range(0f, 1f)]
        public float parallaxFactor = 0.5f; // Qué tanto se mueve (0 = fijo, 1 = igual que la cámara)
    }

    [Header("Configuración de capas")]
    public ParallaxLayer[] layers;

    [Header("Referencia de cámara o jugador")]
    public Transform cameraTransform; // O el jugador
    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        foreach (var layerData in layers)
        {
            if (layerData.layer == null) continue;

            // Movimiento inverso proporcional al parallaxFactor
            float moveX = deltaMovement.x * layerData.parallaxFactor;
            float moveY = deltaMovement.y * layerData.parallaxFactor;

            layerData.layer.position += new Vector3(moveX, moveY, 0);
        }

        lastCameraPosition = cameraTransform.position;
    }
}
