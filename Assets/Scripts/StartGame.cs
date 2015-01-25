using UnityEngine;
using System.Collections;

public class StartGame : MonoBehaviour
{

    public string nextLevel;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Application.LoadLevel(nextLevel);

        }
    }
}
