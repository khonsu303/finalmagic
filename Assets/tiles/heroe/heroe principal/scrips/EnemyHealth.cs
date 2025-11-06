using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 50;
    private int currentHealth;
    private bool isDead = false;

    [Header("UI")]
    public Slider healthBar;            // 👈 Asigna el Slider del Canvas del enemigo
    public TextMeshProUGUI vidaTMP;     // Texto (opcional) para mostrar la vida numérica
    public Canvas worldCanvas;          // Canvas en World Space, hijo del enemigo
    public Vector3 barOffset = new Vector3(0, 1.5f, 0); // Altura sobre el enemigo

    private Animator anim;
    private Rigidbody2D rb;
    private EnemyAI enemyAI;
    private Transform mainCamera;       // para rotar la barra si quieres estilo 3D

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
        mainCamera = Camera.main?.transform;

        // Configura la barra de vida inicial
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }

        UpdateVidaText();
    }

    void Update()
    {
        // Mantener la barra sobre el enemigo
        if (worldCanvas != null)
            worldCanvas.transform.position = transform.position + barOffset;

        // Si es 3D o quieres que siempre mire a la cámara
        if (mainCamera != null && worldCanvas != null)
            worldCanvas.transform.rotation = mainCamera.rotation;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateVidaText();

        // 🔄 Actualiza la barra
        if (healthBar != null)
            healthBar.value = currentHealth;

        Debug.Log($"{gameObject.name} recibió {amount} de daño. Vida actual: {currentHealth}");

        if (currentHealth <= 0)
            Die();
        else if (anim != null)
            anim.SetTrigger("Hit");
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} murió.");
        anim.SetTrigger("Die");

        // Desactivar IA y física
        if (enemyAI != null)
            enemyAI.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
        }

        // Ocultar la barra después de morir
        if (worldCanvas != null)
            worldCanvas.enabled = false;

        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(2f); // espera duración de animación
        Destroy(gameObject);
    }

    private void UpdateVidaText()
    {
        if (vidaTMP != null)
            vidaTMP.text = $"{currentHealth}/{maxHealth}";
    }
}
