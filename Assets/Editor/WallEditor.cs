using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Wall))]
public class WallEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		Wall wall = (Wall)target;

		wall.Horizontal = EditorGUILayout.Toggle("Horizontal: ", wall.Horizontal);
		if(wall.Tile)
		{
			EditorGUILayout.Vector2Field("Tile",new Vector2(wall.Tile.row,wall.Tile.col));
		}
	}
}
