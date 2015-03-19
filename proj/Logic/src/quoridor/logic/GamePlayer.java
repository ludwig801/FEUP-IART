package quoridor.logic;

public class GamePlayer {

	private int id;
	private String name;
	
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
