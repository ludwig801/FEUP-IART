package quoridor.logic;

public class GamePlayer {

	protected int id;
	protected String name;
	
	public GamePlayer(int mId, String mName) {
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
