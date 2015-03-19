package quoridor.logic;

import java.util.ArrayList;

public class GameStateNode {
	public GameState state;
	public ArrayList<GameStateNode> children;
	public int alpha, beta;
	
	public GameStateNode(GameState mState) {
		this.children = new ArrayList<GameStateNode>();
		
		this.state = mState;
		
		// TODO: verify these values
		alpha = Integer.MIN_VALUE;
		beta = Integer.MAX_VALUE;
	}
}
