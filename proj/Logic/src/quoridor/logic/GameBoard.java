package quoridor.logic;

import java.util.ArrayList;

public class GameBoard {

	protected ArrayList<ArrayList<GameTile>> tiles;
	
	public GameBoard(int mSize) {
		int localSize = mSize;
		
		/* [!]
		 * 
		 * If mSize is an even number, then boardSize auto-adjusts to mSize + 1.
		 * 
		 */
		if(mSize % 2 == 0) {
			localSize++;
			//System.out.println("GameBoard() --> mSize(" + mSize + ") is even. (" + localSize + ") will be used.");
		}
		
		// Initialize the multidimensional array that shall contain the game tiles.
		tiles = new ArrayList<ArrayList<GameTile>>();
		for(int i = 0; i < localSize; i++) {
			tiles.add(new ArrayList<GameTile>());
			for(int j = 0; j < localSize; j++) {
				tiles.get(i).add(new GameTile(i,j));
			}
		}
		
		// Link the neighboring tiles
		for(int i = 0; i < localSize; i++) {
			ArrayList<GameTile> line = tiles.get(i);
			for(int j = 0; j < localSize; j++) {
				GameTile tile = line.get(j);
				
				if(tile.getCol() < (line.size()-1)) {
					addLink(tile,line.get(tile.getCol()+1));
				}
				if(tile.getRow() < (line.size()-1)) {
					addLink(tile,tiles.get(tile.getRow()+1).get(tile.getCol()));
				}
			}
		}
	}
	
	public void addLink(GameTile tileA, GameTile tileB) {
		tileA.addNeighbor(tileB);
		tileB.addNeighbor(tileA);
	}
	
	public void removeLink(GameTile tileA, GameTile tileB) {
		tileA.removeNeighbor(tileB);
		tileB.removeNeighbor(tileA);	
	}

	public ArrayList<ArrayList<GameTile>> getTiles() {
		return this.tiles;
	}

	public GameTile getTile(int mRow, int mCol) {
		return tiles.get(mRow).get(mCol);
	}
	
	public int getSize() {
		return tiles.size();
	}
	
	public int getBorder() {
		return tiles.size() - 1;
	}

	public void printLinks() {
		System.out.println("Board Links");
		for(ArrayList<GameTile> line : tiles) {
			for(GameTile tile : line) {
				System.out.print("Tile(" + tile.getRow() + ", " + tile.getCol() + ") --> [");
				for(GameTile neighbor : tile.getNeighbors()) {
					System.out.print(" (" + neighbor.getRow() + ", " + neighbor.getCol() + ")");
				}
				System.out.println(" ]");
			}
		}
	}
}
