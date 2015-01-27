using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

public class EnemyController : MonoBehaviour
{
    public float WalkSpeed = 1;
    public float coneAngle = 11;
    public float coneLength = 5;

    //WalkPath Vars
    private Vector3[] points;
    private MovementPattern movementPattern = new MovementPattern();
    private int startPoint;
    private int endPoint;
    private int iterator = 1;
    private bool isRing;
    private bool isStatic;
    private float longestDistance = float.MinValue;


    private float lerpTime;
    private float speedMultiplier = 0.1f;

    private GameObject player;
    private bool previouseCharacterDetected;
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

        detectedCharacters = new List<GameObject>();

        PathToPoints();
    }

    void FixedUpdate()
    {
        detectedCharacters.Clear();

        UpdatePositionAndRotation();
        ViewConeCharacterIntersection(player);
        ViewConeCharacterIntersection(deadBody);
        HandleDetectedCharacters();
        UpdateAnimation();

        movementPattern.DrawPattern();
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
            PlayerController.Instance.GotYou(true);
        }

        if (detectedCharacters.Contains(player))
        {
            if (!previouseCharacterDetected && characterDetected)
            {
                SoundManager.Instance.OneShot(SoundEffect.Hey, gameObject);
                previouseCharacterDetected = characterDetected;
            }


            bubble.enabled = true;
        }
        else
        {
            bubbleTime = 0;
            previouseCharacterDetected = false;
            bubble.enabled = false;
        }

        if (bubbleTime > bubbleTimeOut)
        {
            bubble.enabled = false;
        }

        bubbleTime += Time.fixedDeltaTime;



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
                PlayerController.Instance.GotYou();
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
            Vector2 pathStartPosition = pathNode.position;

            points = new Vector3[path.points.Length];


            for (int i = 0; i < path.points.Length; i++)
            {
                Vector2 pathPointPosition = path.points[i];

                points[i] = pathNode.position + new Vector3(pathPointPosition.x, pathPointPosition.y);
                Debug.DrawRay(pathNode.position + new Vector3(pathPointPosition.x, pathPointPosition.y), Vector3.up, Color.red, 10);

            }




            movementPattern.CreatePattern(pathNode.position, path.points);



            pathNode.gameObject.SetActive(false);


            GetNearestPoint();
            GetNearestLerpStartPosition();
            CalculateLongestDistance();

        }
        else
        {
            //Debug.LogWarning("Path for " + name + " " + pathName + " not found!");


            movementPattern.CreateStaticPosition();

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

        //float distanceToStartPoint = (points[startPoint] - this.transform.position).magnitude;

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



    private class MovementPattern
    {
        private WayPoint[] wayPoints;

        private float completeDistance;
        private float longestSegmentDistance;

        private bool isRing;
        private bool isStatic;

        public Vector2 UpdatePosition(Vector2 position)
        {

            return new Vector2();
        }

        public void DrawPattern()
        {
            foreach (WayPoint wayPoint in wayPoints)
            {
                Debug.DrawRay(wayPoint.Position, wayPoint.Next.Position - wayPoint.Position, Color.green);
            }
        }

        public void CreateStaticPosition()
        {
            isStatic = true;
        }

        public void CreatePattern(Vector2 startPosition, Vector2[] pathPoints)
        {
            InitializeWayPoints(startPosition, pathPoints);
            LinkWayPoints();
            CalculateDistances();
        }

        private void InitializeWayPoints(Vector2 startPosition, Vector2[] pathPoints)
        {
            wayPoints = new WayPoint[pathPoints.Length];

            for (int i = 0; i < wayPoints.Length; i++)
            {
                wayPoints[i] = new WayPoint();
                wayPoints[i].Position = startPosition + pathPoints[i];
            }

            if ((wayPoints[wayPoints.Length - 1].Position - wayPoints[0].Position).magnitude < 0.5f)
            {
                Array.Copy(wayPoints, wayPoints, wayPoints.Length - 1);
                isRing = true;
            }
        }

        private void LinkWayPoints()
        {
            for (int i = 0; i < wayPoints.Length; i++)
            {
                if (i + 1 == wayPoints.Length)
                {
                    if (isRing)
                        wayPoints[i].Next = wayPoints[0];
                    else
                        wayPoints[i].Next = wayPoints[i - 1];
                }
                else
                    wayPoints[i].Next = wayPoints[i + 1];


                if (i - 1 < 0)
                {
                    if (isRing)
                        wayPoints[i].Prev = wayPoints[wayPoints.Length - 1];
                    else
                        wayPoints[i].Prev = wayPoints[i + 1];
                }
                else
                    wayPoints[i].Prev = wayPoints[i - 1];
            }
        }

        private void CalculateDistances()
        {
            foreach (WayPoint wayPoint in wayPoints)
            {
                wayPoint.ToNextDistance = (wayPoint.Next.Position - wayPoint.Position).magnitude;
                wayPoint.ToPrevDistance = (wayPoint.Prev.Position - wayPoint.Position).magnitude;

                longestSegmentDistance = wayPoint.ToNextDistance > longestSegmentDistance
                                       ? wayPoint.ToNextDistance
                                       : longestSegmentDistance;

                completeDistance += wayPoint.ToPrevDistance;
                wayPoint.DistanceFromStart = completeDistance;
            }

            completeDistance -= wayPoints[0].ToPrevDistance;
            wayPoints[0].DistanceFromStart = 0;
        }


    }
    private class WayPoint
    {
        public WayPoint Next;
        public WayPoint Prev;

        public Vector2 Position;

        public float DistanceFromStart;
        public float ToNextDistance;
        public float ToPrevDistance;
    }
}
