package quoridor.logic;

import java.io.PrintStream;

public class GameState {
	
	public static final int boardSize = 9;
	public static final int boardBorder = boardSize - 1;
	public static final int numPawns = 2;
	public static final int numPlayers = 2;

	public GameBoard board;
	public GamePawn[] pawns;
	
	public int currentPlayer;
	
	public GameState() {
		board = new GameBoard(boardSize);
		
		pawns = new GamePawn[numPawns];
		pawns[0] = new GamePawn(0,board.getTile(0, boardSize / 2));
		pawns[1] = new GamePawn(1,board.getTile(boardSize - 1, boardSize / 2));
		
		currentPlayer = 0;
	}
	
	public GameState(GameState mGame) {
		board = new GameBoard(boardSize);
		currentPlayer = mGame.currentPlayer;
		for(GameTile[] row : mGame.board.tiles) {
			for(GameTile tile : row) {
				if(tile.isWalled()) {
					this.setWall(tile.row, tile.col, tile.wall.horizontal);
				}
			}
		}
		pawns = new GamePawn[numPawns];
		pawns[0] = new GamePawn(0, board.getTile(mGame.pawns[0].tile.row, mGame.pawns[0].tile.col));
		pawns[1] = new GamePawn(1, board.getTile(mGame.pawns[1].tile.row, mGame.pawns[1].tile.col));
		currentPlayer = mGame.currentPlayer;
	}

	public void movePawnTo(int mRow, int mCol) {
		movePawnTo(board.getTile(mRow, mCol));
	}
	
	private void movePawnTo(GameTile mTile) {
		GamePawn pawn = pawns[currentPlayer];
		GameTile pawnTile = pawn.tile;
		
		pawnTile.removePawn();
		movePawnImmediate(pawn,mTile);
	}
	
	private void movePawnImmediate(GamePawn mPawn, GameTile mTile) {
		mPawn.setTile(mTile);
		mTile.addPawn();		
	}
	
	public boolean canMove(int mRow, int mCol) {
		if(!board.isValidPosition(mRow, mCol)) {
			return false;
		}
		GamePawn pawn = pawns[currentPlayer];
		GameTile pawnTile = pawn.tile;
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
			
			if(mTileDown.parent[currentPlayer] == mTile) {
				mTileDown.parent[currentPlayer] = null;
				if(mTile.child[currentPlayer] == mTileDown) {
					mTile.child[currentPlayer] = null;
				}
			}
			
			if(mNeighborDown.parent[currentPlayer] == mNeighbor) {
				mNeighborDown.parent[currentPlayer] = null;
				if(mNeighbor.child[currentPlayer] == mNeighborDown) {
					mNeighbor.child[currentPlayer] = null;
				}
			}
			
			board.removeLink(mTile, mTileDown);
			board.removeLink(mNeighbor, mNeighborDown);
		} else {
			mNeighbor = board.getTile(mTile.row+1, mTile.col);
			GameTile mTileRight = board.getTile(mTile.row, mTile.col + 1);
			GameTile mNeighborRight = board.getTile(mNeighbor.row, mNeighbor.col + 1);
			
			if(mTileRight.parent[currentPlayer] == mTile) {
				mTileRight.parent[currentPlayer] = null;
				if(mTile.child[currentPlayer] == mTileRight) {
					mTile.child[currentPlayer] = null;
				}
			}
			
			if(mNeighborRight.parent[currentPlayer] == mNeighbor) {
				mNeighborRight.parent[currentPlayer] = null;
				if(mNeighbor.child[currentPlayer] == mNeighborRight) {
					mNeighbor.child[currentPlayer] = null;
				}
			}
			
			board.removeLink(mTile, mTileRight);
			board.removeLink(mNeighbor, mNeighborRight);
		}
		mTile.setWall(new GameWall(mHorizontal));
	}

	public boolean checkMoveDistance(GameTile tileFrom, GameTile tileTo) {
		if(Math.abs(tileTo.row - tileFrom.row) > 1) {
			return false;
		}
		
		if(Math.abs(tileTo.col - tileFrom.col) > 1) {
			return false;
		}
		
		return true;	
	}

	public void nextTurn() {
		currentPlayer++;
		currentPlayer %= 2;
	}

	public void print(PrintStream out) {
		out.println("===================================");
		out.println("          Turn: Player " + currentPlayer);
		out.println("===================================");
		out.println();
		
		final int size = board.getSize();
		final int border = size - 1;
		
		for(int row = 0; row < size; row++) {
			final int prevRow = row - 1;
			for(int col = 0; col < size; col++) {
				out.print("[");
				GameTile tile = board.getTile(row, col);
				if(tile.isOccupied()) {
					if(pawns[0].tile.equals(tile)) {
						out.print("0");
					} else {
						out.print("1");
					}
				} else {
					out.print(" ");
				}
				out.print("]");
				
				if(tile.isWalled() && tile.wall.isVertical()) {
					out.print("|");
				} else if (row > 0 && board.getTile(prevRow, col).isWalled() && board.getTile(prevRow, col).wall.isVertical()) {
					out.print("|");
				} else {
					out.print(" ");
				}
			}
			out.println();
			if(row < border) {
				for(int j = 0; j < size; j++) {
					if(board.getTile(row, j).isWalled()) {
						if(board.getTile(row, j).wall.isHorizontal()) {
							out.print("----");
						} else {
							out.print("   |");
						}
					}
					else if(j > 0 && board.getTile(row, j-1).isWalled() && board.getTile(row, j-1).wall.isHorizontal()) {
						out.print("--- ");
					} else {
						out.print("    ");
					}
				}
			}
			out.println();
		}
	}
}
