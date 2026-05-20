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
    public GameObject tokenObject;

    /// <summary>Safe player name — falls back to colour name when running in the Unity Editor.</summary>
    public string DisplayName => sessionPlayer != null ? sessionPlayer.name : color.ToString();
}
