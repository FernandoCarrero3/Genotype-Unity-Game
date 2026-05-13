/// <summary>
/// Gestiona la persistencia del top-10 de puntuaciones.
/// Guarda y carga los récords usando JSON + PlayerPrefs.
/// Singleton accesible desde cualquier escena.
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────

    public static ScoreManager Instance { get; private set; }

    // ── Constantes ───────────────────────────────────────────────────────────

    private const string SCORES_KEY = "Genotype_Scores";
    private const int MAX_ENTRIES = 10;

    // ── Eventos ──────────────────────────────────────────────────────────────

    /// <summary>Se dispara cuando la tabla de récords cambia.</summary>
    public static event Action<List<ScoreEntry>> OnLeaderboardUpdated;

    // ── Estado ───────────────────────────────────────────────────────────────

    private List<ScoreEntry> leaderboard = new List<ScoreEntry>();

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadScores();
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Comprueba si una puntuación entra en el top-10.
    /// Llamar antes de pedir el nombre al jugador.
    /// </summary>
    public bool IsHighScore(int score)
    {
        if (leaderboard.Count < MAX_ENTRIES)
            return true;
        return score > leaderboard[leaderboard.Count - 1].score;
    }

    /// <summary>
    /// Añade una entrada al leaderboard, ordena y recorta a top-10.
    /// Llamar desde la pantalla de Game Over tras introducir el nombre.
    /// </summary>
    public void AddScore(string playerName, int score)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Anónimo";

        ScoreEntry entry = new ScoreEntry
        {
            playerName = playerName.Trim(),
            score = score,
            date = DateTime.Now.ToString("dd/MM/yyyy"),
        };

        leaderboard.Add(entry);

        // Ordenar descendente por puntuación
        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

        // Recortar al top-10
        if (leaderboard.Count > MAX_ENTRIES)
            leaderboard.RemoveRange(MAX_ENTRIES, leaderboard.Count - MAX_ENTRIES);

        SaveScores();
        OnLeaderboardUpdated?.Invoke(leaderboard);

        Debug.Log($"[ScoreManager] Entrada añadida: {entry.playerName} — {entry.score:N0}");
    }

    /// <summary>
    /// Devuelve una copia de la tabla de récords actual.
    /// </summary>
    public List<ScoreEntry> GetLeaderboard()
    {
        return new List<ScoreEntry>(leaderboard);
    }

    /// <summary>
    /// Borra todos los récords guardados.
    /// Útil para debug y para el menú de ajustes.
    /// </summary>
    public void ClearScores()
    {
        leaderboard.Clear();
        PlayerPrefs.DeleteKey(SCORES_KEY);
        PlayerPrefs.Save();
        OnLeaderboardUpdated?.Invoke(leaderboard);
        Debug.Log("[ScoreManager] Récords borrados.");
    }

    // ── Persistencia ─────────────────────────────────────────────────────────

    /// <summary>
    /// Serializa la lista a JSON y la guarda en PlayerPrefs.
    /// </summary>
    private void SaveScores()
    {
        ScoreListWrapper wrapper = new ScoreListWrapper { entries = leaderboard };
        string json = JsonUtility.ToJson(wrapper, prettyPrint: true);
        PlayerPrefs.SetString(SCORES_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[ScoreManager] Récords guardados:\n{json}");
    }

    /// <summary>
    /// Carga y deserializa el JSON desde PlayerPrefs.
    /// Si no hay datos previos inicializa la lista vacía.
    /// </summary>
    private void LoadScores()
    {
        if (!PlayerPrefs.HasKey(SCORES_KEY))
        {
            leaderboard = new List<ScoreEntry>();
            Debug.Log("[ScoreManager] No hay récords previos.");
            return;
        }

        string json = PlayerPrefs.GetString(SCORES_KEY);
        ScoreListWrapper wrapper = JsonUtility.FromJson<ScoreListWrapper>(json);

        if (wrapper != null && wrapper.entries != null)
        {
            leaderboard = wrapper.entries;
            Debug.Log($"[ScoreManager] {leaderboard.Count} récords cargados.");
        }
        else
        {
            leaderboard = new List<ScoreEntry>();
            Debug.LogWarning("[ScoreManager] Error al cargar récords — lista reiniciada.");
        }
    }
}
