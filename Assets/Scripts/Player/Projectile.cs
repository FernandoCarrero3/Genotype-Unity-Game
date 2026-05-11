/// <summary>
/// Controla el comportamiento de un proyectil disparado por el jugador.
/// Se mueve en línea recta en 3D, se autodestruye por tiempo o colisión.
/// </summary>
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento del proyectil.")]
    [SerializeField]
    private float speed = 20f;

    [Tooltip("Segundos antes de autodestruirse si no golpea nada.")]
    [SerializeField]
    private float lifetime = 3f;

    [Header("Daño")]
    [Tooltip("Daño que aplica al enemigo al impactar.")]
    [SerializeField]
    private float damage = 1f;

    private Rigidbody rb;
    private Vector3 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (moveDirection == Vector3.zero)
            return;
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Inicializa la dirección en 3D. Llamar justo después de Instantiate.
    /// </summary>
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;

        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            Debug.Log($"[Projectile] Impacto en enemigo: {other.gameObject.name}");
        }

        Destroy(gameObject);
    }
}
