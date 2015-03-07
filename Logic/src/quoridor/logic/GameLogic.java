package quoridor.logic;

import java.util.ArrayList;

public class GameLogic {
	
	protected static final int numPlayers = 2;
	protected static final int maxMoveDist = 1;

	protected GameBoard board;
	protected ArrayList<GamePlayer> players;
	protected ArrayList<GamePawn> pawns;
	
	protected int currentPlayerIndex;
	
	public static void main(String[] args) {
		GameLogic logic = new GameLogic();
		logic.playGame();
	}
	
	public GameLogic() {
		board = new GameBoard(9);
		
		players = new ArrayList<GamePlayer>(2);
		players.add(new GamePlayer(0,"Player 1"));
		players.add(new GamePlayer(1,"Player 2"));
		
		pawns = new ArrayList<GamePawn>(2);
		pawns.add(new GamePawn(players.get(0),board.getTile(0, 4)));
		pawns.add(new GamePawn(players.get(1),board.getTile(8, 4)));
		currentPlayerIndex = 0;
	}
	
	public void playGame() {
		// TODO : game logic
	}
	
	public void movePawnTo(int mRow, int mCol) {
		movePawnTo(board.getTile(mRow, mCol));
	}
	
	protected void movePawnTo(GameTile mTile) {
		GamePawn pawn = pawns.get(currentPlayerIndex);
		GameTile pawnTile = pawn.getPositionTile();
		
		if(checkMoveDistance(pawnTile, mTile)) {
			pawnTile.removePawn();
			pawn.setPositionTile(mTile);
			mTile.setPawn(pawn);
		}
	}

	public boolean checkMoveDistance(GameTile tileFrom, GameTile tileTo) {
		if(Math.abs(tileTo.getRow() - tileFrom.getRow()) > maxMoveDist) {
			return false;
		}
		
		if(Math.abs(tileTo.getCol() - tileFrom.getCol()) > maxMoveDist) {
			return false;
		}
		
		return true;	
	}

	public void nextTurn() {
		currentPlayerIndex++;
		currentPlayerIndex %= numPlayers;
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
	
	public void printGameState() {
		System.out.println("===================================");
		System.out.println("          Turn: " + players.get(currentPlayerIndex).getName());
		System.out.println("===================================");
		System.out.println();
		
		for(ArrayList<GameTile> line : board.getTiles()) {
			for(GameTile tile : line) {
				System.out.print("[");
				if(tile.isOccupied()) {
					System.out.print(tile.getPawn().getPlayerId());
				} else {
					System.out.print(" ");
				}
				System.out.print("] ");
			}
			System.out.println();
			for(int i = 0; i < board.getSize() * 4; i++) {
				System.out.print(" ");
			}
			System.out.println();
		}
	}
}
