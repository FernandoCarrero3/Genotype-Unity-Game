/// <summary>
/// Contenedor de estadísticas de combate de un enemigo.
/// Se rellena durante la vida del enemigo y se envía al
/// GeneticAlgorithmManager al morir para calcular el fitness.
/// Clase de datos pura — no hereda de MonoBehaviour.
/// </summary>
[System.Serializable]
public class CombatStats
{
    // ── Estadísticas ─────────────────────────────────────────────────────────

    /// <summary>Daño total infligido al jugador durante su vida.</summary>
    public float damageDealtToPlayer;

    /// <summary>Segundos que el enemigo estuvo vivo.</summary>
    public float timeAlive;

    /// <summary>Disparos totales realizados.</summary>
    public int shotsFired;

    /// <summary>Disparos que impactaron al jugador.</summary>
    public int shotsHit;

    /// <summary>HP restante al terminar la oleada (0 si murió en combate).</summary>
    public float hpRemaining;

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// Inicializa todas las estadísticas a cero.
    /// </summary>
    public CombatStats()
    {
        damageDealtToPlayer = 0f;
        timeAlive = 0f;
        shotsFired = 0;
        shotsHit = 0;
        hpRemaining = 0f;
    }

    // ── Utilidades ───────────────────────────────────────────────────────────

    /// <summary>
    /// Ratio de precisión real: impactos / disparos totales.
    /// Devuelve 0 si no ha disparado para evitar división por cero.
    /// </summary>
    public float AccuracyRatio => shotsFired > 0 ? (float)shotsHit / shotsFired : 0f;

    /// <summary>Representación legible para debug en consola.</summary>
    public override string ToString()
    {
        return $"[CombatStats] Damage:{damageDealtToPlayer:F1} | "
            + $"TimeAlive:{timeAlive:F1}s | "
            + $"Shots:{shotsFired} | Hits:{shotsHit} | "
            + $"Accuracy:{AccuracyRatio:P0} | "
            + $"HPRemaining:{hpRemaining:F1}";
    }
}
