package quoridor.logic;

import java.util.ArrayList;

public class GameLogic {

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
		printGameState();
	}
	
	private void printGameState() {
		System.out.println("===========================");
		System.out.println("   Turn: " + players.get(currentPlayerIndex).getName());
		System.out.println("===========================");
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
