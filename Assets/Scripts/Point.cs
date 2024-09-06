using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
	None = -1,
	N,
	NE,
	SE,
	S,
	SW,
	NW,

	MAX
}

public enum PointStatus
{
	None = -1,
	Black,
	White,

}

[RequireComponent(typeof(CircleCollider2D))]
public class Point : MonoBehaviour, IClickable
{
	public Dictionary<Direction, Point> connecteds = new Dictionary<Direction, Point>();
	public PointStatus stat
	{
		get;
		private set;
	}

	public bool mirrored;

	public Ring occupyingRing;

	SpriteRenderer rend;

	private void Awake()
	{
		rend = GetComponent<SpriteRenderer>();
	}

	public Point()
	{
		stat = PointStatus.None;
		mirrored = false;
		occupyingRing = null;
		connecteds = new Dictionary<Direction, Point>();
	}

	//public Point(Point origin)
	//{
	//	pos = origin.transform.position;
	//	stat = origin.stat;
	//	mirrored = origin.mirrored;
	//	occupyingRing = origin.occupyingRing;
	//	connecteds = new Dictionary<Direction, Point>(origin.connecteds);
	//}

	public override int GetHashCode()
	{

		return System.HashCode.Combine(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
		
	}

	public override bool Equals(object other)
	{
		return other is Point pt && pt.GetHashCode() == GetHashCode();
	}

	public static bool operator==(Point lft, Point rht)
	{
		if(lft is null && rht is null)
			return true;
		else if(lft is null != rht is null)
			return  false;
		else
			return lft.Equals(rht);
	}

	public static bool operator !=(Point lft, Point rht)
	{
		return !(lft == rht);
	}

	public void SetMarker(PointStatus stt)
	{
		stat = stt;
		if(stt == PointStatus.None)
		{
			rend.sprite = null;
			rend.color = Color.clear;
		}
		else
		{
			rend.sprite = GameManager.instance.ringAndMarkers[GameManager.MARKER + ((int)stt)];
			rend.color = Color.white;
		}
	}

	public void ReverseMarker()
	{
		if(stat == PointStatus.None)
			return;

		if(stat == PointStatus.Black)
			stat = PointStatus.White;
		else if(stat == PointStatus.White)
			stat = PointStatus.Black;

		rend.sprite = GameManager.instance.ringAndMarkers[GameManager.MARKER + ((int)stat)];
		rend.color = Color.white;
	}

	public void OnClicked()
	{
		if (GameManager.instance.placeMode)
		{
			if (GameManager.instance.PlaceRing(this))
			{
				GameManager.instance.placed = true;
			}
		}
		else if(GameManager.instance.selectedRing != null)
		{
			if (GameManager.instance.selectedRing.GetMovablePoint().Contains(this))
			{
				Direction dir = GameManager.instance.FindDirectionTowards(GameManager.instance.selectedRing.curPos, this);
				if(dir != Direction.MAX)
				{
					GameManager.instance.selectedRing.DoMove(this, dir);
				}
			}
		}
	}
}
