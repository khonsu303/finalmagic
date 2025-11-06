using UnityEngine;
using TMPro;

public class Coin : MonoBehaviour
{
    [Header("Configuración de Sonido")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float soundVolume = 1.0f;

    [Header("Recompensa")]
    public int healAmount = 10;          // Cuánta vida cura al jugador

    private AudioSource audioSource;

    void Start()
    {
        // Asegurar que el objeto tiene un AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    // ✅ Soporte tanto para colisiones 3D como 2D
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER") || other.CompareTag("Player"))
            CollectCoin(other.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PLAYER") || other.CompareTag("Player"))
            CollectCoin(other.gameObject);
    }

    private void CollectCoin(GameObject playerObj)
    {
        // 🎵 Reproducir sonido
        if (collectSound != null)
            audioSource.PlayOneShot(collectSound, soundVolume);

        // ❤️ Curar al jugador (adaptado para tu PlayerActions)
        PlayerActions player = playerObj.GetComponent<PlayerActions>();
        if (player != null)
        {
            // Obtener vida actual por medio del getter
            int currentHealth = player.GetCurrentHealth();
            int newHealth = Mathf.Min(currentHealth + healAmount, player.maxHealth);

            // Usar reflexión para modificar currentHealth (porque es privado)
            var field = typeof(PlayerActions).GetField("currentHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(player, newHealth);
            }

            // Actualizar UI de vida (si existe)
            var vidaTMP = player.vidaTMP;
            if (vidaTMP != null)
                vidaTMP.text = $"❤️ Vida: {newHealth}/{player.maxHealth}";

            if (player.vidaImagenesUI != null)
                player.vidaImagenesUI.UpdateHealthUI((float)newHealth / player.maxHealth);

            Debug.Log($"🧪 Jugador curado +{healAmount} HP (Vida actual: {newHealth}/{player.maxHealth})");
        }

        // 🚫 Ocultar visualmente
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.enabled = false;

        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = false;

        Collider2D c2d = GetComponent<Collider2D>();
        if (c2d != null) c2d.enabled = false;

        // 💣 Destruir después de que el sonido termine
        Destroy(gameObject, collectSound != null ? collectSound.length : 0.1f);
    }
}


