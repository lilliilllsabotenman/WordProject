using UnityEngine;
using UnityEditor;

public class SentenceLayoutEditor : EditorWindow
{
    private SentenceLayoutData data;
    private int selectedIndex = -1;

    // ビュー状態
    private float zoom = 1f;
    private Vector2 panOffset = Vector2.zero;
    private bool isPanning;
    private Vector2 panStartMouse;
    private Vector2 panStartOffset;

    // カードドラッグ
    private bool isDragging;
    private Vector2 dragStart;
    private Vector2 originalPos;

    private readonly Vector2 origin = new Vector2(300f, 210f); // キャンバス内のデフォルト原点
    private const float CANVAS_HEIGHT = 420f;
    private const float PANEL_WIDTH   = 230f;

    [MenuItem("Window/Sentence Layout Editor")]
    public static void Open() => GetWindow<SentenceLayoutEditor>("Sentence Layout Editor");

    [UnityEditor.Callbacks.OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        var asset = EditorUtility.InstanceIDToObject(instanceID) as SentenceLayoutData;
        if (asset == null) return false;

        var window = GetWindow<SentenceLayoutEditor>("Sentence Layout Editor");
        window.data = asset;
        window.Repaint();
        return true; // Unity デフォルトの Inspector 表示をキャンセル
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (data == null)
        {
            EditorGUILayout.HelpBox("SentenceLayoutData アセットをアサインしてください", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        DrawCanvas();
        DrawPanel();
        EditorGUILayout.EndHorizontal();
    }

    // ── ツールバー ──────────────────────────────────────

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        data = (SentenceLayoutData)EditorGUILayout.ObjectField(
            data, typeof(SentenceLayoutData), false, GUILayout.Width(200));

        if (data != null)
        {
            GUILayout.Label("Sprite:", EditorStyles.toolbarButton, GUILayout.Width(44));
            EditorGUI.BeginChangeCheck();
            data.sprite = (Sprite)EditorGUILayout.ObjectField(
                data.sprite, typeof(Sprite), false, GUILayout.Width(150));
            if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(data); Repaint(); }
        }

        GUILayout.FlexibleSpace();

        GUILayout.Label($"Zoom: {zoom:F2}x", EditorStyles.toolbarButton, GUILayout.Width(75));

        if (GUILayout.Button("リセット", EditorStyles.toolbarButton, GUILayout.Width(55)))
        {
            zoom = 1f;
            panOffset = Vector2.zero;
            Repaint();
        }

        if (GUILayout.Button("＋ 追加", EditorStyles.toolbarButton, GUILayout.Width(60)))
            AddEntry();

        GUI.enabled = selectedIndex >= 0 && data != null && selectedIndex < data.entries.Count;
        if (GUILayout.Button("削除", EditorStyles.toolbarButton, GUILayout.Width(50)))
            RemoveSelected();
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    // ── キャンバス ─────────────────────────────────────

    private void DrawCanvas()
    {
        var canvasRect = GUILayoutUtility.GetRect(
            GUIContent.none, GUIStyle.none,
            GUILayout.ExpandWidth(true), GUILayout.Height(CANVAS_HEIGHT));

        EditorGUI.DrawRect(canvasRect, new Color(0.13f, 0.13f, 0.13f));

        var e = Event.current;

        HandleZoom(canvasRect, e);
        HandlePan(canvasRect, e);

        // スプライト中心 = origin + panOffset（キャンバス左上基点）
        var spriteCenter = canvasRect.position + origin + panOffset;

        DrawSprite(canvasRect, spriteCenter);

        if (data == null) return;

        HandleCanvasLeftClick(canvasRect, e);

        for (int i = 0; i < data.entries.Count; i++)
            DrawCard(spriteCenter, i, e);

        HandleCardDrag(e);

        // パン中はカーソルを手のひらに
        if (isPanning)
            EditorGUIUtility.AddCursorRect(canvasRect, MouseCursor.Pan);
    }

    // ズーム（スクロールホイール・マウス位置中心）
    private void HandleZoom(Rect canvasRect, Event e)
    {
        if (e.type != EventType.ScrollWheel || !canvasRect.Contains(e.mousePosition)) return;

        float newZoom = Mathf.Clamp(zoom * (e.delta.y > 0 ? 1f / 1.1f : 1.1f), 0.05f, 10f);
        float ratio   = newZoom / zoom;

        // マウス位置が変わらないよう panOffset を補正
        var mouseInCanvas = e.mousePosition - canvasRect.position;
        var oldCenter     = origin + panOffset;
        panOffset = mouseInCanvas - origin - (mouseInCanvas - oldCenter) * ratio;
        zoom      = newZoom;

        e.Use();
        Repaint();
    }

    // パン（右クリックドラッグ）
    private void HandlePan(Rect canvasRect, Event e)
    {
        if (e.type == EventType.MouseDown && e.button == 1 && canvasRect.Contains(e.mousePosition))
        {
            isPanning      = true;
            panStartMouse  = e.mousePosition;
            panStartOffset = panOffset;
            e.Use();
        }

        if (isPanning && e.type == EventType.MouseDrag && e.button == 1)
        {
            panOffset = panStartOffset + (e.mousePosition - panStartMouse);
            Repaint();
            e.Use();
        }

        if (e.type == EventType.MouseUp && e.button == 1)
            isPanning = false;
    }

    // ── スプライト描画 ─────────────────────────────────

    private void DrawSprite(Rect canvasRect, Vector2 center)
    {
        if (data?.sprite != null)
        {
            var tex  = data.sprite.texture;
            var rect = data.sprite.rect;
            float w  = rect.width  * zoom;
            float h  = rect.height * zoom;
            var drawRect = new Rect(center.x - w * 0.5f, center.y - h * 0.5f, w, h);
            var uv = new Rect(
                rect.x / tex.width,  rect.y / tex.height,
                rect.width / tex.width, rect.height / tex.height);

            GUI.BeginClip(canvasRect);
            var local = new Rect(drawRect.x - canvasRect.x, drawRect.y - canvasRect.y, w, h);
            GUI.DrawTextureWithTexCoords(local, tex, uv);
            GUI.EndClip();
        }

        // 原点マーカー
        float cross = Mathf.Max(6f, 6f * zoom);
        EditorGUI.DrawRect(new Rect(center.x - cross, center.y - 1, cross * 2, 2), Color.cyan);
        EditorGUI.DrawRect(new Rect(center.x - 1, center.y - cross, 2, cross * 2), Color.cyan);

        if (data?.sprite == null)
            GUI.Label(new Rect(center.x + 10, center.y - 10, 180, 20),
                "← Sprite を割り当ててください",
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.cyan } });
    }

    // ── カード描画 ─────────────────────────────────────

    private void DrawCard(Vector2 spriteCenter, int i, Event e)
    {
        var entry    = data.entries[i];
        var center   = spriteCenter + entry.position * zoom * new Vector2(1f, -1f);
        var cardSize = entry.size * zoom;
        var cardRect = new Rect(
            center.x - cardSize.x * 0.5f,
            center.y - cardSize.y * 0.5f,
            cardSize.x, cardSize.y);

        bool selected = i == selectedIndex;
        var bg = entry.isSlot ? new Color(0.25f, 0.38f, 0.58f) : new Color(0.28f, 0.28f, 0.28f);
        if (selected) bg += new Color(0.15f, 0.15f, 0.15f);

        EditorGUI.DrawRect(cardRect, bg);
        DrawBorder(cardRect, selected ? Color.yellow : new Color(0.5f, 0.5f, 0.5f));

        int fontSize = Mathf.Max(8, Mathf.RoundToInt(11 * zoom));
        GUI.Label(cardRect,
            entry.isSlot ? $"[{entry.displayText}]" : entry.displayText,
            new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white },
                fontSize  = fontSize
            });

        if (e.type == EventType.MouseDown && e.button == 0 && cardRect.Contains(e.mousePosition))
        {
            selectedIndex = i;
            isDragging    = true;
            dragStart     = e.mousePosition;
            originalPos   = entry.position;
            e.Use();
        }
    }

    private void HandleCardDrag(Event e)
    {
        if (isDragging && e.type == EventType.MouseDrag && e.button == 0 && selectedIndex >= 0)
        {
            // マウスの移動量をズームで割ってワールド座標に変換
            var delta = e.mousePosition - dragStart;
            data.entries[selectedIndex].position = originalPos + new Vector2(delta.x, -delta.y) / zoom;
            EditorUtility.SetDirty(data);
            Repaint();
            e.Use();
        }

        if (e.type == EventType.MouseUp && e.button == 0)
            isDragging = false;
    }

    private void HandleCanvasLeftClick(Rect canvasRect, Event e)
    {
        if (e.type == EventType.MouseDown && e.button == 0 && canvasRect.Contains(e.mousePosition))
            EditorApplication.delayCall += () => { if (!isDragging) { selectedIndex = -1; Repaint(); } };
    }

    // ── インスペクターパネル ────────────────────────────

    private void DrawPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(PANEL_WIDTH));
        GUILayout.Space(4);

        if (selectedIndex < 0 || data == null || selectedIndex >= data.entries.Count)
        {
            EditorGUILayout.LabelField("カードを選択してください", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
            return;
        }

        var entry = data.entries[selectedIndex];
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField($"Card [{selectedIndex}]", EditorStyles.boldLabel);
        GUILayout.Space(4);

        entry.displayText = EditorGUILayout.TextField("テキスト", entry.displayText);
        entry.isSlot      = EditorGUILayout.Toggle("スロット", entry.isSlot);
        GUILayout.Space(4);
        entry.position    = EditorGUILayout.Vector2Field("位置（Sprite相対）", entry.position);
        entry.size        = EditorGUILayout.Vector2Field("サイズ", entry.size);
        GUILayout.Space(4);
        entry.spriteData  = (PlayerTextSpriteData)EditorGUILayout.ObjectField(
            "スプライトデータ", entry.spriteData, typeof(PlayerTextSpriteData), false);

        if (entry.spriteData?.sprite != null)
        {
            GUILayout.Space(4);
            var preview = AssetPreview.GetAssetPreview(entry.spriteData.sprite);
            if (preview != null)
                GUILayout.Label(preview, GUILayout.Width(80), GUILayout.Height(80));
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(data);
            Repaint();
        }

        EditorGUILayout.EndVertical();
    }

    // ── ユーティリティ ─────────────────────────────────

    private void AddEntry()
    {
        if (data == null) return;
        Undo.RecordObject(data, "Add Card");
        data.entries.Add(new CardLayoutEntry { displayText = "新規", size = new Vector2(100f, 50f) });
        EditorUtility.SetDirty(data);
        selectedIndex = data.entries.Count - 1;
        Repaint();
    }

    private void RemoveSelected()
    {
        Undo.RecordObject(data, "Remove Card");
        data.entries.RemoveAt(selectedIndex);
        EditorUtility.SetDirty(data);
        selectedIndex = -1;
        Repaint();
    }

    private static void DrawBorder(Rect r, Color c)
    {
        EditorGUI.DrawRect(new Rect(r.x,        r.y,        r.width, 1),       c);
        EditorGUI.DrawRect(new Rect(r.x,        r.yMax - 1, r.width, 1),       c);
        EditorGUI.DrawRect(new Rect(r.x,        r.y,        1,       r.height), c);
        EditorGUI.DrawRect(new Rect(r.xMax - 1, r.y,        1,       r.height), c);
    }
}