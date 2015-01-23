using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	private Vector2 movDirection = Vector2.zero;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Rigidbody2D body = (Rigidbody2D)GetComponent("Rigidbody2D");
		int xm = 0;
		int ym = 0;

		if(Input.GetKey("up")){
			ym = 1;
		}
		else if(Input.GetKey("down")){
			ym = -1;
		}

		if(Input.GetKey("left")){
			xm = -1;
		}
		else if(Input.GetKey("right")){
			xm = +1;
		}

		movDirection = new Vector2(xm, ym);
		movDirection *= 5;
		body.velocity = movDirection;
	}
}
