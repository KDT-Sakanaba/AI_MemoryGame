using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>バッチモードでシーンとカードプレハブを生成する（YAML 手編集はしない）。
/// 実行: Unity.exe -batchmode -projectPath <path> -executeMethod MemoryGame.EditorTools.SceneSetup.Run -quit</summary>
public static class SceneSetup
{
    public static void Run()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.transform.position = new Vector3(4.2f, 7f, -4f);
        cam.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

        // Card prefab (Cube + CardView + Collider)
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Card";
        cube.AddComponent<CardView>();
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(cube, "Assets/Prefabs/Card.prefab");
        Object.DestroyImmediate(cube);

        // Board
        var boardGO = new GameObject("Board");
        var board = boardGO.AddComponent<BoardView>();
        var so = new SerializedObject(board);
        so.FindProperty("cardPrefab").objectReferenceValue = prefab;
        so.FindProperty("cellSize").floatValue = 1.2f;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Input
        var inputGO = new GameObject("Input");
        var input = inputGO.AddComponent<InputHandler>();

        // HUD
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        var hud = canvasGO.AddComponent<HudView>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var statsGO = new GameObject("StatsText");
        statsGO.transform.SetParent(canvasGO.transform, false);
        var stats = statsGO.AddComponent<Text>();
        stats.font = font;
        stats.fontSize = 24;
        stats.rectTransform.sizeDelta = new Vector2(600, 40);
        stats.rectTransform.anchoredPosition = new Vector2(0, -200);

        var clearGO = new GameObject("ClearText");
        clearGO.transform.SetParent(canvasGO.transform, false);
        var clear = clearGO.AddComponent<Text>();
        clear.font = font;
        clear.fontSize = 36;
        clear.rectTransform.sizeDelta = new Vector2(600, 60);
        clear.rectTransform.anchoredPosition = new Vector2(0, 100);

        var replayGO = new GameObject("ReplayButton");
        replayGO.transform.SetParent(canvasGO.transform, false);
        var btnImage = replayGO.AddComponent<Image>(); // Graphic -> RectTransform 自動付与
        var btn = replayGO.AddComponent<Button>();
        var btnRect = replayGO.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(200, 50);
        btnRect.anchoredPosition = new Vector2(0, -300);
        var btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(replayGO.transform, false);
        var btnText = btnTextGO.AddComponent<Text>();
        btnText.font = font;
        btnText.fontSize = 24;
        btnText.text = "Replay";

        // GameStarter wiring
        var starterGO = new GameObject("GameStarter");
        var starter = starterGO.AddComponent<GameStarter>();
        var so2 = new SerializedObject(starter);
        so2.FindProperty("board").objectReferenceValue = board;
        so2.FindProperty("input").objectReferenceValue = input;
        so2.FindProperty("hud").objectReferenceValue = hud;
        so2.ApplyModifiedPropertiesWithoutUndo();

        var hudSo = new SerializedObject(hud);
        hudSo.FindProperty("statsText").objectReferenceValue = stats;
        hudSo.FindProperty("clearText").objectReferenceValue = clear;
        hudSo.FindProperty("replayButton").objectReferenceValue = btn;
        hudSo.ApplyModifiedPropertiesWithoutUndo();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MemoryGame.unity");
        Debug.Log("Scene saved: Assets/Scenes/MemoryGame.unity");
    }
}
