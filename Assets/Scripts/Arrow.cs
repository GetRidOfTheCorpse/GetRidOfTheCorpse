using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    
    private Text text;
    private Image image;
    private bool inside = false;

    public bool showOnEnter;

    void Awake()
    {
        text = transform.FindChild("Canvas").FindChild("Text").GetComponent<Text>();
        image = transform.FindChild("Canvas").FindChild("Image").GetComponent<Image>();
    }

    public void SetHelpText(string newText)
    {
        if (text == null || image == null) Awake();
        text.text = newText;
    }

    void Update() {
        if (Input.GetButtonDown("Jump") && inside) {
            KillMyFuckingSelf();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inside = true;

            if (showOnEnter)
            {
                image.enabled = true;
                text.enabled = true;
            } else {
                KillMyFuckingSelf();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            KillMyFuckingSelf();
        }
    }

    void KillMyFuckingSelf() {
        StartCoroutine(FadeOutArrow());
    }

    IEnumerator FadeOutArrow()
    {
        while (image.color.a > 0)
        {
            var color = image.color;
            color.a -= Time.deltaTime * 2;
            image.color = color;
            text.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }

}
