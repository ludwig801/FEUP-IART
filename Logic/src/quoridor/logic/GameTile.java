package quoridor.logic;

import java.util.ArrayList;

public class GameTile {

	protected int row, col;
	protected ArrayList<GameTile> neighbors;
	protected boolean occupied;
	protected GamePawn pawn;
	
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
