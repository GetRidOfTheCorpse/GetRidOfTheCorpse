using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct AnimatedMaterial
{
    public Material material;
    public Texture2D[] textures;
}

public class MaterialAnimation : MonoBehaviour
{
    public AnimatedMaterial[] animatedMaterials;
    public float delay = 0.1f;
    public int i;

    void Start()
    {
        StartCoroutine(Animate(delay));
    }

    // Update is called once per frame
    IEnumerator Animate(float delay)
    {
        while (true)
        {
            foreach (var am in animatedMaterials)
            {
                if (am.material != null)
                {
                    am.material.mainTexture = am.textures[i % am.textures.Length];
                }
            }
            ++i;               
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            else
                yield return null;
        }
    }
}
