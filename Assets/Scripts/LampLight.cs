using UnityEngine;
using System.Collections;

public class LampLight : MonoBehaviour {
	// Use this for initialization
	private void Start() {}

	// Update is called once per frame
	private void Update() {}

	public void SetLight(bool set) {
		GetComponent<Light>().enabled = set;
	}
}