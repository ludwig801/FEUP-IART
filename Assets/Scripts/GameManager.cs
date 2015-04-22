using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	private Logic game;

	public GameObject wallPrefab;
	public int minimaxDepth;

	// Methods
	void Start()
	{
		CreateWalls();
		game = new Logic(minimaxDepth);
	}

	public void PlayAutomatic()
	{
		game.MoveAI(minimaxDepth);
	}

	void CreateWalls()
	{
		for(int i = 0; i < Logic.NumPlayers; i++)
		{
			Transform walls = GameObject.Find(Names.GamePieces).transform.FindChild(Names.Player_ + i).transform.FindChild(Names.Walls).transform;

			for(int j = 0; j < Logic.NumWallsPerPlayer; j++)
			{
				GameObject wall = Instantiate(wallPrefab, new Vector3(), Quaternion.identity) as GameObject;
				wall.transform.SetParent(walls);
			}
		}
	}
}
