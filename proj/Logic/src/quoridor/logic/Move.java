package quoridor.logic;

public class Move {
	
	public static int MOVE_PAWN = 0;
	public static int SET_WALL = 1;
	
	public int type;
	public int row;
	public int col;
	public boolean horizontal;

	public Move(int pRow, int pCol) {
		type = MOVE_PAWN;
		row = pRow;
		col = pCol;
	}
	
	public Move(int pRow, int pCol, boolean pHorizontal) {
		type = SET_WALL;
		row = pRow;
		col = pCol;
		horizontal = pHorizontal;
	}
	
	public String toString() {
		if(type == MOVE_PAWN) {
			return "Move pawn by (" + row + "," + col + ")";
		}
		if(type == SET_WALL) {
			return "Set wall ate (" + row + "," + col + ") [" + horizontal + "]";
		}
		
		return "INVALID";
	}
}
