using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the card deck for Chase's Jungle Adventure.
/// </summary>
public class CardSystem : MonoBehaviour
{
    public JungleBoard.SpaceColor[] cardColors = new JungleBoard.SpaceColor[] {
        JungleBoard.SpaceColor.Red,
        JungleBoard.SpaceColor.Blue,
        JungleBoard.SpaceColor.Green,
        JungleBoard.SpaceColor.Yellow,
        JungleBoard.SpaceColor.Purple,
        JungleBoard.SpaceColor.Orange
    };
    private List<JungleBoard.SpaceColor> deck = new List<JungleBoard.SpaceColor>();
    private System.Random rng = new System.Random();

    public void ShuffleDeck()
    {
        deck.Clear();
        for (int i = 0; i < 10; i++) // 10 of each color
            foreach (var color in cardColors)
                deck.Add(color);
        // Shuffle
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = deck[k];
            deck[k] = deck[n];
            deck[n] = value;
        }
    }

    public Card DrawCard()
    {
        if (deck.Count == 0) ShuffleDeck();
        var color = deck[0];
        deck.RemoveAt(0);
        return new Card { color = color };
    }

    public struct Card
    {
        public JungleBoard.SpaceColor color;
    }
}
