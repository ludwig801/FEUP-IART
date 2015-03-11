package quoridor.logic;

import java.util.ArrayList;
import java.util.concurrent.TimeUnit;

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
		
<<<<<<< HEAD
//		root.state.board.getTile(0, 2).removePawn();
//		root.state.board.getTile(0, 1).setPawn(root.state.pawns.get(0));
		
		root.state.addWall(0, 1, false);
		root.state.addWall(1, 2, false);
		root.state.addWall(2, 0, true);
		root.state.addWall(2, 2, true);
		root.state.addWall(3, 1, true);
		root.state.addWall(3, 3, true);
=======
//		root.state.addWall(0, 1, false);
//		root.state.addWall(2, 1, false);
//		root.state.addWall(3, 2, true);
//		root.state.addWall(1, 2, true);
//		root.state.addWall(2, 3, true);
//		root.state.addWall(1, 0, true);
//		root.state.addWall(0, 2, true);
>>>>>>> b8185c02d0fdbb79433127863c95be1324ca5ce2
		
		long time = System.nanoTime();
		
		calcShortestPathsForBothPlayers();
		
		long elapsedTime = TimeUnit.NANOSECONDS.toMillis(System.nanoTime() - time);
		
		System.out.println("Elapsed time: " + elapsedTime);
	}

	private void calcShortestPathsForBothPlayers() {
		for(int player = 0; player < 2; player++) {
			int victoryRow = (GameState.boardSize + player - 1) % GameState.boardSize;
			for(int i = 0; i < GameState.boardSize; i++) {
				GameTile tile = root.state.board.getTile(victoryRow, i);
				tile.value[player] = 0;
				calcShortestPathForPlayer(tile,tile.value[player],player,victoryRow);
			}
		}
	}

	private void calcShortestPathForPlayer(GameTile tile, int mVal, int player, int victoryRow) {
		
//		System.out.println("Node: (" + tile.row + "," + tile.col + ")");
//		if(tile.parent[player] != null) {
//			System.out.println("  Parent: (" + tile.parent[player].row + "," + tile.parent[player].col + ")");
//		}
		
//		if(player == 0)
//			printShortestPath(root, player);
		
<<<<<<< HEAD
//		if(tile.isOccupied()) {	
//			// Reached objective tile
//			if(tile.pawn.owningPlayer == player) {
//				return;
//			}
//		}
	
		for(GameTile neighbor : tile.neighbors) {
			if(!neighbor.equals(tile)) {
				if(neighbor.value[player] > (mVal + 1)) {
=======
		if(tile.isOccupied()) {	
			// Reached objective tile
			if(tile.pawn.owningPlayer == player) {
				return;
			}
		}
	
		for(GameTile neighbor : tile.neighbors) {
			if(!neighbor.equals(tile)) {
				if((neighbor.value[player] == -1)
						|| neighbor.value[player] > (mVal + 1)) {
>>>>>>> b8185c02d0fdbb79433127863c95be1324ca5ce2
					neighbor.value[player] = mVal + 1;
//					neighbor.parent[player] = tile;
//					tile.child[player] = neighbor;
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

<<<<<<< HEAD
				if(tile.value[player] == Integer.MAX_VALUE) {
=======
				if(tile.value[player] == -1) {
>>>>>>> b8185c02d0fdbb79433127863c95be1324ca5ce2
					System.out.print(" " + "x" + "");
				} else {
					if(tile.value[player] < 10) {
						System.out.print(" ");
					}
					System.out.print("" + tile.value[player] + "");
				}

				if(tile.isWalled() && tile.getWall().isVertical()) {
					System.out.print(" |");
				} else if (row > 0 && node.state.board.getTile(prevRow, col).isWalled()
						&& node.state.board.getTile(prevRow, col).getWall().isVertical()) {
					System.out.print(" |");
				} else {
					GameTile child = tile.child[player];
					GameTile parent = tile.parent[player];
					if(child != null && child.row == row && child.col == col + 1) {
						System.out.print(" <");
					} else if (parent != null && parent.row == row && parent.col == col + 1) {
						System.out.print(" >");
					} else {
						System.out.print("  ");
					}
				}
			}
			System.out.println();
			if(row < border) {
				for(int col = 0; col < size; col++) {
					GameTile tile = node.state.board.getTile(row, col);
					GameTile child = tile.child[player];
					GameTile parent = tile.parent[player];
					
					if(tile.isWalled()) {
						if(tile.getWall().isHorizontal()) {
							System.out.print("----");
						} else {
							if(child != null && child.row == tile.row+1 && child.col == tile.col) {
								System.out.print(" ^ |");
							} else if (parent != null && parent.row == row+1 && parent.col == tile.col) {
								System.out.print(" v |");
							} else {
								System.out.print("   |");
							}
						}
					} else if(col > 0) {
						GameTile tileLeft = node.state.board.getTile(row, col-1);
						if(tileLeft.isWalled()) {
							if(tileLeft.getWall().isHorizontal()) {
								System.out.print("--- ");
							} else {
								if(child != null && child.row == tile.row+1 && child.col == tile.col) {
									System.out.print(" ^  ");
								} else if (parent != null && parent.row == row+1 && parent.col == tile.col) {
									System.out.print(" v  ");
								} else {
									System.out.print("    ");
								}
							}
						} else {
							if(child != null && child.row == tile.row+1 && child.col == tile.col) {
								System.out.print(" ^  ");
							} else if (parent != null && parent.row == row+1 && parent.col == tile.col) {
								System.out.print(" v  ");
							} else {
								System.out.print("    ");
							}
						}
					} else {
						if(child != null && child.row == tile.row+1 && child.col == tile.col) {
							System.out.print(" ^  ");
						} else if (parent != null && parent.row == row+1 && parent.col == tile.col) {
							System.out.print(" v  ");
						} else {
							System.out.print("    ");
						}
					}
				}
			}
			System.out.println();
		}
	}

}
