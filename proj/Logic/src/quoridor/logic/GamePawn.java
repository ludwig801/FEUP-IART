package quoridor.logic;

public class GamePawn {

	public GameTile tile;
	public int player;
	
	public GamePawn(int mOwningPlayer, GameTile mPositionTile) {
		this.player = mOwningPlayer;
		this.tile = mPositionTile;
		this.tile.addPawn();
	}
	
	public void setTile(GameTile mTile) {
		this.tile = mTile;
	}
}
