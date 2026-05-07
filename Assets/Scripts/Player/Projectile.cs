/// <summary>
/// Controla el comportamiento de un proyectil disparado por el jugador.
///
/// Características:
///   - Se mueve en línea recta a velocidad constante usando Rigidbody.
///   - Se autodestruye tras un tiempo límite para no acumular objetos en escena.
///   - Al colisionar con un enemigo llama a su sistema de daño.
///   - Al colisionar con un muro se destruye.
/// </summary>

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento del proyectil.")]
    [SerializeField] private float speed = 20f;

    [Tooltip("Segundos antes de autodestruirse si no golpea nada.")]
    [SerializeField] private float lifetime = 3f;

    [Header("Daño")]
    [Tooltip("Daño que aplica al enemigo al impactar.")]
    [SerializeField] private int damage = 1;

    // ─── Referencias privadas ─────────────────────────────────────────────────

    /// <summary>Rigidbody cacheado.</summary>
    private Rigidbody rb;

    /// <summary>Dirección de movimiento, asignada al instanciar.</summary>
    private Vector3 moveDirection;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Autodestrucción por tiempo límite.
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Movemos el proyectil en línea recta a velocidad constante.
        // Usamos MovePosition para mantener coherencia con el sistema de físicas.
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    // ─── Métodos públicos ─────────────────────────────────────────────────────

    /// <summary>
    /// Inicializa la dirección del proyectil.
    /// Llamar justo después de instanciar desde PlayerShooter.
    /// </summary>
    /// <param name="direction">Vector normalizado de dirección.</param>
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    // ─── Colisiones ───────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        // Ignoramos colisiones con el propio jugador.
        if (other.CompareTag("Player")) return;

        // En el futuro: if (other.CompareTag("Enemy")) other.GetComponent<EnemyHealth>()?.TakeDamage(damage);

        Debug.Log($"[Projectile] Impacto con: {other.gameObject.name}");

        Destroy(gameObject);
    }
}