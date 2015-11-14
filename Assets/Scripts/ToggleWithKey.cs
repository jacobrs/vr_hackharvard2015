using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToggleWithKey : MonoBehaviour {
	public bool debug;
	public int theKey;
	public bool requireModifier;
	public KeyCode modifier = KeyCode.LeftAlt;
	private Toggle toggle;

	private void Awake() {
		toggle = GetComponent<Toggle>();
	}

	private void Update() {
		//int theKey = 0;
		
		//for (int i = 0; i < 10; i++) {
		//theKey = i;
		if (Input.GetKey(theKey.ToString()) && (!requireModifier||Input.GetKey(modifier))) {

			if (debug) Debug.Log("Key pressed: " + theKey,this);

			toggle.isOn = !toggle.isOn;
		}


	}

}

