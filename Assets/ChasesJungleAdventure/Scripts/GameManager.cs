using System.Collections;
using System.Text;
using UnityEngine;
using Board.Core;
using Board.Session;
using Board.Save;

/// <summary>
/// Main state machine for Chase's Jungle Adventure.
/// Handles game flow, player setup, turns, win condition, step-by-step movement,
/// pause-screen integration, and save/load via the Board SDK.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Gameplay")]
    public float stepDelay = 0.45f;

    [Header("References")]
    public JungleBoard jungleBoard;
    public CardSystem cardSystem;
    public PlayerManager playerManager;
    public JungleUIController uiController;

    private int currentPlayerIndex = 0;
    private bool isGameActive = false;
    private string currentSaveId = null;

#if UNITY_EDITOR
    // Tracks simulated player count when running without Board hardware.
    private int _editorPlayerCount = 1;
#endif

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Configure the Board pause screen once at startup.
        BoardApplication.SetPauseScreenContext(
            applicationName: "Chase's Jungle Adventure",
            showSaveOptionUponExit: true
        );

        // Subscribe with a lambda so the async work stays in a separate method.
        BoardApplication.pauseScreenActionReceived += (action, _) => HandlePauseAction(action);
    }

    private async void Start()
    {
        // Check for an existing save so the welcome screen can show "Continue".
        bool hasSave = false;
        try
        {
            var saves = await BoardSaveGameManager.GetSaveGamesMetadata();
            if (saves != null && saves.Length > 0)
            {
                hasSave = true;
                currentSaveId = saves[0].id;
            }
        }
        catch { /* No saves or network unavailable — proceed without. */ }

        uiController.ShowWelcomePanel(hasSave);
        uiController.OnNewGame      += StartPlayerSetup;
        uiController.OnContinueGame += LoadAndContinue;
    }

    // ── Player Setup ─────────────────────────────────────────────────────────

    private void StartPlayerSetup()
    {
        // Reset to only the device-owner profile, then let the host add more.
        try { BoardSession.ResetPlayers(); } catch { }

#if UNITY_EDITOR
        _editorPlayerCount = 1;   // simulate 1 player (the host) for editor testing
        int initialCount = _editorPlayerCount;
#else
        int initialCount = BoardSession.players?.Length ?? 0;
#endif
        uiController.ShowPlayerSetupPanel(initialCount, playerManager.MaxPlayers);
        uiController.OnAddPlayer  += AddPlayerAsync;
        uiController.OnStartGame  += BeginGame;
    }

    private async void AddPlayerAsync()
    {
        bool added = false;
        try
        {
            added = await BoardSession.PresentAddPlayerSelector();
        }
        catch (System.InvalidOperationException)
        {
#if UNITY_EDITOR
            // Board player selector is unavailable in the editor — simulate adding a player.
            if (_editorPlayerCount < playerManager.MaxPlayers)
            {
                _editorPlayerCount++;
                added = true;
            }
#endif
        }

        if (added)
            uiController.UpdatePlayerSetupPanel(
#if UNITY_EDITOR
                _editorPlayerCount,
#else
                BoardSession.players?.Length ?? 0,
#endif
                playerManager.MaxPlayers);
    }

    private void BeginGame()
    {
        isGameActive = true;
        currentPlayerIndex = 0;
        cardSystem.ShuffleDeck();
#if UNITY_EDITOR
        playerManager.ResetPlayers(_editorPlayerCount);
#else
        playerManager.ResetPlayers();
#endif
        uiController.ShowGameBoard();
        // Snap every token to space 0 immediately so they appear at START, not center.
        for (int i = 0; i < playerManager.PlayerCount; i++)
            uiController.MoveToken(playerManager.GetPlayer(i), 0);
        StartCoroutine(TurnLoop());
    }

    // ── Turn Loop ────────────────────────────────────────────────────────────

    private IEnumerator TurnLoop()
    {
        while (isGameActive)
        {
            var player = playerManager.GetPlayer(currentPlayerIndex);
            uiController.ShowPlayerTurn(player);

            // Wait for the player to tap "Draw Card" or place a physical piece.
            yield return uiController.WaitForDrawCard();

            var card = cardSystem.DrawCard();
            int targetIndex;

            if (card.kind == CardSystem.CardKind.Special)
            {
                // Character card: move directly to that board space.
                int specialIndex = jungleBoard.GetSpecialSpaceIndex(card.specialType);
                targetIndex = specialIndex >= 0 ? specialIndex : player.position;
            }
            else
            {
                int stepsToColor = jungleBoard.GetStepsToNextColor(player.position, card.color);
                targetIndex = Mathf.Clamp(player.position + stepsToColor, 0, jungleBoard.GoalIndex);
            }

            int steps = Mathf.Abs(targetIndex - player.position);
            uiController.ShowDrawnCard(card, steps);
            yield return new WaitForSeconds(1.0f);


            yield return MovePlayerToIndex(player, targetIndex);
            uiController.HideCountingDisplay();

            // Require physical piece placement on the correct space before ending turn
            yield return uiController.WaitForPieceOnSpace(player.position);

            // Only resolve board special effects after normal color movement.
            if (card.kind == CardSystem.CardKind.Color &&
                jungleBoard.IsSpecialSpace(player.position, out var special))
                yield return HandleSpecialSpace(player, special);

            if (player.position >= jungleBoard.GoalIndex)
            {
                player.position = jungleBoard.GoalIndex; // snap — never overshoot
                isGameActive = false;
                uiController.ShowWinPanel(player);
                yield break;
            }

            currentPlayerIndex = (currentPlayerIndex + 1) % playerManager.PlayerCount;
        }
    }

    // Step the token one space at a time so kids can count along.
    private IEnumerator MovePlayerToIndex(PlayerToken player, int targetIndex)
    {
        targetIndex = Mathf.Clamp(targetIndex, 0, jungleBoard.GoalIndex);
        int totalSteps = Mathf.Abs(targetIndex - player.position);
        if (totalSteps == 0) yield break;

        int direction = targetIndex > player.position ? 1 : -1;
        for (int i = 1; i <= totalSteps; i++)
        {
            player.position += direction;
            uiController.MoveToken(player, player.position);
            uiController.ShowCountingNumber(i, totalSteps);
            yield return new WaitForSeconds(stepDelay);
        }
    }

    private IEnumerator HandleSpecialSpace(PlayerToken player, JungleBoard.SpecialSpace special)
    {
        try
        {
            uiController.ShowSpecialAnimation(special.type); // shows message + plays animator
            yield return new WaitForSeconds(1.5f);

            // Clamp in case a linked index is misconfigured in the inspector.
            player.position = Mathf.Clamp(special.linkedIndex, 0, jungleBoard.GoalIndex);
            uiController.MoveToken(player, player.position);
            yield return new WaitForSeconds(0.75f);
        }
        finally
        {
            // Always clear the message so UI state cannot block the next turn.
            uiController.HideSpecialMessage();
        }
    }

    // ── Pause Screen ─────────────────────────────────────────────────────────

    private async void HandlePauseAction(BoardPauseAction action)
    {
        switch (action)
        {
            case BoardPauseAction.Resume:
                break;

            case BoardPauseAction.ExitGameSaved:
                await SaveCurrentGameAsync();
                BoardApplication.Exit();
                break;

            case BoardPauseAction.ExitGameUnsaved:
                BoardApplication.Exit();
                break;
        }
    }

    // ── Save / Load ──────────────────────────────────────────────────────────

    private async System.Threading.Tasks.Task SaveCurrentGameAsync()
    {
        if (!isGameActive) return;

        var data  = playerManager.GetSaveData(currentPlayerIndex);
        var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        var meta  = new BoardSaveGameMetadataChange
        {
            description = "Chase's Jungle Adventure",
            playedTime  = 0ul,
            gameVersion = Application.version
        };

        try
        {
            if (currentSaveId != null)
                await BoardSaveGameManager.UpdateSaveGame(currentSaveId, bytes, meta);
            else
            {
                var saved     = await BoardSaveGameManager.CreateSaveGame(bytes, meta);
                currentSaveId = saved.id;
            }
        }
        catch { /* Save failed silently — don't crash the game. */ }
    }

    private async void LoadAndContinue()
    {
        if (currentSaveId == null) { StartPlayerSetup(); return; }

        try
        {
            // LoadSaveGame automatically restores BoardSession.players for the save.
            var bytes = await BoardSaveGameManager.LoadSaveGame(currentSaveId);
            var data  = JsonUtility.FromJson<GameSaveData>(Encoding.UTF8.GetString(bytes));

            currentPlayerIndex = data.currentPlayerIndex;
            isGameActive       = true;

            cardSystem.ShuffleDeck();
            playerManager.ResetPlayers();
            playerManager.RestorePositions(data.playerPositions);

            uiController.ShowGameBoard();
            for (int i = 0; i < playerManager.PlayerCount; i++)
            {
                var p = playerManager.GetPlayer(i);
                uiController.MoveToken(p, p.position);
            }

            StartCoroutine(TurnLoop());
        }
        catch
        {
            // Load failed — fall back to a fresh game.
            StartPlayerSetup();
        }
    }
}
