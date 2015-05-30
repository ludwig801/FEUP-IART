using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    bool customValues;
    bool wall;
    Vector2 pos;
    bool horizontal;
    int player;

    public override void OnInspectorGUI()
    {
        // thks to pitimoi at unityAnswers!
        serializedObject.Update();
        SerializedProperty prop = serializedObject.FindProperty("m_Script");
        EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
        serializedObject.ApplyModifiedProperties();
        //

        GameManager script = (GameManager)target;

        script.boardPrefab = (Transform)EditorGUILayout.ObjectField("Board Prefab", script.boardPrefab, typeof(Transform), true);
        script.wallPrefab = (Transform)EditorGUILayout.ObjectField("Wall Prefab", script.wallPrefab, typeof(Transform), true);
        script.pawnPrefab = (Transform)EditorGUILayout.ObjectField("Pawn Prefab", script.pawnPrefab, typeof(Transform), true);

        script.numWallsPerPlayer = EditorGUILayout.IntSlider("Walls Per Player", script.numWallsPerPlayer, 0, 10);
        script.minimaxDepth = EditorGUILayout.IntSlider("Minimax Depth", script.minimaxDepth, 1, 3);

        script.CPU_0 = EditorGUILayout.Toggle("CPU [0]", script.CPU_0);
        script.CPU_1 = EditorGUILayout.Toggle("CPU [1]", script.CPU_1);

        if (GUILayout.Button("Next Turn"))
        {
            script.NextTurn();
        }
        if (GUILayout.Button("Play AI Move"))
        {
            script.PlayAIMove();
        }
        if (GUILayout.Button("Undo AI Move"))
        {
            script.UndoLastMove();
        }
        if (GUILayout.Button("Print Reference Values"))
        {
            script.PrintDebug();
        }
    }
}
