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

        GameManager manager = (GameManager)target;

        manager.boardPrefab = (Transform)EditorGUILayout.ObjectField("Board Prefab", manager.boardPrefab, typeof(Transform), true);
        manager.wallPrefab = (Transform)EditorGUILayout.ObjectField("Wall Prefab", manager.wallPrefab, typeof(Transform), true);
        manager.pawnPrefab = (Transform)EditorGUILayout.ObjectField("Pawn Prefab", manager.pawnPrefab, typeof(Transform), true);

        manager.numWallsPerPlayer = EditorGUILayout.IntSlider("Walls Per Player", manager.numWallsPerPlayer, 0, 10);
        manager.minimaxDepth = EditorGUILayout.IntSlider("Minimax Depth", manager.minimaxDepth, 1, 3);

        if (GUILayout.Button("Play AI Move"))
        {
            manager.PlayAIMove();
        }
        if (GUILayout.Button("Undo AI Move"))
        {
            manager.UndoAIMove();
        }

        EditorGUILayout.Separator();

        customValues = EditorGUILayout.BeginToggleGroup("Custom Play", customValues);
        player = EditorGUILayout.IntSlider("Player", player, 1, 2);
        pos = EditorGUILayout.Vector2Field("Position", pos);
        wall = EditorGUILayout.BeginToggleGroup("Wall", wall);
        horizontal = EditorGUILayout.Toggle("Horizontal", horizontal);
        EditorGUILayout.EndToggleGroup();
        if (GUILayout.Button("Play"))
        {
            if (wall)
            {
                manager.PlayAIMove(new Move(pos.x, pos.y, horizontal), player - 1);
            }
            else
            {
                manager.PlayAIMove(new Move(pos.x, pos.y), player - 1);
            }
        }
        EditorGUILayout.EndToggleGroup();
    }
}
