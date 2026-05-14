/// <summary>
/// Controlador de la pantalla de Récords.
/// Lee el top-10 desde ScoreManager y genera las filas dinámicamente.
/// </summary>
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeaderboardController : MonoBehaviour
{
    // ── Referencias UI ───────────────────────────────────────────────────────

    [Header("Contenedor donde se generan las filas")]
    [SerializeField]
    private RectTransform scoreListContainer;

    // ── Configuración visual de filas ────────────────────────────────────────

    [Header("Configuración de filas")]
    [SerializeField]
    private float rowHeight = 36f;

    [SerializeField]
    private float rowSpacing = 4f;

    [SerializeField]
    private Color colorGold = new Color(1f, 0.84f, 0f);

    [SerializeField]
    private Color colorSilver = new Color(0.75f, 0.75f, 0.75f);

    [SerializeField]
    private Color colorBronze = new Color(0.8f, 0.5f, 0.2f);

    [SerializeField]
    private Color colorDefault = new Color(0.7f, 0.7f, 0.7f);

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        BuildLeaderboard();
    }

    // ── Construcción de la tabla ─────────────────────────────────────────────

    /// <summary>
    /// Lee los récords desde ScoreManager y genera una fila por entrada.
    /// </summary>
    private void BuildLeaderboard()
    {
        // Limpiar filas anteriores por si se recarga la escena
        foreach (Transform child in scoreListContainer)
            Destroy(child.gameObject);

        if (ScoreManager.Instance == null)
        {
            Debug.LogWarning("[Leaderboard] ScoreManager no encontrado.");
            CreateEmptyMessage("No hay récords todavía.");
            return;
        }

        List<ScoreEntry> entries = ScoreManager.Instance.GetLeaderboard();

        if (entries == null || entries.Count == 0)
        {
            CreateEmptyMessage("No hay récords todavía.");
            return;
        }

        for (int i = 0; i < entries.Count; i++)
            CreateRow(i, entries[i]);
    }

    /// <summary>
    /// Crea una fila de texto con posición, nombre y puntuación.
    /// </summary>
    private void CreateRow(int index, ScoreEntry entry)
    {
        // Contenedor de la fila
        GameObject rowObj = new GameObject($"Row_{index + 1}");
        rowObj.transform.SetParent(scoreListContainer, false);

        RectTransform rowRect = rowObj.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);

        float yPos = -index * (rowHeight + rowSpacing);
        rowRect.anchoredPosition = new Vector2(0f, yPos);
        rowRect.sizeDelta = new Vector2(0f, rowHeight);

        // Color según posición
        Color rowColor = index switch
        {
            0 => colorGold,
            1 => colorSilver,
            2 => colorBronze,
            _ => colorDefault,
        };

        // Texto de posición
        CreateLabel(
            rowObj,
            $"{index + 1}.",
            rowColor,
            anchorMinX: 0f,
            anchorMaxX: 0f,
            offsetMinX: 10f,
            offsetMaxX: 70f,
            fontSize: 20,
            alignRight: false
        );

        // Texto de nombre
        CreateLabel(
            rowObj,
            entry.playerName,
            rowColor,
            anchorMinX: 0f,
            anchorMaxX: 0f,
            offsetMinX: 80f,
            offsetMaxX: 430f,
            fontSize: 20,
            alignRight: false
        );

        // Texto de puntuación
        CreateLabel(
            rowObj,
            entry.score.ToString("N0"),
            rowColor,
            anchorMinX: 0f,
            anchorMaxX: 1f,
            offsetMinX: 440f,
            offsetMaxX: -10f,
            fontSize: 20,
            alignRight: true
        );
    }

    /// <summary>
    /// Crea un TMP_Text hijo dentro de un GameObject padre con los parámetros dados.
    /// </summary>
    private void CreateLabel(
        GameObject parent,
        string text,
        Color color,
        float anchorMinX,
        float anchorMaxX,
        float offsetMinX,
        float offsetMaxX,
        int fontSize,
        bool alignRight
    )
    {
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(parent.transform, false);

        RectTransform rect = labelObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(anchorMinX, 0f);
        rect.anchorMax = new Vector2(anchorMaxX, 1f);
        rect.offsetMin = new Vector2(offsetMinX, 0f);
        rect.offsetMax = new Vector2(offsetMaxX, 0f);

        TMP_Text label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = alignRight
            ? TextAlignmentOptions.MidlineRight
            : TextAlignmentOptions.MidlineLeft;
    }

    /// <summary>
    /// Muestra un mensaje centrado cuando no hay entradas.
    /// </summary>
    private void CreateEmptyMessage(string message)
    {
        GameObject msgObj = new GameObject("EmptyMessage");
        msgObj.transform.SetParent(scoreListContainer, false);

        RectTransform rect = msgObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TMP_Text label = msgObj.AddComponent<TextMeshProUGUI>();
        label.text = message;
        label.fontSize = 24;
        label.color = new Color(0.5f, 0.5f, 0.5f);
        label.alignment = TextAlignmentOptions.Center;
    }

    // ── Navegación ───────────────────────────────────────────────────────────

    /// <summary>Vuelve al Menú Principal.</summary>
    public void OnBackButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
