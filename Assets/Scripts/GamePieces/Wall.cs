using UnityEngine;
using System.Collections;

public class Wall : Piece
{
	private bool horizontal;
	private bool tempLinkBreaker;
	private Transform wallTransform;

	public bool TempLinkBroken
	{
		get
		{
			return tempLinkBreaker;
		}
		set
		{
			tempLinkBreaker = value; 
		}
	}

	public Tile Tile
	{
		get
		{
			return this.tile;
		}
		
		set
		{
			tile = value;
			if(tile != null)
			{
				visible = true;
				transform.position = tile.transform.position + new Vector3(0f, 0.5f, 0f);
				tile.SetWall(this);
			}
			else 
			{
				visible = false;
			}
		}
	}

	public bool Horizontal
	{
		get
		{
			return horizontal;
		}

		set
		{
			horizontal = value;
			if(!horizontal)
			{
				wallTransform.rotation = Quaternion.Euler(0,90,0);
			}
			else
			{
				wallTransform.rotation = Quaternion.identity;
			}
		}
	}

	public bool Vertical
	{
		get
		{
			return !horizontal;
		}
		
		set
		{
			Horizontal = !value;
		}
	}

	public bool Free
	{
		get
		{
			return (tile == null);
		}
	}

	public void Init()
	{
		visible = false;
		horizontal = true;
		tempLinkBreaker = false;
		rendererComponent = transform.FindChild(Names.Wall).GetComponent<Renderer>();
		wallTransform = transform.FindChild(Names.Wall).transform;
	}
}
