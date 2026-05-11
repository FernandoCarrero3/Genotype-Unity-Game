/// <summary>
/// Gestiona la vida de un enemigo.
/// La vida máxima se mapea desde el gen vitality del cromosoma.
/// Expone eventos para que el GeneticAlgorithmManager calcule fitness.
/// </summary>
using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    // ── Constantes de mapeo ──────────────────────────────────────────────────

    private const float MIN_HP = 1f;
    private const float MAX_HP = 10f;

    // ── Estado ───────────────────────────────────────────────────────────────

    private float maxHp;
    private float currentHp;
    private CombatStats stats;
    private float birthTime;

    // ── Eventos ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Se dispara cuando el enemigo muere.
    /// Param: el cromosoma del enemigo muerto (para calcular fitness en el GA).
    /// </summary>
    public static event Action<EnemyChromosome, CombatStats> OnEnemyDeath;

    /// <summary>
    /// Se dispara cuando la vida cambia.
    /// Params: HP actual, HP máximo.
    /// Usado por la barra de vida en World Space (Sprint de HUD).
    /// </summary>
    public event Action<float, float> OnHealthChanged;

    // ── Propiedades públicas ─────────────────────────────────────────────────

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;

    // ── Inicialización ───────────────────────────────────────────────────────

    /// <summary>
    /// Inicializa la vida a partir del cromosoma.
    /// Llamar desde EnemyController justo después de SetChromosome().
    /// </summary>
    public void Initialize(EnemyChromosome chromosome)
    {
        maxHp = Mathf.Lerp(MIN_HP, MAX_HP, chromosome.vitality);
        currentHp = maxHp;
        stats = new CombatStats();
        birthTime = UnityEngine.Time.time;

        Debug.Log($"[EnemyHealth] HP inicializado: {currentHp:F1} / {maxHp:F1}");
        OnHealthChanged?.Invoke(currentHp, maxHp);
    }

    // ── Daño ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica daño al enemigo.
    /// Acepta float para soportar futuros tipos de munición con daño fraccionario.
    /// Llamar desde Projectile.cs al colisionar con un Enemy.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (currentHp <= 0f)
            return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0f);

        Debug.Log(
            $"[EnemyHealth] Daño recibido: {damage:F1} | HP restante: {currentHp:F1}/{maxHp:F1}"
        );
        OnHealthChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0f)
            Die();
    }

    // ── Muerte ───────────────────────────────────────────────────────────────

    private void Die()
    {
        stats.timeAlive = Time.time - birthTime;
        stats.hpRemaining = currentHp;

        Debug.Log($"[EnemyHealth] Enemigo eliminado: {gameObject.name}");
        Debug.Log(stats.ToString());

        EnemyController controller = GetComponent<EnemyController>();
        EnemyChromosome chromosome = controller != null ? controller.Chromosome : null;

        OnEnemyDeath?.Invoke(chromosome, stats);
        Destroy(gameObject);
    }

    /// <summary>
    /// Registra daño infligido al jugador.
    /// Llamar desde EnemyProjectile cuando impacta al jugador.
    /// </summary>
    public void RegisterDamageDealt(float damage)
    {
        if (stats == null)
            return;
        stats.damageDealtToPlayer += damage;
    }

    /// <summary>
    /// Registra un disparo realizado.
    /// Llamar desde EnemyController cada vez que dispara.
    /// </summary>
    public void RegisterShot(bool hit)
    {
        if (stats == null)
            return;
        stats.shotsFired++;
        if (hit)
            stats.shotsHit++;
    }
}
