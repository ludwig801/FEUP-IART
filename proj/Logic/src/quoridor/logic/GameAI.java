package quoridor.logic;

import java.io.FileNotFoundException;
import java.io.PrintStream;
import java.util.LinkedList;
import java.util.concurrent.TimeUnit;

public class GameAI {

	static int depth = 2;
	
	PrintStream out;
	
	private static GameState currentState;
	
	private static GameStateNode root;
	
	public static void main(String[] args) {
		GameState testState = new GameState();
		new GameAI(testState);
	}
	
	public GameAI(GameState mInitialState) {
		try {
			out = new PrintStream("out.txt");
		} catch (FileNotFoundException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		Timer timer = new Timer();
		
		timer.start();
		currentState = mInitialState;
		root = new GameStateNode(true);

		System.out.println("====== GAME TREE =====");
		//createGameTree(root, depth);
		//minimaxAlphaBeta(root, depth);
		personalMinimaxAlphaBeta(root, depth);
		System.out.println("Elapsed: " + TimeUnit.NANOSECONDS.toMillis(timer.getParcial()) + " milliseconds");
		
		//printDepthFirst(root);
		System.out.println("====== TREE SIZE =====");
		System.out.println(getGameTreeSize(root));
	}

	private int getHeuristicValue(GameState state) {
		calcShortestPath(state);
		
//		printShortestPath(state);
		
		GameTile maxTile = state.pawns[0].tile;
		GameTile minTile = state.pawns[1].tile;
		
//		System.out.println("tiles:: " + maxTile.row + ", " + maxTile.col);
//		System.out.println("value: " + maxTile.value);
		
		return (maxTile.value[0] - (GameState.boardBorder - minTile.value[1]));
	}
	
	private int createGameState(GameStateNode node, GameState state, LinkedList<Move> moves) {
		int plies = moves.size();
		for(Move move : moves) {
			//out.println(move.toString());
			GameTile pawnTile = state.pawns[state.currentPlayer].tile; 
			if(move.type == Move.MOVE_PAWN) {
				if(state.canMove(pawnTile.row + move.row, pawnTile.col + move.col)) {
					state.movePawnTo(pawnTile.row + move.row, pawnTile.col + move.col);
				} else {
					break;
				}
			} else if(move.type == Move.SET_WALL) {
				if(state.canSetWall(move.row, move.col, move.horizontal)) {
					state.setWall(move.row, move.col, move.horizontal);
				} else {
					break;
				}
			}
			state.nextTurn();
			plies--;
		}
//		out.println("Plies: " + plies);
		return plies;
	}
	
	private void personalMinimaxAlphaBeta(GameStateNode node, int depth) {
		LinkedList<Move> moves = new LinkedList<Move>();
		node.alpha = Integer.MIN_VALUE;
		node.beta = Integer.MAX_VALUE;
		personalMinimaxAlphaBeta(moves,node,depth);
	}
	
	private int personalMinimaxAlphaBeta(LinkedList<Move> moves, GameStateNode node, int depth) {
		
		int goBackVal = 0;
		
		if(depth == 0) {
			node.state = new GameState(currentState);
			goBackVal = createGameState(node, node.state, moves);
			if(goBackVal <= 0) {
				// Acceptance
				node.heuristicValue = getHeuristicValue(node.state);
				node.state.print(out);
				out.println("H-Val: " + node.heuristicValue);
			}
			node.state = null;
			return goBackVal;
		}
		
		Move tmpMove;

		// Initialize minimax and alpha-beta pruning values
		node.heuristicValue = node.maxNode ? Integer.MIN_VALUE : Integer.MAX_VALUE;

		// Add vertical movements
		for(int mRow = -1; mRow <= 1; mRow += 2) {
			
			tmpMove = new Move(mRow, 0);
			moves.addLast(tmpMove);
			node.addChild(node, tmpMove, !node.maxNode);
			goBackVal = personalMinimaxAlphaBeta(moves, node.getLastChild(), depth - 1);
			moves.removeLast();
			
			if(goBackVal > 1) { 
				node.removeChild(node.getLastChild());
				return goBackVal - 1;
			} else if(goBackVal > 0) { // do not consider this move
				node.removeChild(node.getLastChild());
			} else if(alphaBetaCut(node)) {
				return node.heuristicValue;
			}
		}
		
//		// Add horizontal movements
//		for(int mCol = -1; mCol <= 1; mCol += 2) {
//			
//			tmpMove = new Move(0, mCol);
//			moves.addLast(tmpMove);
//			node.addChild(node, tmpMove, !node.maxNode);
//			goBackVal = personalMinimaxAlphaBeta(moves, node.getLastChild(), depth - 1);
//			moves.removeLast();
//			
//			if(goBackVal > 1) { 
//				node.removeChild(node.getLastChild());
//				return goBackVal - 1;
//			} else if(goBackVal > 0) { // do not consider this move
//				node.removeChild(node.getLastChild());
//				goBackVal = 0;
//			} else if(alphaBetaCut(node)) {
//				return node.heuristicValue;
//			}
//		}
//		
//		// Add horizontal walls
//		for(int mRow = 0; mRow < GameState.boardSize; mRow++) {
//			for(int mCol = 0; mCol < GameState.boardSize; mCol++) {
//				
//				tmpMove = new Move(mRow,mCol,true);
//				moves.addLast(tmpMove);
//				node.addChild(node, tmpMove, !node.maxNode);
//				goBackVal = personalMinimaxAlphaBeta(moves, node.getLastChild(), depth - 1);
//				moves.removeLast();
//				
//				if(goBackVal > 1) { 
//					node.removeChild(node.getLastChild());
//					return goBackVal - 1;
//				} else if(goBackVal > 0) { // do not consider this move
//					node.removeChild(node.getLastChild());
//					goBackVal = 0;
//				} else if(alphaBetaCut(node)) {
//					return node.heuristicValue;
//				}
//			}
//		}
//		
//		// Add vertical walls
//		for(int mRow = 0; mRow < GameState.boardSize; mRow++) {
//			for(int mCol = 0; mCol < GameState.boardSize; mCol++) {
//				
//				tmpMove = new Move(mRow,mCol,false);
//				moves.addLast(tmpMove);
//				node.addChild(node, tmpMove, !node.maxNode);
//				goBackVal = personalMinimaxAlphaBeta(moves, node.getLastChild(), depth - 1);
//				moves.removeLast();
//				
//				if(goBackVal > 1) { 
//					node.removeChild(node.getLastChild());
//					return goBackVal - 1;
//				} else if(goBackVal > 0) { // do not consider this move
//					node.removeChild(node.getLastChild());
//					goBackVal = 0;
//				} else if(alphaBetaCut(node)) {
//					return node.heuristicValue;
//				}
//			}
//		}
		
		tmpMove = null;
		
		return goBackVal;
	}

	private boolean alphaBetaCut(GameStateNode node) {
		int childHeuristic = node.getLastChild().heuristicValue;
		//node.removeChild(node.getLastChild());
		
		if(node.maxNode) {
			node.heuristicValue = Math.max(node.heuristicValue, childHeuristic);
			node.alpha = Math.max(node.alpha, node.heuristicValue);
			if(node.alpha >= node.beta) {
				return true;
			}
		} else {
			node.heuristicValue = Math.min(node.heuristicValue, childHeuristic);
			node.beta = Math.min(node.beta,node.heuristicValue);
			if(node.alpha >= node.beta) {
				return true;
			}
		}
		return false;
	}

	private void calcShortestPath(GameState node) {
		for(GameTile[] row : node.board.tiles) {
			for(GameTile tile : row) {
				tile.value[0] = Integer.MAX_VALUE;
				tile.value[1] = Integer.MAX_VALUE;
			}
		}
		
		int victoryRow = GameState.boardBorder % GameState.boardSize;
		
		for(int i = 0; i < GameState.boardSize; i++) {
			GameTile tile = node.board.getTile(victoryRow, i);
			tile.value[0] = 0;
			calcShortestPath(tile,tile.value[0],victoryRow,0);
		}
	}

	private void calcShortestPath(GameTile tile, int mVal, int victoryRow, int player) {
	
		for(GameTile neighbor : tile.neighbors) {
			if(!neighbor.equals(tile)) {
				if(neighbor.value[player] > (mVal + 1)) {
					neighbor.value[player] = (mVal + 1);
					neighbor.parent[player] = tile;
					tile.child[player] = neighbor;
					calcShortestPath(neighbor, neighbor.value[player], victoryRow, player);
				}
			}
		}
	}

	private int getGameTreeSize(GameStateNode node) {
		int x = 1;
		for(GameStateNode child : node.children) {
			x += getGameTreeSize(child);
		}
		return x;
	}
	
	public static void printShortestPath(GameState node, int player) {
		System.out.println("---------------------------------------");
		System.out.println(" Shortest path lengths");
		System.out.println("---------------------------------------");
		
		final int size = node.board.getSize();
		final int border = size - 1;
		
		for(int row = 0; row < size; row++) {
			final int prevRow = row - 1;
			for(int col = 0; col < size; col++) {
				GameTile tile = node.board.getTile(row, col);

				if(tile.value[player] == Integer.MAX_VALUE) {
					System.out.print(" " + "x" + "");
				} else {
					if(tile.value[player] < 10) {
						System.out.print(" ");
					}
					System.out.print("" + tile.value + "");
				}

				if(tile.isWalled() && tile.wall.isVertical()) {
					System.out.print(" |");
				} else if (row > 0 && node.board.getTile(prevRow, col).isWalled()
						&& node.board.getTile(prevRow, col).wall.isVertical()) {
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
					GameTile tile = node.board.getTile(row, col);
					GameTile child = tile.child[player];
					GameTile parent = tile.parent[player];
					
					if(tile.isWalled()) {
						if(tile.wall.isHorizontal()) {
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
						GameTile tileLeft = node.board.getTile(row, col-1);
						if(tileLeft.isWalled()) {
							if(tileLeft.wall.isHorizontal()) {
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
