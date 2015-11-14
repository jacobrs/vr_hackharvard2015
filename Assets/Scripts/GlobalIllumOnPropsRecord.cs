using System;
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GlobalIllumOnPropsRecord : MonoBehaviour {
	public float? bounce;
	public Light lightCo;
	public bool warnOnZerobounce;
	[NonSerialized]
	private bool setup;

	public bool activated;

	private void Awake() {
		//Debug.Log("Awake");
		Setup();
	}

	private void Update() {}

	private void Setup() {
		if (!setup) {
			setup = true;
			//Debug.Log("Setup");
			lightCo = GetComponent<Light>();
			if (lightCo == null) return;

			if (lightCo.bounceIntensity != 0) bounce = lightCo.bounceIntensity;
		}
	}

	public void OnEnable() {
		Setup();
	}

	public void Activate() {
		Setup(); // Activate can be called before it has been setup by other editor script 'GlobalIllumOnProps'

		activated = true;
		if(warnOnZerobounce&&(bounce==0||bounce==null))Debug.Log("Zero Bounce: " + name, this);
		if (lightCo == null || bounce == null) return;
		lightCo.bounceIntensity = (float) bounce;
	}

	public void DeActivate() {
		Setup(); // Activate can be called before it has been setup by other editor script 'GlobalIllumOnProps'
		activated = false;
		if (lightCo == null || bounce == null) return;
		lightCo.bounceIntensity = 0;
	}

	


}
