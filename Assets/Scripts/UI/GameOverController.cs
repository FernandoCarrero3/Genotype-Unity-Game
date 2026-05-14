/// <summary>
/// Controlador legacy del panel de Game Over en GameScene.
/// En Sprint 6 la lógica de Game Over se movió a la escena GameOver.
/// Este script solo libera el cursor para que la transición funcione.
/// </summary>
using UnityEngine;

public class GameOverController : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        // Liberamos cursor y timeScale para que la corrutina
        // de carga de escena pueda ejecutarse correctamente
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[GameOverController] Preparando transición a escena GameOver...");
    }
}
