using UnityEngine;
using System.Collections;

public class TextBox : MonoBehaviour {
	public string text;
	private bool drawText;
	private string ctext;

	// Use this for initialization
	void Start () {
		drawText = false;
		ctext = "";
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnTriggerEnter2D(Collider2D other){
		if(other.gameObject.tag == "Player"){
			drawText = true;
		}
	}

	void OnGUI(){
		if(drawText){
			if(ctext != text){
				ctext = text.Substring(0, ctext.Length + 1);
			}
			GUI.color = Color.black;
			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.red;
			//GUI.fontSize = 20;
			GUI.Box(new Rect(Screen.width * 0.5f - 50f, Screen.height * 0.5f - 10f, 100f, 20f), ctext);
		}
	}
}
