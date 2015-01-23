using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	public GameObject body;
	private Transform myTransform;
	private Transform bodyTransform;
	private float rot;

	private bool coll = false;
	private bool pickup = false;
	private Vector2 movDirection;

	private int xm = 0;
	private int ym = 0;

	public bool hasBody(){
		return pickup;
	}

	// Use this for initialization
	void Start () {
		myTransform = ((Transform)GetComponent("Transform"));
		bodyTransform = ((Transform)body.GetComponent("Transform"));
	}
	
	// Update is called once per frame
	void Update () {
		if(body == null){
			GameObject[] objs = GameObject.FindGameObjectsWithTag("Body");
			body = objs[0];
		}
		Rigidbody2D rigidBody = (Rigidbody2D)GetComponent("Rigidbody2D");
		

		if(Input.GetKey("up")){
			ym = 1;
		}
		else if(Input.GetKey("down")){
			ym = -1;
		}
		else{
			ym = 0;
		}

		if(Input.GetKey("left")){
			xm = -1;
		}
		else if(Input.GetKey("right")){
			xm = +1;
		}
		else{
			xm = 0;
		}

		if(xm != 0 || ym != 0) rot = Mathf.Atan2(ym, xm)*Mathf.Rad2Deg;


		if(Input.GetKeyDown("space") && coll){
			pickup = true;
			((SpriteRenderer)body.GetComponent("SpriteRenderer")).enabled = false;
		}
		else if(Input.GetKeyUp("space")){
			pickup = false;
			((SpriteRenderer)body.GetComponent("SpriteRenderer")).enabled = true;
		}

		movDirection = new Vector2(xm, ym);
		myTransform.rotation = Quaternion.Euler(0, 0, rot);
		if(pickup)
			movDirection *= 2;
		else
			movDirection *= 5;

		rigidBody.velocity = movDirection;
		if(pickup){
			bodyTransform.position = myTransform.position;
		}
	}

	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Body"){
			coll = true;
		}
	}

	void OnTriggerExit2D(Collider2D other){
		if(other.tag == "Body"){
			coll = false;
		}
	}
}
