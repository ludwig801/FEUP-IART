using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class Game
{
	public const int NumPlayers = 2;
	public const int NumWallsPerPlayer = 20;
	public const int BoardSize = 9;
	public const int BoardBorder = BoardSize - 1;

	private int minimaxDepth;
	private int currentPlayer;
	private int[] wallCount;
	public Pawn[] pawns;
	public Board board;
	public Wall[,] walls;
	private Move bestMove;
	private Stack<Move> moveHistory;
	private Text boardText;

	Stopwatch stopwatch;
	Counter cuts;

	public bool GameOver = false;

	// >> Heuristics <<
	// 
	// > Features
	//    > Agressive
	//        > F1 : Max's distance to goal
	//        > F2 : Max's minimum moves to next column (closer to Min's border)
	//    > Defensive
	//        > F3 : Difference between Max distance to goal and Min's distance to goal
	//        > F4 : Min's minimum moves to next column (closer to Max's border) 
	//
	// > Evalutation Function
	//      H(S) = W(Fi)*Fi(S) + Random;
	//
	// W(Fi)
	//
	// | F1 | x |
	// | F2 | x |
	// | F3 | x |
	// | F4 | x |
	int[] distanceToGoal;
	int[] distanceToNextRow;

	// Methods
	public Game(int minimaxDepth)
	{
		this.minimaxDepth = minimaxDepth;

		board = GameObject.Find(Names.Board).GetComponent<Board>();
		board.Init();

		walls = new Wall[NumPlayers,NumWallsPerPlayer];

		pawns = new Pawn[NumPlayers];
		for(int i = 0; i < NumPlayers; i++)
		{
			for(int j = 0; j < NumWallsPerPlayer; j++)
			{
				walls[i,j] = GameObject.Find(Names.GamePieces).transform.FindChild(Names.Player_ + i).transform.FindChild(Names.Walls).transform.GetChild(j).GetComponent<Wall>();
				walls[i,j].Init();
			}
			pawns[i] = GameObject.Find(Names.GamePieces).transform.FindChild(Names.Player_ + i).transform.FindChild(Names.Pawn).GetComponent<Pawn>();
		}

		pawns[0].Tile = board.GetTileAt(0, board.Size / 2);
		pawns[1].Tile = board.GetTileAt(board.Border, board.Size / 2);

		// Heuristics
		distanceToGoal = new int[NumPlayers];
		distanceToNextRow = new int[NumPlayers];

		//
		moveHistory = new Stack<Move>();
		currentPlayer = 0;

		stopwatch = new Stopwatch();
		cuts = new Counter();
		cuts.Reset();

		boardText = GameObject.Find ("Canvas").transform.FindChild("Panel").FindChild("Board Text").GetComponent<Text>();

		//
		Tests();
	}

	void Tests()
	{
		Move setWall = new Move(1,4,true);
		PlayMove(setWall,0);
		UndoMove(setWall,0);
		CalcDistances();
		PrintFeatures();
		Print(0);
	}
	
	public void MoveAI(int depth)
	{
		stopwatch.Reset();
		stopwatch.Start();

		moveHistory = new Stack<Move>();
		bestMove = null;

		minimaxDepth = depth;
		Node root = new Node(true);
		MinimaxAlphaBeta(root, depth, currentPlayer);

		//UnityEngine.Debug.Log("Move History Size: " + moveHistory.Count);

		UnityEngine.Debug.Log("Will play: " + bestMove.ToString() + "  [" + root.heuristicValue + "]");
		PlayMove(bestMove, currentPlayer);
		UnityEngine.Debug.Log("Elapsed Time: " + stopwatch.ElapsedMilliseconds);
		moveHistory.Pop();
		currentPlayer = NextTurn(currentPlayer);

		CalcDistances();
		PrintFeatures();
		Print(currentPlayer);
	}
	
	void MinimaxAlphaBeta(Node node, int depth, int player)
	{
		if (0 == depth)
		{
			node.heuristicValue = GetHeuristicValue(player);
			return;
		}

		// Assign Moves
		List<Move> moves = GetPossibleMoves(player);

		// Evaluate Moves
		for(int i = 0; i < moves.Count; i++)
		{
			Move move = moves[i];

			Node child = new Node (!node.isMaximizer);

			if(!PlayMove(move,player))
			{
				return;
			}
			
			child.alpha = node.alpha;
			child.beta = node.beta;
			
			MinimaxAlphaBeta(child, depth-1, NextTurn(player));
			
			if(node.isMaximizer)
			{
				if(depth == minimaxDepth &&child.heuristicValue > node.heuristicValue)
				{
					bestMove = move;
				}
				node.heuristicValue = Mathf.Max(node.heuristicValue, child.heuristicValue);
			}
			else
			{
				node.heuristicValue = Mathf.Min(node.heuristicValue, child.heuristicValue);
			}
			
			UndoMove(moveHistory.Pop(),player);
			
			if(AlphaBetaCut(node, child))
			{
				child = null;
				break;
			}
			
			child = null;
		}

		moves = null;
	}
	
	List<Move> GetPossibleMoves(int player)
	{
		List<Move> moves = new List<Move>();

		Tile best = null;
		
		// pawn movements
		for(int i = 0; i < pawns[player].Tile.neighbors.Count; i++)
		{
			Tile tile = pawns[player].Tile.neighbors[i];
			if(!tile.HasPawn)
			{
				if(best == null || tile.GetValue(player) < best.GetValue(player))
				{
					best = tile;
				}
			}
		}

		moves.Add(new Move(best.row, best.col));
		
		// horizontal wall placements
		for(int i = 0; i < board.Size; i++)
		{
			for(int j = 0; j < board.Size; j++)
			{
				if(CanPlaceWall(board.GetTileAt(i,j), true))
				{
					moves.Add(new Move(i, j, true));
				}
			}
		}

		// vertical wall placements
		for(int i = 0; i < board.Size; i++)
		{
			for(int j = 0; j < board.Size; j++)
			{
				if(CanPlaceWall(board.GetTileAt(i,j), false))
				{
					moves.Add(new Move(i, j, false));
				}
			}
		}
		
		return moves;
	}
	
	bool AlphaBetaCut(Node node, Node child)
	{
		if(node.isMaximizer)
		{
			node.alpha = Mathf.Max(node.alpha, node.heuristicValue);
			if(node.alpha > node.beta)
			{
				return true;
			}
			
		}
		else
		{
			node.beta = Mathf.Min(node.beta,node.heuristicValue);
			if(node.alpha > node.beta)
			{
				return true;
			}
			
		}
		
		return false;
	}
	
	bool PlayMove(Move move, int player)
	{
		//UnityEngine.Debug.Log("Play " + move.ToString());

		board.RemoveTempLinks(pawns[NextTurn(player)].Tile);

		switch(move.type)
		{
		case Move.MovePawn:
			moveHistory.Push(new Move(pawns[player].Tile.row,pawns[player].Tile.col));
			board.MovePawnTo(pawns[player], move.row, move.col);
			break;
			
		case Move.SetWall:
			Wall wall = GetWall(player);
			if(wall == null)
			{
				return false;
			}
			moveHistory.Push(new Move(move));
			board.SetWall(wall, move.row, move.col, move.isHorizontal);
			break;
			
		default:
			return false;
		}

		board.SetTempLinks(pawns[player].Tile);
		return true;
	}

	Wall GetWall(int player)
	{
		for(int j = 0; j < NumWallsPerPlayer; j++)
		{
			Wall wall = walls[player,j];
			if(wall.Free)
			{
				return wall;
			}
		}

		return null;
	}
	
	void UndoMove(Move move, int player)
	{
		board.RemoveTempLinks(pawns[player].Tile);

		switch(move.type)
		{
		case Move.MovePawn:
			board.MovePawnTo(pawns[player], move.row, move.col);
			break;
			
		case Move.SetWall:
			board.RemoveWall(move.row, move.col);
			break;
			
		default:
			break;
		}

		board.SetTempLinks(pawns[NextTurn(player)].Tile);
	}
	
	bool CanPlaceWall(Tile tile, bool horizontal)
	{
		int row = tile.row;
		int col = tile.col;

		if(row == 0 || col == board.Border || tile.HasWall)
		{
			return false;
		}
		else if(horizontal)
		{
			for(int i = -1; i <=1 ; i += 2)
			{
				if(board.IsValidPosition(row, col + i))
				{
					Tile b = board.GetTileAt(row, col + i);
					if(b.HasWall && b.Wall.Horizontal)
					{
						return false;
					}
				}
			}
		}
		else // vertical
		{
			for(int i = -1; i <=1 ; i += 2)
			{
				if(board.IsValidPosition(row + i, col))
				{
					Tile b = board.GetTileAt(row + i, col);
					if(b.HasWall && b.Wall.Vertical)
					{
						return false;
					}
				}
			}
		}
		
		return true;
	}
	
	float GetHeuristicValue(int player)
	{
		CalcDistances();

		float F1 = distanceToGoal[player];
		float F2 = distanceToNextRow[player];
		float F3 = distanceToGoal[NextTurn(distanceToGoal[player])] - distanceToGoal[player];
		float F4 = distanceToNextRow[NextTurn(player)];

		return (F3 + F4) - (F1 + F2);
	}
	
	void CalcDistances()
	{
		board.SetTilesValues(int.MaxValue);
		Tile tile;
		int maxIter = 30;

		// Player 0
		int player = 0;
		int victoryRow = board.Border;
		
		for(int j = 0; j < board.Size; j++)
		{
			tile = board.GetTileAt(victoryRow, j);
			tile.SetValue(player,0);
			
			CalcDistances(tile, tile.GetValue(player), victoryRow, player);
		}

		tile = pawns[player].Tile;
		distanceToGoal[player] = tile.GetValue(player);
		int row = tile.row;
		int iter = 0;

		while((row <= pawns[player].Tile.row) && iter <= maxIter)
		{
			tile = tile.GetChild(player);
			row = tile.row;
			iter++;
		}
		if(iter > maxIter)
		{
			UnityEngine.Debug.Log("For Player " + player + " : iterations overflow");
		}
		distanceToNextRow[player] = iter;

		// Player 1
		player = 1;
		victoryRow = 0;
		
		for(int j = 0; j < board.Size; j++)
		{
			tile = board.GetTileAt(victoryRow, j);
			tile.SetValue(player,0);
			
			CalcDistances(tile, tile.GetValue(player), victoryRow, player);
		}

		tile = pawns[player].Tile;
		distanceToGoal[player] = tile.GetValue(player);
		row = tile.row;
		iter = 0;

		while((row >= pawns[player].Tile.row) && iter <= maxIter)
		{
			tile = tile.GetChild(player);
			row = tile.row;
			iter++;
		}
		if(iter > maxIter)
		{
			UnityEngine.Debug.Log("For Player " + player + " : iterations overflow");
		}
		distanceToNextRow[player] = iter;
	}
	
	void CalcDistances(Tile tile, int value, int victoryRow, int player)
	{
		int val = value + 1;
		for(int i = 0; i < tile.neighbors.Count; i++)
		{
			Tile neighbor = tile.neighbors[i];
			if(neighbor.GetValue(player) > val)
			{
				neighbor.SetValue(player,val);
				neighbor.SetChild(player,tile);
				
				CalcDistances(neighbor, neighbor.GetValue(player), victoryRow, player);
			}
		}
	}
	
	int NextTurn(int player)
	{
		player++;
		player %= NumPlayers;
		return player;
	}
	
	int PreviousTurn(int player)
	{
		player--;
		if(player < 0)
		{
			player = NumPlayers + player;
		}
		return player;
	}

	void PrintFeatures()
	{
		UnityEngine.Debug.Log ("Player 0 : Distances [" + distanceToGoal[0] + " | " + distanceToNextRow[0] + "]");
		UnityEngine.Debug.Log ("Player 1 : Distances [" + distanceToGoal[1] + " | " + distanceToNextRow[1] + "]");
	}

	void Print(int player)
	{
		StringWriter stream = new StringWriter();

		for(int row = board.Size - 1; row >= 0; row--)
		{
			int nextRow = row + 1;
			for(int col = 0; col < board.Size; col++)
			{
				Tile tile = board.GetTileAt(row, col);

				stream.Write("[");
				if(tile.GetValue(player) == int.MaxValue)
				{
					stream.Write("X");
				}
				else
				{
					stream.Write(tile.GetValue(player));
				}
				stream.Write("]");
				
				if(tile.HasWall && tile.Wall.Vertical)
				{
					stream.Write("|");
				}
				else if (row < (board.Size - 1) && board.GetTileAt(nextRow, col).HasWall && board.GetTileAt(nextRow, col).Wall.Vertical)
				{
					stream.Write("|");
				}
				else
				{
					if(tile.GetValue(player) < 10)
					{
						stream.Write(" ");
					}
				}
			}
			stream.WriteLine();
			if(row < board.Border)
			{
				for(int col = 0; col < board.Size; col++)
				{
					if(board.GetTileAt(row, col).HasWall)
					{
						if(board.GetTileAt(row, col).Wall.Horizontal)
						{
							stream.Write("----");
						}
						else
						{
							if(col > 0 && board.GetTileAt(row, col-1).HasWall && board.GetTileAt(row, col-1).Wall.Horizontal)
							{
								stream.Write("---|");
							}
							else
							{
								stream.Write("   |");
							}
						}
					}
					else if(col > 0 && board.GetTileAt(row, col-1).HasWall && board.GetTileAt(row, col-1).Wall.Horizontal)
					{
						stream.Write("--- ");
					}
					else
					{
						stream.Write("    ");
					}
				}
			}
			stream.WriteLine();
		}

		boardText.text = stream.ToString();
	}
}
