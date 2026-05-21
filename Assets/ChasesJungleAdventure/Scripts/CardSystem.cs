using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the card deck for Chase's Jungle Adventure.
/// </summary>
public class CardSystem : MonoBehaviour
{
    public enum CardKind { Color, Special }

    public JungleBoard.SpaceColor[] cardColors = new JungleBoard.SpaceColor[] {
        JungleBoard.SpaceColor.Red,
        JungleBoard.SpaceColor.Blue,
        JungleBoard.SpaceColor.Green,
        JungleBoard.SpaceColor.Yellow,
        JungleBoard.SpaceColor.Purple,
        JungleBoard.SpaceColor.Orange
    };
    public JungleBoard.SpecialType[] specialCardTypes = new JungleBoard.SpecialType[] {
        JungleBoard.SpecialType.Spider,
        JungleBoard.SpecialType.Snake,
        JungleBoard.SpecialType.Monkey,
        JungleBoard.SpecialType.Alligator,
        JungleBoard.SpecialType.Raft
    };

    [Header("Deck Mix")]
    public int colorCopiesPerColor = 10;
    public int specialCopiesPerType = 2;

    private List<Card> deck = new List<Card>();
    private System.Random rng = new System.Random();

    public void ShuffleDeck()
    {
        deck.Clear();
        for (int i = 0; i < colorCopiesPerColor; i++)
            foreach (var color in cardColors)
                deck.Add(new Card { kind = CardKind.Color, color = color });

        for (int i = 0; i < specialCopiesPerType; i++)
            foreach (var type in specialCardTypes)
                deck.Add(new Card { kind = CardKind.Special, specialType = type });

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
        var card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    public struct Card
    {
        public CardKind kind;
        public JungleBoard.SpaceColor color;
        public JungleBoard.SpecialType specialType;
    }
}
