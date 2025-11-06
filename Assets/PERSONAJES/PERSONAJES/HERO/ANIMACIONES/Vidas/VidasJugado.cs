using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Imágenes de vida (de más a menos)")]
    public Image vida100;
    public Image vida75;
    public Image vida50;
    public Image vida25;
    public Image vida0;

    private Image[] images;
    private int currentState = -1;

    void Start()
    {
        // Crear arreglo de imágenes
        images = new Image[] { vida100, vida75, vida50, vida25, vida0 };
        // Inicializar vida completa
        UpdateHealthUI(1f);
    }

    public void UpdateHealthUI(float healthPercent)
    {
        if (images == null || images.Length == 0) return;

        int newState;

        if (healthPercent > 0.8f) newState = 0;
        else if (healthPercent > 0.6f) newState = 1;
        else if (healthPercent > 0.4f) newState = 2;
        else if (healthPercent > 0.2f) newState = 3;
        else newState = 4;

        if (newState == currentState) return;
        currentState = newState;

        // Activar solo la imagen correspondiente
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null)
                images[i].gameObject.SetActive(i == newState);
        }
    }
}
