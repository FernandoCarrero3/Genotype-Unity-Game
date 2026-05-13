/// <summary>
/// Wrapper necesario para serializar una List con JsonUtility.
/// JsonUtility no puede serializar listas directamente en la raíz.
/// </summary>
using System;
using System.Collections.Generic;

[Serializable]
public class ScoreListWrapper
{
    public List<ScoreEntry> entries;
}
