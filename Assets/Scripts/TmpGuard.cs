using UnityEngine;
using System.Collections;

public class TmpGuard : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var pathName = name.Substring(name.IndexOf('_') + 1) + "Path";

        var pathNode = transform.parent.Find(pathName);

        if(pathNode != null) {
            EdgeCollider2D path = pathNode.GetComponent<EdgeCollider2D>();

            var waypoints = new Vector3[path.points.Length];
            var i = 0;

            foreach(var point in path.points) {
                waypoints[i++] = pathNode.position + new Vector3(point.x, point.y);
                Debug.DrawRay(pathNode.position + new Vector3(point.x, point.y), Vector3.up, Color.red, 10);
            }

            var firstPoint = path.points[0];
            var secondPoint = path.points[1];
            var dir = (secondPoint - firstPoint);

            transform.position = pathNode.position + new Vector3(firstPoint.x, firstPoint.y);
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            pathNode.gameObject.SetActive(false);
        } else {
            Debug.LogWarning("Path for " + name + " " + pathName + " not found!");
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
