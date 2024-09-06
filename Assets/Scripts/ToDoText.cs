using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToDoText : MonoBehaviour
{
	TMP_Text text;

	private void Awake()
	{
		text = GetComponent<TMP_Text>();
	}

	public void SetText(string st)
	{
		text.text = st;
	}
}
