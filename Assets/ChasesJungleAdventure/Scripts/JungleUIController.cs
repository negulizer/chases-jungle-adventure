using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// All UI calls in one place — keeps the other scripts clean.
///
/// Inspector wiring required:
///   • Assign all panels, buttons, text fields, animators, and spacePositions.
///   • Connect each Button's OnClick() to the matching "On___ButtonPressed()" method below.
///   • Drag the BoardInputHandler onto the inputHandler field so physical piece
///     placement also triggers Draw Card.
/// </summary>
public class JungleUIController : MonoBehaviour
{
    // ── Panels ───────────────────────────────────────────────────────────────
    [Header("Panels")]
    public GameObject welcomePanel;
    public GameObject playerSetupPanel;
    public GameObject gameBoardPanel;
    public GameObject winPanel;

    // ── Welcome Panel ─────────────────────────────────────────────────────────
    [Header("Welcome Buttons")]
    public Button newGameButton;
    public Button continueButton;   // hidden when no save exists

    // ── Player Setup Panel ───────────────────────────────────────────────────
    [Header("Player Setup")]
    public Button addPlayerButton;
    public Button startGameButton;
    public TMP_Text playerCountText;

    // ── Game Board HUD ───────────────────────────────────────────────────────
    [Header("Game Board HUD")]
    public Button drawCardButton;
    public TMP_Text playerTurnText;
    public TMP_Text cardDrawnText;
    public GameObject countingPlate;
    public TMP_Text countingText;
    public GameObject specialMessagePlate;
    public TMP_Text specialMessageText;

    // ── Board Spaces ─────────────────────────────────────────────────────────
    [Header("Board Spaces")]
    public RectTransform[] spacePositions; // Assign 60 board spaces in order

    // ── Win Panel ────────────────────────────────────────────────────────────
    [Header("Win Panel")]
    public TMP_Text winText;

    // ── Special Space Animators ───────────────────────────────────────────────
    [Header("Special Animators")]
    public Animator spiderAnimator;
    public Animator snakeAnimator;
    public Animator monkeyAnimator;
    public Animator alligatorAnimator;
    public Animator raftAnimator;

    // ── Board SDK Input ───────────────────────────────────────────────────────
    [Header("Board Input")]
    public BoardInputHandler inputHandler; // optional — enables piece-as-draw-trigger

    // ── Events (GameManager subscribes to these) ──────────────────────────────
    public event System.Action OnNewGame;
    public event System.Action OnContinueGame;
    public event System.Action OnAddPlayer;
    public event System.Action OnStartGame;

    private bool drawCardPressed = false;

    // ── Panel Helpers ─────────────────────────────────────────────────────────

    public void ShowWelcomePanel(bool hasSave = false)
    {
        welcomePanel.SetActive(true);
        playerSetupPanel.SetActive(false);
        gameBoardPanel.SetActive(false);
        winPanel.SetActive(false);
        if (continueButton != null)
            continueButton.gameObject.SetActive(hasSave);
    }

    public void ShowPlayerSetupPanel(int playerCount, int maxPlayers)
    {
        welcomePanel.SetActive(false);
        playerSetupPanel.SetActive(true);
        UpdatePlayerSetupPanel(playerCount, maxPlayers);
    }

    public void UpdatePlayerSetupPanel(int playerCount, int maxPlayers)
    {
        if (playerCountText  != null) playerCountText.text          = $"Players: {playerCount}";
        if (addPlayerButton  != null) addPlayerButton.interactable  = playerCount < maxPlayers;
        if (startGameButton  != null) startGameButton.interactable  = playerCount >= 1;
    }

    public void ShowGameBoard()
    {
        playerSetupPanel.SetActive(false);
        gameBoardPanel.SetActive(true);
        if (specialMessagePlate != null) specialMessagePlate.SetActive(false);
        if (specialMessageText != null) specialMessageText.gameObject.SetActive(false);
        if (countingPlate       != null) countingPlate.SetActive(false);
        if (countingText       != null) countingText.gameObject.SetActive(false);
        if (drawCardButton     != null) drawCardButton.gameObject.SetActive(false);
    }

    public void ShowWinPanel(PlayerToken winner)
    {
        winPanel.SetActive(true);
        if (winText != null)
            winText.text = $"{winner.DisplayName} reached the Waterfall!\nYou win!";
    }

    // ── Turn HUD ──────────────────────────────────────────────────────────────

    public void ShowPlayerTurn(PlayerToken player)
    {
        if (playerTurnText != null)
            playerTurnText.text = $"{player.DisplayName}'s turn!";
        if (cardDrawnText != null)
            cardDrawnText.text = "";
    }

    /// <param name="steps">How many spaces the token will travel — shown so kids know what to count to.</param>
    public void ShowDrawnCard(CardSystem.Card card, int steps)
    {
        if (cardDrawnText != null)
            cardDrawnText.text =
                $"You drew {card.color.ToString().ToUpper()}!\n" +
                $"Moving {steps} space{(steps == 1 ? "" : "s")}!";
    }

    /// <summary>Shows a big, bold step number so young players can count along.</summary>

    public void ShowCountingNumber(int current, int total)
    {
        if (countingText == null) return;
        if (countingPlate != null) countingPlate.SetActive(true);
        countingText.gameObject.SetActive(true);
        countingText.text = current.ToString();
        StopCoroutine("HideCountingAfterDelay");
        StartCoroutine("HideCountingAfterDelay");
    }

    private IEnumerator HideCountingAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);
        HideCountingDisplay();
    }

    public void HideCountingDisplay()
    {
        if (countingPlate != null) countingPlate.SetActive(false);
        if (countingText  != null) countingText.gameObject.SetActive(false);
        if (cardDrawnText != null) cardDrawnText.text = "";
    }

    // ── Token Movement ────────────────────────────────────────────────────────

    public void MoveToken(PlayerToken player, int spaceIndex)
    {
        if (player.tokenObject != null && spaceIndex < spacePositions.Length)
            player.tokenObject.transform.position = spacePositions[spaceIndex].position;
    }

    // ── Draw Card Input ───────────────────────────────────────────────────────

    /// <summary>
    /// Suspends the turn until the current player taps "Draw Card" or places
    /// a physical Board piece on the table.
    /// </summary>
    public IEnumerator WaitForDrawCard()
    {
        drawCardPressed = false;
        if (inputHandler != null) inputHandler.ResetDrawTrigger();

        if (drawCardButton != null)
        {
            drawCardButton.gameObject.SetActive(true);
            drawCardButton.interactable = true;
        }

        yield return new WaitUntil(() =>
            drawCardPressed ||
            (inputHandler != null && inputHandler.drawTriggered));

        if (drawCardButton != null)
        {
            drawCardButton.interactable = false;
            drawCardButton.gameObject.SetActive(false);
        }
        if (inputHandler != null) inputHandler.ResetDrawTrigger();
    }

    // Connected to drawCardButton.OnClick() in the Inspector.
    public void OnDrawCardButtonPressed() => drawCardPressed = true;

    // ── Special Spaces ────────────────────────────────────────────────────────


    public void ShowSpecialAnimation(JungleBoard.SpecialType type)
    {
        string message = type switch
        {
            JungleBoard.SpecialType.Spider    => "Eek! A giant spider pulls you back!",
            JungleBoard.SpecialType.Snake     => "Hissss! A sneaky snake slides you back!",
            JungleBoard.SpecialType.Monkey    => "Ooh-ooh! A cheeky monkey swings you back!",
            JungleBoard.SpecialType.Alligator => "SNAP! An alligator chomps you backwards!",
            JungleBoard.SpecialType.Raft      => "Whee! Hop on the river raft for a shortcut!",
            _                                 => ""
        };

        if (specialMessageText != null && message.Length > 0)
        {
            if (specialMessagePlate != null) specialMessagePlate.SetActive(true);
            specialMessageText.text = message;
            specialMessageText.gameObject.SetActive(true);
            StopCoroutine("HideSpecialAfterDelay");
            StartCoroutine("HideSpecialAfterDelay");
        }

        Animator anim = type switch
        {
            JungleBoard.SpecialType.Spider    => spiderAnimator,
            JungleBoard.SpecialType.Snake     => snakeAnimator,
            JungleBoard.SpecialType.Monkey    => monkeyAnimator,
            JungleBoard.SpecialType.Alligator => alligatorAnimator,
            JungleBoard.SpecialType.Raft      => raftAnimator,
            _                                 => null
        };
        if (anim != null) anim.SetTrigger("Play");
    }

    private IEnumerator HideSpecialAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        HideSpecialMessage();
    }

    public void HideSpecialMessage()
    {
        if (specialMessagePlate != null) specialMessagePlate.SetActive(false);
        if (specialMessageText != null)
            specialMessageText.gameObject.SetActive(false);
    }

    // ── Button Callbacks (connect each button's OnClick() in the Inspector) ──

    public void OnNewGameButtonPressed()      => OnNewGame?.Invoke();
    public void OnContinueButtonPressed()     => OnContinueGame?.Invoke();
    public void OnAddPlayerButtonPressed()    => OnAddPlayer?.Invoke();
    public void OnStartGameButtonPressed()    => OnStartGame?.Invoke();
}
