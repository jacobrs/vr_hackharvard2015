using UnityEngine;
using System.Collections;

public class ResetLocalTransforms : MonoBehaviour {

	// Use this for initialization
	void Start () {
		transform.localPosition = new Vector3();
		transform.localRotation = new Quaternion();

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
