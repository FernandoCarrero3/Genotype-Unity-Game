/// <summary>
/// Controla el comportamiento de una nave enemiga en 3D.
/// Lee su EnemyChromosome y mapea los genes [0-1] a valores reales de juego.
/// </summary>
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    // ── Cromosoma ────────────────────────────────────────────────────────────

    [SerializeField]
    private EnemyChromosome chromosome;

    /// <summary>Propiedad de solo lectura para que EnemyHealth acceda al cromosoma.</summary>
    public EnemyChromosome Chromosome => chromosome;

    // ── Valores reales mapeados desde genes ──────────────────────────────────

    private float realSpeed;
    private float realCombatRange;
    private float realFireRate;
    private float realSpread;

    // ── Disparo ──────────────────────────────────────────────────────────────

    [Header("Disparo")]
    [Tooltip("Prefab del proyectil enemigo.")]
    [SerializeField]
    private GameObject projectilePrefab;

    [Tooltip("Punto desde donde se spawna el proyectil.")]
    [SerializeField]
    private Transform firePoint;

    private float timeSinceLastShot;

    // ── Referencias ──────────────────────────────────────────────────────────

    private Rigidbody rb;
    private EnemyHealth enemyHealth;
    private bool chromosomeApplied = false;
    private Transform playerTransform;

    // ── Constantes de mapeo ──────────────────────────────────────────────────

    private const float MIN_SPEED = 2f;
    private const float MAX_SPEED = 12f;
    private const float MIN_RANGE = 3f;
    private const float MAX_RANGE = 18f;
    private const float MAX_FIRE_INTERVAL = 4f;
    private const float MIN_FIRE_INTERVAL = 0.5f;
    private const float MAX_SPREAD = 25f;
    private const float MIN_SPREAD = 0f;
    private const float RANGE_TOLERANCE = 1.5f;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyHealth = GetComponent<EnemyHealth>(); // Primero cacheamos referencias

        rb.useGravity = false;
        rb.angularDamping = 10f;

        if (chromosome == null)
            chromosome = EnemyChromosome.CreateRandom();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning("[EnemyController] No se encontró ningún objeto con Tag 'Player'.");

        if (!chromosomeApplied)
            ApplyChromosome();

        timeSinceLastShot = realFireRate;
    }

    private void Update()
    {
        if (playerTransform == null)
            return;
        timeSinceLastShot += Time.deltaTime;
        TryShoot();
    }

    private void FixedUpdate()
    {
        if (playerTransform == null)
            return;
        HandleMovement();
        RotateTowardsPlayer();
    }

    // ── Métodos públicos ─────────────────────────────────────────────────────

    public void SetChromosome(EnemyChromosome newChromosome)
    {
        chromosome = newChromosome;
        ApplyChromosome();
    }

    // ── Métodos privados ─────────────────────────────────────────────────────

    private void ApplyChromosome()
    {
        realSpeed = Mathf.Lerp(MIN_SPEED, MAX_SPEED, chromosome.speed);
        realCombatRange = Mathf.Lerp(MIN_RANGE, MAX_RANGE, chromosome.combatRange);
        realFireRate = Mathf.Lerp(MAX_FIRE_INTERVAL, MIN_FIRE_INTERVAL, chromosome.aggression);
        realSpread = Mathf.Lerp(MAX_SPREAD, MIN_SPREAD, chromosome.precision);

        Debug.Log($"[EnemyController] {chromosome}");
        Debug.Log(
            $"[EnemyController] Speed:{realSpeed:F2} | Range:{realCombatRange:F2} | "
                + $"FireRate:{realFireRate:F2}s | Spread:{realSpread:F2}°"
        );

        // Inicializamos EnemyHealth con el cromosoma si ya está disponible
        if (enemyHealth != null)
            enemyHealth.Initialize(chromosome);

        chromosomeApplied = true;
    }

    /// <summary>
    /// Mueve al enemigo en 3D hacia o desde el jugador
    /// según la diferencia entre distancia actual y realCombatRange.
    /// </summary>
    private void HandleMovement()
    {
        // Dirección completa en 3D hacia el jugador
        Vector3 dirToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = dirToPlayer.magnitude;
        Vector3 dirNormalized = dirToPlayer.normalized;
        float distanceDelta = distanceToPlayer - realCombatRange;

        if (distanceDelta > RANGE_TOLERANCE)
        {
            // Demasiado lejos — avanzar hacia el jugador en 3D
            rb.MovePosition(rb.position + dirNormalized * realSpeed * Time.fixedDeltaTime);
        }
        else if (distanceDelta < -RANGE_TOLERANCE)
        {
            // Demasiado cerca — retroceder en 3D
            rb.MovePosition(rb.position - dirNormalized * realSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Rota suavemente la nave enemiga para que apunte al jugador en 3D.
    /// </summary>
    private void RotateTowardsPlayer()
    {
        Vector3 dirToPlayer = playerTransform.position - transform.position;
        if (dirToPlayer == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer.normalized, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 8f * Time.fixedDeltaTime));
    }

    private void TryShoot()
    {
        if (timeSinceLastShot < realFireRate)
            return;
        if (projectilePrefab == null || firePoint == null)
            return;

        Shoot();
        timeSinceLastShot = 0f;
    }

    /// <summary>
    /// Dispara un proyectil con desviación según el gen precision.
    /// El spread ahora se aplica en 3D usando dos ejes aleatorios.
    /// </summary>
    private void Shoot()
    {
        Vector3 dirToPlayer = (playerTransform.position - firePoint.position).normalized;
        Vector3 spreadDirection = ApplySpread(dirToPlayer);

        GameObject bulletObj = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );
        EnemyProjectile bullet = bulletObj.GetComponent<EnemyProjectile>();

        if (bullet != null)
        {
            // Pasamos la referencia de EnemyHealth al proyectil
            // para que pueda confirmar el impacto y registrarlo
            bullet.SetDirection(spreadDirection);
            bullet.SetOwner(enemyHealth);
        }

        // Registramos el disparo (el hit lo confirmará EnemyProjectile al impactar)
        enemyHealth?.RegisterShot(false);
    }

    /// <summary>
    /// Aplica desviación aleatoria en 3D — tanto horizontal como vertical.
    /// Así un enemigo impreciso falla en cualquier dirección, no solo izquierda/derecha.
    /// </summary>
    private Vector3 ApplySpread(Vector3 baseDirection)
    {
        // Rotación aleatoria en Y (horizontal)
        float randomYaw = Random.Range(-realSpread, realSpread);
        // Rotación aleatoria en X (vertical)
        float randomPitch = Random.Range(-realSpread, realSpread);

        Quaternion spreadRotation = Quaternion.Euler(randomPitch, randomYaw, 0f);
        return spreadRotation * baseDirection;
    }
}
