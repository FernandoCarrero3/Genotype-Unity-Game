/// <summary>
/// Representa el cromosoma (ADN) de un enemigo.
/// Clase de datos pura — no hereda de MonoBehaviour.
/// Cada gen es un float normalizado en [0, 1].
/// EnemyController mapea estos valores a rangos reales de comportamiento.
/// </summary>
[System.Serializable]
public class EnemyChromosome
{
    // ── Genes ────────────────────────────────────────────────────────────────

    /// <summary>Velocidad de movimiento. 0 = muy lento, 1 = muy rápido.</summary>
    public float speed;

    /// <summary>Agresividad (frecuencia de disparo). 0 = casi no dispara, 1 = dispara sin parar.</summary>
    public float aggression;

    /// <summary>Precisión del disparo. 0 = muy impreciso, 1 = disparo perfecto.</summary>
    public float precision;

    /// <summary>Distancia de combate preferida. 0 = cuerpo a cuerpo, 1 = máximo alcance.</summary>
    public float combatRange;

    /// <summary>Vitalidad. 0 = muy frágil, 1 = muy resistente.</summary>
    public float vitality;

    // ── Constructores ────────────────────────────────────────────────────────

    /// <summary>
    /// Constructor base vacío — necesario para serialización de Unity.
    /// No usar directamente. Usar CreateRandom() para cromosomas aleatorios.
    /// </summary>
    public EnemyChromosome()
    {
        speed = 0f;
        aggression = 0f;
        precision = 0f;
        combatRange = 0f;
    }

    /// <summary>
    /// Crea un cromosoma con genes completamente aleatorios.
    /// Usar este método en lugar del constructor vacío para la población inicial.
    /// </summary>
    public static EnemyChromosome CreateRandom()
    {
        return new EnemyChromosome(
            UnityEngine.Random.value,
            UnityEngine.Random.value,
            UnityEngine.Random.value,
            UnityEngine.Random.value,
            UnityEngine.Random.value
        );
    }

    /// <summary>
    /// Crea un cromosoma con genes explícitos.
    /// Usado por el motor genético al cruzar y mutar.
    /// </summary>
    public EnemyChromosome(float speed, float aggression, float precision, float combatRange, float vitality)
    {
        this.speed = Clamp01(speed);
        this.aggression = Clamp01(aggression);
        this.precision = Clamp01(precision);
        this.combatRange = Clamp01(combatRange);
        this.vitality = Clamp01(vitality);
    }

    // ── Utilidades ───────────────────────────────────────────────────────────

    /// <summary>Garantiza que un gen nunca sale del rango [0, 1].</summary>
    private static float Clamp01(float value)
    {
        return UnityEngine.Mathf.Clamp01(value);
    }

    /// <summary>Representación legible para debug en consola.</summary>
    public override string ToString()
    {
        return $"[Chromosome] Speed:{speed:F2} | Aggression:{aggression:F2} | "
            + $"Precision:{precision:F2} | CombatRange:{combatRange:F2} | Vitality:{vitality:F2}";
    }
}
