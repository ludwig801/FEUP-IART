package quoridor.tests;

import static org.junit.Assert.*;

import org.junit.Test;

import quoridor.logic.GameBoard;
import quoridor.logic.GameLogic;

public class GameTests {

	@Test
	public void testBoardSize() {
		GameBoard board = new GameBoard(8);
		
		assertEquals(board.getTiles().size(), 9);	
		assertEquals(board.getTiles().get(0).size(), 9);
	}
	
	@Test
	public void testBoardLinks() {
		GameBoard board = new GameBoard(9);
		
		// Test corners
		assertEquals(board.getTile(0, 0).getNeighbors().size(), 2);
		assertEquals(board.getTile(0, board.getSize()-1).getNeighbors().size(), 2);
		assertEquals(board.getTile(board.getSize()-1, 0).getNeighbors().size(), 2);
		assertEquals(board.getTile(board.getSize()-1, board.getSize()-1).getNeighbors().size(), 2);
		
		// Test limits
		for(int i = 1; i < board.getSize() - 1; i++) {
			assertEquals(board.getTile(i, 0).getNeighbors().size(), 3);
			assertEquals(board.getTile(i, board.getSize()-1).getNeighbors().size(), 3);
		}
		for(int i = 1; i < board.getSize() - 1; i++) {
			assertEquals(board.getTile(0, i).getNeighbors().size(), 3);
			assertEquals(board.getTile(board.getSize()-1,i).getNeighbors().size(), 3);
		}
		
		// Test center
		for(int i = 1; i < board.getSize() - 1; i++) {
			for(int j = 1; j < board.getSize() - 1; j++) {
				assertEquals(board.getTile(i, j).getNeighbors().size(), 4);
			}
		}
	}
	
	@Test
	public void testNextTurn() {
		GameLogic logic = new GameLogic();
		
		logic.nextTurn();
		
		assertEquals(logic.getCurrentPlayerIndex(), 1);
	}
	
	@Test
	public void testMovePawn() {
		GameLogic logic = new GameLogic();
		GameBoard board = logic.getGameBoard();
		
		logic.movePawnTo(1, 4);
		
		assertFalse(board.getTile(0, 4).isOccupied());
		assertTrue(board.getTile(1, 4).isOccupied());
		
		assertEquals(board.getTile(1,4).getPawn(), logic.getPawns().get(logic.getCurrentPlayerIndex()));
		
		logic.nextTurn();
		logic.movePawnTo(7, 4);

		assertFalse(board.getTile(8, 4).isOccupied());
		assertTrue(board.getTile(7, 4).isOccupied());
		
		assertEquals(board.getTile(7,4).getPawn(), logic.getPawns().get(logic.getCurrentPlayerIndex()));
		
		logic.printGameState();
	}
	
	@Test
	public void testMovePawnToNonLegalPosition() {
		GameLogic logic = new GameLogic();
		GameBoard board = logic.getGameBoard();
		
		logic.movePawnTo(2, 4);
		
		assertFalse(board.getTile(2, 4).isOccupied());
		assertTrue(board.getTile(0, 4).isOccupied());
		
		assertEquals(board.getTile(0,4).getPawn(), logic.getPawns().get(logic.getCurrentPlayerIndex()));
		
		logic.nextTurn();
		logic.movePawnTo(4, 3);

		assertFalse(board.getTile(4, 3).isOccupied());
		assertTrue(board.getTile(8, 4).isOccupied());
		
		assertEquals(board.getTile(8,4).getPawn(), logic.getPawns().get(logic.getCurrentPlayerIndex()));
		
		logic.printGameState();
	}

}
