using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;

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
        movementPattern.UpdateGuard(Time.fixedDeltaTime);

        transform.position = movementPattern.GetPosition();
        viewConeTransform.rotation = movementPattern.GetDirection();

        //Vector3 startPosition = points[startPoint];
        //Vector3 endPosition = points[endPoint];

        //float distance = (endPosition - startPosition).magnitude;

        //if (!characterDetected)
        //{
        //    Vector3 previousPosition = this.transform.position;

        //    this.transform.position = Vector3.Lerp(points[startPoint], points[endPoint], lerpTime);

        //    direction = this.transform.position - previousPosition;

        //    lookDirection = direction.magnitude == 0 ? initialRotation * this.transform.up : direction;
        //    lookDirection *= -1;
        //    lookDirection.Normalize();

        //    viewConeTransform.rotation = Quaternion.FromToRotation(Vector3.up, lookDirection * -coneLength);

        //    lerpTime += Time.fixedDeltaTime * (10 / distance) * WalkSpeed * speedMultiplier;
        //}


        //if (lerpTime > 1)
        //{
        //    lerpTime -= 1;
        //    startPoint = endPoint;

        //    if (endPoint == points.Length - 1 && isRing)
        //        endPoint = 0;
        //    else if (endPoint == points.Length - 1 && !isRing)
        //        iterator = -1;
        //    else if (endPoint == 0)
        //        iterator = 1;

        //    endPoint += iterator;
        //}

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

            Debug.Log(name[name.Length - 1] + ":");
            movementPattern.CreatePattern(transform, pathNode.position, path.points);

            pathNode.gameObject.SetActive(false);
        }
        else
        {
            //Debug.LogWarning("Path for " + name + " " + pathName + " not found!");
            movementPattern.CreateStaticPosition(transform);
        }
    }






    private class MovementPattern
    {
        private List<WayPoint> wayPoints;

        private WayPoint lastWayPoint;

        private Vector2 guardPosition;
        private Quaternion guardRotation;

        private float TotalDistance;

        private float totalTime;
        private float lerpTime;

        private bool isRing;
        private bool isStatic;
        private bool goesForward = true;

        public void UpdateGuard(float deltaTime)
        {
            if (!isStatic)
            {
                lastWayPoint = goesForward ? lastWayPoint.Next : lastWayPoint.Prev;

                lerpTime += deltaTime * 10 / lastWayPoint.ToNextDistance * 0.1f;

                if (lerpTime > 1)
                {
                    lerpTime %= 1;

                    if (wayPoints.First() == lastWayPoint)
                        goesForward = false;

                    else if (wayPoints.Last() == lastWayPoint)
                        goesForward = true;

                }
            }
        }

        public Vector2 GetPosition()
        {
            if (!isStatic)
                guardPosition = Vector2.Lerp(lastWayPoint.Position, lastWayPoint.Next.Position, lerpTime);
            return guardPosition;
        }

        public Quaternion GetDirection()
        {
            if (!isStatic)
                guardRotation = Quaternion.Euler(0, 0, 0);
            return guardRotation;
        }

        public void DrawPattern()
        {
            if (!isStatic)
                foreach (WayPoint wayPoint in wayPoints)
                {
                    Debug.DrawRay(wayPoint.Position, wayPoint.Next.Position - wayPoint.Position, Color.green);
                }
        }

        public void CreateStaticPosition(Transform guardTransform)
        {
            guardPosition = guardTransform.position;
            guardRotation = guardTransform.rotation;
            isStatic = true;
        }

        public void CreatePattern(Transform guardTransform, Vector2 patternStartPosition, Vector2[] patternPositions)
        {
            InitializeWayPoints(patternStartPosition, patternPositions);
            LinkWayPoints();
            CalculateDistances();

            CalculateGuardStartPosition(guardTransform.position);
        }

        private void InitializeWayPoints(Vector2 patternStartPosition, Vector2[] patternPositions)
        {
            wayPoints = new List<WayPoint>();

            for (int i = 0; i < patternPositions.Length; i++)
            {
                wayPoints.Add(new WayPoint());
                wayPoints[i].Position = patternStartPosition + patternPositions[i];
                wayPoints[i].index = i;
            }

            if ((wayPoints[wayPoints.Count - 1].Position - wayPoints[0].Position).magnitude < 0.5f)
            {
                wayPoints.RemoveAt(wayPoints.Count - 1);
                isRing = true;
            }
        }

        private void LinkWayPoints()
        {
            for (int i = 0; i < wayPoints.Count; i++)
            {
                if (i + 1 == wayPoints.Count)
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
                        wayPoints[i].Prev = wayPoints[wayPoints.Count - 1];
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

                TotalDistance += wayPoint.ToPrevDistance;
                wayPoint.DistanceFromStart = TotalDistance;
            }

            TotalDistance -= wayPoints[0].ToPrevDistance;
            wayPoints[0].DistanceFromStart = 0;
        }

        private void CalculateGuardStartPosition(Vector2 guardPosition)
        {
            WayPoint nearestSegementWayPoint = wayPoints.OrderBy(waypoint => (waypoint.Position - guardPosition).sqrMagnitude).First();
            lastWayPoint = nearestSegementWayPoint.Next;

            float guardToNextDistance = (nearestSegementWayPoint.Next.Position - guardPosition).magnitude;
            float guardToPrevDistance = (nearestSegementWayPoint.Prev.Position - guardPosition).magnitude;

            float segmentStartToEndDistance = nearestSegementWayPoint.ToNextDistance;
            float guardToSegmentEndWayPointDistance = guardToNextDistance;


            if (guardToPrevDistance / nearestSegementWayPoint.ToPrevDistance <
               guardToNextDistance / nearestSegementWayPoint.ToNextDistance)
            {
                segmentStartToEndDistance = nearestSegementWayPoint.ToPrevDistance;
                guardToSegmentEndWayPointDistance = guardToPrevDistance;
                lastWayPoint = nearestSegementWayPoint;
            }

            lerpTime = guardToSegmentEndWayPointDistance / segmentStartToEndDistance;

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

        public int index;
    }
}
