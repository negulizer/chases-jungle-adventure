using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines the jungle path: 60 spaces winding from the Start clearing to the Waterfall.
/// Special spaces carry a LinkedIndex pointing to where the player ends up after the effect.
/// </summary>
public class JungleBoard : MonoBehaviour
{
    public enum SpaceColor { Red, Blue, Green, Yellow, Purple, Orange }
    public enum SpecialType { None, Spider, Snake, Monkey, Alligator, Raft }

    public struct SpecialSpace
    {
        public SpecialType type;
        public int linkedIndex;
    }

    public int GoalIndex => 59;

    // Board layout: color and special info for each space
    public SpaceColor[] spaceColors = new SpaceColor[60];
    public SpecialSpace[] specialSpaces = new SpecialSpace[60];

    void Awake()
    {
        // Example: Fill with a repeating color pattern and some special spaces
        var colors = (SpaceColor[])System.Enum.GetValues(typeof(SpaceColor));
        for (int i = 0; i < 60; i++)
        {
            spaceColors[i] = colors[i % colors.Length];
            specialSpaces[i] = new SpecialSpace { type = SpecialType.None, linkedIndex = i };
        }
        // Place obstacles and shortcuts
        specialSpaces[7] = new SpecialSpace { type = SpecialType.Spider, linkedIndex = 3 };
        specialSpaces[15] = new SpecialSpace { type = SpecialType.Snake, linkedIndex = 10 };
        specialSpaces[22] = new SpecialSpace { type = SpecialType.Monkey, linkedIndex = 18 };
        specialSpaces[35] = new SpecialSpace { type = SpecialType.Alligator, linkedIndex = 28 };
        specialSpaces[40] = new SpecialSpace { type = SpecialType.Raft, linkedIndex = 50 };
    }

    public int GetStepsToNextColor(int fromIndex, SpaceColor color)
    {
        for (int i = fromIndex + 1; i < spaceColors.Length; i++)
        {
            if (spaceColors[i] == color)
                return i - fromIndex;
        }
        // If not found, go to last space
        return spaceColors.Length - fromIndex - 1;
    }

    public bool IsSpecialSpace(int index, out SpecialSpace special)
    {
        special = specialSpaces[index];
        return special.type != SpecialType.None;
    }

    public int GetSpecialSpaceIndex(SpecialType type)
    {
        for (int i = 0; i < specialSpaces.Length; i++)
            if (specialSpaces[i].type == type)
                return i;
        return -1;
    }
}
