using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoHex : MonoBehaviour, IClickable
{
	public bool following;

	private void Update()
	{
		if (following)
		{
			transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
	}

	public void BeginFollow()
	{
		following = true;
	}

	public void OnClicked()
	{
		if (following)
		{
			following = false;
		}
	}

	public void OnRClicked()
	{
		Destroy(gameObject);
	}
}
