/// <summary>
/// Controlador de la pantalla de Ajustes.
/// Gestiona vidas configurables y volumen general.
/// Los valores se persisten con PlayerPrefs y están disponibles
/// desde cualquier escena mediante SettingsController.GetLives()
/// y SettingsController.GetVolume().
/// </summary>
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    // ── Claves PlayerPrefs ───────────────────────────────────────────────────
    // Constantes públicas para que otros scripts puedan leer los ajustes
    // sin hardcodear strings.

    public const string KEY_LIVES = "setting_lives";
    public const string KEY_VOLUME = "setting_volume";

    // ── Valores por defecto ──────────────────────────────────────────────────

    private const int DEFAULT_LIVES = 3;
    private const float DEFAULT_VOLUME = 1f;

    // ── Referencias UI ───────────────────────────────────────────────────────

    [Header("Vidas")]
    [SerializeField]
    private Slider livesSlider;

    [SerializeField]
    private TMP_Text livesValueText;

    [Header("Volumen")]
    [SerializeField]
    private Slider volumeSlider;

    [SerializeField]
    private TMP_Text volumeValueText;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Start()
    {
        LoadSettings();
    }

    // ── Carga y guardado ─────────────────────────────────────────────────────

    /// <summary>
    /// Lee los valores guardados en PlayerPrefs y los aplica a los sliders.
    /// Si no hay valores guardados usa los valores por defecto.
    /// </summary>
    private void LoadSettings()
    {
        float lives = PlayerPrefs.GetFloat(KEY_LIVES, DEFAULT_LIVES);
        float volume = PlayerPrefs.GetFloat(KEY_VOLUME, DEFAULT_VOLUME);

        // Aplicar a sliders — esto dispara OnValueChanged y actualiza los textos
        livesSlider.value = lives;
        volumeSlider.value = volume;
    }

    /// <summary>
    /// Guarda los valores actuales en PlayerPrefs.
    /// Se llama al pulsar VOLVER para no guardar cambios a medias.
    /// </summary>
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(KEY_LIVES, livesSlider.value);
        PlayerPrefs.SetFloat(KEY_VOLUME, volumeSlider.value);
        PlayerPrefs.Save();

        Debug.Log(
            $"[Settings] Guardado — Vidas: {(int)livesSlider.value} | Volumen: {volumeSlider.value:P0}"
        );
    }

    // ── Callbacks de sliders ─────────────────────────────────────────────────

    /// <summary>
    /// Llamado por el evento OnValueChanged del LivesSlider.
    /// Actualiza el texto en tiempo real.
    /// </summary>
    public void OnLivesChanged(float value)
    {
        livesValueText.text = ((int)value).ToString();
    }

    /// <summary>
    /// Llamado por el evento OnValueChanged del VolumeSlider.
    /// Actualiza el texto en tiempo real y aplica el volumen global.
    /// </summary>
    public void OnVolumeChanged(float value)
    {
        volumeValueText.text = value.ToString("P0"); // "75%" etc.
        AudioListener.volume = value;
    }

    // ── Navegación ───────────────────────────────────────────────────────────

    /// <summary>Guarda y vuelve al Menú Principal.</summary>
    public void OnBackButton()
    {
        SaveSettings();
        SceneManager.LoadScene("MainMenu");
    }

    // ── API estática ─────────────────────────────────────────────────────────
    // Métodos de conveniencia para que GameManager pueda leer los ajustes
    // sin necesitar una referencia al SettingsController.

    /// <summary>Devuelve las vidas configuradas. Por defecto 3.</summary>
    public static int GetLives()
    {
        return (int)PlayerPrefs.GetFloat(KEY_LIVES, DEFAULT_LIVES);
    }

    /// <summary>Devuelve el volumen configurado. Por defecto 1.</summary>
    public static float GetVolume()
    {
        return PlayerPrefs.GetFloat(KEY_VOLUME, DEFAULT_VOLUME);
    }

    /// <summary>Guarda el volumen directamente. Usado desde otras escenas (ej. pausa).</summary>
    public static void SaveVolume(float volume)
    {
        PlayerPrefs.SetFloat(KEY_VOLUME, volume);
        PlayerPrefs.Save();
    }
}
