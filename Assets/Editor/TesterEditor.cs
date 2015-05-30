using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Tester))]
public class TesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // thks to pitimoi at unityAnswers!
        serializedObject.Update();
        SerializedProperty prop = serializedObject.FindProperty("m_Script");
        EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
        serializedObject.ApplyModifiedProperties();
        //

        Tester script = (Tester)target;

        script.numGamesPerPlayer = EditorGUILayout.IntSlider("Games Per Player", script.numGamesPerPlayer, 1, 50);
        script.overrideMaxRounds = EditorGUILayout.BeginToggleGroup("Override Max Rounds", script.overrideMaxRounds);
        script.maxRounds = EditorGUILayout.IntSlider("Max Rounds", script.maxRounds, 1, 10);
        EditorGUILayout.EndToggleGroup();

        script.ongoingRounds = EditorGUILayout.Toggle("Ongoing Rounds", script.ongoingRounds);
    }
}
