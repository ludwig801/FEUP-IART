package quoridor.logic;

public class GamePawn {

	protected GameTile positionTile;
	protected GamePlayer owningPlayer;
	
	public GamePawn(GamePlayer mOwningPlayer, GameTile mPositionTile) {
		this.owningPlayer = mOwningPlayer;
		this.positionTile = mPositionTile;
		this.positionTile.setPawn(this);
	}
	
	public void SetPositionTile(GameTile mTile) {
		this.positionTile = mTile;
	}
	
	public GamePlayer GetOwningPlayer() {
		return this.owningPlayer;
	}
	
	public int getPlayerId() {
		return owningPlayer.getId();
	}
}
