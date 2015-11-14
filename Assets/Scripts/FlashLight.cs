using UnityEngine;

public class FlashLight : MonoBehaviour {
	public void SetLight(bool set) {
		GetComponent<Light>().enabled = set;
	}
}