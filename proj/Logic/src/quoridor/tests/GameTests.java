package quoridor.tests;

import static org.junit.Assert.*;

import org.junit.Test;

import quoridor.logic.GameBoard;
import quoridor.logic.GameState;

public class GameTests {

	@Test
	public void testBoardSize() {
		GameBoard board = new GameBoard(8);
		
		assertEquals(board.tiles.size(), 9);	
		assertEquals(board.tiles.get(0).size(), 9);
	}
	
	@Test
	public void testBoardLinks() {
		GameBoard board = new GameBoard((short) 9);
		
		// Test corners
		assertEquals(board.getTile(0, 0).neighbors.size(), 2);
		assertEquals(board.getTile(0, board.getSize()-1).neighbors.size(), 2);
		assertEquals(board.getTile(board.getSize()-1, 0).neighbors.size(), 2);
		assertEquals(board.getTile(board.getSize()-1, board.getSize()-1).neighbors.size(), 2);
		
		// Test limits
		for(int i = 1; i < board.getSize() - 1; i++) {
			assertEquals(board.getTile(i, 0).neighbors.size(), 3);
			assertEquals(board.getTile(i, board.getSize()-1).neighbors.size(), 3);
		}
		for(int i = 1; i < board.getSize() - 1; i++) {
			assertEquals(board.getTile(0, i).neighbors.size(), 3);
			assertEquals(board.getTile(board.getSize()-1,i).neighbors.size(), 3);
		}
		
		// Test center
		for(int i = 1; i < board.getSize() - 1; i++) {
			for(int j = 1; j < board.getSize() - 1; j++) {
				assertEquals(board.getTile(i, j).neighbors.size(), 4);
			}
		}
	}
	
	@Test
	public void testNextTurn() {
		GameState logic = new GameState();
		
		logic.nextTurn();
		
		assertEquals(logic.currentPlayerIndex, 1);
	}
	
	@Test
	public void testMovePawn() {
		GameState logic = new GameState();
		GameBoard board = logic.board;
		
		logic.movePawnTo(1, 4);
		
		assertFalse(board.getTile(0, 4).isOccupied());
		assertTrue(board.getTile(1, 4).isOccupied());
		
		assertEquals(board.getTile(1,4).pawn, logic.pawns.get(logic.currentPlayerIndex));
		
		logic.nextTurn();
		logic.movePawnTo(7, 4);

		assertFalse(board.getTile(8, 4).isOccupied());
		assertTrue(board.getTile(7, 4).isOccupied());
		
		assertEquals(board.getTile(7,4).pawn, logic.pawns.get(logic.currentPlayerIndex));
	}
	
	@Test
	public void testMovePawnToNonLegalPosition() {
		GameState logic = new GameState();
		GameBoard board = logic.board;
		
		assertFalse(logic.canMove(2, 4));
		
		assertEquals(board.getTile(0,4).pawn, logic.pawns.get(logic.currentPlayerIndex));
		
		logic.nextTurn();

		assertFalse(logic.canMove(4, 3));
		
		assertEquals(board.getTile(8,4).pawn, logic.pawns.get(logic.currentPlayerIndex));
	}
	
	@Test
	public void testAddingWalls() {
		GameState logic = new GameState();
		GameBoard board = logic.board;
		
		logic.setWall(0, 4, true);
		
		logic.setWall(7, 4, false);
		
		assertTrue(board.getTile(0, 4).isWalled());
		assertTrue(board.getTile(0, 4).wall.isHorizontal());
		
		assertTrue(board.getTile(7, 4).isWalled());
		assertTrue(board.getTile(7, 4).wall.isVertical());
	}
	
	@Test
	public void testAddingNonLegalWalls() {
		GameState logic = new GameState();
		GameBoard board = logic.board;
		
		assertTrue(logic.canSetWall(0, 4, true));
		
		logic.setWall(0, 4, true);
		
		assertTrue(board.getTile(0, 4).isWalled());
		assertTrue(board.getTile(0, 4).wall.isHorizontal());
		
		assertFalse(logic.canSetWall(0, 4, false));
		assertFalse(logic.canSetWall(0, 4, true));
		assertFalse(logic.canSetWall(0, 3, true));
	}

}
