using UnityEngine;
using Board.Session;

/// <summary>
/// Runtime state for one player in Chase's Jungle Adventure.
/// Wraps a BoardSessionPlayer with game-specific fields.
/// </summary>
public class PlayerToken
{
    public BoardSessionPlayer sessionPlayer;
    public int position;
    public Color color;
    public string colorName;    // e.g. "Yellow"
    public int playerNumber;    // 1-based
    public GameObject tokenObject;

    /// <summary>Always "Player N (Color)" — clear for kids.</summary>
    public string DisplayName => $"Player {playerNumber} ({colorName})";
}
