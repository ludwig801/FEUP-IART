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
	
	public int row, col;
	public ArrayList<GameTile> neighbors;
	public GamePawn pawn;
	public GameWall wall;
	
	// Shortest path to victory
	// One array position for each player
	public int[] value = new int[2];
	public GameTile[] parent = new GameTile[2];
	public GameTile[] child = new GameTile[2];
	
	public GameTile(int mRow, int mCol) {
		this.row = mRow;
		this.col = mCol;
		
		this.neighbors = new ArrayList<GameTile>();
		
		this.value[0] = Integer.MAX_VALUE;
		this.value[1] = Integer.MAX_VALUE;
		
		child[0] = null;
		child[1] = null;
	}
	
	public boolean isOccupied() {
		return this.pawn != null;
	}
	
	public void setPawn(GamePawn mPawn) {
		this.pawn = mPawn;
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
	
	public void removeWall() {
		this.wall = null;
	}
	
	public void addNeighbor(GameTile mTile) {
		this.neighbors.add(mTile);
	}
	
	public void removeNeighbor(GameTile mTile) {
		this.neighbors.remove(mTile);
	}
	
}
