using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class BodyController : MonoBehaviour, Character
{
    private BoundingCircle BoundingCircle;

    private Transform playerTransform;
    private SpriteRenderer renderer;

    // Use this for initialization
    void Start()
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
        playerTransform = (Transform)(player[0].GetComponent("Transform"));

        renderer = (SpriteRenderer)GetComponent("SpriteRenderer");

        transform.FindChild("Blood").SetParent(transform.parent);

        BoundingCircle = new BoundingCircle(transform.position, 10);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y > playerTransform.position.y)
        {
            renderer.sortingOrder = -1;
        }
        else
        {
            renderer.sortingOrder = 2;
        }

        BoundingCircle.Update(transform.position);
        BoundingCircle.Draw();
    }

    public BoundingCircle GetBoundingCircle()
    {
        return BoundingCircle;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
