using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Link))]
public class LinkEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		Link link = (Link)target;
		
		EditorGUILayout.Toggle("Visible: ", link.Visible);
		EditorGUILayout.Vector2Field("Tile A",new Vector2(link.TileA.row,link.TileA.col));
		EditorGUILayout.Vector2Field("Tile B",new Vector2(link.TileB.row,link.TileB.col));
	}
}
