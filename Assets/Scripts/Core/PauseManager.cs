/// <summary>
/// Gestiona el estado de pausa del juego.
/// Controla Time.timeScale y el cursor.
/// Singleton ligero — vive en el mismo GameObject que GameManager.
/// </summary>
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────

    public static PauseManager Instance { get; private set; }

    // ── Estado ───────────────────────────────────────────────────────────────

    public bool IsPaused { get; private set; } = false;

    // ── Eventos ──────────────────────────────────────────────────────────────

    /// <summary>Se dispara al pausar o reanudar. True = pausado.</summary>
    public static event System.Action<bool> OnPauseChanged;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Garantizar estado limpio al iniciar cualquier escena
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsPaused = false;
    }

    private void Update()
    {
        // Escape alterna pausa — solo en GameScene
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // ── Métodos públicos ─────────────────────────────────────────────────────

    /// <summary>Alterna entre pausado y reanudado.</summary>
    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    /// <summary>Pausa el juego.</summary>
    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnPauseChanged?.Invoke(true);
        Debug.Log("[PauseManager] Juego pausado.");
    }

    /// <summary>Reanuda el juego.</summary>
    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnPauseChanged?.Invoke(false);
        Debug.Log("[PauseManager] Juego reanudado.");
    }

    /// <summary>
    /// Vuelve al menú principal.
    /// Siempre restaura timeScale antes de cambiar de escena.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[PauseManager] Volviendo al menú principal.");
        SceneManager.LoadScene("MainMenu");
    }
}
