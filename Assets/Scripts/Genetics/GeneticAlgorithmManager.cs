/// <summary>
/// Núcleo del Motor Genético de Genotype.
/// Gestiona la evolución de los cromosomas enemigos entre oleadas.
///
/// Ciclo de vida:
/// 1. Recibe cromosomas + estadísticas al morir cada enemigo (OnEnemyDeath)
/// 2. Al terminar la oleada calcula el fitness de cada individuo
/// 3. Selecciona los mejores por torneo
/// 4. Genera nueva población mediante crossover y mutación
/// 5. Entrega los nuevos cromosomas al WaveManager
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithmManager : MonoBehaviour
{
    // ── Configuración ────────────────────────────────────────────────────────

    [Header("Población")]
    [Tooltip("Número de enemigos por oleada.")]
    [SerializeField]
    private int populationSize = 8;

    [Header("Selección")]
    [Tooltip("Número de individuos que compiten en cada torneo.")]
    [SerializeField]
    [Range(2, 5)]
    private int tournamentSize = 3;

    [Header("Crossover")]
    [Tooltip("Probabilidad de que cada gen se intercambie entre padres (0-1).")]
    [SerializeField]
    [Range(0f, 1f)]
    private float crossoverRate = 0.5f;

    [Header("Mutación")]
    [Tooltip("Probabilidad base de que un gen mute (0-1).")]
    [SerializeField]
    [Range(0f, 1f)]
    private float baseMutationRate = 0.1f;

    [Tooltip("Máxima variación al mutar un gen.")]
    [SerializeField]
    [Range(0f, 0.5f)]
    private float mutationStrength = 0.2f;

    [Header("Elitismo")]
    [Tooltip("Número de mejores individuos que pasan intactos a la siguiente generación.")]
    [SerializeField]
    [Range(0, 3)]
    private int eliteCount = 1;

    // ── Estado interno ───────────────────────────────────────────────────────

    /// <summary>Par cromosoma + estadísticas recogido al morir cada enemigo.</summary>
    private struct IndividualRecord
    {
        public EnemyChromosome chromosome;
        public CombatStats stats;
        public float fitness;
    }

    private List<IndividualRecord> currentGeneration = new List<IndividualRecord>();
    private float mutationRate;
    private int generationsWithoutImprovement = 0;
    private float bestFitnessLastGeneration = 0f;

    // ── Normalización de la oleada ───────────────────────────────────────────

    /// <summary>Máximo daño posible al jugador — se actualiza cada oleada.</summary>
    private float maxDamageThisWave = 1f;

    /// <summary>Máximo tiempo de vida registrado en la oleada.</summary>
    private float maxTimeAliveThisWave = 1f;

    // ── Pesos del Fitness ────────────────────────────────────────────────────

    private const float WEIGHT_DAMAGE = 0.40f;
    private const float WEIGHT_TIME = 0.25f;
    private const float WEIGHT_ACCURACY = 0.20f;
    private const float WEIGHT_HP = 0.15f;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        mutationRate = baseMutationRate;
    }

    private void OnEnable()
    {
        EnemyHealth.OnEnemyDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyDeath -= HandleEnemyDeath;
    }

    // ── Recepción de datos ───────────────────────────────────────────────────

    /// <summary>
    /// Se llama automáticamente cada vez que un enemigo muere.
    /// Acumula los datos para evaluar al final de la oleada.
    /// </summary>
    private void HandleEnemyDeath(EnemyChromosome chromosome, CombatStats stats)
    {
        if (chromosome == null || stats == null)
            return;

        // Actualizamos los máximos para normalización posterior
        if (stats.damageDealtToPlayer > maxDamageThisWave)
            maxDamageThisWave = stats.damageDealtToPlayer;

        if (stats.timeAlive > maxTimeAliveThisWave)
            maxTimeAliveThisWave = stats.timeAlive;

        IndividualRecord record = new IndividualRecord
        {
            chromosome = chromosome,
            stats = stats,
            fitness = 0f, // Se calcula al final de la oleada
        };

        currentGeneration.Add(record);
        Debug.Log($"[GA] Enemigo registrado. Total esta oleada: {currentGeneration.Count}");
    }

    // ── Fitness ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Calcula el fitness de todos los individuos de la generación actual.
    /// Fórmula ponderada normalizada — todos los factores en rango [0,1].
    /// </summary>
    private void EvaluateFitness()
    {
        // Necesitamos el HP máximo posible para normalizar hpRemaining
        float maxHp = Mathf.Lerp(1f, 10f, 1f); // vitality = 1 → HP máximo = 10

        for (int i = 0; i < currentGeneration.Count; i++)
        {
            IndividualRecord record = currentGeneration[i];
            CombatStats stats = record.stats;

            float damageFactor = stats.damageDealtToPlayer / maxDamageThisWave;
            float timeFactor = stats.timeAlive / maxTimeAliveThisWave;
            float accuracyFactor = stats.AccuracyRatio;
            float hpFactor = stats.hpRemaining / maxHp;

            float fitness =
                (damageFactor * WEIGHT_DAMAGE)
                + (timeFactor * WEIGHT_TIME)
                + (accuracyFactor * WEIGHT_ACCURACY)
                + (hpFactor * WEIGHT_HP);

            record.fitness = fitness;
            currentGeneration[i] = record;

            Debug.Log($"[GA] Individuo {i} | Fitness: {fitness:F3} | {stats}");
        }
    }

    // ── Selección ────────────────────────────────────────────────────────────

    /// <summary>
    /// Selección por torneo — elige al mejor de un subgrupo aleatorio.
    /// Más robusta que la ruleta y evita que un individuo domine completamente.
    /// </summary>
    private IndividualRecord TournamentSelection()
    {
        IndividualRecord best = currentGeneration[Random.Range(0, currentGeneration.Count)];

        for (int i = 1; i < tournamentSize; i++)
        {
            IndividualRecord candidate = currentGeneration[
                Random.Range(0, currentGeneration.Count)
            ];
            if (candidate.fitness > best.fitness)
                best = candidate;
        }

        return best;
    }

    // ── Crossover ────────────────────────────────────────────────────────────

    /// <summary>
    /// Crossover uniforme — cada gen se hereda del padre A o del padre B
    /// con probabilidad crossoverRate. Produce más diversidad que el crossover
    /// de un punto y es más adecuado para cromosomas cortos (4-5 genes).
    /// </summary>
    private EnemyChromosome UniformCrossover(EnemyChromosome parentA, EnemyChromosome parentB)
    {
        return new EnemyChromosome(
            speed: Random.value < crossoverRate ? parentA.speed : parentB.speed,
            aggression: Random.value < crossoverRate ? parentA.aggression : parentB.aggression,
            precision: Random.value < crossoverRate ? parentA.precision : parentB.precision,
            combatRange: Random.value < crossoverRate ? parentA.combatRange : parentB.combatRange,
            vitality: Random.value < crossoverRate ? parentA.vitality : parentB.vitality
        );
    }

    // ── Mutación ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Mutación gaussiana adaptativa.
    /// Cada gen tiene una probabilidad mutationRate de mutar.
    /// Si la población lleva varias generaciones sin mejorar,
    /// aumentamos la tasa de mutación para escapar de mínimos locales.
    /// </summary>
    private EnemyChromosome Mutate(EnemyChromosome chromosome)
    {
        return new EnemyChromosome(
            speed: MutateGene(chromosome.speed),
            aggression: MutateGene(chromosome.aggression),
            precision: MutateGene(chromosome.precision),
            combatRange: MutateGene(chromosome.combatRange),
            vitality: MutateGene(chromosome.vitality)
        );
    }

    /// <summary>
    /// Aplica mutación a un gen individual.
    /// La variación es aleatoria dentro de [-mutationStrength, +mutationStrength].
    /// </summary>
    private float MutateGene(float gene)
    {
        if (Random.value > mutationRate)
            return gene;
        float delta = Random.Range(-mutationStrength, mutationStrength);
        return Mathf.Clamp01(gene + delta);
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Genera la población inicial aleatoria.
    /// Llamar desde WaveManager al iniciar la primera oleada.
    /// </summary>
    public List<EnemyChromosome> GenerateInitialPopulation()
    {
        List<EnemyChromosome> population = new List<EnemyChromosome>();

        for (int i = 0; i < populationSize; i++)
            population.Add(EnemyChromosome.CreateRandom());

        Debug.Log($"[GA] Población inicial generada: {populationSize} individuos.");
        return population;
    }

    /// <summary>
    /// Evalúa la generación actual y produce la siguiente.
    /// Llamar desde WaveManager al terminar cada oleada.
    /// </summary>
    public List<EnemyChromosome> EvolveNextGeneration()
    {
        if (currentGeneration.Count == 0)
        {
            Debug.LogWarning(
                "[GA] EvolveNextGeneration llamado sin datos. Generando población aleatoria."
            );
            return GenerateInitialPopulation();
        }

        // Paso 1 — Calcular fitness
        EvaluateFitness();

        // Paso 2 — Ordenar por fitness descendente
        currentGeneration.Sort((a, b) => b.fitness.CompareTo(a.fitness));

        float bestFitness = currentGeneration[0].fitness;
        Debug.Log($"[GA] Mejor fitness esta generación: {bestFitness:F3}");

        // Paso 3 — Mutación adaptativa
        if (bestFitness <= bestFitnessLastGeneration)
        {
            generationsWithoutImprovement++;
            mutationRate = baseMutationRate * (1f + generationsWithoutImprovement * 0.5f);
            mutationRate = Mathf.Clamp(mutationRate, baseMutationRate, 0.8f);
            Debug.Log(
                $"[GA] Sin mejora ({generationsWithoutImprovement} gen.) — MutationRate: {mutationRate:F2}"
            );
        }
        else
        {
            generationsWithoutImprovement = 0;
            mutationRate = baseMutationRate;
        }
        bestFitnessLastGeneration = bestFitness;

        // Paso 4 — Construir nueva generación
        List<EnemyChromosome> nextGeneration = new List<EnemyChromosome>();

        // Elitismo — los mejores pasan intactos
        for (int i = 0; i < Mathf.Min(eliteCount, currentGeneration.Count); i++)
        {
            nextGeneration.Add(currentGeneration[i].chromosome);
            Debug.Log($"[GA] Élite {i} conservado | Fitness: {currentGeneration[i].fitness:F3}");
        }

        // Selección + Crossover + Mutación hasta completar la población
        while (nextGeneration.Count < populationSize)
        {
            EnemyChromosome parentA = TournamentSelection().chromosome;
            EnemyChromosome parentB = TournamentSelection().chromosome;
            EnemyChromosome child = UniformCrossover(parentA, parentB);
            EnemyChromosome mutated = Mutate(child);
            nextGeneration.Add(mutated);
        }

        Debug.Log($"[GA] Nueva generación lista: {nextGeneration.Count} cromosomas.");

        // Paso 5 — Resetear para la siguiente oleada
        ResetForNextWave();

        return nextGeneration;
    }

    /// <summary>
    /// Limpia los datos de la oleada anterior.
    /// Llamar siempre al final de EvolveNextGeneration.
    /// </summary>
    private void ResetForNextWave()
    {
        currentGeneration.Clear();
        maxDamageThisWave = 1f;
        maxTimeAliveThisWave = 1f;
    }
}
