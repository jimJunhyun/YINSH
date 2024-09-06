using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamMove : MonoBehaviour
{
	public float camSpd;
	public Vector2 clampAmt;
	Vector2 v;
	Vector2 clickPos;
    public void Move(InputAction.CallbackContext context)
	{
		v = context.ReadValue<Vector2>();
	}

	public void Zoom(InputAction.CallbackContext context)
	{
		
		Vector2 z = context.ReadValue<Vector2>();
		if(z.y > 0)
		{
			Camera.main.orthographicSize -= 1;
			Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1, 10);
		}
		if (z.y < 0)
		{
			Camera.main.orthographicSize += 1;
			Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1, 10);
		}
	}

	public void Click(InputAction.CallbackContext context)
	{
		if(!GameManager.instance.gaming)
			return;
		if (context.performed)
		{
			clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Collider2D[] res = Physics2D.OverlapCircleAll(clickPos, 1);
			List<IClickable> clickables = res.OrderBy(item => (clickPos - (Vector2)item.transform.position).magnitude).Select(t => t.GetComponent<IClickable>()).ToList();
			
			
			Point pt = null;
			Ring rg = null;
			MemoButton btn = null;
			MemoHex hx = null;
			if (clickables.Any(item => item is Point))
			{
				pt = clickables.First(item => item is Point) as Point;
			}
			if (clickables.Any(item => item is Ring))
			{
				rg = clickables.First(item => item is Ring) as Ring;
			}
			if (clickables.Any(item => item is MemoButton))
			{
				btn = clickables.First(item => item is MemoButton) as MemoButton;
			}
			if (clickables.Any(item => item is MemoHex))
			{
				hx = clickables.First(item => item is MemoHex) as MemoHex;
			}

			if(hx != null)
			{
				hx.OnClicked();
			}
			if(btn != null)
			{
				btn.OnClicked();
			}
			else if (rg != null && !GameManager.instance.placeMode)
			{
				rg.OnClicked();
			}
			else if (pt != null && !GameManager.instance.removeMode)
			{
				pt.OnClicked();
			}
		}
	}

	public void RClick(InputAction.CallbackContext context)
	{
		if (!GameManager.instance.gaming)
			return;
		if (context.performed)
		{
			clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Collider2D[] res = Physics2D.OverlapCircleAll(clickPos, 1);
			List<MemoHex> clickables = res.Select(t => t.GetComponent<MemoHex>()).Where(t => t != null).ToList();

			Debug.Log(clickables.Count);

			if(clickables.Count <= 0)
				return;

			for (int i = 0; i < clickables.Count; i++)
			{
				clickables[i].OnRClicked();
			}
		}
	}

	private void Update()
	{
		ClampMoveCam(v);
	}

	public void ClampMoveCam(Vector2 dir)
	{
		if (!GameManager.instance.gaming)
			return;
		Vector3 camPos = Camera.main.transform.position;
		camPos += (Vector3)dir * camSpd * Time.deltaTime;
		camPos.x = Mathf.Clamp(camPos.x, -clampAmt.x, clampAmt.x);
		camPos.y = Mathf.Clamp(camPos.y, -clampAmt.y, clampAmt.y);
		Camera.main.transform.position = camPos;
	}
}
