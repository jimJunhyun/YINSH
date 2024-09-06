using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour, IClickable
{
	public Point curPos;
	public PointStatus myStat;

	SpriteRenderer rend;

	public void Init(PointStatus stt)
	{
		rend = GetComponent<SpriteRenderer>();
		myStat = stt;

		rend.sprite = GameManager.instance.ringAndMarkers[((int)myStat)];
	}

	public void OnClicked()
	{
		if (GameManager.instance.removeMode)
		{
			curPos.occupyingRing = null;
			GameManager.instance.EndRemoveRing();
			Debug.Log("½ÂÁ¡È¹µæ!!!!!!!!!!!!!!!!");
			GameManager.instance.AddWinCount();
			Destroy(gameObject);
		}
		else
		{
			GameManager.instance.SelectRing(this);
		}
	}

	public void DoMove(Point target, Direction dir)
	{
		HashSet<Point> diffPt = new HashSet<Point>();
		Point pt = curPos;
		int moveCnt = 0;
		while(pt != target)
		{
			if(!pt.connecteds.ContainsKey(dir))
				return;
			pt = pt.connecteds[dir];
			
			moveCnt += 1;
		}

		for (int i = 0; i < moveCnt; i++)
		{
			diffPt.Add(curPos);
			if (WalkForward(dir))
			{
				dir = GameManager.instance.Opposite(dir);
			}
		}
		GameManager.instance.DeselectRing();

		GameManager.instance.ExamineBoard(diffPt);

	}

	public bool WalkForward(Direction dir)
	{
		if(curPos.stat == PointStatus.None)
		{
			curPos.SetMarker( myStat);
		}
		else
		{
			curPos.ReverseMarker();
		}
		if(!curPos.connecteds.ContainsKey(dir))
			return false;

		if (curPos.connecteds[dir].mirrored)
			return true;
		if(curPos.connecteds[dir].occupyingRing != null)
			return false;

		curPos.occupyingRing = null;
		curPos = curPos.connecteds[dir];
		curPos.occupyingRing = this;

		transform.position = curPos.transform.position;
		return false;
	}

	public HashSet<Point> GetMovablePoint()
	{
		HashSet<Point> res = new HashSet<Point>();

		Point pt;
		Direction d = Direction.N;
		bool jumping = false;

		while(d != Direction.MAX)
		{
			pt = (curPos);
			jumping = false;
			while (pt.connecteds.ContainsKey(d))
			{
				if(pt.connecteds[d].occupyingRing != null)
					break;

				if(pt.connecteds[d].stat != PointStatus.None)
					jumping = true;

				if (!jumping)
				{
					res.Add(pt.connecteds[d]);
				}
				else if (jumping && pt.connecteds[d].stat == PointStatus.None)
				{
					res.Add(pt.connecteds[d]);
					break;
				}
				pt = pt.connecteds[d];
				
			}

			++d;
		}
		return res;
	}

	
}
