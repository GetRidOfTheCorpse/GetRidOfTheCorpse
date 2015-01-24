using UnityEngine;
using System.Collections;
using System.Linq;


public class SplineController : MonoBehaviour
{
    public GameObject[] Points;
    public float Speed = 1;
    public float coneAngle;

    private int startPoint;
    private int endPoint;
    private int iterator = 1;

    private float speedMultiplier = 0.1f;

    private float longestDistance = float.MinValue;

    private float lerpTime;

    private GameObject player;

    void Start()
    {
        GetNearesPoint();
        CalculateLongestDistance();
        player = GameObject.Find("Player");

    }

    void FixedUpdate()
    {
        UpdatePositionAndRotation();
        ViewConePlayerIntersection();
    }

    private void UpdatePositionAndRotation()
    {
        Vector3 startPosition = Points[startPoint].transform.position;
        Vector3 endPosition = Points[endPoint].transform.position;

        this.transform.position = Vector3.Lerp(Points[startPoint].transform.position, Points[endPoint].transform.position, lerpTime);

        Vector3 target = Points[endPoint].transform.position - this.transform.position;

        Quaternion newRotation = Quaternion.Euler(0, 0, Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg - 90);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, newRotation, lerpTime);

        float distance = (endPosition - startPosition).magnitude;
        lerpTime += Time.fixedDeltaTime * (longestDistance / distance) * Speed * speedMultiplier;


        if (lerpTime > 1)
        {
            lerpTime -= 1;
            startPoint = endPoint;
            endPoint += iterator;
        }

        if (endPoint == Points.Length - 1)
        {
            iterator = -1;
        }
        if (endPoint == 0)
        {
            iterator = 1;
        }
    }

    private void ViewConePlayerIntersection()
    {
        Vector3 toPlayer = player.transform.position - this.transform.position;

        Vector3 up = -this.transform.up;
        up.Normalize();

        Debug.DrawRay(transform.position, up * -10, Color.red);

        Debug.DrawRay(transform.position, Quaternion.AngleAxis(-coneAngle, Vector3.forward) * up * -10, Color.blue);
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(coneAngle, Vector3.forward) * up * -10, Color.blue);


        if (toPlayer.magnitude < 10)
        {
            toPlayer.Normalize();
            up.Normalize();

            Vector3 coneArea = Vector3.Cross(up, toPlayer);
            bool lookingTowardsPlayer = toPlayer.sqrMagnitude > (toPlayer + up).magnitude;

            if (coneArea.magnitude < coneAngle / 45f && lookingTowardsPlayer)
            {
                Debug.Log("Gotcha");
            }

        }


    }

    private void GetNearesPoint()
    {
        float distance = float.MaxValue;
        for (int i = 0; i < Points.Length; i++)
        {
            distance = (Points[i].transform.position - this.transform.position).sqrMagnitude;
            float startDistance = (Points[startPoint].transform.position - this.transform.position).sqrMagnitude;

            if (distance < startDistance)
                startPoint = i;
        }

        if (startPoint == Points.Length - 1)
        {

            iterator = -1;
        }

        endPoint = startPoint + iterator;

    }

    private void CalculateLongestDistance()
    {
        for (int i = 0; i < Points.Length - 1; i++)
        {
            float newDistance = (Points[i + 1].transform.position - Points[i].transform.position).magnitude;
            longestDistance = longestDistance < newDistance ? newDistance : longestDistance;
        }
    }

}
