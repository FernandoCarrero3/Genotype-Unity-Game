using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 4f;

    [Header("Daño")]
    [SerializeField] private int damage = 1;

    private Rigidbody rb;
    private Vector3 moveDirection;

    /// <summary>
    /// Awake garantiza que rb esté listo antes de que
    /// EnemyController llame a SetDirection.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Protección extra: si no tiene dirección no se mueve
        if (moveDirection == Vector3.zero) return;

        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Debug.Log("[EnemyProjectile] Impacto con el jugador.");
        }

        Destroy(gameObject);
    }
}