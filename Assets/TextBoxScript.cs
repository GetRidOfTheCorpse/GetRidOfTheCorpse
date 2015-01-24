using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextBoxScript : MonoBehaviour {
	public string text;

	private Canvas canvas;
	private Text textBox;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D other){
		if(other.gameObject.tag == "Player" && canvas == null){
			canvas = gameObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			textBox = gameObject.AddComponent<Text>();

			Material newMaterialRef = Resources.Load<Material> ("3DTextCoolVetica");
         	Font myFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
 
         	textBox.font = myFont;
         	textBox.material = newMaterialRef;
         	textBox.text = text;
		}
	}
}
