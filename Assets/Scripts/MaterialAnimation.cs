using UnityEngine;
using System.Collections;

public class MaterialAnimation : MonoBehaviour
{

    public Material materialToAnimate;
    public Texture2D[] textures;
    public float delay = 0.1f;
    private int i;

    void Start() {
        StartCoroutine(Animate(delay));
    }

    // Update is called once per frame
    IEnumerator Animate(float delay)
    {
        while (true)
        {
            materialToAnimate.mainTexture = textures[(i++) % textures.Length];
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            else
                yield return null;
        }
    }
}
