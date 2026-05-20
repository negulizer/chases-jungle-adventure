/// <summary>
/// Serialized snapshot of game state persisted via BoardSaveGameManager.
/// JsonUtility requires [System.Serializable] and public fields.
/// </summary>
[System.Serializable]
public class GameSaveData
{
    /// <summary>Board position (0-59) for each player, in player order.</summary>
    public int[] playerPositions;

    /// <summary>Index of the player whose turn is next.</summary>
    public int currentPlayerIndex;
}
