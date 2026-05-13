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

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHealthBar;
        WaveManager.OnWaveStarted += UpdateWaveText;
        GameManager.OnScoreChanged += UpdateScoreText;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHealthBar;
        WaveManager.OnWaveStarted -= UpdateWaveText;
        GameManager.OnScoreChanged -= UpdateScoreText;
    }

    private void Start()
    {
        // Estado inicial del HUD
        UpdateScoreText(0);
        UpdateWaveText(1);

        if (healthBar != null)
            healthBar.value = 1f;
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
}
