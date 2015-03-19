package quoridor.logic;

import java.util.ArrayList;

public class GameState extends Object {
	
	protected static final short boardSize = 9;

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
	
	public GameState(GameState mGame) {
		this();
		currentPlayerIndex = mGame.currentPlayerIndex;
		for(ArrayList<GameTile> row : mGame.board.tiles) {
			for(GameTile tile : row) {
				if(tile.isWalled()) {
					this.setWall(tile.row, tile.col, tile.wall.horizontal);
				}
			}
		}
		for(int i = 0; i < mGame.pawns.size(); i++) {
			GamePawn p1 = mGame.pawns.get(i);
			GamePawn p2 = this.pawns.get(i);
			p2.tile.removePawn();
			GameTile mTile = this.board.getTile(p1.tile.row, p1.tile.col);
			movePawnImmediate(p2,mTile);
		}
	}

	public void playGame() {
		// TODO : game logic
	}
	
	public void movePawnTo(int mRow, int mCol) {
		movePawnTo(board.getTile(mRow, mCol));
	}
	
	private void movePawnTo(GameTile mTile) {
		GamePawn pawn = pawns.get(currentPlayerIndex);
		GameTile pawnTile = pawn.getTile();
		
		pawnTile.removePawn();
		movePawnImmediate(pawn,mTile);
	}
	
	private void movePawnImmediate(GamePawn mPawn, GameTile mTile) {
		mPawn.setTile(mTile);
		mTile.setPawn(mPawn);		
	}
	
	public boolean canMove(int mRow, int mCol) {
		if(!board.isValidPosition(mRow, mCol)) {
			return false;
		}
		GamePawn pawn = pawns.get(currentPlayerIndex);
		GameTile pawnTile = pawn.getTile();
		return canMove(pawnTile, board.getTile(mRow, mCol));
	}
	
	private boolean canMove(GameTile mPawnTile, GameTile mTile) {
		if(!checkMoveDistance(mPawnTile, mTile)) {
			return false;
		}
		
		if(!checkClearMove(mPawnTile, mTile)) {
			return false;
		}
		
		return true;
	}
	
	private boolean checkClearMove(GameTile pawnTile, GameTile mTile) {
		if(pawnTile.col > mTile.col) {
			if(mTile.isWalled() && mTile.wall.isVertical()) {
				return false;
			} else {
				int aboveRow = mTile.row - 1;
				int aboveCol = mTile.col - 1;
				if(board.isValidPosition(aboveRow,  aboveCol)) {
					GameTile aboveTile = board.getTile(aboveRow, aboveCol);
					if(aboveTile.isWalled() && aboveTile.wall.isVertical()) {
						return false;
					}
				}				
			}
		}
		if(pawnTile.col < mTile.col) {
			if(pawnTile.isWalled() && pawnTile.wall.isVertical()) {
				return false;
			} else {
				int aboveRow = mTile.row - 1;
				int aboveCol = mTile.col;
				if(board.isValidPosition(aboveRow,  aboveCol)) {
					GameTile aboveTile = board.getTile(aboveRow, aboveCol);
					if(aboveTile.isWalled() && aboveTile.wall.isVertical()) {
						return false;
					}
				}
			}
		}
		if(pawnTile.row > mTile.row) {
			if(mTile.isWalled() && mTile.wall.isHorizontal()) {
				return false;
			} else {
				int aboveRow = mTile.row - 1;
				int aboveCol = mTile.col - 1;
				if(board.isValidPosition(aboveRow,  aboveCol)) {
					GameTile aboveTile = board.getTile(aboveRow, aboveCol);
					if(aboveTile.isWalled() && aboveTile.wall.isHorizontal()) {
						return false;
					}
				}				
			}
		}
		if(pawnTile.row < mTile.row) {
			if(pawnTile.isWalled() && pawnTile.wall.isHorizontal()) {
				return false;
			} else {
				int leftRow = mTile.row;
				int leftCol = mTile.col - 1;
				if(board.isValidPosition(leftRow,  leftCol)) {
					GameTile leftTile = board.getTile(leftRow, leftCol);
					if(leftTile.isWalled() && leftTile.wall.isHorizontal()) {
						return false;
					}
				}				
			}
		}
		
		return true;
	}

	public boolean canSetWall(int mRow, int mCol, boolean mHorizontal) {
		return canSetWall(board.getTile(mRow, mCol),mHorizontal);
	}
	
	private boolean canSetWall(GameTile mTile, boolean mHorizontal) {
		if(mTile.isWalled()) {
			return false;
		}

		if(mTile.row == board.getBorder() || mTile.col == board.getBorder()) {
			return false;
		}
		
		int mRow = mTile.row;
		int mCol = mTile.col;
		GameTile neighbor;
		
		if(mHorizontal) {
			// The left tile cannot have an horizontal wall
			if(board.isValidPosition(mRow, mCol-1)) {
				neighbor = board.getTile(mRow, mCol-1);
				if(neighbor.isWalled() && neighbor.wall.isHorizontal()) {
					return false;
				}
			}
			// The right tile cannot have an horizontal wall
			if(board.isValidPosition(mRow, mCol+1)) {
				neighbor = board.getTile(mRow, mCol+1);
				if(neighbor.isWalled() && neighbor.wall.isHorizontal()) {
					return false;
				}
			}
			return true;
		} else { // Vertical
			// The upper tile cannot have an horizontal wall
			if(board.isValidPosition(mRow-1, mCol)) {
				neighbor = board.getTile(mRow-1, mCol);
				if(neighbor.isWalled() && neighbor.wall.isVertical()) {
					return false;
				}
			}
			// The down tile cannot have an horizontal wall
			if(board.isValidPosition(mRow+1, mCol)) {
				neighbor = board.getTile(mRow+1, mCol);
				if(neighbor.isWalled() && neighbor.wall.isVertical()) {
					return false;
				}
			}
			return true;
		}
	}
	
	public void setWall(int mRow, int mCol, boolean mHorizontal) {
		
		GameTile mTile = board.getTile(mRow, mCol);
		GameTile mNeighbor;

		if(mHorizontal) {
			mNeighbor = board.getTile(mTile.row, mTile.col+1);
			GameTile mTileDown = board.getTile(mTile.row + 1, mTile.col);
			GameTile mNeighborDown = board.getTile(mNeighbor.row + 1, mNeighbor.col);
			
			if(mTileDown.parent[currentPlayerIndex] == mTile) {
				mTileDown.parent[currentPlayerIndex] = null;
				if(mTile.child[currentPlayerIndex] == mTileDown) {
					mTile.child[currentPlayerIndex] = null;
				}
			}
			
			if(mNeighborDown.parent[currentPlayerIndex] == mNeighbor) {
				mNeighborDown.parent[currentPlayerIndex] = null;
				if(mNeighbor.child[currentPlayerIndex] == mNeighborDown) {
					mNeighbor.child[currentPlayerIndex] = null;
				}
			}
			
			mTile.removeNeighbor(mTileDown);
			mTileDown.removeNeighbor(mTile);
			mNeighbor.removeNeighbor(mNeighborDown);
			mNeighborDown.removeNeighbor(mNeighbor);
		} else {
			mNeighbor = board.getTile(mTile.row+1, mTile.col);
			GameTile mTileRight = board.getTile(mTile.row, mTile.col + 1);
			GameTile mNeighborRight = board.getTile(mNeighbor.row, mNeighbor.col + 1);
			
			if(mTileRight.parent[currentPlayerIndex] == mTile) {
				mTileRight.parent[currentPlayerIndex] = null;
				if(mTile.child[currentPlayerIndex] == mTileRight) {
					mTile.child[currentPlayerIndex] = null;
				}
			}
			
			if(mNeighborRight.parent[currentPlayerIndex] == mNeighbor) {
				mNeighborRight.parent[currentPlayerIndex] = null;
				if(mNeighbor.child[currentPlayerIndex] == mNeighborRight) {
					mNeighbor.child[currentPlayerIndex] = null;
				}
			}
			
			mTile.removeNeighbor(mTileRight);
			mTileRight.removeNeighbor(mTile);
			mNeighbor.removeNeighbor(mNeighborRight);
			mNeighborRight.removeNeighbor(mNeighbor);
		}
		mTile.setWall(new GameWall(mHorizontal));
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
