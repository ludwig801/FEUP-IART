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

	private void createGameTree(GameStateNode node, int depth) {
		LinkedList<Move> moves = new LinkedList<Move>();
		createGameTree(moves,node,depth);
	}
	
	private int createGameTree(LinkedList<Move> moves, GameStateNode node, int depth) {
		
		int val = 0;

		if(depth == 0) {
			node.state = new GameState(currentState);
			val = createGameState(node, node.state, moves);
			node.state = null;
			return val;
		}

		Move tmpMove;
		
		// Add vertical movements
		for(int mRow = -1; mRow <= 1; mRow += 2) {
			
			tmpMove = new Move(mRow, 0);
			node.addChild(node, tmpMove, !node.maxNode);
			moves.addLast(tmpMove);
			val = createGameTree(moves, node.getLastChild(), depth - 1);
			moves.removeLast();
			if(val >= 0){
				node.removeChild(node.getLastChild());
				tmpMove = null;
				return val - 1;
			}
		}
		
		// Add horizontal movements
		for(int mCol = -1; mCol <= 1; mCol += 2) {
			
			tmpMove = new Move(0, mCol);
			node.addChild(node, tmpMove, !node.maxNode);
			moves.addLast(tmpMove);
			val = createGameTree(moves, node.getLastChild(), depth - 1);
			moves.removeLast();
			if(val > 0){
				node.removeChild(node.getLastChild());
				tmpMove = null;
				return val - 1;
			}
		}
		
		// Add walls
		for(int mRow = 0; mRow < GameState.boardSize; mRow++) {
			for(int mCol = 0; mCol < GameState.boardSize; mCol++) {
				
				tmpMove = new Move(mRow,mCol,true);
				node.addChild(node, tmpMove, !node.maxNode);
				moves.addLast(tmpMove);
				val = createGameTree(moves, node.getLastChild(), depth - 1);
				moves.removeLast();
				if(val > 0){
					node.removeChild(node.getLastChild());
					tmpMove = null;
					return val - 1;
				}
				
				tmpMove = new Move(mRow,mCol,false);
				node.addChild(node, tmpMove, !node.maxNode);
				moves.addLast(tmpMove);
				val = createGameTree(moves, node.getLastChild(), depth - 1);
				moves.removeLast();
				if(val > 0){
					node.removeChild(node.getLastChild());
					tmpMove = null;
					return val - 1;
				}
			}
		}
		
		tmpMove = null;
		
		return 0;
	}

	private int getHeuristicValue(GameState state) {
		calcShortestPath(state);
		
//		printShortestPath(state);
		
		GameTile maxTile = state.pawns[0].tile;
		GameTile minTile = state.pawns[1].tile;
		
//		System.out.println("tiles:: " + maxTile.row + ", " + maxTile.col);
//		System.out.println("value: " + maxTile.value);
		
		return (maxTile.value /* + maxTile.row - minTile.row */);
	}

	private int createGameState(GameStateNode node, GameState state, LinkedList<Move> moves) {
		int plies = moves.size();
		for(Move move : moves) {
			//out.println(move.toString());
			GameTile pawnTile = state.pawns[state.currentPlayerIndex].tile; 
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

	public int minimaxAlphaBeta(GameStateNode node, int depth) {
		if(depth == 0 /* || isTerminalNode(node) */) {
			return node.heuristicValue;
		}
		if(node.maxNode) {
			node.heuristicValue = Integer.MIN_VALUE;
			for(GameStateNode child : node.children) {
				node.heuristicValue = Math.max(node.heuristicValue, minimaxAlphaBeta(child, depth - 1));
				node.alpha = Math.max(node.alpha, node.heuristicValue);
				if(node.beta <= node.alpha) {
					break;
				}
			}
			return node.heuristicValue;
		} else {
			node.heuristicValue = Integer.MAX_VALUE;
			for(GameStateNode child : node.children) {
				node.heuristicValue = Math.min(node.heuristicValue, minimaxAlphaBeta(child,depth-1));
				node.beta = Math.min(node.beta,node.heuristicValue);
				if(node.beta <= node.alpha) {
					break;
				}
			}
			return node.heuristicValue;
		}
	}
	
	public void personalMinimaxAlphaBeta(GameStateNode node, int depth) {
		LinkedList<Move> moves = new LinkedList<Move>();
		node.alpha = Integer.MIN_VALUE;
		node.beta = Integer.MAX_VALUE;
		personalMinimaxAlphaBeta(moves,node,depth);
	}
	
	public int personalMinimaxAlphaBeta(LinkedList<Move> moves, GameStateNode node, int depth) {
		
		int goBackVal = 0;
		
		if(depth == 0) {
			node.state = new GameState(currentState);
			goBackVal = createGameState(node, node.state, moves);
			if(goBackVal <= 0) {
				// Acceptance
				node.heuristicValue = getHeuristicValue(node.state);
				node.state.print(out);
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
		
		// Add horizontal movements
		for(int mCol = -1; mCol <= 1; mCol += 2) {
			
			tmpMove = new Move(0, mCol);
			moves.addLast(tmpMove);
			node.addChild(node, tmpMove, !node.maxNode);
			goBackVal = personalMinimaxAlphaBeta(moves, node.getLastChild(), depth - 1);
			moves.removeLast();
			
			if(goBackVal > 1) { 
				node.removeChild(node.getLastChild());
				return goBackVal - 1;
			} else if(goBackVal > 0) { // do not consider this move
				node.removeChild(node.getLastChild());
				goBackVal = 0;
			} else if(alphaBetaCut(node)) {
				return node.heuristicValue;
			}
		}
//		
//		// Add horizontal walls
//		for(int mRow = 0; mRow < GameState.boardSize; mRow++) {
//			for(int mCol = 0; mCol < GameState.boardSize; mCol++) {
//				
//				tmpMove = new Move(mRow,mCol,true);
//				node.addChild(node, tmpMove, !node.maxNode);
//				moves.addLast(tmpMove);
//				minimaxRes = personalMinimaxAlphaBeta(moves, node.getLastChild(), val, depth - 1);
//				moves.removeLast();
//				if(val > 0){
//					node.removeChild(node.getLastChild());
//					tmpMove = null;
//					val = val - 1;
//					return (node.maxNode ? Integer.MIN_VALUE : Integer.MAX_VALUE);
//				}
//				if(node.maxNode) {
//					node.heuristicValue = Math.max(node.heuristicValue, minimaxRes);
//					if(node.beta <= node.alpha) {
//						return node.heuristicValue;
//					}
//				} else {
//					node.heuristicValue = Math.max(node.heuristicValue, minimaxRes);
//					if(node.beta <= node.alpha) {
//						return node.heuristicValue;
//					}
//				}
//			}
//		}
//		
//		// Add vertical walls
//		for(int mRow = 0; mRow < GameState.boardSize; mRow++) {
//			for(int mCol = 0; mCol < GameState.boardSize; mCol++) {
//
//				tmpMove = new Move(mRow,mCol,false);
//				node.addChild(node, tmpMove, !node.maxNode);
//				moves.addLast(tmpMove);
//				minimaxRes = personalMinimaxAlphaBeta(moves, node.getLastChild(), val, depth - 1);
//				moves.removeLast();
//				if(val > 0){
//					node.removeChild(node.getLastChild());
//					tmpMove = null;
//					val = val - 1;
//					return (node.maxNode ? Integer.MIN_VALUE : Integer.MAX_VALUE);
//				}
//				if(node.maxNode) {
//					node.heuristicValue = Math.max(node.heuristicValue, minimaxRes);
//					if(node.beta <= node.alpha) {
//						return node.heuristicValue;
//					}
//				} else {
//					node.heuristicValue = Math.max(node.heuristicValue, minimaxRes);
//					if(node.beta <= node.alpha) {
//						return node.heuristicValue;
//					}
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
				tile.value = Integer.MAX_VALUE;
			}
		}
		
		int victoryRow = (int)((GameState.boardBorder) % GameState.boardSize);
		
		for(int i = 0; i < GameState.boardSize; i++) {
			GameTile tile = node.board.getTile(victoryRow, i);
			tile.value = 0;
			calcShortestPath(tile,tile.value,victoryRow);
		}
	}

	private void calcShortestPath(GameTile tile, int mVal, int victoryRow) {
	
		for(GameTile neighbor : tile.neighbors) {
			if(!neighbor.equals(tile)) {
				if(neighbor.value > (mVal + 1)) {
					neighbor.value = (mVal + 1);
					neighbor.parent = tile;
					tile.child = neighbor;
					calcShortestPath(neighbor, neighbor.value, victoryRow);
				}
			}
		}
	}
	
	private void printDepthFirst(GameStateNode current) {
		out.println("Heur. Val: " + current.heuristicValue);
		for(GameStateNode child : current.children) {
			printDepthFirst(child);
		}
	}
	
	private void printBreadthFirst(GameStateNode current) {
//		LinkedList<GameStateNode> q = new LinkedList<GameStateNode>();
//		q.add(current);
//		while(!q.isEmpty()) {
//			GameStateNode v = q.poll();
//			v.print();
//			q.addAll(v.children);
//		}
	}
	
	
	private int getGameTreeSize(GameStateNode node) {
		int x = 1;
		for(GameStateNode child : node.children) {
			x += getGameTreeSize(child);
		}
		return x;
	}
	
	public static void printShortestPath(GameState node) {
		System.out.println("---------------------------------------");
		System.out.println(" Shortest path lengths");
		System.out.println("---------------------------------------");
		
		final int size = node.board.getSize();
		final int border = size - 1;
		
		for(int row = 0; row < size; row++) {
			final int prevRow = row - 1;
			for(int col = 0; col < size; col++) {
				GameTile tile = node.board.getTile(row, col);

				if(tile.value == Integer.MAX_VALUE) {
					System.out.print(" " + "x" + "");
				} else {
					if(tile.value < 10) {
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
					GameTile child = tile.child;
					GameTile parent = tile.parent;
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
					GameTile child = tile.child;
					GameTile parent = tile.parent;
					
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
