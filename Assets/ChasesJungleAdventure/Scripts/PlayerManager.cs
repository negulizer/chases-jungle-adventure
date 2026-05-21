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
    public Color[] playerColors = new Color[]
    {
        new Color(1.00f, 0.92f, 0.04f),  // Yellow
        new Color(0.72f, 0.08f, 1.00f),  // Purple
        new Color(1.00f, 0.48f, 0.02f),  // Orange
        new Color(1.00f, 0.38f, 0.70f),  // Pink
    };

    private static readonly string[] ColorNames = { "Yellow", "Purple", "Orange", "Pink" };

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
            // Color the Shirt child; fall back to root Image for legacy prefabs.
            var shirtT = tokenObj.transform.Find("Shirt");
            var img = (shirtT != null ? shirtT.GetComponent<UnityEngine.UI.Image>() : null)
                      ?? tokenObj.GetComponent<UnityEngine.UI.Image>();
            Color col  = playerColors[i % playerColors.Length];
            string cName = ColorNames[i % ColorNames.Length];
            if (img != null) img.color = col;

            // Update the number label on the token figure.
            var markT = tokenObj.transform.Find("Mark");
            if (markT != null)
            {
                var markTMP = markT.GetComponent<TMPro.TextMeshProUGUI>();
                if (markTMP != null) markTMP.text = $"{i + 1}";
            }

            players.Add(new PlayerToken
            {
                sessionPlayer = sessionPlayers?[i],
                position      = 0,
                color         = col,
                colorName     = cName,
                playerNumber  = i + 1,
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
