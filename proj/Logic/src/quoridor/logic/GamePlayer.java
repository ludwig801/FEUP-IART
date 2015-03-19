package quoridor.logic;

public class GamePlayer {

	protected byte id;
	protected String name;
	
	public GamePlayer(byte mId, String mName) {
		this.id = mId;
		this.name = mName;
	}
	
	public String getName() {
		return this.name;
	}
	
	public int getId() {
		return this.id;
	}
}
