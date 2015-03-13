package quoridor.logic;

public class GamePawn {

	protected GameTile positionTile;
	protected int owningPlayer;
	
	public GamePawn(int mOwningPlayer, GameTile mPositionTile) {
		this.owningPlayer = mOwningPlayer;
		this.positionTile = mPositionTile;
		this.positionTile.setPawn(this);
	}
	
	public void setPositionTile(GameTile mTile) {
		this.positionTile = mTile;
	}
	
	public GameTile getPositionTile() {
		return this.positionTile;
	}
	
	public int getOwningPlayer() {
		return this.owningPlayer;
	}
}
