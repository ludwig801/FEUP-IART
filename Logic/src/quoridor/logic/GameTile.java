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
	protected boolean occupied;
	protected GamePawn pawn;
	protected boolean walled;
	protected GameWall wall;
	
	
	public GameTile(int mRow, int mCol) {
		
		this.row = mRow;
		this.col = mCol;
		
		this.occupied = false;
		
		this.neighbors = new ArrayList<GameTile>(4);
	}
	
	public int getRow() {
		return this.row;
	}
	
	public int getCol() {
		return this.col;
	}

	public boolean isOccupied() {
		return this.occupied;
	}
	
	public void setPawn(GamePawn mPawn) {
		this.pawn = mPawn;
		this.occupied = true;
	}
	
	public GamePawn getPawn() {
		return this.pawn;
	}
	
	public void removePawn() {
		this.pawn = null;
		this.occupied = false;
	}
	
	public boolean isWalled() {
		return this.walled;
	}
	
	public void setWall(GameWall mWall) {
		this.wall = mWall;
		this.walled = true;
	}
	
	public GameWall getWall() {
		return this.wall;
	}
	
	public void removeWall() {
		this.wall = null;
		this.walled = false;
	}
	
	public void SetNeighbor(GameTile mTile) {
		this.neighbors.add(mTile);
	}
	
	public void removeNeighbor(GameTile mTile) {
		this.neighbors.remove(mTile);
	}

	public ArrayList<GameTile> getNeighbors() {
		return this.neighbors;
	}
}
