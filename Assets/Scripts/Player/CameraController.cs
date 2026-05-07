/// <summary>
/// Cámara en tercera persona que sigue y rota suavemente con la nave del jugador.
///
/// Características:
///   - Se posiciona a una distancia y altura fijas relativas a la nave.
///   - Sigue la posición con Lerp para un movimiento suave sin tirones.
///   - Rota gradualmente con la nave usando Slerp para evitar giros bruscos.
///   - Todos los parámetros son ajustables desde el Inspector.
/// </summary>

using UnityEngine;

public class CameraController : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("Referencia")]
    [Tooltip("Transform de la nave del jugador a seguir.")]
    [SerializeField] private Transform playerTransform;

    [Header("Posición")]
    [Tooltip("Altura de la cámara sobre la nave.")]
    [SerializeField] private float height = 8f;

    [Tooltip("Distancia detrás de la nave.")]
    [SerializeField] private float distance = 5f;

    [Tooltip("Suavidad del seguimiento de posición (mayor = más pegada).")]
    [SerializeField] private float positionSmoothSpeed = 5f;

    [Header("Rotación")]
    [Tooltip("Suavidad de la rotación con la nave (menor = más delay).")]
    [SerializeField] private float rotationSmoothSpeed = 3f;

    [Tooltip("Ángulo de inclinación hacia abajo en grados.")]
    [SerializeField] private float tiltAngle = 55f;

    // ─── Variables privadas ───────────────────────────────────────────────────

    /// <summary>Rotación horizontal actual de la cámara, interpolada suavemente.</summary>
    private Quaternion currentYRotation;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        // Inicializamos la rotación actual con la de la nave para evitar
        // un giro brusco al arrancar la escena.
        if (playerTransform != null)
            currentYRotation = Quaternion.Euler(0f, playerTransform.eulerAngles.y, 0f);
    }

    /// <summary>
    /// LateUpdate se ejecuta después de Update y FixedUpdate.
    /// Es el momento correcto para mover la cámara, ya que el jugador
    /// ya ha actualizado su posición en este frame.
    /// </summary>
    private void LateUpdate()
    {
        if (playerTransform == null) return;

        FollowPlayer();
    }

    // ─── Métodos privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Calcula la posición y rotación objetivo de la cámara y las aplica
    /// suavemente usando Lerp y Slerp.
    /// </summary>
    private void FollowPlayer()
    {
        // ── 1. Rotación ──────────────────────────────────────────────────────
        // Tomamos solo el giro en Y de la nave (rotación horizontal).
        // Ignoramos X y Z para que la cámara no se incline con la nave.
        Quaternion targetYRotation = Quaternion.Euler(
            0f, playerTransform.eulerAngles.y, 0f);

        // Interpolamos suavemente hacia la rotación objetivo.
        currentYRotation = Quaternion.Slerp(
            currentYRotation,
            targetYRotation,
            rotationSmoothSpeed * Time.deltaTime);

        // ── 2. Posición ──────────────────────────────────────────────────────
        // Calculamos el offset detrás y arriba de la nave según su rotación actual.
        Vector3 offset = currentYRotation * new Vector3(0f, height, -distance);
        Vector3 targetPosition = playerTransform.position + offset;

        // Lerp suave hacia la posición objetivo.
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionSmoothSpeed * Time.deltaTime);

        // ── 3. Orientación ───────────────────────────────────────────────────
        // La cámara siempre mira al jugador con el ángulo de inclinación definido.
        transform.rotation = currentYRotation * Quaternion.Euler(tiltAngle, 0f, 0f);
    }
}