using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // thks to pitimoi at unityAnswers!
        serializedObject.Update();
        SerializedProperty prop = serializedObject.FindProperty("m_Script");
        EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
        serializedObject.ApplyModifiedProperties();
        //

        GameManager manager = (GameManager)target;

        manager.wallPrefab = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", manager.wallPrefab, typeof(GameObject), true);

        manager.minimaxDepth = EditorGUILayout.IntSlider("Minimax Depth", manager.minimaxDepth, 1, 3);
        manager.numGamesPerPlayer = EditorGUILayout.IntSlider("Games Per Player", manager.numGamesPerPlayer, 1, 50);
        manager.maxPlies = EditorGUILayout.IntSlider("Max Plies Per Game", manager.maxPlies, 10, 300);

        manager.runBattery = GUILayout.Toggle(manager.runBattery, "Run Game Battery");
        if (GUILayout.Button("Reset Game"))
        {
            manager.Reset();
        }
        if (GUILayout.Button("Play AI Move"))
        {
            manager.MoveAI();
        }
    }
}
