package quoridor.logic;

public class GameWall extends Object {

	protected boolean horizontal;
	
	public GameWall(boolean mIsHorizontal) {
		this.horizontal = mIsHorizontal;
	}
	
	public boolean isHorizontal() {
		return this.horizontal;
	}
	
	public boolean isVertical() {
		return !(this.horizontal);
	}
}
