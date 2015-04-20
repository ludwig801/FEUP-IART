using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Tile))]
public class TileEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		Tile tile = (Tile)target;

		EditorGUILayout.ObjectField("Pawn",tile.Pawn,typeof(Pawn),false);
		EditorGUILayout.ObjectField("Wall",tile.Wall,typeof(Wall),false);
	}
}
