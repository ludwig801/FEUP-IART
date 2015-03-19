package quoridor.logic;

import java.util.ArrayList;

public class GameStateNode extends GameState {
	
	public ArrayList<GameStateNode> children;
	public int alpha, beta;
	
	public GameStateNode(GameState mState) {
		super(mState);
		this.children = new ArrayList<GameStateNode>();
		
		// TODO: verify these values
		alpha = Integer.MIN_VALUE;
		beta = Integer.MAX_VALUE;
	}
}
