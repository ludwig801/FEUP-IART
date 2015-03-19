package quoridor.logic;

public class GameBoard {

	public GameTile[][] tiles;
	
	public GameBoard(int k) {
		int localSize = k;
		
		// If mSize is an even number, then boardSize auto-adjusts to mSize + 1.
		if(k % 2 == 0) {
			localSize++;
		}
		
		// Initialize the multidimensional array that shall contain the game tiles.
		tiles = new GameTile[localSize][localSize];
		for(int i = 0; i < localSize; i++) {
			for(int j = 0; j < localSize; j++) {
				tiles[i][j] = new GameTile(i,j);
			}
		}
		
		// Link the neighboring tiles
		for(int i = 0; i < localSize; i++) {
			GameTile[] line = tiles[i];
			for(int j = 0; j < localSize; j++) {
				GameTile tile = line[j];
				
				if(tile.col < (line.length-1)) {
					addLink(tile,line[tile.col+1]);
				}
				if(tile.row < (line.length-1)) {
					addLink(tile,tiles[tile.row+1][tile.col]);
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

	public GameTile getTile(int mRow, int mCol) {
		return tiles[mRow][mCol];
	}
	
	public boolean isValidPosition(int mRow, int mCol) {
		return mRow >= 0 && mRow <= getBorder() && mCol >= 0 && mCol <= getBorder();
	}
	
	public int getSize() {
		return tiles.length;
	}
	
	public int getBorder() {
		return tiles.length - 1;
	}

	public void printLinks() {
		System.out.println("Board Links");
		for(GameTile[] line : tiles) {
			for(GameTile tile : line) {
				System.out.print("Tile(" + tile.row + ", " + tile.col + ") --> [");
				for(GameTile neighbor : tile.neighbors) {
					System.out.print(" (" + neighbor.row + ", " + neighbor.col + ")");
				}
				System.out.println(" ]");
			}
		}
	}
}
