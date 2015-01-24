using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    public Camera Camera;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Camera.transform.position = this.transform.position;
        Camera.transform.position += new Vector3(0, 0, -10);
    }

    void FixedUpdate()
    {
        this.transform.position += Vector3.up * Input.GetAxis("Vertical");
        this.transform.position += Vector3.right * Input.GetAxis("Horizontal");

        Camera.transform.position = Vector3.Lerp(Camera.transform.position, this.transform.position, 0.001f);

    }
}
