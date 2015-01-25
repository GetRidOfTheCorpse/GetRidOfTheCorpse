using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    public float WalkSpeed = 1;
    public float coneAngle = 11;
    public float coneLength = 5;

    private Vector3[] points;

    private int startPoint;
    private int endPoint;
    private int iterator = 1;
    private bool isRing;
    private bool isStatic;

    private float longestDistance = float.MinValue;

    private float lerpTime;
    private float speedMultiplier = 0.1f;

    private GameObject player;
    private bool characterDetected;

    private GameObject deadBody;
    private List<GameObject> detectedCharacters;
    private SpriteRenderer bubble;
    private float bubbleTime;
    private float bubbleTimeOut = 0.5f;

    private SpriteRenderer ownSpriteRenderer;
    private Animator animator;
    private Vector3 direction = Vector3.zero;
    private float threshold = 0.5f;

    private Vector3 lookDirection;
    private Quaternion initialRotation;
    private Transform viewConeTransform;

    void Start()
    {
        PathToPoints();
        player = GameObject.FindGameObjectWithTag("Player");
        deadBody = GameObject.FindGameObjectWithTag("Body");
        bubble = (SpriteRenderer)transform.FindChild("bubble").GetComponent("SpriteRenderer");
        animator = (Animator)GetComponent("Animator");
        animator.speed = 0.5f;

        ownSpriteRenderer = (SpriteRenderer)GetComponent("SpriteRenderer");
        ownSpriteRenderer.color = Color.white;

        initialRotation = this.transform.rotation;

        viewConeTransform = transform.GetChild(0);
        viewConeTransform.localScale = new Vector3((coneAngle / 20f), coneLength * 0.25f, 1);

        Vector3 target = Vector3.up;
        this.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg - 90);


    }

    void FixedUpdate()
    {
        detectedCharacters = new List<GameObject>();

        UpdatePositionAndRotation();
        ViewConeCharacterIntersection(player);
        ViewConeCharacterIntersection(deadBody);
        HandleDetectedCharacters();
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        direction = viewConeTransform.up;
        direction.Normalize();

        if (characterDetected)
        {
            animator.SetBool("Moving", false);
        }
        else if (direction.y > threshold)
        {

            animator.SetInteger("Direction", 2);
            if (!isStatic)
                animator.SetBool("Moving", true);
        }
        else if (Mathf.Abs(direction.y) > threshold)
        {

            animator.SetInteger("Direction", 0);
            if (!isStatic)
                animator.SetBool("Moving", true);
        }
        else
        {
            if (direction.x > threshold)
            {

                animator.SetInteger("Direction", 3);
                if (!isStatic)
                    animator.SetBool("Moving", true);
            }

            else if (Mathf.Abs(direction.x) > threshold)
            {

                animator.SetInteger("Direction", 1);
                if (!isStatic)
                    animator.SetBool("Moving", true);
            }

        }

    }

    private void UpdatePositionAndRotation()
    {

        Vector3 startPosition = points[startPoint];
        Vector3 endPosition = points[endPoint];

        float distance = (endPosition - startPosition).magnitude;

        if (!characterDetected)
        {
            Vector3 previousPosition = this.transform.position;

            this.transform.position = Vector3.Lerp(points[startPoint], points[endPoint], lerpTime);

            direction = this.transform.position - previousPosition;

            lookDirection = direction.magnitude == 0 ? initialRotation * this.transform.up : direction;
            lookDirection *= -1;
            lookDirection.Normalize();

            viewConeTransform.rotation = Quaternion.FromToRotation(Vector3.up, lookDirection * -coneLength);

            lerpTime += Time.fixedDeltaTime * (10 / distance) * WalkSpeed * speedMultiplier;
        }


        if (lerpTime > 1)
        {
            lerpTime -= 1;
            startPoint = endPoint;

            if (endPoint == points.Length - 1 && isRing)
                endPoint = 0;

            else if (endPoint == points.Length - 1 && !isRing)
                iterator = -1;

            else if (endPoint == 0)
                iterator = 1;

            endPoint += iterator;
        }

    }

    private void HandleDetectedCharacters()
    {
        characterDetected = detectedCharacters.Count > 0;

        if (detectedCharacters.Contains(deadBody))
        {
            Application.LoadLevel("LoseScreen");
        }
        else if (detectedCharacters.Contains(player))
        {
            if (bubbleTime < bubbleTimeOut)
            {
                bubbleTime += Time.fixedDeltaTime;
                bubble.enabled = true;
            }
            else bubble.enabled = false;
        }
        else bubbleTime = 0;




    }

    private void ViewConeCharacterIntersection(GameObject character)
    {
        Vector3 toCharacter = character.transform.position - this.transform.position;

        Debug.DrawRay(transform.position, lookDirection * -coneLength, Color.red);

        Debug.DrawRay(transform.position, Quaternion.AngleAxis(-coneAngle, Vector3.forward) * lookDirection * -coneLength, Color.blue);
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(coneAngle, Vector3.forward) * lookDirection * -coneLength, Color.blue);


        if (toCharacter.magnitude < coneLength)
        {
            toCharacter.Normalize();

            Vector3 coneArea = Vector3.Cross(lookDirection, toCharacter);
            bool lookingTowardsPlayer = toCharacter.magnitude > (toCharacter + lookDirection).magnitude;

            if (coneArea.magnitude < coneAngle / 45f && lookingTowardsPlayer)
            {
                detectedCharacters.Add(character);
                character.SendMessage("GotYou", SendMessageOptions.DontRequireReceiver);

                //SoundManager.Instance.OneShot(SoundEffect.Hey, gameObject);
            }

        }

    }

    private void PathToPoints()
    {
        string[] name = this.name.Split('_');
        string pathName = name[name.Length - 2] + "_" + name[name.Length - 1] + "Path";

        Transform pathNode = transform.parent.Find(pathName);

        if (pathNode != null)
        {
            EdgeCollider2D path = pathNode.GetComponent<EdgeCollider2D>();

            points = new Vector3[path.points.Length];
            int i = 0;

            foreach (Vector2 point in path.points)
            {
                points[i++] = pathNode.position + new Vector3(point.x, point.y);
                Debug.DrawRay(pathNode.position + new Vector3(point.x, point.y), Vector3.up, Color.red, 10);
            }

            pathNode.gameObject.SetActive(false);

            if ((points[points.Length - 1] - points[0]).magnitude < 1)
            {
                points[points.Length - 1] = points[0];
                isRing = true;
            }

            GetNearestPoint();
            GetNearestLerpStartPosition();
            CalculateLongestDistance();
        }
        else
        {
            Debug.LogWarning("Path for " + name + " " + pathName + " not found!");

            points = new Vector3[] { this.transform.position, this.transform.position };
            isStatic = true;
        }


    }

    private void GetNearestPoint()
    {
        Dictionary<int, Vector3> sortedPositions = new Dictionary<int, Vector3>();

        for (int i = 0; i < points.Length; i++)
        {
            sortedPositions.Add(i, points[i]);
        }

        sortedPositions = sortedPositions.OrderBy(item => (item.Value - this.transform.position).sqrMagnitude).ToDictionary(item => item.Key, item => item.Value);

        startPoint = sortedPositions.ElementAt(0).Key;


        int indexToPointBefore = startPoint - 1 < 0 ? points.Length - 2 : startPoint - 1;
        int indexToPointAfter = startPoint + 1 > points.Length - 1 ? 1 : startPoint + 1;

        float distanceToPointBefore = (this.transform.position - points[indexToPointBefore]).magnitude;
        float distanceToPointAfter = (points[indexToPointAfter] - this.transform.position).magnitude;

        float distanceStartToPointBefore = (points[indexToPointBefore] - points[startPoint]).magnitude;
        float distanceStartToPointAfter = (points[indexToPointAfter] - points[startPoint]).magnitude;

        float distanceToStartPoint = (points[startPoint] - this.transform.position).magnitude;

        if (distanceToPointBefore / distanceStartToPointBefore < distanceToPointAfter / distanceStartToPointAfter)
            startPoint = indexToPointBefore;


        if (startPoint == points.Length - 1 && !isRing)
            iterator = -1;

        if (startPoint == points.Length - 1 && isRing)
            startPoint = 0;


        endPoint = startPoint + iterator;


    }

    private void GetNearestLerpStartPosition()
    {
        float toStartPosition = (points[startPoint] - this.transform.position).magnitude;
        float wholeDistance = (points[endPoint] - points[startPoint]).magnitude;

        lerpTime = toStartPosition / wholeDistance;
    }

    private void CalculateLongestDistance()
    {
        for (int i = 0; i < points.Length - 1; i++)
        {
            float newDistance = (points[i + 1] - points[i]).magnitude;
            longestDistance = longestDistance < newDistance ? newDistance : longestDistance;
        }
    }

}
