package quoridor.logic;

public class GamePawn extends Object {

	public GameTile tile;
	public int player;
	
	public GamePawn(int mOwningPlayer, GameTile mPositionTile) {
		this.player = mOwningPlayer;
		this.tile = mPositionTile;
		this.tile.setPawn(this);
	}
	
	public void setTile(GameTile mTile) {
		this.tile = mTile;
	}
}
