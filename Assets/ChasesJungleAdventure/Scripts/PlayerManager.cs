using System.Collections.Generic;
using UnityEngine;
using Board.Session;

/// <summary>
/// Creates and tracks all player tokens; handles save/load serialization.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public GameObject tokenPrefab;
    public Transform tokenParent;
    public Color[] playerColors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow };

    private List<PlayerToken> players = new List<PlayerToken>();

    public int PlayerCount => players.Count;

    /// <summary>Maximum number of simultaneous players (one colour per player).</summary>
    public int MaxPlayers => playerColors.Length;

    /// <summary>
    /// Instantiate tokens from BoardSession.players.
    /// Pass editorMockCount &gt; 0 when running in the Unity Editor to create that
    /// many mock players (no real BoardSessionPlayer attached).
    /// </summary>
    public void ResetPlayers(int editorMockCount = 0)
    {
        foreach (var p in players)
            if (p.tokenObject != null) Destroy(p.tokenObject);
        players.Clear();

        BoardSessionPlayer[] sessionPlayers = editorMockCount > 0 ? null : BoardSession.players;
        int count = editorMockCount > 0 ? editorMockCount : (sessionPlayers?.Length ?? 0);

        for (int i = 0; i < count; i++)
        {
            var tokenObj = Instantiate(tokenPrefab, tokenParent);
            var img = tokenObj.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = playerColors[i % playerColors.Length];

            players.Add(new PlayerToken
            {
                sessionPlayer = sessionPlayers?[i],   // null in editor — DisplayName handles it
                position      = 0,
                color         = playerColors[i % playerColors.Length],
                tokenObject   = tokenObj
            });
        }
    }

    public PlayerToken GetPlayer(int index) => players[index];

    // ── Save / Load Helpers ───────────────────────────────────────────────────

    /// <summary>Snapshot the current board positions for serialisation.</summary>
    public GameSaveData GetSaveData(int currentPlayerIndex)
    {
        var positions = new int[players.Count];
        for (int i = 0; i < players.Count; i++)
            positions[i] = players[i].position;

        return new GameSaveData
        {
            playerPositions    = positions,
            currentPlayerIndex = currentPlayerIndex
        };
    }

    /// <summary>Restore positions after a save is loaded (tokens must already exist).</summary>
    public void RestorePositions(int[] positions)
    {
        for (int i = 0; i < players.Count && i < positions.Length; i++)
            players[i].position = positions[i];
    }
}
