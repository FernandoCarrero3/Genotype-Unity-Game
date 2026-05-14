/// <summary>
/// Controlador del Menú Principal.
/// Gestiona la navegación entre escenas desde la pantalla de inicio.
/// </summary>
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // ── Nombres de escena ────────────────────────────────────────────────────
    // Constantes para evitar strings sueltos por el código — si renombramos
    // una escena, solo hay que cambiarla aquí.

    private const string SCENE_GAME = "GameScene";
    private const string SCENE_SETTINGS = "Settings";
    private const string SCENE_RECORDS = "Leaderboard";

    // ── Botones públicos ─────────────────────────────────────────────────────

    /// <summary>Carga la escena de juego.</summary>
    public void OnPlayButton()
    {
        Debug.Log("[MainMenu] Iniciando partida...");
        SceneManager.LoadScene(SCENE_GAME);
    }

    /// <summary>Carga la pantalla de ajustes.</summary>
    public void OnSettingsButton()
    {
        Debug.Log("[MainMenu] Abriendo ajustes...");
        SceneManager.LoadScene(SCENE_SETTINGS);
    }

    /// <summary>Carga la tabla de récords.</summary>
    public void OnRecordsButton()
    {
        Debug.Log("[MainMenu] Abriendo récords...");
        SceneManager.LoadScene(SCENE_RECORDS);
    }
}
