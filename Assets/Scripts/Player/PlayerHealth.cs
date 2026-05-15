/// <summary>
/// Gestiona la vida del jugador.
///
/// Características:
///   - Vida máxima configurable desde el Inspector.
///   - Método público TakeDamage() que llamarán los proyectiles enemigos.
///   - Evento OnPlayerDeath que otros sistemas pueden escuchar (HUD, GameManager...).
///   - Invulnerabilidad temporal tras recibir daño para evitar hits múltiples.
/// </summary>
using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("Vida")]
    [Tooltip("Vida máxima del jugador.")]
    [SerializeField]
    private int maxHealth = 3;

    [Tooltip("Segundos de invulnerabilidad tras recibir daño.")]
    [SerializeField]
    private float invulnerabilityDuration = 1.5f;

    // ─── Eventos ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Se dispara cuando el jugador muere.
    /// El GameManager escuchará este evento para cambiar de escena.
    /// </summary>
    public static event Action OnPlayerDeath;

    /// <summary>
    /// Se dispara cada vez que la vida cambia.
    /// El HUD escuchará este evento para actualizar los iconos de vida.
    /// Parámetros: vida actual, vida máxima.
    /// </summary>
    public static event Action<int, int> OnHealthChanged;

    // ─── Estado privado ───────────────────────────────────────────────────────

    /// <summary>Vida actual del jugador.</summary>
    private int currentHealth;

    /// <summary>True mientras el jugador es invulnerable tras recibir daño.</summary>
    private bool isInvulnerable = false;

    // ─── Propiedades públicas ─────────────────────────────────────────────────

    /// <summary>Vida actual (solo lectura desde fuera).</summary>
    public int CurrentHealth => currentHealth;

    /// <summary>Vida máxima (solo lectura desde fuera).</summary>
    public int MaxHealth => maxHealth;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        // Awake no lee Settings — otros Awake pueden no haber corrido aún.
        // La inicialización real ocurre en Start().
    }

    private void Start()
    {
        // Leemos las vidas configuradas por el jugador en la pantalla de Ajustes.
        // Si no hay datos guardados, SettingsController devuelve el valor por defecto (3).
        maxHealth = SettingsController.GetLives();
        currentHealth = maxHealth;

        Debug.Log(
            $"[PlayerHealth] Vida inicializada: {currentHealth}/{maxHealth} (desde Settings)"
        );

        // Notificamos al HUD para que dibuje la barra correctamente desde el inicio.
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ─── Métodos públicos ─────────────────────────────────────────────────────

    /// <summary>
    /// Aplica daño al jugador.
    /// Llamar desde el script del proyectil enemigo al colisionar.
    /// </summary>
    /// <param name="damage">Cantidad de daño a aplicar.</param>
    public void TakeDamage(int damage)
    {
        // Si es invulnerable ignoramos el golpe completamente.
        if (isInvulnerable)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Nunca por debajo de 0.

        // Notificamos al HUD del cambio.
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"[PlayerHealth] Daño recibido: {damage} | Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Activamos invulnerabilidad temporal.
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    /// <summary>
    /// Recupera vida. Útil para power-ups futuros.
    /// </summary>
    /// <param name="amount">Cantidad de vida a recuperar.</param>
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Nunca por encima del máximo.

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"[PlayerHealth] Curado: {amount} | Vida actual: {currentHealth}");
    }

    // ─── Métodos privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Gestiona la muerte del jugador.
    /// Dispara el evento para que el GameManager reaccione.
    /// </summary>
    private void Die()
    {
        Debug.Log("[PlayerHealth] El jugador ha muerto.");

        // Desactivamos el objeto para que no siga recibiendo input ni colisiones.
        gameObject.SetActive(false);

        // Notificamos a todos los sistemas suscritos.
        OnPlayerDeath?.Invoke();
    }

    /// <summary>
    /// Corrutina que activa y desactiva la invulnerabilidad temporal.
    /// </summary>
    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        yield return new WaitForSeconds(invulnerabilityDuration);

        isInvulnerable = false;
    }
}
