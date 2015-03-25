package quoridor.logic;

import java.util.LinkedList;

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
	public LinkedList<GameTile> neighbors;
	public boolean pawn;
	public GameWall wall;
	
	// Shortest path to victory
	// One array position for each player
	public int[] value;
	public GameTile[] parent;
	public GameTile[] child;
	
	public GameTile(int mRow, int mCol) {
		this.row = mRow;
		this.col = mCol;
		
		this.neighbors = new LinkedList<GameTile>();
		this.wall = null;
		
		this.value = new int[2];	
		this.parent = new GameTile[2];
		this.child = new GameTile[2];
	}
	
	public boolean isOccupied() {
		return this.pawn;
	}
	
	public void addPawn() {
		this.pawn = true;
	}
	
	public void removePawn() {
		this.pawn = false;
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
		this.neighbors.addLast(mTile);
	}
	
	public void removeNeighbor(GameTile mTile) {
		this.neighbors.remove(mTile);
	}
	
}
