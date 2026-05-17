/// <summary>
/// Controla el HUD de la partida.
/// Escucha los eventos del GameManager, PlayerHealth y WaveManager
/// y actualiza los elementos visuales en pantalla.
/// </summary>
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    // ── Referencias UI ───────────────────────────────────────────────────────

    [Header("Vida")]
    [Tooltip("Slider que representa la barra de vida del jugador.")]
    [SerializeField]
    private Slider healthBar;

    [Header("Oleada")]
    [Tooltip("Texto que muestra la oleada actual.")]
    [SerializeField]
    private TextMeshProUGUI waveText;

    [Header("Puntuación")]
    [Tooltip("Texto que muestra la puntuación actual.")]
    [SerializeField]
    private TextMeshProUGUI scoreText;

    [Header("Pausa")]
    [Tooltip("Panel de pausa — se activa/desactiva según el estado.")]
    [SerializeField]
    private GameObject pausePanel;

    [Tooltip("Slider de volumen en el panel de pausa.")]
    [SerializeField]
    private Slider volumeSlider;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHealthBar;
        WaveManager.OnWaveStarted += UpdateWaveText;
        GameManager.OnScoreChanged += UpdateScoreText;
        PauseManager.OnPauseChanged += UpdatePausePanel;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHealthBar;
        WaveManager.OnWaveStarted -= UpdateWaveText;
        GameManager.OnScoreChanged -= UpdateScoreText;
        PauseManager.OnPauseChanged -= UpdatePausePanel;
    }

    private void Start()
    {
        // Estado inicial del HUD
        UpdateScoreText(0);
        UpdateWaveText(1);

        if (healthBar != null)
            healthBar.value = 1f;

        if (volumeSlider != null)
        {
            volumeSlider.value = SettingsController.GetVolume();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    // ── Actualizaciones ──────────────────────────────────────────────────────

    /// <summary>
    /// Actualiza la barra de vida. Recibe vida actual y máxima desde PlayerHealth.
    /// </summary>
    private void UpdateHealthBar(int current, int max)
    {
        if (healthBar == null)
            return;
        healthBar.value = (float)current / max;
    }

    /// <summary>
    /// Actualiza el texto de oleada. Recibe el número desde WaveManager.
    /// </summary>
    private void UpdateWaveText(int wave)
    {
        if (waveText == null)
            return;
        waveText.text = $"Oleada {wave}";
    }

    /// <summary>
    /// Actualiza el texto de puntuación. Recibe el score desde GameManager.
    /// </summary>
    private void UpdateScoreText(int score)
    {
        if (scoreText == null)
            return;
        scoreText.text = score.ToString("N0");
    }

    /// <summary>
    /// Muestra u oculta el panel de pausa según el estado.
    /// </summary>
    private void UpdatePausePanel(bool isPaused)
    {
        if (pausePanel == null)
            return;
        pausePanel.SetActive(isPaused);
    }

    /// <summary>
    /// Llamado cuando el jugador mueve el slider de volumen en pausa.
    /// Aplica el volumen inmediatamente y lo guarda en Settings.
    /// </summary>
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        SettingsController.SaveVolume(value);
        Debug.Log($"[HUDController] Volumen ajustado: {value:F2}");
    }
}
