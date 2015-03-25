package quoridor.logic;

import java.io.FileNotFoundException;
import java.io.PrintStream;
import java.util.LinkedList;
import java.util.concurrent.TimeUnit;

public class GameAI {

	static int depth = 4;
	
	PrintStream out;
	
	private static GameState currentState;
	
	private static Node root;
	
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
		
		System.out.println("======= MINIMAX ======");
		root = new Node(true);
		minimaxAlphaBeta(root, depth);
		
		System.out.println("Elapsed: " + TimeUnit.NANOSECONDS.toMillis(timer.getParcial()) + " milliseconds");
	}

	private int getHeuristicValue(GameState state) {
		calcShortestPath(state);
		
		return (state.pawns[1].tile.value[1] - state.pawns[0].tile.value[0]);
	}
	
	private void minimaxAlphaBeta(Node node, int depth) {
		minimaxAlphaBeta(currentState,node,depth);
	}
	
	private void minimaxAlphaBeta(GameState state, Node node, int depth) {

		if(depth == 0) { // leaf node
			node.heuristicValue = this.getHeuristicValue(state);
			//out.println("Heuristic value: " + node.heuristicValue);
			//state.print(out);
			return;
		}
		
		Node child = new Node(!node.maximizerNode);
		
		LinkedList<Move> moves = getPossibleMoves(state);
		
		for(Move move : moves) {
			state.playMove(move);
			state.nextTurn();
			
			child.alpha = node.alpha;
			child.beta = node.beta;
			
			minimaxAlphaBeta(state, child, depth - 1);
			
			state.nextTurn();
			state.undoMove();
			
			if(alphaBetaCut(node, child)) {
				break;
			}
		}
		
		moves = null;
		child = null;
	}
	
	private LinkedList<Move> getPossibleMoves(GameState state) {
		LinkedList<Move> moves = new LinkedList<Move>();
		
		// pawn movements
		for(GameTile tile : state.pawns[state.currentPlayer].tile.neighbors) {
			if(state.canMove(tile.row, tile.col)) {
				moves.add(new Move(tile.row, tile.col));				
			}
		}
		
		// wall placements
		for(int row = 0; row < GameState.boardSize; row++) {
			for(int col = 0; col < GameState.boardSize; col++) {
				if(state.canSetWall(row, col, true)) {
					moves.add(new Move(row, col, true));
				}
				if(state.canSetWall(row, col, false)) {
					moves.add(new Move(row, col, false));
				}
			}
		}
		
		return moves;
	}

	private boolean alphaBetaCut(Node node, Node child) {
		
		if(node.maximizerNode) {
			node.heuristicValue = Math.max(node.heuristicValue, child.heuristicValue);
			node.alpha = Math.max(node.alpha, node.heuristicValue);
			if(node.alpha >= node.beta) {
				return true;
			}
		} else {
			node.heuristicValue = Math.min(node.heuristicValue, child.heuristicValue);
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
		
		int victoryRow = GameState.boardBorder;
		
		for(int i = 0; i < GameState.boardSize; i++) {
			GameTile tile = node.board.getTile(victoryRow, i);
			tile.value[0] = 0;
			calcShortestPath(tile,tile.value[0],victoryRow,0);
		}
		
		victoryRow = 0;
		
		for(int i = 0; i < GameState.boardSize; i++) {
			GameTile tile = node.board.getTile(victoryRow, i);
			tile.value[1] = 0;
			calcShortestPath(tile,tile.value[1],victoryRow,1);
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
