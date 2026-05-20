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
        var boardPanel   = MakePanel(canvasGO, "GameBoardPanel",   new Color(0.04f, 0.20f, 0.04f));
        var winPanel     = MakePanel(canvasGO, "WinPanel",         new Color(0.85f, 0.65f, 0.05f));

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        //   WELCOME PANEL
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        var titleTMP = MakeTMP(welcomePanel, "TitleText",
            "Chase's Jungle Adventure!", 72f, Color.white);
        Anchor(titleTMP.gameObject, new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.88f));
        titleTMP.fontStyle = FontStyles.Bold;

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

        var playerCountTMP = MakeTMP(setupPanel, "PlayerCountText", "Players: 1", 48f, Color.yellow);
        Anchor(playerCountTMP.gameObject, new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.72f));

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
        var playerTurnTMP = MakeTMP(boardPanel, "PlayerTurnText", "Player 1's turn!", 50f, Color.white);
        Anchor(playerTurnTMP.gameObject, new Vector2(0.03f, 0.90f), new Vector2(0.97f, 0.99f));

        var cardDrawnTMP = MakeTMP(boardPanel, "CardDrawnText", "", 40f, Color.yellow);
        Anchor(cardDrawnTMP.gameObject, new Vector2(0.03f, 0.82f), new Vector2(0.97f, 0.90f));

        // Big counting number — center overlay, hidden except during movement
        var countingTMP = MakeTMP(boardPanel, "CountingText", "", 130f, Color.white);
        Anchor(countingTMP.gameObject, new Vector2(0.35f, 0.38f), new Vector2(0.65f, 0.62f));
        countingTMP.fontStyle = FontStyles.Bold;
        countingTMP.gameObject.SetActive(false);

        // Special-space message — center overlay, hidden until triggered
        var specialMsgTMP = MakeTMP(boardPanel, "SpecialMessageText", "", 46f, new Color(1f, 0.95f, 0.2f));
        Anchor(specialMsgTMP.gameObject, new Vector2(0.05f, 0.40f), new Vector2(0.95f, 0.60f));
        specialMsgTMP.fontStyle = FontStyles.Bold;
        specialMsgTMP.gameObject.SetActive(false);

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

        var boardSpacesGO = new GameObject("BoardSpaces");
        boardSpacesGO.transform.SetParent(boardPanel.transform, false);
        var bsRT = boardSpacesGO.AddComponent<RectTransform>();
        // Centre the board container slightly below screen-centre (gives room for top HUD)
        bsRT.anchorMin = bsRT.anchorMax = new Vector2(0.5f, 0.48f);
        bsRT.sizeDelta        = Vector2.zero;
        bsRT.anchoredPosition = Vector2.zero;

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

        // Brown connectors between consecutive spaces (drawn first, so spaces appear on top)
        for (int i = 0; i < 59; i++)
        {
            var cGO = new GameObject($"Conn_{i:D2}");
            cGO.transform.SetParent(boardSpacesGO.transform, false);
            var crt = cGO.AddComponent<RectTransform>();
            var dir = spacePts[i + 1] - spacePts[i];
            crt.anchoredPosition = (spacePts[i] + spacePts[i + 1]) * 0.5f;
            crt.sizeDelta        = new Vector2(dir.magnitude - 52f, 12f);
            crt.localRotation    = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            cGO.AddComponent<Image>().color = new Color(0.55f, 0.35f, 0.08f, 0.75f);
        }

        // Special-space labels
        var labels = new string[60];
        labels[0]  = "START";
        labels[7]  = "SPIDER";
        labels[15] = "SNAKE";
        labels[22] = "MONKEY";
        labels[35] = "GATOR";
        labels[40] = "RAFT";
        labels[59] = "WIN!";

        var spaceRTs = new RectTransform[60];
        for (int i = 0; i < 60; i++)
        {
            var spGO = new GameObject($"Space_{i:D2}");
            spGO.transform.SetParent(boardSpacesGO.transform, false);

            var rt = spGO.AddComponent<RectTransform>();
            rt.sizeDelta        = new Vector2(68, 68);
            rt.anchoredPosition = spacePts[i];

            // Color — special spaces get their own tint
            Color col = SpaceColors[i % SpaceColors.Length];
            if      (i ==  0)                                  col = new Color(0.20f, 0.70f, 0.20f); // start — bright green
            else if (i == 59)                                  col = new Color(0.20f, 0.55f, 1.00f); // waterfall — blue
            else if (i == 40)                                  col = new Color(0.10f, 0.80f, 0.90f); // raft — teal
            else if (i == 7 || i == 15 || i == 22 || i == 35) // obstacle — darkened
                col = new Color(col.r * 0.60f, col.g * 0.60f, col.b * 0.60f, 1f);

            spGO.AddComponent<Image>().color = col;

            // Small space-number label (bottom half of the square)
            var numGO  = new GameObject("Num");
            numGO.transform.SetParent(spGO.transform, false);
            var nrt    = numGO.AddComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0f, 0f);
            nrt.anchorMax = new Vector2(1f, 0.45f);
            nrt.offsetMin = nrt.offsetMax = Vector2.zero;
            var numTMP = numGO.AddComponent<TextMeshProUGUI>();
            numTMP.text      = i.ToString();
            numTMP.fontSize  = 13f;
            numTMP.color     = new Color(1f, 1f, 1f, 0.75f);
            numTMP.alignment = TextAlignmentOptions.BottomRight;

            // Named label (top half, special spaces only)
            if (labels[i] != null)
            {
                var lblGO = new GameObject("Label");
                lblGO.transform.SetParent(spGO.transform, false);
                var lrt   = lblGO.AddComponent<RectTransform>();
                lrt.anchorMin = new Vector2(0f, 0.45f);
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;
                var lbl   = lblGO.AddComponent<TextMeshProUGUI>();
                lbl.text      = labels[i];
                lbl.fontSize  = 10f;
                lbl.fontStyle = FontStyles.Bold;
                lbl.alignment = TextAlignmentOptions.Center;
                lbl.color     = Color.white;
            }

            spaceRTs[i] = rt;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        //   WIN PANEL
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        var winTMP = MakeTMP(winPanel, "WinText",
            "You reached the Waterfall!\nYOU WIN!", 80f, Color.white);
        Anchor(winTMP.gameObject, new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.75f));
        winTMP.fontStyle = FontStyles.Bold;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        //   PLAYER TOKEN PREFAB
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        EnsureFolder("Assets/ChasesJungleAdventure");
        var tokenGO = new GameObject("PlayerToken");
        var trt     = tokenGO.AddComponent<RectTransform>();
        trt.sizeDelta = new Vector2(54, 54);
        tokenGO.AddComponent<Image>().color = Color.white;

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
        soUI.FindProperty("countingText").objectReferenceValue       = countingTMP;
        soUI.FindProperty("specialMessageText").objectReferenceValue = specialMsgTMP;
        soUI.FindProperty("winText").objectReferenceValue            = winTMP;
        soUI.FindProperty("playerCountText").objectReferenceValue    = playerCountTMP;
        soUI.FindProperty("inputHandler").objectReferenceValue       = bih;

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
        go.AddComponent<Image>().color = new Color(0.15f, 0.55f, 0.15f);
        var btn = go.AddComponent<Button>();

        var lblGO = new GameObject("Text");
        lblGO.transform.SetParent(go.transform, false);
        var lrt = lblGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 38f;
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
