package quoridor.logic;

import java.util.ArrayList;

public class GameAI {
	
	class GameStateNode {
		
		public GameState state;
		public ArrayList<GameState> children;
		public int alpha, beta;
		
		public GameStateNode(GameState mState) {
			this.children = new ArrayList<GameState>();
			
			this.state = mState;
			
			// TODO: verify these values
			alpha = Integer.MIN_VALUE;
			beta = Integer.MAX_VALUE;
		}
	}

	static GameStateNode root;
	
	public static void main(String[] args) {
		GameState testState = new GameState();
		GameAI testAI = new GameAI(testState);
		
		printShortestPath(root,0);
		printShortestPath(root,1);
	}
	
	public GameAI(GameState mInitialState) {
		
		root = new GameStateNode(mInitialState);
		
		root.state.addWall(2, 1, true);
		root.state.addWall(1, 0, false);
		
		calcShortestPathsForBothPlayers();
	}

	private void calcShortestPathsForBothPlayers() {
		for(int player = 0; player < 2; player++) {
			int victoryRow = (GameState.boardSize + player - 1) % GameState.boardSize;
			for(int i = 0; i < root.state.boardSize; i++) {
				GameTile tile = root.state.board.getTile(victoryRow, i);
				tile.value[player] = 0;
				calcShortestPathForPlayer(tile,tile.value[player],player,victoryRow);
			}
			
			for(ArrayList<GameTile> mRows : root.state.board.tiles) {
				for(GameTile mTile : mRows) {
					mTile.visited = false;
				}
			}
		}
	}

	private void calcShortestPathForPlayer(GameTile tile, int mVal, int player, int victoryRow) {
		
		if(tile.isOccupied()) {	
			// Reached objective tile
			if(tile.pawn.owningPlayer == player) {
				if(tile.value[player] > mVal) {
					tile.value[player] = mVal;
				}
				return;
			}
		}
	
		for(GameTile neighbor : tile.neighbors) {
			if(!neighbor.equals(tile)) {
				if(neighbor.value[player] == -1) {
					neighbor.value[player] = mVal + 1;
					calcShortestPathForPlayer(neighbor, neighbor.value[player], player, victoryRow);
				}
				else if(neighbor.value[player] > (mVal + 1)) {
					neighbor.value[player] = mVal + 1;
					calcShortestPathForPlayer(neighbor, neighbor.value[player], player, victoryRow);
				}
			}
		}
	}
	
	public static void printShortestPath(GameStateNode node, int player) {
		System.out.println("---------------------------------------");
		System.out.println(" Shortest path lengths for: " + player);
		System.out.println("---------------------------------------");
		
		final int size = node.state.board.getSize();
		final int border = size - 1;
		
		for(int row = 0; row < size; row++) {
			final int prevRow = row - 1;
			for(int col = 0; col < size; col++) {
				GameTile tile = node.state.board.getTile(row, col);
				
				System.out.print("[" + tile.value[player] + "]");

				if(tile.isWalled() && tile.getWall().isVertical()) {
					System.out.print("|");
				} else if (row > 0 && node.state.board.getTile(prevRow, col).isWalled()
						&& node.state.board.getTile(prevRow, col).getWall().isVertical()) {
					System.out.print("|");
				} else {
					System.out.print(" ");
				}
			}
			System.out.println();
			if(row < border) {
				for(int j = 0; j < size; j++) {
					if(node.state.board.getTile(row, j).isWalled()) {
						if(node.state.board.getTile(row, j).getWall().isHorizontal()) {
							System.out.print("----");
						} else {
							System.out.print("   |");
						}
						
					}
					else if(j > 0 && node.state.board.getTile(row, j-1).isWalled()
							&& node.state.board.getTile(row, j-1).getWall().isHorizontal()) {
						System.out.print("--- ");
					} else {
						System.out.print("    ");
					}
				}
			}
			System.out.println();
		}
	}

}
