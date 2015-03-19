package quoridor.logic;

public class GamePawn extends Object {

	protected GameTile tile;
	protected int player;
	
	public GamePawn(int mOwningPlayer, GameTile mPositionTile) {
		this.player = mOwningPlayer;
		this.tile = mPositionTile;
		this.tile.setPawn(this);
	}
	
	public void setTile(GameTile mTile) {
		this.tile = mTile;
	}
	
	public GameTile getTile() {
		return this.tile;
	}
	
	public int getOwningPlayer() {
		return this.player;
	}
}
