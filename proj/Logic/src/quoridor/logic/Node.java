package quoridor.logic;

public class Node {
	public int alpha, beta;
	public boolean maximizerNode;
	
	public int heuristicValue;
	
	public Node(boolean maxNode) {
		this.maximizerNode = maxNode;
		this.alpha = Integer.MIN_VALUE;
		this.beta = Integer.MAX_VALUE;
	}
	
	public Node(boolean maxNode, int pAlpha, int pBeta) {
		this.maximizerNode = maxNode;
		this.alpha = pAlpha;
		this.beta = pBeta;
	}
	
}
