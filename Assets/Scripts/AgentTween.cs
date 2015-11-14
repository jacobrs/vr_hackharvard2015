using UnityEngine;

/// <summary>
/// Crude wayto ease/tween the agents position as the motion jumping from waypoints can be sudden and harsh
/// Here we have the agent you can visibly see; gently follow an invisible agent that is moving to way points
/// One issue is it can pass thru floor/walls if it moves very quickly so I placed a spherical collider on the agent
/// </summary>
public class AgentTween : MonoBehaviour {
	public GameObject target;
	public float speed = 8;
	public bool sleeping;
	//private float min
	// Use this for initialization
	private void Start() {
		target = transform.parent.GetComponentInChildren<NavMeshAgent>().gameObject;
	}

	private void OnDisable() {
		transform.localPosition = new Vector3();
	}

	private void Update() {
		if (target == null) return;
		//if (transform.position != target.transform.position) {
		if(Vector3.Distance(transform.position,target.transform.position)>0.01f) {
			var newPos = Vector3.Lerp(transform.position, target.transform.position, speed*Time.deltaTime);
			sleeping = false;
			// Contemplating using rays/linecast to force Ball to hover at minimum distance from floor, decided using Collider is better
			//	 RaycastHit hit;
			//	float floorDist;
			//if (Physics.Linecast(newPos, -Vector3.up, out hit)){ // Raycast down
			//    floorDist = hit.distance;
			//}
			transform.position = newPos;
		} else {
			if (!sleeping) transform.position = target.transform.position;
			sleeping = true;
		}
	}
}