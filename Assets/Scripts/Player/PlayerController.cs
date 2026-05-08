/// <summary>
/// Controla el vuelo 3D de la nave del jugador estilo avión de combate.
/// La nave siempre avanza hacia donde mira.
/// - W/S: acelerar / frenar
/// - A/D: rotar izquierda / derecha (Yaw)
/// - Ratón Y: rotar arriba / abajo (Pitch)
/// - Q/E: rotar sobre sí misma (Roll)
/// </summary>

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Velocidad")]
    [Tooltip("Fuerza de aceleración hacia adelante.")]
    [SerializeField] private float thrustForce = 20f;

    [Tooltip("Velocidad máxima de la nave.")]
    [SerializeField] private float maxSpeed = 15f;

    [Header("Rotación")]
    [Tooltip("Velocidad de rotación Yaw (A/D) y Pitch (ratón).")]
    [SerializeField] private float rotationSpeed = 90f;

    [Tooltip("Velocidad de Roll (Q/E).")]
    [SerializeField] private float rollSpeed = 60f;

    [Tooltip("Sensibilidad del ratón para el Pitch.")]
    [SerializeField] private float mouseSensitivity = 2f;

    // ── Referencias ──────────────────────────────────────────────────────────

    private Rigidbody rb;

    // ── Input ────────────────────────────────────────────────────────────────

    private float thrustInput;
    private float yawInput;
    private float pitchInput;
    private float rollInput;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Bloqueamos el cursor para que el ratón no salga de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        ApplyThrust();
        ApplyRotation();
        ClampVelocity();
    }

    // ── Input ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lee todos los inputs del jugador.
    /// Se hace en Update para no perder inputs entre frames.
    /// </summary>
    private void ReadInput()
    {
        // W/S — acelerar / frenar
        thrustInput = Input.GetAxisRaw("Vertical");

        // A/D — girar izquierda/derecha
        yawInput = Input.GetAxisRaw("Horizontal");

        // Ratón Y — subir/bajar el morro
        // Invertimos porque mover el ratón hacia abajo debe bajar el morro
        pitchInput = -Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Q/E — rotar sobre el eje de avance
        rollInput = 0f;
        if (Input.GetKey(KeyCode.Q)) rollInput = -1f;
        if (Input.GetKey(KeyCode.E)) rollInput =  1f;
    }

    // ── Movimiento ───────────────────────────────────────────────────────────

    /// <summary>
    /// Empuja la nave hacia donde está mirando (transform.forward).
    /// W acelera, S frena.
    /// </summary>
    private void ApplyThrust()
    {
        if (thrustInput == 0f) return;
        rb.AddForce(transform.forward * thrustInput * thrustForce, ForceMode.Force);
    }

    /// <summary>
    /// Aplica las tres rotaciones independientes:
    /// Pitch (arriba/abajo), Yaw (izquierda/derecha), Roll (giro).
    /// Usamos Rigidbody.MoveRotation para respetar la física.
    /// </summary>
    private void ApplyRotation()
    {
        float pitch = pitchInput  * rotationSpeed * Time.fixedDeltaTime;
        float yaw   = yawInput    * rotationSpeed * Time.fixedDeltaTime;
        float roll  = rollInput   * rollSpeed      * Time.fixedDeltaTime;

        // Construimos la rotación incremental en espacio LOCAL de la nave
        Quaternion deltaRotation = Quaternion.Euler(pitch, yaw, roll);

        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    /// <summary>
    /// Limita la velocidad máxima de la nave.
    /// </summary>
    private void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }
}