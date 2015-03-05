using UnityEngine;

namespace Assets.Scripts
{
    public class BoundingCircle
    {

        public Vector2 Center { get; private set; }

        public float Radius { get; private set; }


        public BoundingCircle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Intersect(BoundingCircle other)
        {
            float distanceCenterCenter = (Center - other.Center).sqrMagnitude;
            float bothRadiiSqr = (Radius + other.Radius) * (Radius + other.Radius);

            return distanceCenterCenter < bothRadiiSqr;
        }

        public void Update(Vector2 newCenter)
        {
            Center = newCenter;
        }

        public void Draw()
        {
            Draw(64);
        }

        public void Draw(int segmentCount)
        {
            float slice = 2 * Mathf.PI / segmentCount;

            int i = 0;
            while (i < segmentCount)
            {
                float angle = slice * i;
                Vector2 startPosition = new Vector2(Center.x + Radius * Mathf.Cos(angle), Center.y + Radius * Mathf.Sin(angle));

                angle = slice * ++i;
                Vector2 endPosition = new Vector2(Center.x + Radius * Mathf.Cos(angle), Center.y + Radius * Mathf.Sin(angle));

                Debug.DrawLine(startPosition, endPosition, Color.yellow);
            }
        }
    }
}
