using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Pawn))]
public class PawnEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		Pawn pawn = (Pawn)target;

		if(pawn.Tile != null)
		{
			EditorGUILayout.ObjectField("Tile: ",pawn.Tile.gameObject,typeof(GameObject),false);
		}
	}
}

