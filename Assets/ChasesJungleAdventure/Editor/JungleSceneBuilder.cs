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
    // ── Space colors: vivid Candy Land palette ─────────────────────────────
    static readonly Color[] SpaceColors =
    {
        new Color(1.00f, 0.15f, 0.15f),  // vivid red
        new Color(0.08f, 0.44f, 1.00f),  // bright blue
        new Color(0.04f, 0.90f, 0.20f),  // vivid green
        new Color(1.00f, 0.92f, 0.00f),  // bright yellow
        new Color(0.72f, 0.08f, 1.00f),  // vivid purple
        new Color(1.00f, 0.50f, 0.02f),  // bright orange
    };

    static readonly Color PathShadowColor     = new Color(0.30f, 0.18f, 0.04f, 0.42f);
    static readonly Color PathBaseColor       = new Color(0.97f, 0.93f, 0.76f, 1.00f);
    static readonly Color PathHighlightColor  = new Color(1.00f, 0.99f, 0.92f, 0.62f);
    static readonly Color PanelJungleTint     = new Color(0.04f, 0.40f, 0.10f);
    static readonly Color LeafDarkColor       = new Color(0.05f, 0.50f, 0.12f, 0.96f);
    static readonly Color LeafLightColor      = new Color(0.14f, 0.72f, 0.18f, 0.90f);
    static readonly Color MistColor           = new Color(1f, 1f, 1f, 0.13f);
    static readonly Color WaterfallColor      = new Color(0.28f, 0.82f, 1.00f, 0.70f);

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
            // Sine arc gives each row a gentle natural curve (peaks at mid-row)
            float wave = Mathf.Sin(t * Mathf.PI) * 26f;
            spacePts[i] = new Vector2(x, yStart + row * yStep + wave);
        }

        // Zone-themed area decorations — rendered behind the path and spaces
        AddZoneDecorations(boardSpacesGO, spacePts, circleSprite);

        // Board trail — three-layer ribbon: shadow + cream base + highlight
        for (int i = 0; i < 59; i++)
        {
            var dir = spacePts[i + 1] - spacePts[i];
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Vector2 center = (spacePts[i] + spacePts[i + 1]) * 0.5f;
            float connLen = Mathf.Max(0f, dir.magnitude - 60f);
            // Shadow (slightly wider and darker)
            MakeTrailSegment(boardSpacesGO, $"TrailShadow_{i:D2}", center, connLen + 6f, 80f, angle,
                PathShadowColor);
            // Main cream ribbon
            MakeTrailSegment(boardSpacesGO, $"Trail_{i:D2}", center, connLen, 68f, angle,
                PathBaseColor);
            // Centre highlight for depth
            MakeTrailSegment(boardSpacesGO, $"TrailHL_{i:D2}", center, connLen, 22f, angle,
                PathHighlightColor);
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
        trt.sizeDelta = new Vector2(52f, 76f);   // taller than wide — person shape

        // ── Shadow at bottom ──────────────────────────────────────────────────
        var tokenShadow = new GameObject("Shadow");
        tokenShadow.transform.SetParent(tokenGO.transform, false);
        var shadowRT = tokenShadow.AddComponent<RectTransform>();
        shadowRT.anchorMin = new Vector2(0.12f, 0f);
        shadowRT.anchorMax = new Vector2(0.88f, 0.09f);
        shadowRT.offsetMin = shadowRT.offsetMax = Vector2.zero;
        var shadowImg = tokenShadow.AddComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.28f);
        if (circleSprite != null) shadowImg.sprite = circleSprite;

        // ── Left leg ──────────────────────────────────────────────────────────
        var legL = new GameObject("LegL");
        legL.transform.SetParent(tokenGO.transform, false);
        var llRT = legL.AddComponent<RectTransform>();
        llRT.anchorMin = new Vector2(0.18f, 0.06f);
        llRT.anchorMax = new Vector2(0.44f, 0.38f);
        llRT.offsetMin = llRT.offsetMax = Vector2.zero;
        legL.AddComponent<Image>().color = new Color(0.22f, 0.22f, 0.68f);  // blue trousers

        // ── Right leg ─────────────────────────────────────────────────────────
        var legR = new GameObject("LegR");
        legR.transform.SetParent(tokenGO.transform, false);
        var lrRT = legR.AddComponent<RectTransform>();
        lrRT.anchorMin = new Vector2(0.56f, 0.06f);
        lrRT.anchorMax = new Vector2(0.82f, 0.38f);
        lrRT.offsetMin = lrRT.offsetMax = Vector2.zero;
        legR.AddComponent<Image>().color = new Color(0.22f, 0.22f, 0.68f);  // blue trousers

        // ── Left arm ──────────────────────────────────────────────────────────
        var armL = new GameObject("ArmL");
        armL.transform.SetParent(tokenGO.transform, false);
        var alRT = armL.AddComponent<RectTransform>();
        alRT.anchorMin = new Vector2(0.00f, 0.38f);
        alRT.anchorMax = new Vector2(0.22f, 0.64f);
        alRT.offsetMin = alRT.offsetMax = Vector2.zero;
        armL.AddComponent<Image>().color = new Color(0.96f, 0.80f, 0.62f);  // skin

        // ── Right arm ─────────────────────────────────────────────────────────
        var armR = new GameObject("ArmR");
        armR.transform.SetParent(tokenGO.transform, false);
        var arRT = armR.AddComponent<RectTransform>();
        arRT.anchorMin = new Vector2(0.78f, 0.38f);
        arRT.anchorMax = new Vector2(1.00f, 0.64f);
        arRT.offsetMin = arRT.offsetMax = Vector2.zero;
        armR.AddComponent<Image>().color = new Color(0.96f, 0.80f, 0.62f);  // skin

        // ── Shirt / body — PlayerManager sets this child's color at runtime ──
        var tokenShirt = new GameObject("Shirt");
        tokenShirt.transform.SetParent(tokenGO.transform, false);
        var shirtRT = tokenShirt.AddComponent<RectTransform>();
        shirtRT.anchorMin = new Vector2(0.18f, 0.36f);
        shirtRT.anchorMax = new Vector2(0.82f, 0.68f);
        shirtRT.offsetMin = shirtRT.offsetMax = Vector2.zero;
        var shirtImg = tokenShirt.AddComponent<Image>();
        shirtImg.color = Color.red;  // default; overwritten at runtime per player
        // Subtle white outline for readability against any board colour
        var shirtOutline = new GameObject("Outline");
        shirtOutline.transform.SetParent(tokenShirt.transform, false);
        var soRT = shirtOutline.AddComponent<RectTransform>();
        soRT.anchorMin = new Vector2(-0.10f, -0.10f);
        soRT.anchorMax = new Vector2(1.10f, 1.10f);
        soRT.offsetMin = soRT.offsetMax = Vector2.zero;
        shirtOutline.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.35f);
        shirtOutline.transform.SetAsFirstSibling();

        // ── Head ──────────────────────────────────────────────────────────────
        var tokenHead = new GameObject("Head");
        tokenHead.transform.SetParent(tokenGO.transform, false);
        var headRT = tokenHead.AddComponent<RectTransform>();
        headRT.anchorMin = new Vector2(0.26f, 0.66f);
        headRT.anchorMax = new Vector2(0.74f, 1.00f);
        headRT.offsetMin = headRT.offsetMax = Vector2.zero;
        var headImg = tokenHead.AddComponent<Image>();
        headImg.color = new Color(0.98f, 0.82f, 0.62f);  // skin
        if (circleSprite != null) headImg.sprite = circleSprite;

        // Hair band across top of head
        var hair = new GameObject("Hair");
        hair.transform.SetParent(tokenHead.transform, false);
        var hairRT = hair.AddComponent<RectTransform>();
        hairRT.anchorMin = new Vector2(0.08f, 0.70f);
        hairRT.anchorMax = new Vector2(0.92f, 1.00f);
        hairRT.offsetMin = hairRT.offsetMax = Vector2.zero;
        hair.AddComponent<Image>().color = new Color(0.28f, 0.16f, 0.05f);  // brown hair

        // Left eye
        var eyeL = new GameObject("EyeL");
        eyeL.transform.SetParent(tokenHead.transform, false);
        var elRT = eyeL.AddComponent<RectTransform>();
        elRT.anchorMin = new Vector2(0.20f, 0.40f);
        elRT.anchorMax = new Vector2(0.40f, 0.62f);
        elRT.offsetMin = elRT.offsetMax = Vector2.zero;
        var elImg = eyeL.AddComponent<Image>();
        elImg.color = new Color(0.10f, 0.08f, 0.04f);
        if (circleSprite != null) elImg.sprite = circleSprite;

        // Right eye
        var eyeR = new GameObject("EyeR");
        eyeR.transform.SetParent(tokenHead.transform, false);
        var erRT = eyeR.AddComponent<RectTransform>();
        erRT.anchorMin = new Vector2(0.60f, 0.40f);
        erRT.anchorMax = new Vector2(0.80f, 0.62f);
        erRT.offsetMin = erRT.offsetMax = Vector2.zero;
        var erImg = eyeR.AddComponent<Image>();
        erImg.color = new Color(0.10f, 0.08f, 0.04f);
        if (circleSprite != null) erImg.sprite = circleSprite;

        // Smile
        var smile = new GameObject("Smile");
        smile.transform.SetParent(tokenHead.transform, false);
        var smRT = smile.AddComponent<RectTransform>();
        smRT.anchorMin = new Vector2(0.26f, 0.16f);
        smRT.anchorMax = new Vector2(0.74f, 0.34f);
        smRT.offsetMin = smRT.offsetMax = Vector2.zero;
        smile.AddComponent<Image>().color = new Color(0.78f, 0.30f, 0.18f);

        // Player number — small text on shirt
        var tokenMark = new GameObject("Mark");
        tokenMark.transform.SetParent(tokenGO.transform, false);
        var tokenMarkRT = tokenMark.AddComponent<RectTransform>();
        tokenMarkRT.anchorMin = new Vector2(0.22f, 0.36f);
        tokenMarkRT.anchorMax = new Vector2(0.78f, 0.66f);
        tokenMarkRT.offsetMin = tokenMarkRT.offsetMax = Vector2.zero;
        var tokenMarkTMP = tokenMark.AddComponent<TextMeshProUGUI>();
        tokenMarkTMP.text = "1";
        tokenMarkTMP.fontSize = 20f;
        tokenMarkTMP.fontStyle = FontStyles.Bold;
        tokenMarkTMP.color = new Color(1f, 1f, 1f, 0.90f);
        tokenMarkTMP.alignment = TextAlignmentOptions.Center;

        string       pfPath      = "Assets/ChasesJungleAdventure/PlayerToken.prefab";
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
        // Vivid sky — bright tropical blue
        MakeStretch(boardPanel, "SkyBand",   new Vector2(0f, 0.48f), new Vector2(1f, 1f), new Color(0.16f, 0.60f, 1.00f));
        MakeStretch(boardPanel, "SkyGlow",   new Vector2(0f, 0.70f), new Vector2(1f, 1f), new Color(0.55f, 0.88f, 1.00f, 0.50f));

        // Bright jungle floor
        MakeStretch(boardPanel, "GroundBand", new Vector2(0f, 0f), new Vector2(1f, 0.52f), new Color(0.08f, 0.60f, 0.10f));
        MakeStretch(boardPanel, "GroundMid",  new Vector2(0f, 0.28f), new Vector2(1f, 0.52f), new Color(0.06f, 0.50f, 0.08f, 0.75f));

        // Canopy mid-band
        MakeStretch(boardPanel, "CanopyBack", new Vector2(0f, 0.44f), new Vector2(1f, 0.74f), new Color(0.06f, 0.56f, 0.12f, 0.88f));
        MakeStretch(boardPanel, "MistBand",   new Vector2(0f, 0.41f), new Vector2(1f, 0.57f), MistColor);

        // Sun — bright yellow circle in upper-right sky
        MakeRibbon(boardPanel, "SunBody",  new Vector2(0.83f, 0.91f), new Vector2(108f, 108f), 0f, new Color(1.00f, 0.94f, 0.08f, 0.96f));
        MakeRibbon(boardPanel, "SunHalo",  new Vector2(0.83f, 0.91f), new Vector2(160f, 160f), 0f, new Color(1.00f, 0.92f, 0.30f, 0.22f));
        MakeRibbon(boardPanel, "SunRay1",  new Vector2(0.83f, 0.91f), new Vector2(230f, 14f),  15f, new Color(1.00f, 0.96f, 0.30f, 0.25f));
        MakeRibbon(boardPanel, "SunRay2",  new Vector2(0.83f, 0.91f), new Vector2(230f, 14f),  60f, new Color(1.00f, 0.96f, 0.30f, 0.25f));
        MakeRibbon(boardPanel, "SunRay3",  new Vector2(0.83f, 0.91f), new Vector2(230f, 14f), 105f, new Color(1.00f, 0.96f, 0.30f, 0.25f));
        MakeRibbon(boardPanel, "SunRay4",  new Vector2(0.83f, 0.91f), new Vector2(230f, 14f), 150f, new Color(1.00f, 0.96f, 0.30f, 0.25f));

        // Dense leaf clusters around all four corners + sides
        MakeLeafCluster(boardPanel, "LeafTL1",  new Vector2(0.03f, 0.92f), 320f, LeafDarkColor,  -15f);
        MakeLeafCluster(boardPanel, "LeafTL2",  new Vector2(0.11f, 0.84f), 210f, LeafLightColor,  12f);
        MakeLeafCluster(boardPanel, "LeafTR1",  new Vector2(0.96f, 0.90f), 300f, LeafLightColor,  24f);
        MakeLeafCluster(boardPanel, "LeafTR2",  new Vector2(0.86f, 0.82f), 190f, LeafDarkColor,  -12f);
        MakeLeafCluster(boardPanel, "LeafBL1",  new Vector2(0.03f, 0.08f), 310f, LeafLightColor,  22f);
        MakeLeafCluster(boardPanel, "LeafBL2",  new Vector2(0.13f, 0.17f), 190f, LeafDarkColor,  -26f);
        MakeLeafCluster(boardPanel, "LeafBR1",  new Vector2(0.97f, 0.10f), 310f, LeafDarkColor,  -22f);
        MakeLeafCluster(boardPanel, "LeafBR2",  new Vector2(0.86f, 0.19f), 195f, LeafLightColor,  18f);
        MakeLeafCluster(boardPanel, "LeafML",   new Vector2(0.01f, 0.50f), 210f, LeafDarkColor,    5f);
        MakeLeafCluster(boardPanel, "LeafMR",   new Vector2(0.99f, 0.52f), 220f, LeafLightColor,  -5f);

        // River + waterfall (right side)
        MakeRibbon(boardPanel, "RiverRibbon1",  new Vector2(0.83f, 0.50f), new Vector2(530f, 94f),  -24f, new Color(0.08f, 0.50f, 1.00f, 0.52f));
        MakeRibbon(boardPanel, "RiverRibbon2",  new Vector2(0.88f, 0.54f), new Vector2(310f, 62f),  -16f, new Color(0.38f, 0.78f, 1.00f, 0.32f));
        MakeRibbon(boardPanel, "WaterfallGlow", new Vector2(0.95f, 0.72f), new Vector2(150f, 370f),   2f, WaterfallColor);
        MakeRibbon(boardPanel, "WaterfallShine",new Vector2(0.95f, 0.72f), new Vector2( 52f, 370f),   0f, new Color(0.85f, 0.97f, 1.00f, 0.40f));
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
        rt.sizeDelta = new Vector2(92f, 92f);
        var img = go.AddComponent<Image>();
        img.color = PathBaseColor;
        if (circle != null) img.sprite = circle;
    }

    // ════════════════════════════════════════════════════════════════════════
    //   ZONE DECORATION METHODS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Draws themed environment art behind the board path for each special zone.
    /// All elements are added to a single "ZoneDecorations" child that is set as
    /// the first sibling so it renders below everything else.
    /// </summary>
    static void AddZoneDecorations(GameObject boardSpaces, Vector2[] pts, Sprite circle)
    {
        var z = new GameObject("ZoneDecorations");
        z.transform.SetParent(boardSpaces.transform, false);
        var zRT = z.AddComponent<RectTransform>();
        zRT.anchorMin = zRT.anchorMax = new Vector2(0.5f, 0.5f);
        zRT.sizeDelta = Vector2.zero;
        zRT.anchoredPosition = Vector2.zero;
        z.transform.SetAsFirstSibling();  // render behind trail and spaces

        // ── Zone Background Glows ────────────────────────────────────────────

        // Spider Swamp (space 7) — dark purple glow + web pattern
        MakeZoneCircle(z, "SpiderSwampBg", pts[7], 240f, new Color(0.08f, 0.02f, 0.20f, 0.65f), circle);
        for (int i = 0; i < 8; i++)
            MakeZoneRect(z, $"WebStrand{i}", pts[7], new Vector2(210f, 3f), i * 22.5f,
                new Color(0.60f, 0.55f, 0.76f, 0.28f));
        MakeZoneCircle(z, "WebRing1", pts[7],  82f, new Color(0.60f, 0.55f, 0.76f, 0.22f), circle);
        MakeZoneCircle(z, "WebRing2", pts[7], 144f, new Color(0.60f, 0.55f, 0.76f, 0.15f), circle);
        MakeZoneCircle(z, "WebRing3", pts[7], 200f, new Color(0.60f, 0.55f, 0.76f, 0.10f), circle);

        // Snake swamp (space 15) — dark green + grass tufts
        MakeZoneCircle(z, "SnakeSwampBg", pts[15], 215f, new Color(0.05f, 0.26f, 0.05f, 0.58f), circle);
        for (int i = 0; i < 10; i++)
        {
            float gx = Mathf.Lerp(-95f, 95f, i / 9f);
            float gh = 28f + (i % 3) * 12f;
            MakeZoneRect(z, $"Grass{i}", pts[15] + new Vector2(gx, gh * 0.34f),
                new Vector2(5f, gh), -12f + i * 6f, new Color(0.10f, 0.80f, 0.12f, 0.76f));
        }

        // Monkey canopy (space 22) — dense green + hanging vines
        MakeZoneCircle(z, "CanopyBg", pts[22], 250f, new Color(0.04f, 0.38f, 0.06f, 0.60f), circle);
        for (int v = 0; v < 9; v++)
        {
            float vx   = Mathf.Lerp(-105f, 105f, v / 8f);
            float vLen = 58f + (v % 2 == 0 ? 26f : 0f);
            MakeZoneRect(z, $"VineStem{v}", pts[22] + new Vector2(vx, vLen * 0.30f),
                new Vector2(5f, vLen), (v % 3 - 1) * 8f, new Color(0.16f, 0.62f, 0.10f, 0.84f));
            MakeZoneCircle(z, $"VineLeaf{v}", pts[22] + new Vector2(vx, -vLen * 0.08f),
                18f, new Color(0.08f, 0.88f, 0.14f, 0.90f), circle);
        }

        // River zone (spaces 33–41) — blue pools + river band
        for (int s = 33; s <= 41 && s < pts.Length; s++)
            MakeZoneCircle(z, $"RiverPool{s}", pts[s], 92f,
                new Color(0.05f, 0.36f, 0.95f, 0.42f), circle);
        if (41 < pts.Length)
        {
            Vector2 mid  = (pts[33] + pts[41]) * 0.5f;
            float   dist = (pts[41] - pts[33]).magnitude;
            float   ang  = Mathf.Atan2(pts[41].y - pts[33].y,
                                       pts[41].x - pts[33].x) * Mathf.Rad2Deg;
            MakeZoneRect(z, "RiverBand",  mid, new Vector2(dist + 65f, 98f),  ang,
                new Color(0.05f, 0.36f, 0.95f, 0.48f));
            MakeZoneRect(z, "RiverShine", mid, new Vector2(dist + 65f, 34f), ang,
                new Color(0.46f, 0.82f, 1.00f, 0.28f));
        }

        // ── Animal Characters (drawn above their backgrounds) ─────────────────
        // Each animal is centered 65px above its space so the head clears the space dot.
        DrawSpider   (z, pts[7]  + new Vector2(  0f, 62f), circle);
        DrawSnake    (z, pts[15] + new Vector2(  8f, 62f), circle);
        DrawMonkey   (z, pts[22] + new Vector2(  0f, 58f), circle);
        DrawAlligator(z, pts[35] + new Vector2(  0f, 58f), circle);
    }

    // ── Spider ───────────────────────────────────────────────────────────────
    static void DrawSpider(GameObject p, Vector2 c, Sprite o)
    {
        Color purpleDk = new Color(0.12f, 0.03f, 0.26f);
        Color purpleMd = new Color(0.20f, 0.06f, 0.38f);
        Color orange   = new Color(1.00f, 0.54f, 0.04f);
        Color crimson  = new Color(0.90f, 0.06f, 0.06f);
        Color ivory    = new Color(0.90f, 0.88f, 0.82f);

        // Shadow
        MakeZoneCircle(p, "SpiderShadow", c + new Vector2(2f, -62f), 64f, new Color(0,0,0,0.18f), o);

        // Legs — upper + lower segments, 4 on each side
        float[] legRootY = {  8f, -2f, -12f, -22f };
        float[] angL     = { 52f, 72f, 102f, 128f };
        float[] angR     = {-52f,-72f,-102f,-128f };
        for (int i = 0; i < 4; i++)
        {
            float   ry = legRootY[i];
            Vector2 eL = c + new Vector2(-46f - i * 4f, ry - 10f);
            Vector2 eR = c + new Vector2( 46f + i * 4f, ry - 10f);
            MakeZoneRect(p, $"LLUp{i}",   c + new Vector2(-15f, ry), new Vector2(34f, 5f), angL[i],        purpleDk);
            MakeZoneRect(p, $"LLDn{i}",   eL + new Vector2(-12f,-6f), new Vector2(30f, 4f), angL[i] + 38f, purpleMd);
            MakeZoneRect(p, $"LRUp{i}",   c + new Vector2( 15f, ry), new Vector2(34f, 5f), angR[i],        purpleDk);
            MakeZoneRect(p, $"LRDn{i}",   eR + new Vector2( 12f,-6f), new Vector2(30f, 4f), angR[i] - 38f, purpleMd);
        }

        // Abdomen (large lower body)
        MakeZoneCircle(p, "Abdomen",    c + new Vector2(0f,-26f), 56f, purpleDk, o);
        // Hourglass danger marking
        MakeZoneCircle(p, "HGlassTop",  c + new Vector2(0f,-18f), 14f, orange,   o);
        MakeZoneCircle(p, "HGlassBot",  c + new Vector2(0f,-36f), 10f, orange,   o);
        MakeZoneRect(p,   "HGlassLink", c + new Vector2(0f,-27f), new Vector2(5f, 18f), 0f, new Color(orange.r,orange.g,orange.b,0.55f));

        // Thorax
        MakeZoneCircle(p, "Thorax", c + new Vector2(0f,  6f), 32f, purpleDk, o);
        // Cephalothorax / head
        MakeZoneCircle(p, "Head",   c + new Vector2(0f, 22f), 28f, purpleMd, o);

        // Eight eyes (two rows of four)
        float[] ex = {-11f, -4f,  4f, 11f };
        float[] ey = { 28f, 34f, 34f, 28f };
        float[] er = {  7f,  6f,  6f,  7f };
        for (int e = 0; e < 4; e++)
            MakeZoneCircle(p, $"Eye{e}", c + new Vector2(ex[e], ey[e]), er[e], crimson, o);
        for (int e = 0; e < 4; e++)
            MakeZoneCircle(p, $"Eye{e+4}", c + new Vector2(ex[e], ey[e] + 6f), er[e] - 1f, crimson, o);

        // Fangs
        MakeZoneCircle(p, "FangBaseL", c + new Vector2(-7f, 14f), 8f, purpleMd, o);
        MakeZoneCircle(p, "FangBaseR", c + new Vector2( 7f, 14f), 8f, purpleMd, o);
        MakeZoneRect(p,  "FangL",      c + new Vector2(-8f,  5f), new Vector2(5f, 13f), -10f, ivory);
        MakeZoneRect(p,  "FangR",      c + new Vector2( 8f,  5f), new Vector2(5f, 13f),  10f, ivory);
        // Venom drops
        MakeZoneCircle(p, "VenomL", c + new Vector2(-8f, -1f), 5f, new Color(0.36f, 0.92f, 0.28f, 0.92f), o);
        MakeZoneCircle(p, "VenomR", c + new Vector2( 8f, -1f), 5f, new Color(0.36f, 0.92f, 0.28f, 0.92f), o);
    }

    // ── Snake ────────────────────────────────────────────────────────────────
    static void DrawSnake(GameObject p, Vector2 c, Sprite o)
    {
        Color snkGrn = new Color(0.10f, 0.72f, 0.14f);
        Color snkDrk = new Color(0.06f, 0.48f, 0.08f);
        Color belly  = new Color(0.84f, 0.92f, 0.56f);
        Color eyeYel = new Color(1.00f, 0.80f, 0.04f);
        Color tongue = new Color(0.92f, 0.08f, 0.10f);

        // Bottom coil
        MakeZoneRect(p, "CoilBot",      c + new Vector2(0f,-48f), new Vector2(94f, 36f), 0f, snkGrn);
        MakeZoneRect(p, "CoilBotBelly", c + new Vector2(0f,-48f), new Vector2(58f, 36f), 0f, belly);
        // Scale diamonds on bottom coil
        for (int d = 0; d < 7; d++)
        {
            float dx = Mathf.Lerp(-38f, 38f, d / 6f);
            MakeZoneRect(p, $"BScale{d}", c + new Vector2(dx, -42f + (d%2)*14f), new Vector2(12f,12f), 45f, snkDrk);
        }
        // Rattle tail (right end)
        MakeZoneRect(p, "Rattle1", c + new Vector2( 48f,-48f), new Vector2(18f,14f), 0f, new Color(0.64f,0.52f,0.20f));
        MakeZoneRect(p, "Rattle2", c + new Vector2( 60f,-48f), new Vector2(14f,11f), 0f, new Color(0.52f,0.42f,0.16f));
        MakeZoneRect(p, "Rattle3", c + new Vector2( 70f,-48f), new Vector2(10f, 9f), 0f, new Color(0.42f,0.34f,0.12f));

        // Middle coil
        MakeZoneRect(p, "CoilMid",      c + new Vector2(0f,-14f), new Vector2(82f, 32f), 0f, snkGrn);
        MakeZoneRect(p, "CoilMidBelly", c + new Vector2(0f,-14f), new Vector2(48f, 32f), 0f, belly);

        // Neck rising from left
        MakeZoneRect(p, "Neck",      c + new Vector2(-10f, 12f), new Vector2(26f, 38f), 14f, snkGrn);
        MakeZoneRect(p, "NeckBelly", c + new Vector2( -8f, 12f), new Vector2(14f, 38f), 14f, belly);

        // Cobra hood (flared head fan)
        MakeZoneCircle(p, "Hood",      c + new Vector2(-18f, 48f), 54f, snkGrn, o);
        MakeZoneCircle(p, "HoodInner", c + new Vector2(-18f, 48f), 40f, new Color(snkGrn.r, snkGrn.g, snkGrn.b, 0.55f), o);
        MakeZoneRect(p,  "HoodPatL",   c + new Vector2(-26f, 44f), new Vector2(9f, 26f),  19f, snkDrk);
        MakeZoneRect(p,  "HoodPatR",   c + new Vector2(-12f, 44f), new Vector2(9f, 26f), -19f, snkDrk);

        // Head
        MakeZoneCircle(p, "SnkHead", c + new Vector2(-20f, 56f), 34f, snkGrn, o);
        MakeZoneCircle(p, "SnkFace", c + new Vector2(-22f, 52f), 24f, belly,   o);

        // Eyes (golden with vertical slit pupils)
        MakeZoneCircle(p, "EyeL",  c + new Vector2(-29f, 60f), 9f, eyeYel, o);
        MakeZoneCircle(p, "EyeR",  c + new Vector2(-12f, 60f), 9f, eyeYel, o);
        MakeZoneRect(p,  "SlitL",  c + new Vector2(-29f, 60f), new Vector2(3f, 10f), 0f, new Color(0.04f,0.04f,0.04f));
        MakeZoneRect(p,  "SlitR",  c + new Vector2(-12f, 60f), new Vector2(3f, 10f), 0f, new Color(0.04f,0.04f,0.04f));

        // Forked tongue
        MakeZoneRect(p, "TongueStem", c + new Vector2(-32f, 44f), new Vector2(4f, 16f), -16f, tongue);
        MakeZoneRect(p, "ForkL",      c + new Vector2(-38f, 34f), new Vector2(3f, 10f), -34f, tongue);
        MakeZoneRect(p, "ForkR",      c + new Vector2(-28f, 34f), new Vector2(3f, 10f),   6f, tongue);
    }

    // ── Monkey ───────────────────────────────────────────────────────────────
    static void DrawMonkey(GameObject p, Vector2 c, Sprite o)
    {
        Color furDk  = new Color(0.36f, 0.20f, 0.04f);
        Color furLt  = new Color(0.50f, 0.30f, 0.08f);
        Color face   = new Color(0.84f, 0.62f, 0.38f);
        Color drk    = new Color(0.22f, 0.10f, 0.02f);
        Color banana = new Color(1.00f, 0.88f, 0.06f);

        // Curling tail (arc of decreasing circles)
        float[] tx = { 32f, 52f, 68f, 76f, 72f, 60f, 46f, 36f };
        float[] ty = {-50f,-52f,-42f,-26f,-10f,  2f,  8f,  5f };
        float[] tr = { 13f, 12f, 11f, 10f,  9f,  9f,  8f,  7f };
        for (int t = 0; t < 8; t++)
            MakeZoneCircle(p, $"Tail{t}", c + new Vector2(tx[t], ty[t]), tr[t], furDk, o);

        // Legs
        MakeZoneRect(p, "LegL",  c + new Vector2(-20f,-50f), new Vector2(22f, 40f),  8f, furDk);
        MakeZoneRect(p, "LegR",  c + new Vector2( 20f,-50f), new Vector2(22f, 40f), -8f, furDk);
        MakeZoneCircle(p, "FootL", c + new Vector2(-24f,-68f), 16f, furDk, o);
        MakeZoneCircle(p, "FootR", c + new Vector2( 24f,-68f), 16f, furDk, o);
        // Toes
        for (int t = 0; t < 3; t++)
        {
            MakeZoneCircle(p, $"ToeLL{t}", c + new Vector2(-30f + t * 6f, -76f), 5f, furLt, o);
            MakeZoneCircle(p, $"ToeRL{t}", c + new Vector2( -6f + t * 6f, -76f), 5f, furLt, o);
        }

        // Body + belly
        MakeZoneCircle(p, "Body",  c + new Vector2(0f,-16f), 58f, furDk, o);
        MakeZoneCircle(p, "Belly", c + new Vector2(0f,-18f), 36f, face,   o);

        // Arms
        MakeZoneRect(p, "ArmL",  c + new Vector2(-44f,-14f), new Vector2(18f, 50f), -22f, furDk);
        MakeZoneRect(p, "ArmR",  c + new Vector2( 44f,-14f), new Vector2(18f, 50f),  22f, furDk);
        MakeZoneCircle(p, "HandL", c + new Vector2(-56f,-36f), 14f, face, o);
        MakeZoneCircle(p, "HandR", c + new Vector2( 56f,-36f), 14f, face, o);

        // Banana in right hand
        MakeZoneRect(p, "BananaStem", c + new Vector2( 66f,-42f), new Vector2( 5f, 10f),  0f, drk);
        MakeZoneRect(p, "BananaBody", c + new Vector2( 68f,-54f), new Vector2( 9f, 28f), 18f, banana);
        MakeZoneRect(p, "BananaEnd",  c + new Vector2( 73f,-64f), new Vector2( 7f,  9f), 36f, new Color(0.80f,0.68f,0.04f));

        // Head
        MakeZoneCircle(p, "Head",     c + new Vector2(0f, 26f), 50f, furDk,  o);
        MakeZoneCircle(p, "EarL",     c + new Vector2(-38f, 24f), 18f, furDk,  o);
        MakeZoneCircle(p, "EarLInr",  c + new Vector2(-38f, 24f), 10f, face,   o);
        MakeZoneCircle(p, "EarR",     c + new Vector2( 38f, 24f), 18f, furDk,  o);
        MakeZoneCircle(p, "EarRInr",  c + new Vector2( 38f, 24f), 10f, face,   o);
        MakeZoneCircle(p, "FacePlate",c + new Vector2(0f, 20f), 34f, face,   o);

        // Eyes + pupils + shine
        MakeZoneCircle(p, "EyeL",   c + new Vector2(-12f, 28f), 10f, drk,      o);
        MakeZoneCircle(p, "EyeR",   c + new Vector2( 12f, 28f), 10f, drk,      o);
        MakeZoneCircle(p, "PupilL", c + new Vector2(-12f, 27f),  5f, Color.black, o);
        MakeZoneCircle(p, "PupilR", c + new Vector2( 12f, 27f),  5f, Color.black, o);
        MakeZoneCircle(p, "ShineL", c + new Vector2(-10f, 29f),  2f, Color.white, o);
        MakeZoneCircle(p, "ShineR", c + new Vector2( 14f, 29f),  2f, Color.white, o);

        // Muzzle + nostrils + smile
        MakeZoneCircle(p, "Muzzle",   c + new Vector2(0f, 12f), 20f, face, o);
        MakeZoneCircle(p, "NostrilL", c + new Vector2(-5f,14f),  4f, drk,  o);
        MakeZoneCircle(p, "NostrilR", c + new Vector2( 5f,14f),  4f, drk,  o);
        MakeZoneRect(p,  "Smile",     c + new Vector2(0f,  6f), new Vector2(18f,4f), 0f, drk);

        // Friendly brows + hair tuft
        MakeZoneRect(p, "BrowL",    c + new Vector2(-12f,34f), new Vector2(14f,4f), -8f, drk);
        MakeZoneRect(p, "BrowR",    c + new Vector2( 12f,34f), new Vector2(14f,4f),  8f, drk);
        MakeZoneCircle(p, "Tuft1",  c + new Vector2(-6f,50f), 12f, furDk, o);
        MakeZoneCircle(p, "Tuft2",  c + new Vector2( 0f,54f), 13f, furDk, o);
        MakeZoneCircle(p, "Tuft3",  c + new Vector2( 6f,50f), 12f, furDk, o);
    }

    // ── Alligator Warrior ────────────────────────────────────────────────────
    static void DrawAlligator(GameObject p, Vector2 c, Sprite o)
    {
        Color gGrn   = new Color(0.12f, 0.62f, 0.14f);
        Color gDrk   = new Color(0.07f, 0.42f, 0.09f);
        Color armor  = new Color(0.50f, 0.52f, 0.58f);
        Color arDk   = new Color(0.36f, 0.38f, 0.44f);
        Color wood   = new Color(0.52f, 0.30f, 0.10f);
        Color wdDk   = new Color(0.36f, 0.20f, 0.06f);

        // Shadow
        MakeZoneCircle(p, "GatorShadow", c + new Vector2(2f,-82f), 88f, new Color(0,0,0,0.20f), o);

        // Tail
        MakeZoneRect(p, "TailBase", c + new Vector2(  6f,-72f), new Vector2(18f,20f),  0f, gGrn);
        MakeZoneRect(p, "TailMid",  c + new Vector2( 20f,-80f), new Vector2(14f,18f), 26f, gGrn);
        MakeZoneRect(p, "TailTip",  c + new Vector2( 33f,-88f), new Vector2(10f,14f), 46f, gGrn);

        // Legs
        MakeZoneRect(p, "LegL",    c + new Vector2(-22f,-50f), new Vector2(24f,46f),  0f, gGrn);
        MakeZoneRect(p, "LegR",    c + new Vector2( 22f,-50f), new Vector2(24f,46f),  0f, gGrn);
        MakeZoneRect(p, "BootL",   c + new Vector2(-22f,-74f), new Vector2(28f,20f),  0f, armor);
        MakeZoneRect(p, "BootR",   c + new Vector2( 22f,-74f), new Vector2(28f,20f),  0f, armor);
        MakeZoneRect(p, "BandL",   c + new Vector2(-22f,-64f), new Vector2(30f, 6f),  0f, arDk);
        MakeZoneRect(p, "BandR",   c + new Vector2( 22f,-64f), new Vector2(30f, 6f),  0f, arDk);

        // Body (green) + chest armour
        MakeZoneCircle(p, "GatorBody",  c + new Vector2(0f,-22f), 68f, gGrn,  o);
        MakeZoneCircle(p, "ChestArmor", c + new Vector2(0f,-18f), 56f, armor,  o);
        MakeZoneCircle(p, "ChestInner", c + new Vector2(0f,-18f), 34f, arDk,   o);
        // Rivets on chest plate
        MakeZoneCircle(p, "RivT",  c + new Vector2(  0f, -6f), 5f, armor, o);
        MakeZoneCircle(p, "RivL",  c + new Vector2(-18f,-18f), 5f, armor, o);
        MakeZoneCircle(p, "RivR",  c + new Vector2( 18f,-18f), 5f, armor, o);
        MakeZoneCircle(p, "RivB",  c + new Vector2(  0f,-30f), 5f, armor, o);

        // Shoulder pads
        MakeZoneCircle(p, "ShoulderL", c + new Vector2(-40f,-8f), 26f, armor, o);
        MakeZoneCircle(p, "ShoulderR", c + new Vector2( 40f,-8f), 26f, armor, o);

        // ── Shield arm (left) ─────────────────────────────────────────────────
        MakeZoneRect(p, "ArmL",        c + new Vector2(-52f,-28f), new Vector2(20f,42f), -12f, gGrn);
        // Shield body (wood panels)
        MakeZoneRect(p, "ShldBody",    c + new Vector2(-92f,-26f), new Vector2(50f,62f),   0f, wood);
        MakeZoneRect(p, "ShldPanL",    c + new Vector2(-104f,-26f), new Vector2(22f,58f),  0f, wdDk);
        MakeZoneRect(p, "ShldPanR",    c + new Vector2( -80f,-26f), new Vector2(22f,58f),  0f, new Color(0.58f,0.35f,0.12f));
        // Shield metal border
        MakeZoneRect(p, "ShldBdTop",   c + new Vector2(-92f,  4f), new Vector2(50f, 7f),   0f, armor);
        MakeZoneRect(p, "ShldBdBot",   c + new Vector2(-92f,-56f), new Vector2(50f, 7f),   0f, armor);
        MakeZoneRect(p, "ShldBdMid",   c + new Vector2(-92f,-26f), new Vector2( 7f,62f),   0f, armor);
        // Shield boss (centre metal knob)
        MakeZoneCircle(p, "ShldBoss",  c + new Vector2(-92f,-26f), 16f, arDk,  o);
        MakeZoneCircle(p, "ShldShine", c + new Vector2(-92f,-26f),  8f, armor,  o);

        // ── Axe arm (right) ───────────────────────────────────────────────────
        MakeZoneRect(p, "ArmR",      c + new Vector2(52f,-28f), new Vector2(20f,42f),  12f, gGrn);
        // Axe handle
        MakeZoneRect(p, "AxeHndl",   c + new Vector2(82f,-26f), new Vector2(10f,66f),  22f, wood);
        MakeZoneRect(p, "AxeWrap1",  c + new Vector2(80f,-14f), new Vector2(14f, 6f),  22f, wdDk);
        MakeZoneRect(p, "AxeWrap2",  c + new Vector2(86f,-30f), new Vector2(14f, 6f),  22f, wdDk);
        // Axe blade (large crescent approximated with overlapping circles)
        MakeZoneCircle(p, "AxeBlade",  c + new Vector2(102f, -4f), 46f, armor, o);
        MakeZoneCircle(p, "AxeEdge",   c + new Vector2(108f,  2f), 30f, new Color(0.80f,0.82f,0.88f), o);
        MakeZoneCircle(p, "AxeShine",  c + new Vector2(113f,  7f), 14f, new Color(1f,1f,1f,0.60f), o);
        // Axe back spike
        MakeZoneRect(p, "AxeSpike",  c + new Vector2(92f, 16f), new Vector2(6f,20f), -28f, arDk);

        // ── Head ──────────────────────────────────────────────────────────────
        MakeZoneCircle(p, "GatorHead", c + new Vector2(0f, 34f), 60f, gGrn, o);
        // Dorsal ridges / spines
        MakeZoneRect(p, "Spine1", c + new Vector2(-8f,62f), new Vector2(8f,15f),  10f, gDrk);
        MakeZoneRect(p, "Spine2", c + new Vector2( 0f,66f), new Vector2(8f,17f),   0f, gDrk);
        MakeZoneRect(p, "Spine3", c + new Vector2( 8f,62f), new Vector2(8f,15f), -10f, gDrk);

        // Snout (upper + lower jaw)
        MakeZoneRect(p, "UpperJaw", c + new Vector2(0f, 18f), new Vector2(60f,22f),  0f, gGrn);
        MakeZoneRect(p, "LowerJaw", c + new Vector2(0f,  6f), new Vector2(54f,16f),  0f, gDrk);
        // Teeth
        for (int t = 0; t < 6; t++)
        {
            float tx = Mathf.Lerp(-20f, 20f, t / 5f);
            MakeZoneRect(p, $"Tooth{t}", c + new Vector2(tx, 10f), new Vector2(6f, 10f), 0f, Color.white);
        }
        // Nostril bumps
        MakeZoneCircle(p, "NostL", c + new Vector2(-8f, 22f), 7f, gDrk, o);
        MakeZoneCircle(p, "NostR", c + new Vector2( 8f, 22f), 7f, gDrk, o);

        // Eyes (white + dark pupil + angry brow)
        MakeZoneCircle(p, "EyeWL", c + new Vector2(-18f,44f), 14f, Color.white, o);
        MakeZoneCircle(p, "EyeWR", c + new Vector2( 18f,44f), 14f, Color.white, o);
        MakeZoneCircle(p, "PupilL",c + new Vector2(-16f,42f),  8f, new Color(0.06f,0.05f,0.04f), o);
        MakeZoneCircle(p, "PupilR",c + new Vector2( 20f,42f),  8f, new Color(0.06f,0.05f,0.04f), o);
        MakeZoneRect(p,  "BrowL",  c + new Vector2(-18f,52f), new Vector2(20f,5f),  22f, gDrk);
        MakeZoneRect(p,  "BrowR",  c + new Vector2( 18f,52f), new Vector2(20f,5f), -22f, gDrk);
    }

    static void MakeZoneCircle(GameObject parent, string name, Vector2 pos,
                                float size, Color color, Sprite circle)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(size, size);
        var img = go.AddComponent<Image>();
        img.color  = color;
        if (circle != null) img.sprite = circle;
    }

    static void MakeZoneRect(GameObject parent, string name, Vector2 pos,
                              Vector2 size, float angle, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin     = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        rt.localRotation    = Quaternion.Euler(0f, 0f, angle);
        go.AddComponent<Image>().color = color;
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
