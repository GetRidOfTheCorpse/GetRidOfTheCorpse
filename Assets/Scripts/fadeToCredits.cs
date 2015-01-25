using UnityEngine;
using System.Collections;

public class fadeToCredits : MonoBehaviour
{
    public float startOffset;
    public float transisionSpeed;

    private float lerpTime;
    CanvasRenderer renderer;


    void Start()
    {
        renderer = GetComponent<CanvasRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        lerpTime += Time.deltaTime;

        float lerp = (startOffset - lerpTime) / transisionSpeed;
        renderer.SetAlpha(lerp);

        if (lerp <= 0)
            Application.LoadLevel("Credits");



    }
}
