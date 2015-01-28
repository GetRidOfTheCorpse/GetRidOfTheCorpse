using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Remoting.Messaging;

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

        viewConeTransform = transform.GetChild(0);
        viewConeTransform.localScale = new Vector3((coneAngle / 20f), coneLength * 0.25f, 1);

        detectedCharacters = new List<GameObject>();

        PathToPoints();

        this.transform.rotation = Quaternion.Euler(0, 0, 0);

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
        float direction = viewConeTransform.eulerAngles.z;
        bool canMove = !movementPattern.IsStatic;

        if (characterDetected)
        {
            animator.SetBool("Moving", false);
        }
        else
        {
            if (canMove)
                animator.SetBool("Moving", true);

            if (direction < 45 || direction > 315)
            {
                animator.SetInteger("Direction", 2);//up
            }
            else if (direction < 135)
            {
                animator.SetInteger("Direction", 1);//left
            }
            else if (direction < 225)
            {
                animator.SetInteger("Direction", 0);//down
            }
            else
            {
                animator.SetInteger("Direction", 3);//right
            }
        }




    }

    private void UpdatePositionAndRotation()
    {
        if (!characterDetected)
        {
            movementPattern.UpdateGuard(Time.fixedDeltaTime);

            transform.position = movementPattern.GetPosition();
            viewConeTransform.rotation = movementPattern.GetDirection();
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
        Vector3 lookDirection = -viewConeTransform.up;

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
        public bool IsStatic
        {
            get { return isStatic; }
        }

        private List<WayPoint> wayPoints;

        private WayPoint lastWayPoint;

        private Vector2 guardPosition;
        private Quaternion guardRotation;

        private Direction direction = Direction.ToNext;

        private float TotalDistance;

        private float totalTime;
        private float lerpTime;

        private bool isRing;
        private bool isStatic;


        public void UpdateGuard(float deltaTime)
        {
            if (!isStatic)
            {

                lerpTime += deltaTime * 10 / lastWayPoint.ToNextDistance[direction] * 0.1f;

                if (lerpTime > 1)
                {
                    lerpTime %= 1;

                    if (!isRing)
                    {
                        if (lastWayPoint.Next[Direction.ToNext] == lastWayPoint.Next[Direction.ToPrev])
                            direction = (Direction)(((int)direction + 1) % 2);
                    }

                    lastWayPoint = lastWayPoint.Next[direction];
                }
            }
        }

        public Vector2 GetPosition()
        {
            if (!isStatic)
                guardPosition = Vector2.Lerp(lastWayPoint.Position, lastWayPoint.Next[direction].Position, lerpTime);
            return guardPosition;
        }

        public Quaternion GetDirection()
        {
            if (!isStatic)
            {
                Vector2 orientation = lastWayPoint.Next[direction].Position - lastWayPoint.Position;
                float upAngle = Vector2.Angle(Vector2.up, orientation);
                upAngle *= Mathf.Sign(-orientation.x);

                guardRotation = Quaternion.Euler(0, 0, upAngle);
            }
            return guardRotation;
        }

        public void DrawPattern()
        {
            if (!isStatic)
                foreach (WayPoint wayPoint in wayPoints)
                {
                    Debug.DrawRay(wayPoint.Position, wayPoint.Next[direction].Position - wayPoint.Position, Color.green);
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
            }

            if ((wayPoints[wayPoints.Count - 1].Position - wayPoints[0].Position).magnitude < 0.5f)
            {
                if (wayPoints.Count > 2)
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
                        wayPoints[i].Next.Add(Direction.ToNext, wayPoints[0]);
                    else
                        wayPoints[i].Next.Add(Direction.ToNext, wayPoints[i - 1]);
                }
                else
                    wayPoints[i].Next.Add(Direction.ToNext, wayPoints[i + 1]);


                if (i - 1 < 0)
                {
                    if (isRing)
                        wayPoints[i].Next.Add(Direction.ToPrev, wayPoints[wayPoints.Count - 1]);
                    else
                        wayPoints[i].Next.Add(Direction.ToPrev, wayPoints[i + 1]);
                }
                else
                    wayPoints[i].Next.Add(Direction.ToPrev, wayPoints[i - 1]);
            }
        }

        private void CalculateDistances()
        {
            foreach (WayPoint wayPoint in wayPoints)
            {
                wayPoint.ToNextDistance[Direction.ToNext] = (wayPoint.Next[Direction.ToNext].Position - wayPoint.Position).magnitude;
                wayPoint.ToNextDistance[Direction.ToPrev] = (wayPoint.Next[Direction.ToPrev].Position - wayPoint.Position).magnitude;

                TotalDistance += wayPoint.ToNextDistance[Direction.ToPrev];
                wayPoint.DistanceFromStart = TotalDistance;
            }

            TotalDistance -= wayPoints[0].ToNextDistance[Direction.ToPrev];
            wayPoints[0].DistanceFromStart = 0;
        }

        private void CalculateGuardStartPosition(Vector2 guardPosition)
        {
            WayPoint nearestSegementWayPoint = wayPoints.OrderBy(waypoint => (waypoint.Position - guardPosition).sqrMagnitude).First();
            lastWayPoint = nearestSegementWayPoint.Next[Direction.ToNext];

            float guardToNextDistance = (nearestSegementWayPoint.Next[Direction.ToNext].Position - guardPosition).magnitude;
            float guardToPrevDistance = (nearestSegementWayPoint.Next[Direction.ToPrev].Position - guardPosition).magnitude;

            float segmentStartToEndDistance = nearestSegementWayPoint.ToNextDistance[Direction.ToNext];
            float guardToSegmentEndWayPointDistance = guardToNextDistance;


            if (guardToPrevDistance / nearestSegementWayPoint.ToNextDistance[Direction.ToPrev] <
               guardToNextDistance / nearestSegementWayPoint.ToNextDistance[Direction.ToNext])
            {
                segmentStartToEndDistance = nearestSegementWayPoint.ToNextDistance[Direction.ToPrev];
                guardToSegmentEndWayPointDistance = guardToPrevDistance;
                lastWayPoint = nearestSegementWayPoint;
            }

            lerpTime = guardToSegmentEndWayPointDistance / segmentStartToEndDistance;

        }

    }
    private class WayPoint
    {
        public Dictionary<Direction, WayPoint> Next = new Dictionary<Direction, WayPoint>(2);
        public Dictionary<Direction, float> ToNextDistance = new Dictionary<Direction, float>(2);

        public Vector2 Position;

        public float DistanceFromStart;
    }

    private enum Direction
    {
        ToNext,
        ToPrev
    }
}


