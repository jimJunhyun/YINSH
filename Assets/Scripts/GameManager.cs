using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCommand
{
	public Vector3 pos;
	public Dictionary<Direction, Point> connecteds;

	public override int GetHashCode()
	{
		return System.HashCode.Combine(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
	}

	public override bool Equals(object obj)
	{
		return obj is GenerateCommand g && g.GetHashCode() == GetHashCode();
	}

	public GenerateCommand(Vector3 p)
	{
		pos = p;
		connecteds =new Dictionary<Direction, Point>();
	}
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

	public Ring selectedRing;
	public int winCount;
	public Point pointRef;
	public Ring ringRef;

	public List<Sprite> ringAndMarkers;

	internal bool placeMode;
	internal bool removeMode;
	internal bool placed;
	internal PointStatus nextPlacement;
	internal bool gaming = true;

	public const int PLACECOUNT = 4;
	public const int MARKER = 2;
	public const int MIRRORPERCENTAGE = 20;

	Point mapGenPt;
	ToDoText todo;

	HashSet<Point> youngestPoints = new HashSet<Point>();
	HashSet<Point> allPoints = new HashSet<Point>();
	
	List<KeyValuePair< Point, Direction>> mapGenCalls = new List<KeyValuePair<Point, Direction>>();
	List<GenerateCommand> collapsedGenCalls = new List<GenerateCommand>();

	List<Point> deleteCalls = new List<Point>();


	private void Awake()
	{
		instance = this;
		mapGenPt = GameObject.Find("MapGenPt").GetComponent<Point>();
		todo = GameObject.Find("InstructionText").GetComponent<ToDoText>();
		nextPlacement = PointStatus.Black;
		placeMode = false;
		placed = false;

	}

	private void Start()
	{
		DoGenerateMap(mapGenPt);

		DoPlaceMode();
	}

	public void AddWinCount()
	{
		winCount += 1;
		if(winCount >= 3)
		{
			todo.SetText("승리했습니다!\n\n게임 종료하기.");
			gaming = false;
		}
	}

	public void SelectRing(Ring rng)
	{
		selectedRing = rng;
		todo.SetText("연결된 지점을 눌러 <sprite=0> 를 이동하기.");
	}

	public void DeselectRing()
	{
		selectedRing = null;
		todo.SetText("승점 얻기.");
	}

	public void ExamineBoard(HashSet<Point> diffPoints) //이동이 다 끝나고 하기
	{
		//?????????????????????????????????????
		foreach (var diffPoint in diffPoints)
		{
			if(diffPoint.stat == PointStatus.None)
				continue;
			foreach (var item in diffPoint.connecteds)
			{
				if(item.Key >= Direction.S)
				{
					continue;
				}
				if (diffPoint.stat != PointStatus.None && (item.Value.stat == diffPoint.stat || (diffPoint.connecteds.ContainsKey(item.Key + 3) && diffPoint.connecteds[item.Key + 3].stat == diffPoint.stat)))
				{
					List<Point> dir1 = Examine(diffPoint, item.Key, new List<Point>());
					List<Point> dir2 = Examine(diffPoint, item.Key + 3, new List<Point>());
					if (dir1.Count + dir2.Count + 1 >= 5)
					{
						for (int i = 0; i < dir1.Count; i++)
						{
							dir1[i].SetMarker(PointStatus.None);
						}
						for (int i = 0; i < dir2.Count; i++)
						{
							dir2[i].SetMarker(PointStatus.None);
						}
						diffPoint.SetMarker(PointStatus.None);

						
						RemoveRing();
					}
				}
			}
		}
		
	}

	public void RemoveRing()
	{
		removeMode = true;
		todo.SetText("원하는 <sprite=0> 를 제거해 승점 얻기.");
	}

	public void EndRemoveRing()
	{
		removeMode = false;
		todo.SetText("승점 더 얻기.");
	}

	public List<Point> Examine(Point pt, Direction dir, List<Point> repCount)
	{
		if (pt.connecteds.ContainsKey(dir))
		{
			if(pt.connecteds[dir].stat == pt.stat)
			{
				repCount.Add(pt.connecteds[dir]);
				return Examine(pt.connecteds[dir], dir, repCount);
				
			}
		}
		return repCount;
		
	}

	public Direction FindDirectionTowards(Point from, Point to)
	{
		Point pt;
		for (Direction i = Direction.N; i < Direction.MAX; i++)
		{
			pt = (from);
			while (pt.connecteds.ContainsKey(i))
			{
				if (pt.connecteds[i].Equals(to))
				{
					return i;
				}
				pt = pt.connecteds[i];
			}
		}
		return Direction.MAX;
	}

	public void DoGenerateMap(Point source)
	{
		youngestPoints.Add(source);
		for (int i = 0; i < 5; i++)
		{
			foreach (var item in youngestPoints)
			{
				for (Direction d = Direction.N; d < Direction.MAX; d++)
				{
					if (!item.connecteds.ContainsKey(d))
					{
						AddPointGenCall(item, d);
					}
				}
			}
			youngestPoints.Clear();

			HandleGenCalls();
		}
		foreach (var item in youngestPoints)
		{
			if(item.connecteds.Count == 3)
			{
				AddPointDeleteCall(item);
			}
		}
		HandleDeleteCall();
	}

	public void AddPointGenCall(Point from, Direction to)
	{
		mapGenCalls.Add(new KeyValuePair<Point, Direction>(from, to));
	}

	public void AddPointDeleteCall(Point pt)
	{
		deleteCalls.Add(pt);
	}

	public void HandleDeleteCall()
	{
		foreach (var item in deleteCalls)
		{
			foreach (var point in allPoints)
			{
				if (point.connecteds.ContainsValue(item))
				{
					Dictionary<Direction, Point> newCon = new Dictionary<Direction, Point>(point.connecteds);
					foreach (var con in point.connecteds)
					{
						if (con.Value == item)
						{
							newCon.Remove(con.Key);
						}
					}
					point.connecteds = newCon;
				}
			}
			allPoints.Remove(item);
			Destroy(item.gameObject);
		}
	}

	public void HandleGenCalls()
	{
		Vector3 pt;
		Direction dir;
		foreach (var item in mapGenCalls)
		{
			pt = CalculatePosition(item.Key, item.Value);
			dir = item.Value;
			GenerateCommand cmd = new GenerateCommand(pt);
			if (collapsedGenCalls.Exists(x => x.GetHashCode() == cmd.GetHashCode()))
			{
				cmd = collapsedGenCalls.Find(x => x.GetHashCode() == cmd.GetHashCode());

				dir = Opposite(dir);

				cmd.connecteds.Add(dir, item.Key);

				//Debug.Log("같은것을 찾음  " + cmd.pos + "에" + dir + " : " + item.Key.transform.position  + " 와의 연결 추가.");
			}
			else
			{
				dir = Opposite(dir);

				cmd.connecteds.Add(dir, item.Key);

				collapsedGenCalls.Add(cmd);
				//Debug.Log("새로 넣음  " + cmd.pos + "에" + dir + " : " + item.Key.transform.position + " 와의 연결을 가짐.");
			}
			
		}
		
		mapGenCalls.Clear();
		foreach (var cmd in collapsedGenCalls)
		{
			
			Point generated = Instantiate<Point>(pointRef, cmd.pos, Quaternion.identity, mapGenPt.transform.parent);
			generated.connecteds = new Dictionary<Direction, Point>( cmd.connecteds);

			generated.mirrored = (Random.Range(0f, 1f) * 100f) <= MIRRORPERCENTAGE;

			foreach (var cons in cmd.connecteds)
			{
				cons.Value.connecteds.Add(Opposite(cons.Key), generated);
			}

			//Debug.Log("생성 : " + cmd.pos);


			foreach(var parCon in cmd.connecteds.Values)
			{
				foreach (var parConCon in parCon.connecteds.Values)
				{
					if (!cmd.connecteds.ContainsValue(parConCon))
					{
						Direction d = CalculateDirectionGenerative(parConCon.transform.position, cmd.pos);
						if (d != Direction.None)
						{
							generated.connecteds.Add(CalculateDirectionGenerative(generated.transform.position, parConCon.transform.position), parConCon);
							parConCon.connecteds.Add(CalculateDirectionGenerative(parConCon.transform.position, generated.transform.position), generated);
						}

						//if (collapsedGenCalls[collapsedGenCalls.Count - 1].GetHashCode() == cmd.GetHashCode())
						//{
						//	generated.connecteds.Add(CalculateDirectionGenerative(generated.transform.position, first.transform.position), first);
						//	first.connecteds.Add(CalculateDirectionGenerative(first.transform.position, generated.transform.position), generated);
						//}
					}
				}
			}

			//foreach (var cons in generated.connecteds)
			//{
			//	Debug.Log(cons.Key + " : " + cons.Value.transform.position + "과 연결.");
			//}


			youngestPoints.Add(generated);
			allPoints.Add(generated);
		}
		collapsedGenCalls.Clear();
	}

	public Direction CalculateDirectionGenerative(Vector2 from, Vector2 to)
	{
		Vector2 dir = to - from;

		if(Mathf.RoundToInt(dir.magnitude) > 2)
			return Direction.None;

		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		angle = Mathf.Round(angle);
		//Debug.Log(angle);
		for (Direction d = Direction.N; d < Direction.MAX; d++)
		{
			if(Mathf.Abs(Mathf.DeltaAngle(angle, (90 - 60 * ((int)d)))) <= 1)
			{
				//Debug.Log(d);
				return d;
			}

		}
		return Direction.None;
	}

	public Vector3 CalculatePosition(Point pt, Direction dir)
	{
		float angle;
		switch (dir)
		{
			case Direction.N:
				angle = 90;
				break;
			case Direction.NE:
				angle = 30;
				break;
			case Direction.SE:
				angle = -30;
				break;
			case Direction.S:
				angle = -90;
				break;
			case Direction.SW:
				angle = -150;
				break;
			case Direction.NW:
				angle = 150;
				break;
			default:
				angle = 0;
				break;
		}
		//Debug.Log(pt.transform.name + " 의 " + dir + " 방향은 : " + (pt.transform.position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * 2, Mathf.Sin(angle * Mathf.Deg2Rad) * 2)).ToString());
		return pt.transform.position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * 2, Mathf.Sin(angle * Mathf.Deg2Rad) * 2);
	}

	public bool PlaceRing(Point at)
	{
		if(at.occupyingRing != null)
			return false;

		Ring ring = Instantiate(ringRef,at.transform.position, Quaternion.identity, transform);
		ring.curPos = at;
		ring.Init(nextPlacement);
		at.occupyingRing = ring;
		return true;
	}

	public void DoPlaceMode()
	{
		placeMode = true;
		todo.SetText("원하는 곳을 눌러 <sprite=0> 를 배치하기.");
		StartCoroutine(PlaceMode());
	}

	IEnumerator PlaceMode()
	{
		int placeCount = 0;
		while(placeCount < PLACECOUNT)
		{
			yield return new WaitUntil(()=>placed);

			placed = false;
			placeCount += 1;
			if(nextPlacement == PointStatus.Black)
				nextPlacement = PointStatus.White;
			else
				nextPlacement = PointStatus.Black;
		}
		placeMode = false;
		todo.SetText("승점 얻기.");
	}

	public Direction Opposite(Direction dir)
	{
		if(dir >= Direction.S)
			return dir - 3;
		else
			return dir + 3;
	}
}
