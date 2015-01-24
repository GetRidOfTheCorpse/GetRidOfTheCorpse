using UnityEngine;
using System.Collections;

public class BodyController : MonoBehaviour {
	private Transform playerTransform;
	private Transform myTransform;
	private SpriteRenderer renderer;
	// Use this for initialization
	void Start () {
		GameObject[] obj = GameObject.FindGameObjectsWithTag("Player");
		playerTransform = (Transform)(obj[0].GetComponent("Transform"));
		myTransform = (Transform) GetComponent("Transform");
		renderer = (SpriteRenderer) GetComponent("SpriteRenderer");
	}
	
	// Update is called once per frame
	void Update () {
		if(myTransform.position.y > playerTransform.position.y){
			renderer.sortingOrder = -1;
		}
		else{
			renderer.sortingOrder = 1;
		}
	}
}
