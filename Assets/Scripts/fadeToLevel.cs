using UnityEngine;
using System.Collections;

public class fadeToLevel : MonoBehaviour
{
    public string LevelName;
    public float StartOffset;
    public float TransisionSpeed;

    private float LerpTime;
    CanvasRenderer Renderer;


    void Start()
    {
        Renderer = GetComponent<CanvasRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        LerpTime += Time.deltaTime;

        float lerp = (StartOffset - LerpTime) / TransisionSpeed;
        Renderer.SetAlpha(lerp);

        if (lerp <= 0)
            Application.LoadLevel(LevelName);



    }
}
