using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BookshelfReader : MonoBehaviour {

    RectTransform mainCanvas;
    Image overlayBackground;
    Text overlayText;
    PlayerController controller;
    string[] bookTexts;

    bool canReadBookshelf;
	
	void Start () {
        canReadBookshelf = false;

        controller = GetComponent<PlayerController>();

        mainCanvas = GameObject.Find("Main Canvas").GetComponent<RectTransform>();
        overlayBackground = mainCanvas.FindChild("OverlayBackground").GetComponent<Image>();
        overlayText = overlayBackground.transform.FindChild("Text").GetComponent<Text>();

        var bookTextRaw = Resources.Load<TextAsset>("Levels/book_texts");
        bookTexts = bookTextRaw.text.Split('\n');

        overlayBackground.CrossFadeAlpha(0, 0, true);
        overlayText.CrossFadeAlpha(0, 0, true);
	}

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && canReadBookshelf)
        {
            var targetAlpha = controller.enabled ? 1 : 0;
            controller.enabled = !controller.enabled;

            if(!controller.enabled) {
                overlayText.text = bookTexts[Random.Range(0, bookTexts.Length)];
                MainCanvas.Instance.ShowHelpText(HelpText.ActionToClose);
            } else {
                MainCanvas.Instance.ShowHelpText(HelpText.ActionToRead);
            }

            overlayBackground.CrossFadeAlpha(targetAlpha, 0.33f, true);
            overlayText.CrossFadeAlpha(targetAlpha, 0.33f, true);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Bookshelf")) {
            canReadBookshelf = true;
            MainCanvas.Instance.ShowHelpText(HelpText.ActionToRead);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Bookshelf")) {
            canReadBookshelf = false;
            MainCanvas.Instance.HideHelpText(HelpText.ActionToRead);
        }
    }

}
