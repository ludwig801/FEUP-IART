package quoridor.tests;

import static org.junit.Assert.*;

import org.junit.Test;

import quoridor.logic.GameBoard;

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

}
