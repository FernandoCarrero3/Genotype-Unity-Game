/// <summary>
/// Cámara que sigue a la nave desde atrás en espacio 3D.
/// Se posiciona siempre detrás y ligeramente arriba de la nave.
/// Sigue la posición con suavidad y la rotación con algo de delay
/// para simular inercia y dar sensación de peso a la nave.
/// </summary>

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Referencia")]
    [Tooltip("Transform de la nave del jugador.")]
    [SerializeField] private Transform playerTransform;

    [Header("Posición")]
    [Tooltip("Distancia detrás de la nave.")]
    [SerializeField] private float distance = 8f;

    [Tooltip("Altura sobre la nave.")]
    [SerializeField] private float height = 2f;

    [Tooltip("Suavidad del seguimiento de posición.")]
    [SerializeField] private float positionSmoothSpeed = 10f;

    [Header("Rotación")]
    [Tooltip("Suavidad del seguimiento de rotación. Menor = más delay = más inercia.")]
    [SerializeField] private float rotationSmoothSpeed = 5f;

    [Header("Colisión")]
    [Tooltip("Layer de las paredes para el raycast de cámara.")]
    [SerializeField] private LayerMask wallLayer;

    [Tooltip("Distancia mínima entre la cámara y una pared.")]
    [SerializeField] private float wallOffset = 0.5f;

    /// <summary>Rotación actual interpolada de la cámara.</summary>
    private Quaternion currentRotation;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        if (playerTransform != null)
            currentRotation = playerTransform.rotation;
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;
        FollowPlayer();
    }

    // ── Seguimiento ──────────────────────────────────────────────────────────

    /// <summary>
    /// Actualiza posición y rotación de la cámara cada frame.
    /// LateUpdate garantiza que la nave ya movió su posición este frame.
    /// </summary>
    private void FollowPlayer()
    {
        // 1. Interpolamos la rotación hacia la de la nave con delay
        currentRotation = Quaternion.Slerp(
            currentRotation,
            playerTransform.rotation,
            rotationSmoothSpeed * Time.deltaTime);

        // 2. Calculamos el offset detrás y arriba en espacio LOCAL de la nave
        Vector3 offset         = currentRotation * new Vector3(0f, height, -distance);
        Vector3 targetPosition = playerTransform.position + offset;

        // 3. Raycast para evitar que la cámara atraviese paredes
        targetPosition = ClampCameraToWalls(playerTransform.position, targetPosition);

        // 4. Suavizamos la posición
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionSmoothSpeed * Time.deltaTime);

        // 5. La cámara siempre mira hacia la nave con rotación suavizada
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            currentRotation,
            rotationSmoothSpeed * Time.deltaTime);
    }

    // ── Colisión con paredes ─────────────────────────────────────────────────

    /// <summary>
    /// Lanza un raycast desde el jugador hacia la posición objetivo de la cámara.
    /// Solo detecta el layer Wall — ignora jugador, enemigos y proyectiles.
    /// Si hay una pared en medio, acerca la cámara hasta justo antes del impacto.
    /// </summary>
    private Vector3 ClampCameraToWalls(Vector3 from, Vector3 to)
    {
        Vector3 direction    = to - from;
        float   castDistance = direction.magnitude;

        if (Physics.Raycast(from, direction.normalized, out RaycastHit hit, castDistance, wallLayer))
        {
            // Retrocedemos desde el punto de impacto hacia el jugador
            return hit.point - direction.normalized * wallOffset;
        }

        return to;
    }
}