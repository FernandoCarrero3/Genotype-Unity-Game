/// <summary>
/// Controlador de la escena Game Over.
/// Lee el score final y la oleada alcanzada desde GameManager,
/// determina si es un nuevo récord y gestiona el guardado del nombre.
/// </summary>
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverSceneController : MonoBehaviour
{
    // ── Referencias UI ───────────────────────────────────────────────────────

    [Header("Textos informativos")]
    [SerializeField]
    private TMP_Text finalScoreText;

    [SerializeField]
    private TMP_Text waveReachedText;

    [Header("Panel de nombre (solo si es récord)")]
    [SerializeField]
    private GameObject nameInputPanel;

    [SerializeField]
    private TMP_InputField nameInputField;

    // ── Estado interno ───────────────────────────────────────────────────────

    private int finalScore;
    private int finalWave;
    private bool scoreSaved = false;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Start()
    {
        // Liberar cursor — puede venir bloqueado desde GameScene
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ReadResultsFromGameManager();
        DisplayResults();
        CheckIfNewRecord();
    }

    // ── Inicialización ───────────────────────────────────────────────────────

    /// <summary>
    /// Lee score y oleada desde GameManager si existe.
    /// Si no existe (escena abierta directamente en editor) usa valores de prueba.
    /// </summary>
    private void ReadResultsFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            finalScore = GameManager.Instance.Score;
            finalWave = GameManager.Instance.CurrentWave;
        }
        else
        {
            // Fallback para pruebas en editor abriendo la escena directamente
            Debug.LogWarning(
                "[GameOverScene] GameManager no encontrado — usando valores de prueba."
            );
            finalScore = 0;
            finalWave = 1;
        }
    }

    /// <summary>
    /// Actualiza los textos de puntuación y oleada en pantalla.
    /// </summary>
    private void DisplayResults()
    {
        finalScoreText.text = $"Puntuación: {finalScore}";
        waveReachedText.text = $"Oleada alcanzada: {finalWave}";
    }

    /// <summary>
    /// Comprueba si el score actual entra en el top-10.
    /// Muestra u oculta el panel de nombre según el resultado.
    /// </summary>
    private void CheckIfNewRecord()
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.IsHighScore(finalScore))
        {
            nameInputPanel.SetActive(true);
            Debug.Log("[GameOverScene] ¡Nuevo récord!");
        }
        else
        {
            nameInputPanel.SetActive(false);
            Debug.Log("[GameOverScene] No es récord — panel de nombre oculto.");
        }
    }

    // ── Botones ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Guarda el récord con el nombre introducido y oculta el panel.
    /// Protegido con flag para evitar doble guardado.
    /// </summary>
    public void OnConfirmButton()
    {
        if (scoreSaved)
            return;

        string playerName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
            playerName = "Anónimo";

        ScoreManager.Instance.AddScore(playerName, finalScore);
        scoreSaved = true;

        nameInputPanel.SetActive(false);
        Debug.Log($"[GameOverScene] Récord guardado — {playerName}: {finalScore}");
    }

    /// <summary>
    /// Vuelve al Menú Principal.
    /// Destruye el GameManager para que la próxima partida empiece limpia.
    /// </summary>
    public void OnMainMenuButton()
    {
        // Destruir GameManager para que al volver a jugar empiece desde cero
        if (GameManager.Instance != null)
            Destroy(GameManager.Instance.gameObject);

        SceneManager.LoadScene("MainMenu");
    }
}
