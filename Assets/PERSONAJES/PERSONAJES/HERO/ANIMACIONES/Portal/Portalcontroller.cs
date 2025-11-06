using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SpriteRenderer))]
public class PortalEffects : MonoBehaviour
{
    [Header("Jugador")]
    public Transform player;

    [Header("Audio")]
    public AudioClip proximityClip;
    public AudioClip enterClip;
    public float proximityDistance = 5f;

    [Header("Brillo")]
    public Color baseColor = Color.cyan;       // Color normal
    public Color glowColor = Color.white;      // Color al máximo
    public float pulseSpeed = 2f;              // Velocidad del pulso
    public float glowIntensity = 2f;           // Intensidad máxima

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private bool isPlayingProximity = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("PLAYER")?.transform;

        if (spriteRenderer != null)
            spriteRenderer.material = new Material(spriteRenderer.material); // Evita modificar el material original
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 🔹 Sonido de proximidad
        if (distance <= proximityDistance)
        {
            if (!isPlayingProximity)
            {
                audioSource.clip = proximityClip;
                audioSource.loop = true;
                audioSource.Play();
                isPlayingProximity = true;
            }

            // 🔹 Pulso del brillo
            if (spriteRenderer != null)
            {
                float emission = Mathf.PingPong(Time.time * pulseSpeed, glowIntensity);
                spriteRenderer.color = Color.Lerp(baseColor, glowColor, emission / glowIntensity);
            }
        }
        else
        {
            if (isPlayingProximity)
            {
                audioSource.Stop();
                isPlayingProximity = false;
            }

            // Volver al color base
            if (spriteRenderer != null)
                spriteRenderer.color = baseColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PLAYER"))
        {
            // Reproduce sonido de entrada
            audioSource.PlayOneShot(enterClip);

            // 🔹 Brillo máximo al entrar
            if (spriteRenderer != null)
                spriteRenderer.color = glowColor;

            Debug.Log("Jugador entró al portal 🔊");
        }
    }
}
