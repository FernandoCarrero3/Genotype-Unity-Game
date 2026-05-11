/// <summary>
/// Gestiona las oleadas de enemigos.
/// Coordina el spawn de enemigos con el GeneticAlgorithmManager
/// para que cada oleada use cromosomas evolucionados.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    // ── Referencias ──────────────────────────────────────────────────────────

    [Header("Referencias")]
    [Tooltip("Prefab del enemigo a spawnear.")]
    [SerializeField]
    private GameObject enemyPrefab;

    [Tooltip("Referencia al GeneticAlgorithmManager en escena.")]
    [SerializeField]
    private GeneticAlgorithmManager geneticManager;

    // ── Configuración de Spawn ───────────────────────────────────────────────

    [Header("Spawn")]
    [Tooltip("Puntos donde pueden aparecer los enemigos.")]
    [SerializeField]
    private Transform[] spawnPoints;

    [Tooltip("Segundos entre el spawn de cada enemigo.")]
    [SerializeField]
    private float spawnInterval = 1f;

    [Tooltip("Segundos de pausa entre oleadas.")]
    [SerializeField]
    private float timeBetweenWaves = 5f;

    // ── Estado ───────────────────────────────────────────────────────────────

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;
    private List<EnemyChromosome> currentChromosomes;

    // ── Eventos ──────────────────────────────────────────────────────────────

    /// <summary>Se dispara al comenzar una oleada. Param: número de oleada.</summary>
    public static event System.Action<int> OnWaveStarted;

    /// <summary>Se dispara al terminar una oleada. Param: número de oleada.</summary>
    public static event System.Action<int> OnWaveCompleted;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void OnEnable()
    {
        EnemyHealth.OnEnemyDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyDeath -= HandleEnemyDeath;
    }

    private void Start()
    {
        // Validaciones
        if (geneticManager == null)
        {
            Debug.LogError("[WaveManager] GeneticAlgorithmManager no asignado.");
            return;
        }
        if (enemyPrefab == null)
        {
            Debug.LogError("[WaveManager] EnemyPrefab no asignado.");
            return;
        }

        // Generamos la población inicial y arrancamos
        currentChromosomes = geneticManager.GenerateInitialPopulation();
        StartCoroutine(StartWaveRoutine());
    }

    // ── Oleadas ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Corrutina principal — spawna enemigos uno a uno con intervalo.
    /// </summary>
    private IEnumerator StartWaveRoutine()
    {
        currentWave++;
        waveInProgress = true;
        enemiesAlive = currentChromosomes.Count;

        Debug.Log(
            $"[WaveManager] ── Oleada {currentWave} iniciada "
                + $"({currentChromosomes.Count} enemigos) ──"
        );

        OnWaveStarted?.Invoke(currentWave);

        // Spawneamos cada enemigo con su cromosoma
        for (int i = 0; i < currentChromosomes.Count; i++)
        {
            SpawnEnemy(currentChromosomes[i], i);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// Instancia un enemigo en un punto de spawn y le asigna su cromosoma.
    /// </summary>
    private void SpawnEnemy(EnemyChromosome chromosome, int index)
    {
        // Elegimos punto de spawn — rotamos entre los disponibles
        Transform spawnPoint = spawnPoints[index % spawnPoints.Length];

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Asignamos el cromosoma evolucionado
        EnemyController controller = enemyObj.GetComponent<EnemyController>();
        if (controller != null)
            controller.SetChromosome(chromosome);

        Debug.Log(
            $"[WaveManager] Enemigo {index + 1} spawneado en {spawnPoint.name} " + $"| {chromosome}"
        );
    }

    // ── Detección de fin de oleada ───────────────────────────────────────────

    /// <summary>
    /// Se llama cada vez que un enemigo muere.
    /// Cuando todos han muerto termina la oleada.
    /// </summary>
    private void HandleEnemyDeath(EnemyChromosome chromosome, CombatStats stats)
    {
        enemiesAlive--;
        Debug.Log($"[WaveManager] Enemigos restantes: {enemiesAlive}");

        if (enemiesAlive <= 0 && waveInProgress)
            StartCoroutine(EndWaveRoutine());
    }

    /// <summary>
    /// Al terminar la oleada llama al GA y prepara la siguiente.
    /// </summary>
    private IEnumerator EndWaveRoutine()
    {
        waveInProgress = false;

        Debug.Log($"[WaveManager] ── Oleada {currentWave} completada ──");
        OnWaveCompleted?.Invoke(currentWave);

        // Pausa entre oleadas
        Debug.Log($"[WaveManager] Siguiente oleada en {timeBetweenWaves}s...");
        yield return new WaitForSeconds(timeBetweenWaves);

        // El GA evalúa y evoluciona la población
        currentChromosomes = geneticManager.EvolveNextGeneration();

        // Iniciamos la siguiente oleada
        StartCoroutine(StartWaveRoutine());
    }
}
