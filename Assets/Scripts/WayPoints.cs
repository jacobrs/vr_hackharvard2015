using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class WayPoints : MonoBehaviour {
	public List<WayPoint> wayPoints;

	public void OnEnable() {
		if (wayPoints == null || wayPoints.Count == 0) {
			wayPoints = FindObjectsOfType<WayPoint>().ToList();
		}
	}
}