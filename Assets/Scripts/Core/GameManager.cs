/// <summary>
/// Gestiona el estado global de la partida: score y flujo de juego.
/// Singleton accesible desde cualquier script mediante GameManager.Instance.
/// </summary>
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────

    public static GameManager Instance { get; private set; }

    // ── Estado del juego ─────────────────────────────────────────────────────

    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
    }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    // ── Score ────────────────────────────────────────────────────────────────

    [Header("Puntuación")]
    [Tooltip("Puntos que otorga cada enemigo eliminado.")]
    [SerializeField]
    private int pointsPerKill = 100;

    public int Score { get; private set; } = 0;

    // ── Eventos ──────────────────────────────────────────────────────────────

    /// <summary>Se dispara cuando cambia la puntuación. Param: puntuación actual.</summary>
    public static event Action<int> OnScoreChanged;

    /// <summary>Se dispara cuando el juego termina.</summary>
    public static event Action OnGameOver;

    /// <summary>Se dispara cuando cambia el estado del juego.</summary>
    public static event Action<GameState> OnGameStateChanged;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
        EnemyHealth.OnEnemyDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
        EnemyHealth.OnEnemyDeath -= HandleEnemyDeath;
    }

    // ── Handlers de eventos ──────────────────────────────────────────────────

    private void HandlePlayerDeath()
    {
        if (CurrentState != GameState.Playing)
            return;

        Debug.Log("[GameManager] Jugador muerto — Game Over.");
        SetState(GameState.GameOver);
        OnGameOver?.Invoke();
    }

    private void HandleEnemyDeath(EnemyChromosome chromosome, CombatStats stats)
    {
        if (CurrentState != GameState.Playing)
            return;

        AddScore(pointsPerKill);
    }

    // ── Métodos públicos ─────────────────────────────────────────────────────

    /// <summary>
    /// Suma puntos al score y notifica al HUD.
    /// </summary>
    public void AddScore(int points)
    {
        Score += points;
        Debug.Log($"[GameManager] Score: {Score}");
        OnScoreChanged?.Invoke(Score);
    }

    /// <summary>
    /// Reinicia el score. Llamar al iniciar una nueva partida.
    /// </summary>
    public void ResetScore()
    {
        Score = 0;
        OnScoreChanged?.Invoke(Score);
    }

    // ── Estado ───────────────────────────────────────────────────────────────

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] Estado: {newState}");
        OnGameStateChanged?.Invoke(newState);
    }
}
