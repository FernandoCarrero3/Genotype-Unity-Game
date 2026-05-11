using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField]
    private float speed = 15f;

    [SerializeField]
    private float lifetime = 4f;

    [Header("Daño")]
    [SerializeField]
    private int damage = 1;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private EnemyHealth owner;

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
        if (moveDirection == Vector3.zero)
            return;

        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    /// <summary>
    /// Recibe la referencia al EnemyHealth del enemigo que disparó.
    /// Permite confirmar el impacto y actualizar las estadísticas de precisión.
    /// </summary>
    public void SetOwner(EnemyHealth enemyHealth)
    {
        owner = enemyHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
            return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);

                // Confirmamos impacto al enemigo que disparó
                // Actualizamos shotsHit y damageDealtToPlayer
                if (owner != null)
                {
                    owner.RegisterShot(true);
                    owner.RegisterDamageDealt(damage);
                }

                Debug.Log("[EnemyProjectile] Impacto con el jugador.");
            }
        }

        Destroy(gameObject);
    }
}
