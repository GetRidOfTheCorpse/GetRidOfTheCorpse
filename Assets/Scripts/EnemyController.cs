using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private MovementPattern movementPattern;

    private Character player;
    private Character corpse;

    private List<Character> detectedCharacters;
    private int detectedCharactersCount;
    private bool characterDetected;

    private List<Character> sensedCharacters;

    private SpriteRenderer bubble;
    private float alertTime;
    private const float alertTimeOut = 0.5f;

    private SpriteRenderer guardSpriteRenderer;
    private Animator animator;

    private Transform viewCone;
    private const float viewConeAngle = 22.5f;
    private const float viewConeLength = 5;

    private float sleepingTime;
    private bool awake;
    private bool sleeping;

    private bool isStationary;

    void Start()
    {
        SetNessesaryGameCompontents();
        ConvertPathToMovementPattern();
        InitializeDefaultValues();
    }

    private void SetNessesaryGameCompontents()
    {
        guardSpriteRenderer = (SpriteRenderer)GetComponent("SpriteRenderer");
        animator = (Animator)GetComponent("Animator");

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        corpse = GameObject.FindGameObjectWithTag("Body").GetComponent<BodyController>();
        viewCone = transform.FindChild("viewCone");

        bubble = (SpriteRenderer)transform.FindChild("bubble").GetComponent("SpriteRenderer");
    }

    private void InitializeDefaultValues()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);

        animator.speed = 0.5f;

        guardSpriteRenderer.color = Color.white;

        viewCone.localScale = new Vector2((viewConeAngle / 22.5f), viewConeLength * 0.245f);

        detectedCharacters = new List<Character>();
        sensedCharacters = new List<Character>();

        UpdatePositionAndRotation();
        UpdateAnimation();
    }

    void FixedUpdate()
    {

        movementPattern.DrawPattern();
        movementPattern.DrawBoundingCircle();
        detectedCharacters.Clear();
        sensedCharacters.Clear();

        BoundingCircleIntersection(corpse);
        BoundingCircleIntersection(player);

        HandleSensedCharacters();

        if (!sleeping)
        {
            ViewConeCharacterIntersection(corpse);
            ViewConeCharacterIntersection(player);

            HandleDetectedCharacters();

            UpdatePositionAndRotation();
            UpdateAnimation();
        }
        else sleepingTime += Time.deltaTime;
    }

    private void UpdateAnimation()
    {
        float direction = viewCone.eulerAngles.z;

        if (characterDetected || sleeping)
        {
            animator.SetBool("Moving", false);
        }
        else
        {
            if (!isStationary)
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
            movementPattern.UpdatePattern(Time.fixedDeltaTime);

            transform.position = movementPattern.GetPosition();
            viewCone.rotation = movementPattern.GetDirection();
        }

    }

    private void HandleDetectedCharacters()
    {
        characterDetected = detectedCharacters.Count > 0;

        if (detectedCharactersCount != detectedCharacters.Count)
        {
            if (detectedCharacters.Contains(corpse))
            {
                PlayerController.Instance.GotYou(true);
            }

            if (detectedCharacters.Contains(player))
            {
                SoundManager.Instance.OneShot(SoundEffect.Hey, gameObject);
                alertTime = 0;
            }
        }
        else if (alertTime < alertTimeOut)
        {
            bubble.enabled = true;

            alertTime += Time.fixedDeltaTime;
        }
        else bubble.enabled = false;

        detectedCharactersCount = detectedCharacters.Count;

    }

    private void HandleSensedCharacters()
    {
        if (sensedCharacters.Count > 0)
        {
            sleeping = false;

            if (!awake && !isStationary)
            {
                movementPattern.WakeUpPattern(sleepingTime);
                awake = true;

                sleepingTime = 0;
            }
        }
        else
        {
            sleeping = true;
            awake = false;
        }
    }

    private void ViewConeCharacterIntersection(Character character)
    {
        DrawDebugViewCone();

        Vector3 toCharacter = character.GetTransform().position - transform.position;
        Vector3 lookDirection = viewCone.up;

        if (toCharacter.magnitude < viewConeLength)
        {
            float coneArea = Mathf.Abs(Vector2.Angle(lookDirection, toCharacter));

            if (coneArea < viewConeAngle)
            {
                detectedCharacters.Add(character);
                PlayerController.Instance.GotYou();
            }
        }

    }

    private void BoundingCircleIntersection(Character character)
    {
        if (movementPattern.IsBoundingCircleIntersected(character.GetBoundingCircle()))
        {
            sensedCharacters.Add(character);
        }

    }

    private void DrawDebugViewCone()
    {
        Debug.DrawRay(transform.position, viewCone.up * viewConeLength, Color.red);

        Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, -viewConeAngle) * viewCone.up * viewConeLength, Color.blue);
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, viewConeAngle) * viewCone.up * viewConeLength, Color.blue);

        Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, -viewConeAngle / 2f) * viewCone.up * viewConeLength, Color.blue);
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, viewConeAngle / 2f) * viewCone.up * viewConeLength, Color.blue);

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

            movementPattern.CreatePattern(transform, pathNode.position, path.points, viewConeLength);

            pathNode.gameObject.SetActive(false);
        }
        else
        {
            movementPattern.CreateStaticPosition(transform, viewConeLength);

            isStationary = true;
        }
    }

    private class MovementPattern
    {
        private List<WayPoint> wayPoints;

        private BoundingCircle boundingCircle;
        private float viewConeMargin;

        private WayPoint lastWayPoint;

        private Vector2 characterPosition;
        private Quaternion characterRotation;

        private Direction direction;

        private float totalDistance;
        private const float walkingSpeed = 2;

        private float lerpTime;

        private bool isRing;
        private bool isStationary;

        public void WakeUpPattern(float timeSlept)
        {
            float distanceTraveled = timeSlept * walkingSpeed + lastWayPoint.DistanceToTheFollowingIn[direction] * lerpTime;
            distanceTraveled += lastWayPoint.DistanceToStartOfThe[direction];

            if (!isRing && distanceTraveled / totalDistance > 1)
            {
                direction = (Direction)(((int)direction + 1) % 2);
                distanceTraveled -= totalDistance;
            }

            float progress = distanceTraveled % totalDistance;

            lastWayPoint = direction == Direction.Next ? wayPoints[0] : wayPoints[wayPoints.Count - 1];


            while (lastWayPoint.DistanceToTheFollowingIn[direction] < progress)
            {
                progress -= lastWayPoint.DistanceToTheFollowingIn[direction];
                lastWayPoint = lastWayPoint.TakeTheFollowingIn[direction];
            }

            lerpTime = progress / lastWayPoint.DistanceToTheFollowingIn[direction];
        }

        public void UpdatePattern(float deltaTime)
        {
            if (!isStationary)
            {
                lerpTime += deltaTime * walkingSpeed / lastWayPoint.DistanceToTheFollowingIn[direction];

                if (lerpTime > 1)
                {
                    lerpTime %= 1;

                    lastWayPoint = lastWayPoint.TakeTheFollowingIn[direction];

                    if (!isRing)
                    {
                        if (lastWayPoint.TakeTheFollowingIn[Direction.Next] == lastWayPoint.TakeTheFollowingIn[Direction.Prev])
                            direction = (Direction)(((int)direction + 1) % 2);
                    }
                }
            }
        }

        public Vector2 GetPosition()
        {
            if (!isStationary)
                characterPosition = Vector2.Lerp(lastWayPoint.Position, lastWayPoint.TakeTheFollowingIn[direction].Position, lerpTime);
            return characterPosition;
        }

        public Quaternion GetDirection()
        {
            if (!isStationary)
            {
                Vector2 orientation = lastWayPoint.TakeTheFollowingIn[direction].Position - lastWayPoint.Position;
                float upAngle = Vector2.Angle(Vector2.up, orientation);
                upAngle *= Mathf.Sign(-orientation.x);

                characterRotation = Quaternion.Euler(0, 0, upAngle);
            }
            return characterRotation;
        }

        public bool IsBoundingCircleIntersected(BoundingCircle other)
        {
            return boundingCircle.Intersect(other);
        }

        public void DrawPattern()
        {
            if (!isStationary)
                foreach (WayPoint wayPoint in wayPoints)
                {
                    Debug.DrawRay(wayPoint.Position, wayPoint.TakeTheFollowingIn[direction].Position - wayPoint.Position, Color.green);
                }
        }

        public void DrawBoundingCircle()
        {
            boundingCircle.Draw();
        }

        public void CreateStaticPosition(Transform characterTransform, float viewConeLength)
        {
            viewConeMargin = viewConeLength;

            characterPosition = characterTransform.position;
            characterRotation = characterTransform.rotation;
            isStationary = true;

            WayPoint stationaryWayPoint = new WayPoint { Position = characterPosition };
            wayPoints = new List<WayPoint> { stationaryWayPoint };

            CreateBoundingCircle();
        }

        public void CreatePattern(Transform characterTransform, Vector2 patternStartPosition, Vector2[] patternPositions, float viewConeLength)
        {
            viewConeMargin = viewConeLength;

            InitializeWayPoints(patternStartPosition, patternPositions);
            LinkWayPoints();
            CalculateDistances();
            CreateBoundingCircle();

            CalculateCharacterStartPosition(characterTransform.position);
        }

        private void CreateBoundingCircle()
        {
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;

            foreach (WayPoint wayPoint in wayPoints)
            {
                xMin = wayPoint.Position.x < xMin ? wayPoint.Position.x : xMin;
                xMax = wayPoint.Position.x > xMax ? wayPoint.Position.x : xMax;
                yMin = wayPoint.Position.y < yMin ? wayPoint.Position.y : yMin;
                yMax = wayPoint.Position.y > yMax ? wayPoint.Position.y : yMax;
            }

            float xCenter = xMin + (xMax - xMin) / 2;
            float yCenter = yMin + (yMax - yMin) / 2;
            Vector2 center = new Vector2(xCenter, yCenter);

            float xRadius = xMax - xCenter;
            float yRadius = yMax - yCenter;
            float radius = Mathf.Sqrt(xRadius * xRadius + yRadius * yRadius) + viewConeMargin;

            boundingCircle = new BoundingCircle(center, radius);
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
                        wayPoints[i].TakeTheFollowingIn.Add(Direction.Next, wayPoints[0]);
                    else
                        wayPoints[i].TakeTheFollowingIn.Add(Direction.Next, wayPoints[i - 1]);
                }
                else
                    wayPoints[i].TakeTheFollowingIn.Add(Direction.Next, wayPoints[i + 1]);


                if (i - 1 < 0)
                {
                    if (isRing)
                        wayPoints[i].TakeTheFollowingIn.Add(Direction.Prev, wayPoints[wayPoints.Count - 1]);
                    else
                        wayPoints[i].TakeTheFollowingIn.Add(Direction.Prev, wayPoints[i + 1]);
                }
                else
                    wayPoints[i].TakeTheFollowingIn.Add(Direction.Prev, wayPoints[i - 1]);
            }
        }

        private void CalculateDistances()
        {
            float reverseTotalDistance = 0;

            for (int i = 0; i < wayPoints.Count; i++)
            {
                WayPoint wayPoint = wayPoints[i];
                wayPoint.DistanceToTheFollowingIn[Direction.Next] = (wayPoint.TakeTheFollowingIn[Direction.Next].Position - wayPoint.Position).magnitude;
                wayPoint.DistanceToTheFollowingIn[Direction.Prev] = (wayPoint.TakeTheFollowingIn[Direction.Prev].Position - wayPoint.Position).magnitude;

                wayPoint.DistanceToStartOfThe[Direction.Next] = totalDistance;
                totalDistance += wayPoint.DistanceToTheFollowingIn[Direction.Next];

                wayPoint = wayPoints[(wayPoints.Count - 1) - i];
                wayPoint.DistanceToStartOfThe[Direction.Prev] = reverseTotalDistance;
                reverseTotalDistance += (wayPoint.TakeTheFollowingIn[Direction.Prev].Position - wayPoint.Position).magnitude;
            }

            if (!isRing)
                totalDistance -= wayPoints[wayPoints.Count - 1].DistanceToTheFollowingIn[Direction.Next];
        }

        private void CalculateCharacterStartPosition(Vector2 characterPosition)
        {
            WayPoint nearestSegementWayPoint = wayPoints.OrderBy(waypoint => (waypoint.Position - characterPosition).sqrMagnitude).First();
            lastWayPoint = nearestSegementWayPoint;

            float characterToNextDistance = (nearestSegementWayPoint.TakeTheFollowingIn[Direction.Next].Position - characterPosition).magnitude;
            float characterToPrevDistance = (nearestSegementWayPoint.TakeTheFollowingIn[Direction.Prev].Position - characterPosition).magnitude;

            float characterToSegmentEndWayPointDistance = characterToNextDistance;

            if (characterToPrevDistance / nearestSegementWayPoint.DistanceToTheFollowingIn[Direction.Prev] <
               characterToNextDistance / nearestSegementWayPoint.DistanceToTheFollowingIn[Direction.Next])
            {
                characterToSegmentEndWayPointDistance = characterToPrevDistance;
                lastWayPoint = lastWayPoint.TakeTheFollowingIn[Direction.Prev];
            }

            lerpTime = 1 - (characterToSegmentEndWayPointDistance / nearestSegementWayPoint.DistanceToTheFollowingIn[Direction.Next]);
        }

    }

    private class WayPoint
    {
        public readonly Dictionary<Direction, WayPoint> TakeTheFollowingIn = new Dictionary<Direction, WayPoint>(2);
        public readonly Dictionary<Direction, float> DistanceToTheFollowingIn = new Dictionary<Direction, float>(2);
        public readonly Dictionary<Direction, float> DistanceToStartOfThe = new Dictionary<Direction, float>(2);

        public Vector2 Position;
    }

    private enum Direction
    {
        Next,
        Prev
    }
}


