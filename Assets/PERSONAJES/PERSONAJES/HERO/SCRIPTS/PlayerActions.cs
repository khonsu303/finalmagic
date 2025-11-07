using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerActions : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("UI Vida")]
    public TextMeshProUGUI vidaTMP;
    public PlayerHealthUI vidaImagenesUI;

    [Header("Temporizador de escena")]
    public float tiempoLimite = 60f;
    private float tiempoRestante;
    public TextMeshProUGUI tiempoTMP;

    [Header("Game Over")]
    public TextMeshProUGUI gameOverText;
    public float gameOverDisplayTime = 3f;
    public string sceneOnDeath;

    [Header("Ataques")]
    public Transform attackPoint;

    [System.Serializable]
    public class ProyectilData
    {
        public string nombre;
        public GameObject prefab;
        public int damage = 10;
        public float speed = 10f;
        public float range = 5f;
        public float cooldown = 5f;
    }

    [Header("Proyectiles")]
    public ProyectilData proyectilQ;
    public ProyectilData proyectilE;
    public ProyectilData proyectilC;

    private float nextQ = 0f;
    private float nextE = 0f;
    private float nextC = 0f;

    [Header("Referencias")]
    public PlayerMovement movementScript;
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip habilidadQSound;
    public AudioClip habilidadESound;
    public AudioClip habilidadCSound;
    public AudioClip muerteSound;

    void Start()
    {
        currentHealth = maxHealth;
        tiempoRestante = tiempoLimite;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (movementScript == null)
            movementScript = GetComponent<PlayerMovement>();

        UpdateVidaText();

        if (vidaImagenesUI == null)
            vidaImagenesUI = FindObjectOfType<PlayerHealthUI>();

        if (vidaImagenesUI != null)
            vidaImagenesUI.UpdateHealthUI(1f);

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        if (tiempoTMP != null)
            tiempoTMP.text = FormatearTiempo(tiempoRestante);
    }

    void Update()
    {
        if (isDead) return;

        HandleAttackInput();
        ActualizarTemporizador();

        // Prueba de muerte instantánea
        if (Input.GetKeyDown(KeyCode.K))
            TakeDamage(currentHealth);
    }

    // ---------------- TEMPORIZADOR ----------------
    private void ActualizarTemporizador()
    {
        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            if (tiempoTMP != null)
                tiempoTMP.text = FormatearTiempo(Mathf.Max(0, tiempoRestante));
        }
        else if (!isDead)
        {
            Debug.Log("⏳ Tiempo agotado: el jugador ha muerto");
            Die();
        }
    }

    private string FormatearTiempo(float tiempo)
    {
        int minutos = Mathf.FloorToInt(tiempo / 60);
        int segundos = Mathf.FloorToInt(tiempo % 60);
        return $"{minutos:00}:{segundos:00}";
    }

    // ---------------- ATAQUES ----------------
    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= nextQ)
        {
            ShootProyectil(proyectilQ);
            PlaySound(habilidadQSound);
            nextQ = Time.time + proyectilQ.cooldown;
        }

        if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextE)
        {
            ShootProyectil(proyectilE);
            PlaySound(habilidadESound);
            nextE = Time.time + proyectilE.cooldown;
        }

        if (Input.GetMouseButtonDown(1) && Time.time >= nextC)
        {
            ShootProyectil(proyectilC);
            PlaySound(habilidadCSound);
            nextC = Time.time + proyectilC.cooldown;
        }
    }

    private void ShootProyectil(ProyectilData pData)
    {
        if (anim != null)
            anim.SetTrigger("Attack0");

        if (attackPoint != null && pData.prefab != null)
        {
            GameObject proyectilObj = Instantiate(pData.prefab, attackPoint.position, attackPoint.rotation);
            Proyectil p = proyectilObj.GetComponent<Proyectil>();
            if (p != null)
            {
                p.damage = pData.damage;
                p.speed = pData.speed;
                p.range = pData.range;

                float dir = transform.localScale.x >= 0 ? 1f : -1f;
                p.SetDirection(dir);
            }
        }
    }

    // ---------------- DAÑO Y VIDA ----------------
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateVidaText();

        if (vidaImagenesUI != null)
            vidaImagenesUI.UpdateHealthUI((float)currentHealth / maxHealth);

        if (currentHealth <= 0)
            Die();
        else if (anim != null)
            anim.SetTrigger("Hit");
    }

    // ---------------- MUERTE ----------------
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null)
            anim.SetTrigger("Die");

        if (movementScript != null)
            movementScript.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
        }

        // 🔊 Sonido de muerte
        PlaySound(muerteSound);

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "MORISTE";
            gameOverText.color = Color.red;
        }

        StartCoroutine(LoadSceneAfterDeath());
    }

    private System.Collections.IEnumerator LoadSceneAfterDeath()
    {
        // Esperar animación de muerte
        if (anim != null)
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(state.length);
        }

        // Esperar tiempo del mensaje
        yield return new WaitForSeconds(gameOverDisplayTime);

        // Cambiar escena
        if (!string.IsNullOrEmpty(sceneOnDeath))
            SceneManager.LoadScene(sceneOnDeath);
        else
            Debug.LogWarning("❌ No se asignó ninguna escena al morir en el Inspector");
    }

    // ---------------- UI ----------------
    private void UpdateVidaText()
    {
        if (vidaTMP != null)
            vidaTMP.text = $"❤️ Vida: {currentHealth}/{maxHealth}";
    }

    public int GetCurrentHealth() => currentHealth;

    // ---------------- AUDIO ----------------
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
