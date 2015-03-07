package quoridor.logic;

public class GameWall {
	
	/*
	 * IMPORTANT REFERENCE 
	 * 
	 * Position GameTile	[X]
	 * Other GameTiles		[ ]
	 * 
	 * =====================================================================
	 *        POSSIBLE WALL PLACEMENTS AND POSITION TILE REFERENCE
	 * =====================================================================
	 *									|| 
	 * [X] [ ] [ ]						||   [X]|[ ] [ ]
	 * -------							||	    | 
	 * [ ] [ ] [ ]						||	 [ ]|[ ] [ ]
	 *									|| 
	 * [ ] [ ] [ ]						||	 [ ] [ ] [ ]
	 * 									||
	 * Notice that, in an horizontal	||  Notice that, in a vertical wall,
	 * wall, the position tile (X)		||  the position tile is STILL in the
	 * refers to its top-left			||  top-left (Northwest) direction of
	 * direction (Northwest).			||  the wall.
	 * 									||
	 * =====================================================================
	 */

	protected GameTile positionTile;
	protected boolean horizontal;
	
	public GameWall(GameTile mPositionTile, boolean mIsHorizontal) {
		this.positionTile = mPositionTile;
		this.horizontal = mIsHorizontal;
	}
	
	public boolean isHorizontal() {
		return this.horizontal;
	}
	
	public boolean isVertical() {
		return !(this.horizontal);
	}
	
	public GameTile getPositionTile() {
		return this.positionTile;
	}

}
