using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Tester))]
public class TesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Tester script = (Tester)target;

        script.numGamesPerPlayer = EditorGUILayout.IntSlider("Games Per Player", script.numGamesPerPlayer, 1, 50);

        if (GUILayout.Button("Start Rounds"))
        {
            script.StartRounds();
        }
        else if (GUILayout.Button("Stop Rounds"))
        {
            script.StopRounds();
        }
    }
}
