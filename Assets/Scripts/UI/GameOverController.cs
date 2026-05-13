/// <summary>
/// Controla el panel de Game Over.
/// Gestiona la puntuación final, entrada de nombre para récords y reinicio.
/// </summary>
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    // ── Referencias UI ───────────────────────────────────────────────────────

    [Header("Panel")]
    [SerializeField]
    private GameObject gameOverPanel;

    [Header("Textos")]
    [SerializeField]
    private TextMeshProUGUI finalScoreText;

    [Header("Nombre")]
    [Tooltip("Panel que aparece solo si el jugador entra en el top-10.")]
    [SerializeField]
    private GameObject nameInputPanel;

    [Tooltip("Campo de texto donde el jugador escribe su nombre.")]
    [SerializeField]
    private TMP_InputField nameInputField;

    // ── Estado ───────────────────────────────────────────────────────────────

    private int finalScore;
    private bool scoreSaved = false;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= HandleGameOver;
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void HandleGameOver()
    {
        finalScore = GameManager.Instance != null ? GameManager.Instance.Score : 0;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = $"Puntuación: {finalScore:N0}";

        // Mostrar campo de nombre solo si es un récord
        bool isHighScore =
            ScoreManager.Instance != null && ScoreManager.Instance.IsHighScore(finalScore);

        if (nameInputPanel != null)
            nameInputPanel.SetActive(isHighScore);

        // Si no es récord guardamos directamente con nombre vacío
        if (!isHighScore)
            SaveScore("---");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        Debug.Log(
            $"[GameOverController] Game Over — Score: {finalScore} | " + $"¿Récord?: {isHighScore}"
        );
    }

    // ── Botones ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Confirma el nombre introducido y guarda el récord.
    /// Asignar al OnClick de un botón "Confirmar" dentro de NameInputPanel.
    /// </summary>
    public void OnConfirmNameButton()
    {
        if (scoreSaved)
            return;

        string playerName = nameInputField != null ? nameInputField.text : "Anónimo";

        SaveScore(playerName);

        // Ocultamos el panel de nombre tras confirmar
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);
    }

    /// <summary>
    /// Reinicia la partida recargando la escena actual.
    /// Si el jugador no confirmó su nombre, guardamos con "Anónimo".
    /// </summary>
    public void OnRestartButton()
    {
        if (!scoreSaved)
            SaveScore("Anónimo");

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameManager.Instance?.ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ── Persistencia ─────────────────────────────────────────────────────────

    private void SaveScore(string playerName)
    {
        if (scoreSaved)
            return;
        ScoreManager.Instance?.AddScore(playerName, finalScore);
        scoreSaved = true;
    }
}
