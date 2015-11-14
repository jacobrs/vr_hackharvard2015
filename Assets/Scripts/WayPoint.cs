using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class WayPoint : MonoBehaviour {
	public List<WayPoint> connectedWayPoints;
	public bool autoFindNextWayPoint = true;
	public bool debugView = false;

	public void OnEnable() {
		if (autoFindNextWayPoint) connectedWayPoints = null;
		if (connectedWayPoints == null || connectedWayPoints.Count == 0) {
			connectedWayPoints = new List<WayPoint>();
			//Debug.Log("children: " + transform.parent.childCount, transform.parent);
			for (int i = 0; i < transform.parent.childCount; i++) {
				var child = transform.parent.GetChild(i);
				if (child == transform) {
					if ((i + 1 + 1) > transform.parent.childCount) {
						//Debug.Log("endOfLine", this);
						child = transform.parent.GetChild(0);
					} else {
						child = transform.parent.GetChild(i + 1);

						//break;
					}
					//Debug.Log("	child:" + i + ":" + child.name, child);
					if (child.GetComponent<WayPoint>() != null) connectedWayPoints.Add(child.GetComponent<WayPoint>());
					break;
				}
			}
		}
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position, 0.1f);
		foreach (var connectedWayPoint in connectedWayPoints) {
			if (connectedWayPoint != null) {
				Gizmos.color = new Color(0, 0, 0, 0.3f);

				Gizmos.DrawLine(transform.position, connectedWayPoint.transform.position);
			}
		}
	}

	private void OnDrawGizmos() {
		Gizmos.color = new Color(1, 1, 1, 0.3f);
		Gizmos.DrawSphere(transform.position, 0.1f);
		foreach (var connectedWayPoint in connectedWayPoints) {
			if (connectedWayPoint != null) {
				Gizmos.color = new Color(0, 0, 0, 0.3f);

				if (debugView) Gizmos.DrawLine(transform.position, connectedWayPoint.transform.position);
			}
		}
	}
}