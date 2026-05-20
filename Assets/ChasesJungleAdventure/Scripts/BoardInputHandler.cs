using UnityEngine;
using Board.Input;

/// <summary>
/// Bridges physical Board pieces to game input.
///
/// How it works for Chase's Jungle Adventure:
///   • Any new piece placed on the table (phase == Began) sets drawTriggered = true.
///   • JungleUIController.WaitForDrawCard() polls this flag so that placing a piece
///     is equivalent to tapping the on-screen "Draw Card" button.
///   • Call ResetDrawTrigger() after consuming the event so it only fires once per placement.
/// </summary>
public class BoardInputHandler : MonoBehaviour
{
    /// <summary>
    /// True for one frame when a new piece is placed. Consumed by JungleUIController.
    /// </summary>
    public bool drawTriggered = false;

    private void Update()
    {
        // BoardContactPhase.Began fires for exactly one frame when a piece is first detected.
        foreach (var contact in BoardInput.GetActiveContacts(BoardContactType.Glyph))
        {
            if (contact.phase == BoardContactPhase.Began)
            {
                drawTriggered = true;
                break; // One new piece is enough to trigger a draw.
            }
        }
    }

    /// <summary>Call this after consuming drawTriggered so it doesn't re-fire.</summary>
    public void ResetDrawTrigger() => drawTriggered = false;
}

