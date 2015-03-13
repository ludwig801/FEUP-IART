package quoridor.logic;

import java.util.ArrayList;

public class GameState {
	
	protected static final int boardSize = 9;

	protected GameBoard board;
	protected ArrayList<GamePawn> pawns;
	
	protected int currentPlayerIndex;
	
	public static void main(String[] args) {
		GameState state = new GameState();
		state.playGame();
	}
	
	public GameState() {
		board = new GameBoard(boardSize);
		
		pawns = new ArrayList<GamePawn>(2);
		pawns.add(new GamePawn(0,board.getTile(0, boardSize / 2)));
		pawns.add(new GamePawn(1,board.getTile(boardSize - 1, boardSize / 2)));
		currentPlayerIndex = 0;
	}
	
	public void playGame() {
		// TODO : game logic
	}
	
	public void movePawnTo(int mRow, int mCol) {
		movePawnTo(board.getTile(mRow, mCol));
	}
	
	private void movePawnTo(GameTile mTile) {
		GamePawn pawn = pawns.get(currentPlayerIndex);
		GameTile pawnTile = pawn.getPositionTile();
		
		if(!checkMoveDistance(pawnTile, mTile)) {
			return;
		}
		
		pawnTile.removePawn();
		pawn.setPositionTile(mTile);
		mTile.setPawn(pawn);
	}
	
	public void addWall(int mRow, int mCol, boolean mHorizontal) {
		GameTile neighbor;
		if(mHorizontal) {
			if(mCol < board.getBorder()) {
				neighbor = board.getTile(mRow, mCol+1);
				if(neighbor.isWalled()) {
					return;
				}
			} else {
				return;
			}
			
		} else {
			if(mRow < board.getBorder()) {
				neighbor = board.getTile(mRow+1, mCol);
				if(neighbor.isWalled()) {
					return;
				}
			} else {
				return;
			}
		}
		addWall(board.getTile(mRow, mCol),neighbor,mHorizontal);
	}
	
	private void addWall(GameTile mTile, GameTile mNeighbor, boolean mHorizontal) {
		if(!mTile.isWalled()) {
			if(mHorizontal) {
				GameTile mTileDown = board.getTile(mTile.row + 1, mTile.col);
				GameTile mNeighborDown = board.getTile(mNeighbor.row + 1, mNeighbor.col);
				
				mTile.removeNeighbor(mTileDown);
				mTileDown.removeNeighbor(mTile);
				mNeighbor.removeNeighbor(mNeighborDown);
				mNeighborDown.removeNeighbor(mNeighbor);
			} else {
				GameTile mTileRight = board.getTile(mTile.row, mTile.col + 1);
				GameTile mNeighborRight = board.getTile(mNeighbor.row, mNeighbor.col + 1);
				
				mTile.removeNeighbor(mTileRight);
				mTileRight.removeNeighbor(mTile);
				mNeighbor.removeNeighbor(mNeighborRight);
				mNeighborRight.removeNeighbor(mNeighbor);
			}
			mTile.setWall(new GameWall(mHorizontal));
		}
	}

	public boolean checkMoveDistance(GameTile tileFrom, GameTile tileTo) {
		if(Math.abs(tileTo.getRow() - tileFrom.getRow()) > 1) {
			return false;
		}
		
		if(Math.abs(tileTo.getCol() - tileFrom.getCol()) > 1) {
			return false;
		}
		
		return true;	
	}

	public void nextTurn() {
		currentPlayerIndex++;
		currentPlayerIndex %= 2;
	}
	
	public int getCurrentPlayerIndex() {	
		return this.currentPlayerIndex;
	}
	
	public GameBoard getGameBoard() {
		return this.board;
	}
	
	public ArrayList<GamePawn> getPawns() {
		return this.pawns;
	}
	
	public void print() {
		System.out.println("===================================");
		System.out.println("          Turn: Player " + currentPlayerIndex);
		System.out.println("===================================");
		System.out.println();
		
		final int size = board.getSize();
		final int border = size - 1;
		
		for(int row = 0; row < size; row++) {
			final int prevRow = row - 1;
			for(int col = 0; col < size; col++) {
				System.out.print("[");
				GameTile tile = board.getTile(row, col);
				if(tile.isOccupied()) {
					System.out.print(tile.getPawn().getOwningPlayer());
				} else {
					System.out.print(" ");
				}
				System.out.print("]");
				
				if(tile.isWalled() && tile.getWall().isVertical()) {
					System.out.print("|");
				} else if (row > 0 && board.getTile(prevRow, col).isWalled() && board.getTile(prevRow, col).getWall().isVertical()) {
					System.out.print("|");
				} else {
					System.out.print(" ");
				}
			}
			System.out.println();
			if(row < border) {
				for(int j = 0; j < size; j++) {
					if(board.getTile(row, j).isWalled()) {
						if(board.getTile(row, j).getWall().isHorizontal()) {
							System.out.print("----");
						} else {
							System.out.print("   |");
						}
						
					}
					else if(j > 0 && board.getTile(row, j-1).isWalled() && board.getTile(row, j-1).getWall().isHorizontal()) {
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
