package quoridor.logic;

import java.util.ArrayList;
import java.util.LinkedList;
import java.util.concurrent.TimeUnit;

public class GameAI {

	static int lookAhead = 2;
	
	static GameStateNode root;
	
	public static void main(String[] args) {
		GameState testState = new GameState();
		GameAI testAI = new GameAI(testState);

		//printShortestPath(root,0);
		//printShortestPath(root,1);
	}
	
	public GameAI(GameState mInitialState) {
		Timer timer = new Timer();
		
		timer.start();
		System.out.println("=== GAMESTATE INIT ===");
		root = new GameStateNode(mInitialState);
		System.out.println("Elapsed: " + timer.getParcial() + " nanoseconds");
		
		System.out.println("=== WALL PLACEMENT ===");
		System.out.println("Elapsed: " + timer.getParcial() + " nanoseconds");

		System.out.println("====== GAME TREE =====");
		createGameTree(root, lookAhead);
		System.out.println("Elapsed: " + TimeUnit.NANOSECONDS.toMillis(timer.getParcial()) + " milliseconds");
		
		System.out.println("====== FINNISHED =====");
		System.out.println("Elapsed: " + timer.getTotal() + " nanoseconds");
		
		//printGameTreeDepthFirst(root);
		
		//System.out.println("======== BFS =======");
		//printBreadthFirst(root);
		
		//System.out.println("====== TREE SIZE =====");
		//System.out.println(getGameTreeSize(root));
	}
	
	private int getGameTreeSize(GameStateNode current) {
		int sum = 1;
		for(GameStateNode child : current.children) {
			sum += getGameTreeSize(child);
		}
		return sum;
	}

	private void createGameTree(GameStateNode current, int lookAhead2) {
		if(lookAhead2 > 0) {
			calcShortestPath(current);
			
			current.children = getNextGameStates(current.state);
			
			int newLevel = (lookAhead2 - 1);			
			for(GameStateNode child : current.children) {
				createGameTree(child, newLevel);
			}
		}
	}

	private void calcShortestPath(GameStateNode node) {
		for(ArrayList<GameTile> row : node.state.board.tiles) {
			for(GameTile tile : row) {
				tile.value[0] = Integer.MAX_VALUE;
				tile.value[1] = Integer.MAX_VALUE;
			}
		}
		for(int player = 0; player < 2; player++) {
			int victoryRow = (int)((GameState.boardSize + player - 1) % GameState.boardSize);
			for(int i = 0; i < GameState.boardSize; i++) {
				GameTile tile = node.state.board.getTile(victoryRow, i);
				tile.value[player] = 0;
				calcShortestPath(tile,tile.value[player],player,victoryRow);
			}
		}
	}

	private void calcShortestPath(GameTile tile, int mVal, int player, int victoryRow) {
		
//		System.out.println("Node: (" + tile.row + "," + tile.col + ")");
//		if(tile.parent[player] != null) {
//			System.out.println("  Parent: (" + tile.parent[player].row + "," + tile.parent[player].col + ")");
//		}
		
//		if(player == 0)
//			printShortestPath(root, player);

//		if(tile.isOccupied()) {	
//			// Reached objective tile
//			if(tile.pawn.owningPlayer == player) {
//				return;
//			}
//		}
	
		for(GameTile neighbor : tile.neighbors) {
			if(!neighbor.equals(tile)) {
				if(neighbor.value[player] > (mVal + 1)) {
					neighbor.value[player] = (mVal + 1);
					neighbor.parent[player] = tile;
					tile.child[player] = neighbor;
					calcShortestPath(neighbor, neighbor.value[player], player, victoryRow);
				}
			}
		}
	}
	
	public ArrayList<GameStateNode> getNextGameStates(GameState current) {
		ArrayList<GameStateNode> toReturn = new ArrayList<GameStateNode>();
		
		// possible player moves
		GameTile pawnPos = current.pawns.get(current.currentPlayerIndex).getTile();
		int mRow = pawnPos.row;
		int mCol = pawnPos.col;
		// left
		if(current.canMove(mRow,mCol-1)) {
			toReturn.add(new GameStateNode(new GameState(current)));
			toReturn.get(toReturn.size()-1).state.movePawnTo(mRow, mCol-1);
			toReturn.get(toReturn.size()-1).state.nextTurn();
		}
		// up
		if(current.canMove(mRow-1,mCol)) {
			toReturn.add(new GameStateNode(new GameState(current)));
			toReturn.get(toReturn.size()-1).state.movePawnTo(mRow-1, mCol);
			toReturn.get(toReturn.size()-1).state.nextTurn();
		}
		// right
		if(current.canMove(mRow,mCol+1)) {
			toReturn.add(new GameStateNode(new GameState(current)));
			toReturn.get(toReturn.size()-1).state.movePawnTo(mRow, mCol+1);
			toReturn.get(toReturn.size()-1).state.nextTurn();
		}
		// down
		if(current.canMove(mRow+1,mCol)) {
			toReturn.add(new GameStateNode(new GameState(current)));
			toReturn.get(toReturn.size()-1).state.movePawnTo(mRow+1, mCol);
			toReturn.get(toReturn.size()-1).state.nextTurn();
		}
		
		for(ArrayList<GameTile> row : current.board.tiles) {
			for(GameTile tile : row) {
				if(current.canSetWall(tile.row, tile.col, true)) {
					toReturn.add(new GameStateNode(new GameState(current)));
					toReturn.get(toReturn.size()-1).state.setWall(tile.row, tile.col, true);
					toReturn.get(toReturn.size()-1).state.nextTurn();
				} else if(current.canSetWall(tile.row, tile.col, false)) {
					toReturn.add(new GameStateNode(new GameState(current)));
					toReturn.get(toReturn.size()-1).state.setWall(tile.row, tile.col, false);
					toReturn.get(toReturn.size()-1).state.nextTurn();
				}
			}
		}
		
		return toReturn;
	}
	
	private void printDepthFirst(GameStateNode current) {
		current.state.print();
		for(GameStateNode child : current.children) {
			child.state.print();
			printDepthFirst(child);
		}
	}
	
	private void printBreadthFirst(GameStateNode current) {
		LinkedList<GameStateNode> q = new LinkedList<GameStateNode>();
		q.add(current);
		while(!q.isEmpty()) {
			GameStateNode v = q.poll();
			v.state.print();
			q.addAll(v.children);
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

				if(tile.value[player] == Integer.MAX_VALUE) {
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
