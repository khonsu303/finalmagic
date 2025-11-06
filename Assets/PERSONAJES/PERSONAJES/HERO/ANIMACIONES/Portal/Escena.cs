using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class PortalWithDelay : MonoBehaviour
{
    [Header("Jugador")]
    public Transform player; // Asignar o se busca por tag

    [Header("Audio")]
    public AudioClip proximityClip; // Sonido mientras está cerca
    public AudioClip enterClip;     // Sonido al entrar

    [Header("Escena")]
    public string sceneToLoad;       // Nombre exacto de la escena
    public float delay = 3f;         // Tiempo que debe estar el jugador en el portal

    [Header("Distancia de proximidad")]
    public float proximityDistance = 5f;

    private AudioSource audioSource;
    private bool isPlayingProximity = false;
    private bool playerInside = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("PLAYER")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Sonido de proximidad
        if (distance <= proximityDistance)
        {
            if (!isPlayingProximity)
            {
                audioSource.clip = proximityClip;
                audioSource.loop = true;
                audioSource.Play();
                isPlayingProximity = true;
            }
        }
        else
        {
            if (isPlayingProximity)
            {
                audioSource.Stop();
                isPlayingProximity = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PLAYER"))
        {
            playerInside = true;
            StartCoroutine(EnterPortal());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PLAYER"))
        {
            playerInside = false; // Salió antes de tiempo
        }
    }

    private System.Collections.IEnumerator EnterPortal()
    {
        float timer = 0f;

        // Espera mientras el jugador sigue dentro
        while (timer < delay && playerInside)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (playerInside)
        {
            // Sonido al entrar
            if (enterClip != null)
                audioSource.PlayOneShot(enterClip);

            // Cambiar escena
            if (!string.IsNullOrEmpty(sceneToLoad))
                SceneManager.LoadScene(sceneToLoad);
        }
    }
}
