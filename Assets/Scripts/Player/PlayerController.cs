/// <summary>
/// Controla el movimiento y la rotación de la nave del jugador
/// usando físicas Rigidbody en un plano Zero-G (sin gravedad).
/// </summary>

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("Movimiento")]
    [Tooltip("Fuerza de aceleración aplicada cada FixedUpdate.")]
    [SerializeField] private float thrustForce = 20f;

    [Tooltip("Velocidad máxima de la nave (unidades/segundo).")]
    [SerializeField] private float maxSpeed = 10f;

    [Header("Rotación")]
    [Tooltip("Velocidad de rotación hacia la dirección de movimiento.")]
    [SerializeField] private float rotationSpeed = 10f;

    // ─── Referencias privadas ─────────────────────────────────────────────────

    /// <summary>Rigidbody cacheado para no llamar GetComponent cada frame.</summary>
    private Rigidbody rb;

    /// <summary>Dirección de entrada del jugador en el plano XZ.</summary>
    private Vector3 inputDirection;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        ApplyThrust();
        ClampVelocity();
        RotateTowardsMovement();
    }

    // ─── Métodos privados ─────────────────────────────────────────────────────

/// <summary>
/// Lee WASD y construye un vector de dirección relativo a la cámara.
/// Así W siempre significa "hacia donde mira la cámara", no "norte del mundo".
/// </summary>
private void ReadInput()
{
    float horizontal = Input.GetAxisRaw("Horizontal");
    float vertical   = Input.GetAxisRaw("Vertical");

    // Tomamos los vectores de la cámara pero ignoramos la inclinación en Y
    // para que el movimiento sea siempre en el plano XZ.
    Vector3 camForward = Camera.main.transform.forward;
    Vector3 camRight   = Camera.main.transform.right;

    // Aplanamos los vectores al plano XZ (Y = 0).
    camForward.y = 0f;
    camRight.y   = 0f;

    camForward.Normalize();
    camRight.Normalize();

    // Combinamos la entrada con las direcciones de la cámara.
    inputDirection = (camForward * vertical + camRight * horizontal).normalized;
}

    /// <summary>
    /// Aplica fuerza en la dirección de entrada.
    /// ForceMode.Force respeta la masa del Rigidbody.
    /// </summary>
    private void ApplyThrust()
    {
        if (inputDirection == Vector3.zero) return;

        rb.AddForce(inputDirection * thrustForce, ForceMode.Force);
    }

    /// <summary>
    /// Limita la magnitud de la velocidad a maxSpeed manteniendo la dirección.
    /// </summary>
    private void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    /// <summary>
    /// Rota suavemente la nave hacia la dirección actual de movimiento.
    /// </summary>
    private void RotateTowardsMovement()
    {
        if (rb.linearVelocity.magnitude < 0.1f) return;

        Quaternion targetRotation = Quaternion.LookRotation(
            rb.linearVelocity.normalized, Vector3.up);

        rb.MoveRotation(Quaternion.Slerp(
            rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }
}