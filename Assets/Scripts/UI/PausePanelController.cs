/// <summary>
/// Controlador del panel de pausa.
/// Conecta los botones con PauseManager por código — evita
/// que las referencias del Inspector se pierdan al recargar la escena.
/// </summary>
using UnityEngine;
using UnityEngine.UI;

public class PausePanelController : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField]
    private Button resumeButton;

    [SerializeField]
    private Button mainMenuButton;

    private void Start()
    {
        // Conectamos los botones por código — independiente del Inspector
        resumeButton.onClick.AddListener(() => PauseManager.Instance.Resume());
        mainMenuButton.onClick.AddListener(() => PauseManager.Instance.GoToMainMenu());

        Debug.Log("[PausePanelController] Botones conectados a PauseManager.");
    }
}
