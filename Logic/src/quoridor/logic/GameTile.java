package quoridor.logic;

import java.util.ArrayList;

public class GameTile {

	/*
	 * IMPORTANT WALL REFERENCE 
	 * 
	 * Position GameTile	[X]
	 * Other GameTiles		[ ]
	 * 
	 * =====================================================================
	 *        POSSIBLE WALL PLACEMENTS AND TILE REFERENCE
	 * =====================================================================
	 *									|| 
	 * [X] [ ] [ ]						||   [X]|[ ] [ ]
	 * -------							||	    | 
	 * [ ] [ ] [ ]						||	 [ ]|[ ] [ ]
	 *									|| 
	 * [ ] [ ] [ ]						||	 [ ] [ ] [ ]
	 * 									||
	 * Notice that, in an horizontal	||  Notice that, in a vertical wall,
	 * wall, the position tile (X)		||  the position tile is STILL in the
	 * refers to its top-left			||  top-left (Northwest) direction of
	 * direction (Northwest).			||  the wall.
	 * 									||
	 * =====================================================================
	 */
	
	protected int row, col;
	protected ArrayList<GameTile> neighbors;
	protected GamePawn pawn;
	protected GameWall wall;
	
	// Shortest path to victory
	// One array position for each player
	public boolean visited;
	public int[] value = new int[2];
	public GameTile[] parent = new GameTile[2];
	public GameTile[] child = new GameTile[2];
	
	public GameTile(int mRow, int mCol) {
		this.row = mRow;
		this.col = mCol;
		
		this.neighbors = new ArrayList<GameTile>();
		
		this.visited = false;
		
		this.value[0] = Integer.MAX_VALUE;
		this.value[1] = Integer.MAX_VALUE;
		
		child[0] = null;
		child[1] = null;
	}
	
	public int getRow() {
		return this.row;
	}
	
	public int getCol() {
		return this.col;
	}

	public boolean isOccupied() {
		return this.pawn != null;
	}
	
	public void setPawn(GamePawn mPawn) {
		this.pawn = mPawn;
	}
	
	public GamePawn getPawn() {
		return this.pawn;
	}
	
	public void removePawn() {
		this.pawn = null;
	}
	
	public boolean isWalled() {
		return this.wall != null;
	}
	
	public void setWall(GameWall mWall) {
		this.wall = mWall;
	}
	
	public GameWall getWall() {
		return this.wall;
	}
	
	public void removeWall() {
		this.wall = null;
	}
	
	public void addNeighbor(GameTile mTile) {
		this.neighbors.add(mTile);
	}
	
	public void removeNeighbor(GameTile mTile) {
		this.neighbors.remove(mTile);
	}

	public ArrayList<GameTile> getNeighbors() {
		return this.neighbors;
	}
	
}
