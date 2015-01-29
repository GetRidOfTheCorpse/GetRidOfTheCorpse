using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float WalkSpeed = 1;
    public float coneAngle = 11;
    public float coneLength = 5;

    private MovementPattern movementPattern;

    private GameObject player;
    private bool previouseCharacterDetected;
    private bool characterDetected;

    private GameObject deadBody;
    private List<GameObject> detectedCharacters;
    private SpriteRenderer bubble;
    private float bubbleTime;
    private float bubbleTimeOut = 0.5f;

    private SpriteRenderer guardSpriteRenderer;
    private Animator animator;

    private Transform viewCone;

    private bool isStatic;

    void Start()
    {
        FindNessesaryGameCompontent();
        ConvertPathToMovementPattern();
        InitializeDefaultValues();
    }

    private void FindNessesaryGameCompontent()
    {
        guardSpriteRenderer = (SpriteRenderer)GetComponent("SpriteRenderer");
        animator = (Animator)GetComponent("Animator");

        player = GameObject.FindGameObjectWithTag("Player");
        deadBody = GameObject.FindGameObjectWithTag("Body");
        viewCone = transform.FindChild("viewCone");

        bubble = (SpriteRenderer)transform.FindChild("bubble").GetComponent("SpriteRenderer");
    }

    private void InitializeDefaultValues()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);

        animator.speed = 0.5f;

        guardSpriteRenderer.color = Color.white;

        viewCone.localScale = new Vector3((coneAngle / 22.5f), coneLength * 0.245f, 1);

        detectedCharacters = new List<GameObject>();
    }

    void FixedUpdate()
    {
        movementPattern.DrawPattern();
        detectedCharacters.Clear();

        UpdatePositionAndRotation();
        UpdateAnimation();

        ViewConeCharacterIntersection(player);
        ViewConeCharacterIntersection(deadBody);

        HandleDetectedCharacters();
    }

    private void UpdateAnimation()
    {
        float direction = viewCone.eulerAngles.z;
        bool canMove = !isStatic;

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
            viewCone.rotation = movementPattern.GetDirection();
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
        Vector3 toCharacter = character.transform.position - transform.position;
        Vector3 lookDirection = -viewCone.up;

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

    private void ConvertPathToMovementPattern()
    {
        string[] name = this.name.Split('_');
        string pathName = name[name.Length - 2] + "_" + name[name.Length - 1] + "Path";

        Transform pathNode = transform.parent.Find(pathName);

        movementPattern = new MovementPattern();

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
            isStatic = true;
        }
    }


    private class MovementPattern
    {
        private List<WayPoint> wayPoints;

        private WayPoint lastWayPoint;

        private Vector2 guardPosition;
        private Quaternion guardRotation;

        private Direction direction;

        private float totalPatternDistance;

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

                totalPatternDistance += wayPoint.ToNextDistance[Direction.ToPrev];
                wayPoint.DistanceFromStart = totalPatternDistance;
            }

            totalPatternDistance -= wayPoints[0].ToNextDistance[Direction.ToPrev];
            wayPoints[0].DistanceFromStart = 0;
        }

        private void CalculateGuardStartPosition(Vector2 guardPosition)
        {
            WayPoint nearestSegementWayPoint = wayPoints.OrderBy(waypoint => (waypoint.Position - guardPosition).sqrMagnitude).First();
            lastWayPoint = nearestSegementWayPoint;

            float guardToNextDistance = (nearestSegementWayPoint.Next[Direction.ToNext].Position - guardPosition).magnitude;
            float guardToPrevDistance = (nearestSegementWayPoint.Next[Direction.ToPrev].Position - guardPosition).magnitude;

            float guardToSegmentEndWayPointDistance = guardToNextDistance;

            if (guardToPrevDistance / nearestSegementWayPoint.ToNextDistance[Direction.ToPrev] <
               guardToNextDistance / nearestSegementWayPoint.ToNextDistance[Direction.ToNext])
            {
                guardToSegmentEndWayPointDistance = guardToPrevDistance;
                lastWayPoint = lastWayPoint.Next[Direction.ToPrev];
            }

            lerpTime = 1 - (guardToSegmentEndWayPointDistance / nearestSegementWayPoint.ToNextDistance[Direction.ToNext]);
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


