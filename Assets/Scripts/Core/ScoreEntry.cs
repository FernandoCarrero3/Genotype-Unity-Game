/// <summary>
/// Representa una entrada individual en la tabla de récords.
/// Clase serializable para compatibilidad con JsonUtility.
/// </summary>
using System;

[Serializable]
public class ScoreEntry
{
    /// <summary>Nombre del jugador.</summary>
    public string playerName;

    /// <summary>Puntuación obtenida.</summary>
    public int score;

    /// <summary>Fecha de la partida en formato dd/MM/yyyy.</summary>
    public string date;
}
