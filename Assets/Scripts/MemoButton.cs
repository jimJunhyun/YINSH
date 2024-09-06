using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoButton : MonoBehaviour, IClickable
{
	public MemoHex mirrorRef;
	public void OnClicked()
	{
		MemoHex hex = Instantiate(mirrorRef, Vector3.zero, mirrorRef.transform.rotation, GameManager.instance.transform);
		hex.BeginFollow();
	}
}
