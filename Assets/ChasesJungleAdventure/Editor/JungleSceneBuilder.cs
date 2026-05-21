// ============================================================
//  Chase's Jungle Adventure — Scene Builder
//
//  STEP 1: Copy ALL .cs files from the Scripts folder into
//          your Unity Assets folder and wait for compile.
//
//  STEP 2: Copy THIS file into Assets/Editor/ (any Editor folder).
//
//  STEP 3: In Unity top menu → Board > Build Chase's Jungle Adventure Scene
//
//  The entire scene is built and wired automatically.
// ============================================================
#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

public static class JungleSceneBuilder
{
    // ── Space colors: Red, Blue, Green, Yellow, Purple, Orange ───────────────
    static readonly Color[] SpaceColors =
    {
        new Color(0.95f, 0.25f, 0.25f),
        new Color(0.25f, 0.45f, 1.00f),
        new Color(0.15f, 0.80f, 0.20f),
        new Color(1.00f, 0.88f, 0.10f),
        new Color(0.65f, 0.15f, 0.95f),
        new Color(1.00f, 0.58f, 0.10f),
    };

    static readonly Color PathShadowColor     = new Color(0.18f, 0.10f, 0.02f, 0.32f);
    static readonly Color PathBaseColor       = new Color(0.78f, 0.58f, 0.20f, 0.96f);
    static readonly Color PathHighlightColor  = new Color(0.95f, 0.84f, 0.48f, 0.92f);
    static readonly Color PanelJungleTint     = new Color(0.06f, 0.28f, 0.12f);
    static readonly Color LeafDarkColor       = new Color(0.08f, 0.31f, 0.12f, 0.90f);
    static readonly Color LeafLightColor      = new Color(0.20f, 0.50f, 0.18f, 0.75f);
    static readonly Color MistColor           = new Color(1f, 1f, 1f, 0.08f);
    static readonly Color WaterfallColor      = new Color(0.52f, 0.86f, 1.00f, 0.50f);

    // ── Find a user script type without a hard compile-time reference ────────
    // This means the scene builder compiles even before the game scripts exist.
    static System.Type G(string name) =>
        System.AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name))
            .FirstOrDefault(t => t != null);

    [MenuItem("Board/Build Chase's Jungle Adventure Scene")]
    public static void BuildScene()
    {
        // ── Step 1: Make sure all game scripts are compiled and available ────
        string[] required =
        {
            "GameManager", "JungleBoard", "CardSystem",
            "PlayerManager", "JungleUIController", "BoardInputHandler"
        };

        var missing = required.Where(n => G(n) == null).ToArray();
        if (missing.Length > 0)
        {
            EditorUtility.DisplayDialog(
                "Missing Game Scripts",
                "These scripts are not in your Unity project yet:\n\n" +
                "  - " + string.Join("\n  - ", missing) + "\n\n" +
                "HOW TO FIX:\n" +
                "1. Go to  github.com/negulizer/chases-jungle-adventure\n" +
                "2. Click the green 'Code' button -> Download ZIP\n" +
                "3. Unzip it\n" +
                "4. Open the folder:  Assets / ChasesJungleAdventure / Scripts\n" +
                "5. Drag ALL the .cs files into your Unity Assets panel\n" +
                "6. Wait for the spinner (bottom-right of Unity) to stop\n" +
                "7. Then run  Board > Build Chase's Jungle Adventure Scene  again",
                "OK");
            return;
        }

        // ── Step 2: Ask to save the current scene, then start fresh ─────────
        if (EditorSceneManager.GetActiveScene().isDirty)
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ════════════════════════════════════════════════════════════════════
        //   BUILD EVERYTHING
        // ════════════════════════════════════════════════════════════════════

        // ── Canvas ───────────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem — required for buttons to receive input.
        // We add the EventSystem component and then try the new Input System module first
        // (required when Active Input Handling = "Input System Package" or "Both").
        // If that type isn't available we fall back to the legacy StandaloneInputModule.
        var esGO       = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        var newModule  = System.Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (newModule != null)
            esGO.AddComponent(newModule);
        else
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── One GameManager object holds ALL scripts ──────────────────────────
        var gmGO = new GameObject("GameManager");
        var gm   = gmGO.AddComponent(G("GameManager"));
        var jb   = gmGO.AddComponent(G("JungleBoard"));
        var cs   = gmGO.AddComponent(G("CardSystem"));
        var pm   = gmGO.AddComponent(G("PlayerManager"));
        var ui   = gmGO.AddComponent(G("JungleUIController"));
        var bih  = gmGO.AddComponent(G("BoardInputHandler"));

        // ── Four full-screen panels ───────────────────────────────────────────
        var welcomePanel = MakePanel(canvasGO, "WelcomePanel",     new Color(0.08f, 0.40f, 0.08f));
        var setupPanel   = MakePanel(canvasGO, "PlayerSetupPanel", new Color(0.08f, 0.25f, 0.50f));
        var boardPanel   = MakePanel(canvasGO, "GameBoardPanel",   PanelJungleTint);
        var winPanel     = MakePanel(canvasGO, "WinPanel",         new Color(0.85f, 0.65f, 0.05f));

        DecorateMenuPanel(welcomePanel, new Color(0.08f, 0.34f, 0.14f, 0.92f), new Color(0.95f, 0.84f, 0.44f, 0.18f));
        DecorateMenuPanel(setupPanel,   new Color(0.06f, 0.24f, 0.40f, 0.92f), new Color(0.60f, 0.88f, 1.00f, 0.16f));
        DecorateMenuPanel(winPanel,     new Color(0.70f, 0.48f, 0.05f, 0.92f), new Color(1.00f, 0.94f, 0.70f, 0.20f));
        DecorateBoardBackground(boardPanel);

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        //   WELCOME PANEL
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        var titleTMP = MakeTMP(welcomePanel, "TitleText",
            "Chase's Jungle Adventure!", 72f, Color.white);
        Anchor(titleTMP.gameObject, new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.88f));
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.characterSpacing = 5f;

        var newGameBtn  = MakeButton(welcomePanel, "NewGameButton",  "New Game");
        CenterAt(newGameBtn.gameObject, new Vector2(0,  -30), new Vector2(420, 90));

        var continueBtn = MakeButton(welcomePanel, "ContinueButton", "Continue");
        CenterAt(continueBtn.gameObject, new Vector2(0, -140), new Vector2(420, 90));
        continueBtn.gameObject.SetActive(false); // hidden — shown only when a save exists

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        //   PLAYER SETUP PANEL
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        var setupTitleTMP = MakeTMP(setupPanel, "SetupTitle", "Who's Playing?", 64f, Color.white);
        Anchor(setupTitleTMP.gameObject, new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.90f));
        setupTitleTMP.fontStyle = FontStyles.Bold;
        setupTitleTMP.characterSpacing = 3f;

        var playerCountTMP = MakeTMP(setupPanel, "PlayerCountText", "Players: 1", 48f, Color.yellow);
        Anchor(playerCountTMP.gameObject, new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.72f));
        playerCountTMP.fontStyle = FontStyles.Bold;

        var addPlayerBtn = MakeButton(setupPanel, "AddPlayerButton",  "Add Player");
        CenterAt(addPlayerBtn.gameObject, new Vector2(0, -30), new Vector2(420, 90));

        var startGameBtn = MakeButton(setupPanel, "StartGameButton", "Start Adventure!");
        CenterAt(startGameBtn.gameObject, new Vector2(0, -140), new Vector2(500, 90));

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        //   GAME BOARD PANEL
        //   Layout (1920x1080, Y=0 is center):
        //     Top HUD:   Y = 88%-97%  (player turn + card drawn)
        //     Board:     Y = 14%-84%  (60 spaces, 6 rows of 10)
        //     Button:    Y = 2%-12%   (Draw Card)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        // HUD — two text lines anchored to the top strip
        var hudTopPlate = MakeRoundedPlate(boardPanel, "HudTopPlate", new Vector2(0.5f, 0.905f), new Vector2(1220f, 150f), new Color(0.06f, 0.18f, 0.08f, 0.70f), 0f);
        hudTopPlate.transform.SetAsLastSibling();

        var playerTurnTMP = MakeTMP(boardPanel, "PlayerTurnText", "Player 1's turn!", 50f, Color.white);
        Anchor(playerTurnTMP.gameObject, new Vector2(0.03f, 0.90f), new Vector2(0.97f, 0.99f));
        playerTurnTMP.fontStyle = FontStyles.Bold;
        playerTurnTMP.characterSpacing = 2f;

        var cardDrawnTMP = MakeTMP(boardPanel, "CardDrawnText", "", 40f, Color.yellow);
        Anchor(cardDrawnTMP.gameObject, new Vector2(0.03f, 0.82f), new Vector2(0.97f, 0.90f));
        cardDrawnTMP.fontStyle = FontStyles.Bold;

        // Big counting number — left of the card popup so they never overlap
        var countingPlate = MakeRoundedPlate(boardPanel, "CountingPlate", new Vector2(0.37f, 0.53f), new Vector2(300f, 240f), new Color(0.05f, 0.11f, 0.06f, 0.68f), -4f);
        countingPlate.SetActive(false);
        countingPlate.transform.SetAsLastSibling();
        var countingTMP = MakeTMP(boardPanel, "CountingText", "", 130f, Color.white);
        Anchor(countingTMP.gameObject, new Vector2(0.28f, 0.40f), new Vector2(0.46f, 0.66f));
        countingTMP.fontStyle = FontStyles.Bold;
        countingTMP.gameObject.SetActive(false);
        countingTMP.characterSpacing = 10f;

        // Special-space message — center overlay, hidden until triggered
        var specialPlate = MakeRoundedPlate(boardPanel, "SpecialPlate", new Vector2(0.5f, 0.50f), new Vector2(1180f, 180f), new Color(0.16f, 0.09f, 0.02f, 0.78f), 0f);
        specialPlate.SetActive(false);
        specialPlate.transform.SetAsLastSibling();
        var specialMsgTMP = MakeTMP(boardPanel, "SpecialMessageText", "", 46f, new Color(1f, 0.95f, 0.2f));
        Anchor(specialMsgTMP.gameObject, new Vector2(0.05f, 0.40f), new Vector2(0.95f, 0.60f));
        specialMsgTMP.fontStyle = FontStyles.Bold;
        specialMsgTMP.gameObject.SetActive(false);
        specialMsgTMP.characterSpacing = 1.5f;

        // Draw Card button — anchored to bottom strip, hidden until it's draw time
        var drawCardBtn = MakeButton(boardPanel, "DrawCardButton", "Draw a Card!");
        BottomCenterAt(drawCardBtn.gameObject, 20f, new Vector2(500, 90));
        drawCardBtn.gameObject.SetActive(false);

        // ── 60 Board spaces in a winding S-path ──────────────────────────────
        //   Rows 0,2,4 go Left→Right; rows 1,3,5 go Right→Left.
        //   The board area sits in the middle 70% of the screen height.
        //
        //   xMin=-700  xMax=700  → 10 spaces, 155.5 px apart horizontally
        //   yStart=-300  yStep=120 → 6 rows (y = -300,-180,-60,+60,+180,+300)

        // Circle sprite needed for Candy Land-style round spaces.
        EnsureFolder("Assets/ChasesJungleAdventure");
        var circleSprite = GetOrCreateCircleSprite();

        var boardSpacesGO = new GameObject("BoardSpaces");
        boardSpacesGO.transform.SetParent(boardPanel.transform, false);
        var bsRT = boardSpacesGO.AddComponent<RectTransform>();
        // Centre the board container slightly below screen-centre (gives room for top HUD)
        bsRT.anchorMin = bsRT.anchorMax = new Vector2(0.5f, 0.48f);
        bsRT.sizeDelta        = Vector2.zero;
        bsRT.anchoredPosition = Vector2.zero;
        // Keep the board trail and spaces above the decorative background layers.
        boardSpacesGO.transform.SetAsLastSibling();

        const float xMin   = -700f, xMax   =  700f;
        const float yStart = -300f, yStep  =  120f;

        var spacePts = new Vector2[60];
        for (int i = 0; i < 60; i++)
        {
            int   row = i / 10, col = i % 10;
            float t   = col / 9f;
            float x   = (row % 2 == 0) ? Mathf.Lerp(xMin, xMax, t)
                                        : Mathf.Lerp(xMax, xMin, t);
            spacePts[i] = new Vector2(x, yStart + row * yStep);
        }

        // Board trail — single cream ribbon, Candy Land style
        for (int i = 0; i < 59; i++)
        {
            var dir = spacePts[i + 1] - spacePts[i];
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Vector2 center = (spacePts[i] + spacePts[i + 1]) * 0.5f;
            float connLen = Mathf.Max(0f, dir.magnitude - 62f);
            MakeTrailSegment(boardSpacesGO, $"Trail_{i:D2}", center, connLen, 56f, angle,
                new Color(0.94f, 0.90f, 0.74f, 1f));
        }

        // Circular turn pads at row-end positions so corners look rounded
        int[] turnSpaces = { 9, 10, 19, 20, 29, 30, 39, 40, 49, 50 };
        foreach (int idx in turnSpaces)
            MakeTurnCircle(boardSpacesGO, $"TurnPad_{idx:D2}", spacePts[idx], circleSprite);

        // Text labels are only used for START/WIN.
        var labels = new string[60];
        labels[0]  = "START";
        labels[59] = "WIN";

        var spaceRTs = new RectTransform[60];
        for (int i = 0; i < 60; i++)
        {
            var spGO = new GameObject($"Space_{i:D2}");
            spGO.transform.SetParent(boardSpacesGO.transform, false);

            var rt = spGO.AddComponent<RectTransform>();
            bool isSpecial = i == 0 || i == 7 || i == 15 || i == 22 || i == 35 || i == 40 || i == 59;
            rt.sizeDelta        = isSpecial ? new Vector2(90, 90) : new Vector2(80, 80);
            rt.anchoredPosition = spacePts[i];

            // Color — special spaces get their own tint
            Color col = SpaceColors[i % SpaceColors.Length];
            if      (i ==  0)                                  col = new Color(0.20f, 0.70f, 0.20f); // start — bright green
            else if (i == 59)                                  col = new Color(0.20f, 0.55f, 1.00f); // waterfall — blue
            else if (i == 40)                                  col = new Color(0.10f, 0.80f, 0.90f); // raft — teal
            else if (i == 7 || i == 15 || i == 22 || i == 35) // obstacle — darkened
                col = new Color(col.r * 0.60f, col.g * 0.60f, col.b * 0.60f, 1f);

            BuildBoardSpaceVisual(spGO, col, isSpecial, circleSprite);

            // Real icon graphics for special spaces (no emoji text fallback boxes).
            if (i == 7)  AddSpecialIconGraphic(spGO.transform, "Spider", circleSprite);
            if (i == 15) AddSpecialIconGraphic(spGO.transform, "Snake", circleSprite);
            if (i == 22) AddSpecialIconGraphic(spGO.transform, "Monkey", circleSprite);
            if (i == 35) AddSpecialIconGraphic(spGO.transform, "Alligator", circleSprite);
            if (i == 40) AddSpecialIconGraphic(spGO.transform, "Raft", circleSprite);

            // START/WIN labels only.
            if (labels[i] != null)
            {
                var lblGO = new GameObject("Label");
                lblGO.transform.SetParent(spGO.transform, false);
                var lrt   = lblGO.AddComponent<RectTransform>();
                lrt.anchorMin = new Vector2(-0.10f, 0.04f);
                lrt.anchorMax = new Vector2(1.10f, 0.44f);
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;
                var lbl   = lblGO.AddComponent<TextMeshProUGUI>();
                lbl.text      = labels[i];
                lbl.fontSize  = 16f;
                lbl.fontStyle = FontStyles.Bold;
                lbl.alignment = TextAlignmentOptions.Center;
                lbl.color     = Color.white;
                lbl.overflowMode = TextOverflowModes.Overflow;
            }

            spaceRTs[i] = rt;
        }

        // ── Ensure overlays are always on top ─────────────────────────────
        // (Counting and special message plates/texts must be above all board/tokens)
        countingPlate.transform.SetAsLastSibling();
        countingTMP.transform.SetAsLastSibling();
        specialPlate.transform.SetAsLastSibling();
        specialMsgTMP.transform.SetAsLastSibling();

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        //   WIN PANEL
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        var winTMP = MakeTMP(winPanel, "WinText",
            "You reached the Waterfall!\nYOU WIN!", 80f, Color.white);
        Anchor(winTMP.gameObject, new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.75f));
        winTMP.fontStyle = FontStyles.Bold;
        winTMP.characterSpacing = 4f;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━        //   DRAWN CARD VISUAL  (Candy Land-style card that pops up mid-board)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        var cardVisualGO = new GameObject("CardVisual");
        cardVisualGO.transform.SetParent(boardPanel.transform, false);
        var cvRT = cardVisualGO.AddComponent<RectTransform>();
        cvRT.anchorMin = cvRT.anchorMax = new Vector2(0.5f, 0.5f);
        cvRT.sizeDelta = new Vector2(280f, 370f);
        cvRT.anchoredPosition = new Vector2(320f, 40f);
        var cvBg = cardVisualGO.AddComponent<Image>();
        cvBg.color = new Color(0.98f, 0.96f, 0.97f, 1f);

        // Pink inner border
        var cvInner = new GameObject("Inner"); cvInner.transform.SetParent(cardVisualGO.transform, false);
        var cvIRT = cvInner.AddComponent<RectTransform>();
        cvIRT.anchorMin = new Vector2(0.04f, 0.04f); cvIRT.anchorMax = new Vector2(0.96f, 0.96f);
        cvIRT.offsetMin = cvIRT.offsetMax = Vector2.zero;
        cvInner.AddComponent<Image>().color = new Color(0.97f, 0.87f, 0.94f, 1f);

        // Large colour swatch (circle)
        var swatchGO = new GameObject("ColorSwatch"); swatchGO.transform.SetParent(cardVisualGO.transform, false);
        var swatchRT = swatchGO.AddComponent<RectTransform>();
        swatchRT.anchorMin = new Vector2(0.12f, 0.24f); swatchRT.anchorMax = new Vector2(0.88f, 0.88f);
        swatchRT.offsetMin = swatchRT.offsetMax = Vector2.zero;
        var swatchImg = swatchGO.AddComponent<Image>();
        swatchImg.color = Color.red;
        if (circleSprite != null) swatchImg.sprite = circleSprite;

        // Dedicated special-card icons layered in the same region as the color swatch.
        var specialIconsGO = new GameObject("SpecialIcons");
        specialIconsGO.transform.SetParent(cardVisualGO.transform, false);
        var siRT = specialIconsGO.AddComponent<RectTransform>();
        siRT.anchorMin = new Vector2(0.12f, 0.24f); siRT.anchorMax = new Vector2(0.88f, 0.88f);
        siRT.offsetMin = siRT.offsetMax = Vector2.zero;
        AddSpecialIconGraphic(specialIconsGO.transform, "Spider", circleSprite);
        AddSpecialIconGraphic(specialIconsGO.transform, "Snake", circleSprite);
        AddSpecialIconGraphic(specialIconsGO.transform, "Monkey", circleSprite);
        AddSpecialIconGraphic(specialIconsGO.transform, "Alligator", circleSprite);
        AddSpecialIconGraphic(specialIconsGO.transform, "Raft", circleSprite);
        specialIconsGO.SetActive(false);

        // Shine on swatch
        var swatchShine = new GameObject("Shine"); swatchShine.transform.SetParent(swatchGO.transform, false);
        var ssRT = swatchShine.AddComponent<RectTransform>();
        ssRT.anchorMin = new Vector2(0.14f, 0.56f); ssRT.anchorMax = new Vector2(0.86f, 0.86f);
        ssRT.offsetMin = ssRT.offsetMax = Vector2.zero;
        ssRT.localRotation = Quaternion.Euler(0, 0, -12f);
        swatchShine.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.28f);

        // Bottom label
        var cvLblGO = new GameObject("CardLabel"); cvLblGO.transform.SetParent(cardVisualGO.transform, false);
        var cvLblRT = cvLblGO.AddComponent<RectTransform>();
        cvLblRT.anchorMin = new Vector2(0.05f, 0.03f); cvLblRT.anchorMax = new Vector2(0.95f, 0.22f);
        cvLblRT.offsetMin = cvLblRT.offsetMax = Vector2.zero;
        var cvLblTMP = cvLblGO.AddComponent<TextMeshProUGUI>();
        cvLblTMP.text = "Move forward!"; cvLblTMP.fontSize = 26f;
        cvLblTMP.fontStyle = FontStyles.Bold; cvLblTMP.alignment = TextAlignmentOptions.Center;
        cvLblTMP.color = new Color(0.45f, 0.20f, 0.55f, 1f);
        cardVisualGO.SetActive(false);
        cardVisualGO.transform.SetAsLastSibling();

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━        //   PLAYER TOKEN PREFAB
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        EnsureFolder("Assets/ChasesJungleAdventure");
        var tokenGO = new GameObject("PlayerToken");
        var trt     = tokenGO.AddComponent<RectTransform>();
        trt.sizeDelta = new Vector2(54, 54);
        var tokenShadow = new GameObject("Shadow");
        tokenShadow.transform.SetParent(tokenGO.transform, false);
        var tokenShadowRT = tokenShadow.AddComponent<RectTransform>();
        tokenShadowRT.anchorMin = Vector2.zero;
        tokenShadowRT.anchorMax = Vector2.one;
        tokenShadowRT.offsetMin = new Vector2(5f, -5f);
        tokenShadowRT.offsetMax = new Vector2(5f, -5f);
        tokenShadow.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.22f);

        var tokenRing = tokenGO.AddComponent<Image>();
        tokenRing.color = new Color(0.23f, 0.15f, 0.05f, 1f);

        var tokenFace = new GameObject("Face");
        tokenFace.transform.SetParent(tokenGO.transform, false);
        var tokenFaceRT = tokenFace.AddComponent<RectTransform>();
        tokenFaceRT.anchorMin = new Vector2(0.14f, 0.14f);
        tokenFaceRT.anchorMax = new Vector2(0.86f, 0.86f);
        tokenFaceRT.offsetMin = tokenFaceRT.offsetMax = Vector2.zero;
        tokenFace.AddComponent<Image>().color = Color.white;

        var tokenShine = new GameObject("Shine");
        tokenShine.transform.SetParent(tokenFace.transform, false);
        var tokenShineRT = tokenShine.AddComponent<RectTransform>();
        tokenShineRT.anchorMin = new Vector2(0.16f, 0.56f);
        tokenShineRT.anchorMax = new Vector2(0.84f, 0.84f);
        tokenShineRT.offsetMin = tokenShineRT.offsetMax = Vector2.zero;
        tokenShineRT.localRotation = Quaternion.Euler(0, 0, -10f);
        tokenShine.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.28f);

        var tokenMark = new GameObject("Mark");
        tokenMark.transform.SetParent(tokenGO.transform, false);
        var tokenMarkRT = tokenMark.AddComponent<RectTransform>();
        tokenMarkRT.anchorMin = new Vector2(0.24f, 0.24f);
        tokenMarkRT.anchorMax = new Vector2(0.76f, 0.76f);
        tokenMarkRT.offsetMin = tokenMarkRT.offsetMax = Vector2.zero;
        var tokenMarkTMP = tokenMark.AddComponent<TextMeshProUGUI>();
        tokenMarkTMP.text = "!";
        tokenMarkTMP.fontSize = 28f;
        tokenMarkTMP.fontStyle = FontStyles.Bold;
        tokenMarkTMP.color = new Color(0.16f, 0.10f, 0.02f, 0.90f);
        tokenMarkTMP.alignment = TextAlignmentOptions.Center;

        string       pfPath     = "Assets/ChasesJungleAdventure/PlayerToken.prefab";
        var          tokenPrefab = PrefabUtility.SaveAsPrefabAsset(tokenGO, pfPath);
        Object.DestroyImmediate(tokenGO);
        AssetDatabase.Refresh();

        // Token parent container (tokens are instantiated here at runtime)
        var tokenParentGO = new GameObject("TokenParent");
        tokenParentGO.transform.SetParent(boardPanel.transform, false);
        var tpRT = tokenParentGO.AddComponent<RectTransform>();
        tpRT.anchorMin = Vector2.zero; tpRT.anchorMax = Vector2.one;
        tpRT.offsetMin = tpRT.offsetMax = Vector2.zero;

        // ── Hide panels not visible at startup ────────────────────────────────
        setupPanel.SetActive(false);
        boardPanel.SetActive(false);
        winPanel.SetActive(false);

        // ════════════════════════════════════════════════════════════════════
        //   WIRE ALL INSPECTOR FIELDS
        //   (using SerializedObject so Unity tracks the links properly)
        // ════════════════════════════════════════════════════════════════════

        // GameManager
        var soGM = new SerializedObject(gm);
        soGM.FindProperty("jungleBoard").objectReferenceValue   = jb;
        soGM.FindProperty("cardSystem").objectReferenceValue    = cs;
        soGM.FindProperty("playerManager").objectReferenceValue = pm;
        soGM.FindProperty("uiController").objectReferenceValue  = ui;
        soGM.ApplyModifiedProperties();

        // PlayerManager
        var soPM = new SerializedObject(pm);
        soPM.FindProperty("tokenPrefab").objectReferenceValue = tokenPrefab;
        soPM.FindProperty("tokenParent").objectReferenceValue = tokenParentGO.transform;
        soPM.ApplyModifiedProperties();

        // JungleUIController — all UI references
        var soUI = new SerializedObject(ui);
        soUI.FindProperty("welcomePanel").objectReferenceValue       = welcomePanel;
        soUI.FindProperty("playerSetupPanel").objectReferenceValue   = setupPanel;
        soUI.FindProperty("gameBoardPanel").objectReferenceValue     = boardPanel;
        soUI.FindProperty("winPanel").objectReferenceValue           = winPanel;
        soUI.FindProperty("newGameButton").objectReferenceValue      = newGameBtn;
        soUI.FindProperty("continueButton").objectReferenceValue     = continueBtn;
        soUI.FindProperty("addPlayerButton").objectReferenceValue    = addPlayerBtn;
        soUI.FindProperty("startGameButton").objectReferenceValue    = startGameBtn;
        soUI.FindProperty("drawCardButton").objectReferenceValue     = drawCardBtn;
        soUI.FindProperty("playerTurnText").objectReferenceValue     = playerTurnTMP;
        soUI.FindProperty("cardDrawnText").objectReferenceValue      = cardDrawnTMP;
        soUI.FindProperty("countingPlate").objectReferenceValue      = countingPlate;
        soUI.FindProperty("countingText").objectReferenceValue       = countingTMP;
        soUI.FindProperty("specialMessagePlate").objectReferenceValue = specialPlate;
        soUI.FindProperty("specialMessageText").objectReferenceValue = specialMsgTMP;
        soUI.FindProperty("winText").objectReferenceValue            = winTMP;
        soUI.FindProperty("playerCountText").objectReferenceValue    = playerCountTMP;
        soUI.FindProperty("inputHandler").objectReferenceValue       = bih;
        soUI.FindProperty("cardVisual").objectReferenceValue         = cardVisualGO;
        soUI.FindProperty("cardColorSwatch").objectReferenceValue    = swatchImg;

        var spProp = soUI.FindProperty("spacePositions");
        spProp.arraySize = 60;
        for (int i = 0; i < 60; i++)
            spProp.GetArrayElementAtIndex(i).objectReferenceValue = spaceRTs[i];

        soUI.ApplyModifiedProperties();

        // ── Wire button OnClick events (via reflection — no compile-time dependency) ──
        Wire(newGameBtn,   ui, "OnNewGameButtonPressed");
        Wire(continueBtn,  ui, "OnContinueButtonPressed");
        Wire(addPlayerBtn, ui, "OnAddPlayerButtonPressed");
        Wire(startGameBtn, ui, "OnStartGameButtonPressed");
        Wire(drawCardBtn,  ui, "OnDrawCardButtonPressed");

        // ── Save the scene ────────────────────────────────────────────────────
        EnsureFolder("Assets/ChasesJungleAdventure/Scenes");
        EditorSceneManager.SaveScene(
            EditorSceneManager.GetActiveScene(),
            "Assets/ChasesJungleAdventure/Scenes/GameScene.unity");

        EditorUtility.DisplayDialog(
            "Scene Built Successfully!",
            "Chase's Jungle Adventure scene is ready!\n\n" +
            "Press the Play button (triangle at the top of Unity) to test it.\n\n" +
            "You will see the green Welcome screen with a 'New Game' button.",
            "Let's Play!");
    }

    // ════════════════════════════════════════════════════════════════════════
    //   HELPER METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Full-screen panel stretched to fill its parent.</summary>
    static GameObject MakePanel(GameObject parent, string name, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = bg;
        return go;
    }

    static void DecorateMenuPanel(GameObject panel, Color plateColor, Color sparkleColor)
    {
        MakeStretch(panel, "TopGlow", new Vector2(0f, 0.62f), new Vector2(1f, 1f), sparkleColor);
        MakeStretch(panel, "BottomShade", new Vector2(0f, 0f), new Vector2(1f, 0.26f), new Color(0f, 0f, 0f, 0.14f));
        MakeRoundedPlate(panel, "MainPlate", new Vector2(0.5f, 0.50f), new Vector2(1240f, 720f), plateColor, 0f).transform.SetAsFirstSibling();
    }

    static void DecorateBoardBackground(GameObject boardPanel)
    {
        MakeStretch(boardPanel, "SkyBand", new Vector2(0f, 0.55f), new Vector2(1f, 1f), new Color(0.34f, 0.67f, 0.92f));
        MakeStretch(boardPanel, "CanopyBack", new Vector2(0f, 0.45f), new Vector2(1f, 0.78f), new Color(0.18f, 0.48f, 0.18f, 0.95f));
        MakeStretch(boardPanel, "GroundBand", new Vector2(0f, 0f), new Vector2(1f, 0.44f), new Color(0.21f, 0.42f, 0.12f, 1f));
        MakeStretch(boardPanel, "MistBand", new Vector2(0f, 0.40f), new Vector2(1f, 0.58f), MistColor);

        MakeLeafCluster(boardPanel, "LeafClusterLeftTop", new Vector2(0.10f, 0.88f), 260f, LeafDarkColor, -20f);
        MakeLeafCluster(boardPanel, "LeafClusterRightTop", new Vector2(0.87f, 0.86f), 300f, LeafLightColor, 18f);
        MakeLeafCluster(boardPanel, "LeafClusterLeftBottom", new Vector2(0.08f, 0.16f), 280f, LeafLightColor, 22f);
        MakeLeafCluster(boardPanel, "LeafClusterRightBottom", new Vector2(0.90f, 0.18f), 300f, LeafDarkColor, -18f);

        MakeRibbon(boardPanel, "RiverRibbon", new Vector2(0.79f, 0.47f), new Vector2(480f, 120f), -18f, new Color(0.22f, 0.70f, 0.95f, 0.60f));
        MakeRibbon(boardPanel, "WaterfallGlow", new Vector2(0.93f, 0.71f), new Vector2(180f, 420f), 0f, WaterfallColor);
    }

    static GameObject MakeRoundedPlate(GameObject parent, string name, Vector2 anchor, Vector2 size, Color color, float angle)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.Euler(0, 0, angle);
        go.AddComponent<Image>().color = color;

        var shine = new GameObject("Shine");
        shine.transform.SetParent(go.transform, false);
        var shineRt = shine.AddComponent<RectTransform>();
        shineRt.anchorMin = new Vector2(0.05f, 0.58f);
        shineRt.anchorMax = new Vector2(0.95f, 0.92f);
        shineRt.offsetMin = shineRt.offsetMax = Vector2.zero;
        shine.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
        return go;
    }

    static void MakeLeafCluster(GameObject parent, string name, Vector2 anchor, float size, Color color, float tilt)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent.transform, false);
        var rt = root.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(size, size * 0.55f);
        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.Euler(0, 0, tilt);

        for (int i = 0; i < 5; i++)
        {
            var leaf = new GameObject($"Leaf_{i}");
            leaf.transform.SetParent(root.transform, false);
            var lrt = leaf.AddComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(size * 0.42f, size * 0.16f);
            lrt.anchoredPosition = new Vector2((i - 2) * size * 0.10f, (i % 2 == 0 ? 18f : -10f));
            lrt.localRotation = Quaternion.Euler(0, 0, -38f + (i * 18f));
            leaf.AddComponent<Image>().color = i % 2 == 0 ? color : new Color(color.r + 0.05f, color.g + 0.07f, color.b + 0.02f, color.a * 0.85f);
        }
    }

    static void MakeRibbon(GameObject parent, string name, Vector2 anchor, Vector2 size, float angle, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.Euler(0, 0, angle);
        go.AddComponent<Image>().color = color;
    }

    static void MakeStretch(GameObject parent, string name, Vector2 min, Vector2 max, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = color;
    }

    static void MakeTrailSegment(GameObject parent, string name, Vector2 center, float width, float height, float angle, Color color, Vector2 offset = default)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = center + offset;
        rt.sizeDelta = new Vector2(width, height);
        rt.localRotation = Quaternion.Euler(0, 0, angle);
        go.AddComponent<Image>().color = color;
    }

    static void MakeTurnPad(GameObject parent, string name, Vector2 center)
    {
        MakeTrailSegment(parent, name + "_Base", center, 90f, 90f, 0f, new Color(0.94f, 0.90f, 0.74f, 1f));
    }

    static void MakeTurnCircle(GameObject parent, string name, Vector2 center, Sprite circle)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = center;
        rt.sizeDelta = new Vector2(88f, 88f);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.94f, 0.90f, 0.74f, 1f);
        if (circle != null) img.sprite = circle;
    }

    /// <summary>Creates and saves a circle sprite to disk so it persists in the scene.</summary>
    static Sprite GetOrCreateCircleSprite()
    {
        const string path = "Assets/ChasesJungleAdventure/JungleCircle.png";
        // Re-use if already saved
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size / 2f, size / 2f);
        float r = size / 2f - 1.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(r - d + 1f)));
            }
        tex.Apply();
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.Refresh();

        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.textureType        = TextureImporterType.Sprite;
            ti.spritePixelsPerUnit = size;
            ti.filterMode         = FilterMode.Bilinear;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void BuildBoardSpaceVisual(GameObject space, Color fillColor, bool isSpecial, Sprite circle)
    {
        // White border ring using circle sprite
        var ringImg = space.AddComponent<Image>();
        ringImg.color = Color.white;
        if (circle != null) ringImg.sprite = circle;
        ringImg.raycastTarget = false;

        // Coloured face slightly inset
        var face = new GameObject("Face");
        face.transform.SetParent(space.transform, false);
        var faceRt = face.AddComponent<RectTransform>();
        faceRt.anchorMin = new Vector2(0.12f, 0.12f);
        faceRt.anchorMax = new Vector2(0.88f, 0.88f);
        faceRt.offsetMin = faceRt.offsetMax = Vector2.zero;
        var faceImg = face.AddComponent<Image>();
        faceImg.color = fillColor;
        if (circle != null) faceImg.sprite = circle;

        // Shine on non-specials; glow ring on specials
        if (!isSpecial)
        {
            var shine = new GameObject("Shine"); shine.transform.SetParent(face.transform, false);
            var sRT = shine.AddComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.14f, 0.56f); sRT.anchorMax = new Vector2(0.86f, 0.84f);
            sRT.offsetMin = sRT.offsetMax = Vector2.zero;
            shine.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);
        }
        else
        {
            var glow = new GameObject("Glow"); glow.transform.SetParent(space.transform, false);
            var gRT = glow.AddComponent<RectTransform>();
            gRT.anchorMin = new Vector2(-0.18f, -0.18f); gRT.anchorMax = new Vector2(1.18f, 1.18f);
            gRT.offsetMin = gRT.offsetMax = Vector2.zero;
            var glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(fillColor.r, fillColor.g, fillColor.b, 0.25f);
            if (circle != null) glowImg.sprite = circle;
            glow.transform.SetAsFirstSibling();
        }
    }

    static void AddSpecialIconGraphic(Transform parent, string type, Sprite circle)
    {
        var root = new GameObject(type);
        root.transform.SetParent(parent, false);
        var rrt = root.AddComponent<RectTransform>();
        rrt.anchorMin = new Vector2(0.18f, 0.18f);
        rrt.anchorMax = new Vector2(0.82f, 0.82f);
        rrt.offsetMin = rrt.offsetMax = Vector2.zero;

        // Badge background
        var bg = new GameObject("Bg");
        bg.transform.SetParent(root.transform, false);
        var bgrt = bg.AddComponent<RectTransform>();
        bgrt.anchorMin = new Vector2(0.10f, 0.10f);
        bgrt.anchorMax = new Vector2(0.90f, 0.90f);
        bgrt.offsetMin = bgrt.offsetMax = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(1f, 1f, 1f, 0.92f);
        if (circle != null) bgImg.sprite = circle;

        switch (type)
        {
            case "Spider":
                AddCircle(root.transform, "Body", new Vector2(0.34f, 0.32f), new Vector2(0.66f, 0.64f), new Color(0.22f, 0.22f, 0.24f), circle);
                AddCircle(root.transform, "Head", new Vector2(0.40f, 0.56f), new Vector2(0.60f, 0.76f), new Color(0.18f, 0.18f, 0.20f), circle);
                AddLeg(root.transform, "L1", new Vector2(0.24f, 0.56f), 36f);
                AddLeg(root.transform, "L2", new Vector2(0.22f, 0.46f), 18f);
                AddLeg(root.transform, "L3", new Vector2(0.22f, 0.36f), -18f);
                AddLeg(root.transform, "L4", new Vector2(0.24f, 0.26f), -36f);
                AddLeg(root.transform, "R1", new Vector2(0.76f, 0.56f), -36f);
                AddLeg(root.transform, "R2", new Vector2(0.78f, 0.46f), -18f);
                AddLeg(root.transform, "R3", new Vector2(0.78f, 0.36f), 18f);
                AddLeg(root.transform, "R4", new Vector2(0.76f, 0.26f), 36f);
                break;

            case "Snake":
                AddCircle(root.transform, "S1", new Vector2(0.18f, 0.34f), new Vector2(0.42f, 0.58f), new Color(0.35f, 0.72f, 0.23f), circle);
                AddCircle(root.transform, "S2", new Vector2(0.36f, 0.42f), new Vector2(0.60f, 0.66f), new Color(0.30f, 0.66f, 0.20f), circle);
                AddCircle(root.transform, "S3", new Vector2(0.54f, 0.30f), new Vector2(0.82f, 0.58f), new Color(0.26f, 0.60f, 0.18f), circle);
                AddCircle(root.transform, "Eye", new Vector2(0.66f, 0.44f), new Vector2(0.74f, 0.52f), Color.white, circle);
                break;

            case "Monkey":
                AddCircle(root.transform, "Head", new Vector2(0.28f, 0.30f), new Vector2(0.72f, 0.74f), new Color(0.70f, 0.46f, 0.27f), circle);
                AddCircle(root.transform, "EarL", new Vector2(0.16f, 0.44f), new Vector2(0.34f, 0.62f), new Color(0.64f, 0.40f, 0.23f), circle);
                AddCircle(root.transform, "EarR", new Vector2(0.66f, 0.44f), new Vector2(0.84f, 0.62f), new Color(0.64f, 0.40f, 0.23f), circle);
                AddCircle(root.transform, "Muzzle", new Vector2(0.38f, 0.34f), new Vector2(0.62f, 0.52f), new Color(0.90f, 0.78f, 0.58f), circle);
                break;

            case "Alligator":
                AddRect(root.transform, "Body", new Vector2(0.20f, 0.34f), new Vector2(0.80f, 0.62f), new Color(0.17f, 0.60f, 0.38f));
                AddRect(root.transform, "Jaw", new Vector2(0.56f, 0.28f), new Vector2(0.84f, 0.46f), new Color(0.20f, 0.68f, 0.43f));
                AddCircle(root.transform, "Eye", new Vector2(0.30f, 0.52f), new Vector2(0.38f, 0.60f), Color.white, circle);
                AddRect(root.transform, "Tooth1", new Vector2(0.64f, 0.28f), new Vector2(0.67f, 0.38f), Color.white);
                AddRect(root.transform, "Tooth2", new Vector2(0.70f, 0.28f), new Vector2(0.73f, 0.38f), Color.white);
                break;

            case "Raft":
                AddRect(root.transform, "Plank1", new Vector2(0.20f, 0.34f), new Vector2(0.80f, 0.42f), new Color(0.73f, 0.49f, 0.24f));
                AddRect(root.transform, "Plank2", new Vector2(0.20f, 0.44f), new Vector2(0.80f, 0.52f), new Color(0.78f, 0.54f, 0.28f));
                AddRect(root.transform, "Plank3", new Vector2(0.20f, 0.54f), new Vector2(0.80f, 0.62f), new Color(0.73f, 0.49f, 0.24f));
                AddRect(root.transform, "RopeL", new Vector2(0.24f, 0.32f), new Vector2(0.28f, 0.64f), new Color(0.40f, 0.26f, 0.12f));
                AddRect(root.transform, "RopeR", new Vector2(0.72f, 0.32f), new Vector2(0.76f, 0.64f), new Color(0.40f, 0.26f, 0.12f));
                break;
        }
    }

    static void AddCircle(Transform parent, string name, Vector2 min, Vector2 max, Color color, Sprite circle)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = color;
        if (circle != null) img.sprite = circle;
    }

    static void AddRect(Transform parent, string name, Vector2 min, Vector2 max, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = color;
    }

    static void AddLeg(Transform parent, string name, Vector2 anchor, float angle)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(0.18f * 128f, 0.035f * 128f);
        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);
        go.AddComponent<Image>().color = new Color(0.16f, 0.16f, 0.18f, 1f);
    }

    /// <summary>Creates a TextMeshProUGUI. Call Anchor() afterwards to position it.</summary>
    static TextMeshProUGUI MakeTMP(
        GameObject parent, string name, string text, float size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>(); // positioning set by caller
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    /// <summary>Creates a Button with a TMP label child. Call CenterAt() afterwards.</summary>
    static Button MakeButton(GameObject parent, string name, string label)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = new Color(0.18f, 0.58f, 0.19f);
        var btn = go.AddComponent<Button>();

        var outline = new GameObject("Outline");
        outline.transform.SetParent(go.transform, false);
        var ort = outline.AddComponent<RectTransform>();
        ort.anchorMin = new Vector2(0.03f, 0.08f);
        ort.anchorMax = new Vector2(0.97f, 0.92f);
        ort.offsetMin = ort.offsetMax = Vector2.zero;
        outline.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.10f);
        outline.transform.SetAsFirstSibling();

        var lblGO = new GameObject("Text");
        lblGO.transform.SetParent(go.transform, false);
        var lrt = lblGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 38f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.characterSpacing = 2f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return btn;
    }

    /// <summary>Position a UI element using normalized anchor fractions of the parent.</summary>
    static void Anchor(GameObject go, Vector2 min, Vector2 max, float pad = 0)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = new Vector2( pad,  pad);
        rt.offsetMax = new Vector2(-pad, -pad);
    }

    /// <summary>Centre-anchored element at a specific position with a fixed size.</summary>
    static void CenterAt(GameObject go, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
    }

    /// <summary>Bottom-centre-anchored element sitting above the bottom edge.</summary>
    static void BottomCenterAt(GameObject go, float marginFromBottom, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, marginFromBottom);
        rt.sizeDelta        = size;
    }

    /// <summary>
    /// Connects a Button's OnClick event to a method by name using reflection.
    /// This avoids any compile-time dependency on the game script types.
    /// </summary>
    static void Wire(Button btn, Component target, string methodName)
    {
        var method = target.GetType().GetMethod(methodName);
        if (method == null)
        {
            Debug.LogWarning($"[JungleSceneBuilder] Method not found: {methodName}");
            return;
        }
        var action = System.Delegate.CreateDelegate(
            typeof(UnityEngine.Events.UnityAction), target, method)
            as UnityEngine.Events.UnityAction;
        UnityEventTools.AddPersistentListener(btn.onClick, action);
    }

    /// <summary>Creates nested folders if they don't already exist.</summary>
    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/');
        var cur   = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }
}
#endif
