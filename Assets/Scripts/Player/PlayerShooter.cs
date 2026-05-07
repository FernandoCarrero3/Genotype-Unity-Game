/// <summary>
/// Gestiona el disparo del jugador.
///
/// Características:
///   - Dispara en la dirección que mira la nave.
///   - Cadencia de disparo configurable (disparos por segundo).
///   - Referencia al prefab del proyectil asignable desde el Inspector.
///   - Punto de spawn del proyectil en la punta de la nave.
/// </summary>

using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("Proyectil")]
    [Tooltip("Prefab del proyectil a instanciar.")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("Punto desde donde se spawna el proyectil (punta de la nave).")]
    [SerializeField] private Transform firePoint;

    [Header("Cadencia")]
    [Tooltip("Disparos por segundo.")]
    [SerializeField] private float fireRate = 5f;

    // ─── Estado privado ───────────────────────────────────────────────────────

    /// <summary>Tiempo que debe pasar entre disparos (calculado desde fireRate).</summary>
    private float fireCooldown;

    /// <summary>Contador de tiempo desde el último disparo.</summary>
    private float timeSinceLastShot;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        // Calculamos el cooldown a partir de la cadencia.
        // Ej: 5 disparos/segundo → 0.2 segundos entre disparos.
        fireCooldown = 1f / fireRate;

        // Empezamos con el cooldown lleno para poder disparar al instante.
        timeSinceLastShot = fireCooldown;
    }

    private void Update()
    {
        // Acumulamos el tiempo transcurrido.
        timeSinceLastShot += Time.deltaTime;

        // Disparo con click izquierdo o tecla Space, respetando la cadencia.
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
        {
            TryShoot();
        }
    }

    // ─── Métodos privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Intenta disparar si el cooldown lo permite.
    /// </summary>
    private void TryShoot()
    {
        if (timeSinceLastShot < fireCooldown) return;

        Shoot();
        timeSinceLastShot = 0f;
    }

    /// <summary>
    /// Instancia el proyectil en el firePoint y lo lanza
    /// en la dirección que mira la nave.
    /// </summary>
    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[PlayerShooter] No hay prefab de proyectil asignado.");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning("[PlayerShooter] No hay firePoint asignado.");
            return;
        }

        // Instanciamos el proyectil en la posición y rotación del firePoint.
        GameObject bulletObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Obtenemos el componente Projectile y le asignamos la dirección.
        Projectile bullet = bulletObj.GetComponent<Projectile>();

        if (bullet != null)
        {
            // La nave mira hacia transform.forward — esa es la dirección de disparo.
            bullet.SetDirection(transform.forward);
        }
    }
}