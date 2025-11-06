using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    // Llamar desde el botón "Jugar"
    public void Jugar()
    {
        SceneManager.LoadScene("NIVEL 1");
    }

    // Llamar desde el botón "Salir"
    public void Salir()
    {
        Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        // Detiene el juego si estás en el Editor de Unity
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Cierra el juego si está compilado
        Application.Quit();
#endif
    }
}
