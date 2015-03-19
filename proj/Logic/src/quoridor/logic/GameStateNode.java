package quoridor.logic;

import java.util.ArrayList;

public class GameStateNode {
	
	public Move move;
	public ArrayList<GameStateNode> children;
	
	public int heuristicValue;
	public boolean maxNode;
	// For alpha-beta pruning
	public int alpha, beta;
	
	// Leaf nodes only
	public GameState state;
	
	public GameStateNode(boolean pMax) {
		this(null, null, pMax);
	}
	
	public GameStateNode(GameStateNode pParent, Move pMove, boolean pMax) {
		this.move = pMove;
		this.children = new ArrayList<GameStateNode>();
		
		// TODO: verify these values
		alpha = Integer.MIN_VALUE;
		beta = Integer.MAX_VALUE;
		
		this.maxNode = pMax;
	}
	
	public void addChild(GameStateNode pParent, Move pMove, boolean pMax) {
		this.children.add(new GameStateNode(pParent,pMove,pMax));
	}
	
	public void removeChild(GameStateNode pChild) {
		this.children.remove(pChild);
	}
	
	public GameStateNode getLastChild() {
		return this.children.get(children.size() - 1);
	}
}
