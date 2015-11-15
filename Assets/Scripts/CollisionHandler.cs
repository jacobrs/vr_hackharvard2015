using UnityEngine;
using System.Collections;

public class CollisionHandler : MonoBehaviour {

	public static Vector3 lastCollision = Vector3.zero;
	public static Vector3 pendingTagLocation;

  public static float timeSinceLastCollision;

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {
		if(Time.timeSinceLevelLoad - timeSinceLastCollision >= 1
			&& lastCollision != Vector3.zero){
			lastCollision = Vector3.zero;
    }
	}

	void OnCollisionEnter(Collision collision) {
		lastCollision = collision.contacts[0].point;
		timeSinceLastCollision = Time.timeSinceLevelLoad;
		if(collision.gameObject.name.Contains("bone")){
			if(MotionHandler.wasTapped){
				MotionHandler.wasTapped = false;
				GetComponent<Rigidbody>().angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
				pendingTagLocation = lastCollision;
			}
		}
  }
}
