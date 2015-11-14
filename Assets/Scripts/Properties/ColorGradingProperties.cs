using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;


[ExecuteInEditMode]
public class ColorGradingProperties : ObjectProperties {
	private Tonemapping toneMapping;
	//public Light _moon;
	public Gradient exposure;
	//public Gradient exposure, brightness,contrast,saturation;
	public Vector2 exposure2 = new Vector2(1, 2), brightness2 = new Vector2(1, 1), contrast2 = new Vector2(1, 1.1f), saturation2 = new Vector2(0, 1);
	public float crossFade;


	public new void Awake() {
		toneMapping = GetComponent<Tonemapping>();
		base.Awake();
	}

	public override void Run() {
		if (!enabled) return;
		UpdateTone(time);
	}


	public void Update() {}

	//public new void OnEnable() {
	//	if(debugLogness)Debug.Log("ColorGrading OnEnable");
	//	UpdateTone(time);
	//	base.OnEnable();
	//}

	private void UpdateTone(float timeIn) {
		if (debugLogness) Debug.Log("ColorGrading UpdateTone: time:" + timeIn + " crossFade:" + crossFade + " :" + toneMapping, this);
		if (toneMapping == null) toneMapping = GetComponent<Tonemapping>();
		var crossTo = 0f;
		var crossFade2 = crossFade;
		if (crossFade < 0) {
			crossTo = 30;
			crossFade2 = -crossFade;
		}
		if (exposure != null) toneMapping.exposureAdjustment = Mathf.Lerp(Mathf.Lerp(exposure2.x, exposure2.y, exposure.Evaluate(timeIn).r), crossTo, crossFade2);
		if (exposure != null) toneMapping.Brightness = Mathf.Lerp(brightness2.x, brightness2.y, exposure.Evaluate(timeIn).r);
		//if (exposure != null) toneMapping.Brightness = Mathf.Lerp(Mathf.Lerp(brightness2.x, brightness2.y, exposure.Evaluate(timeIn).r), Mathf.Lerp(brightness2.x, brightness2.y, exposure.Evaluate(timeIn).r)*0.0f, Mathf.Max(0, crossFade*2f - 0.5f));

		if (exposure != null) toneMapping.Contrast = Mathf.Lerp(Mathf.Lerp(contrast2.x, contrast2.y, exposure.Evaluate(timeIn).r), 1f, crossFade);
		if (exposure != null) toneMapping.Saturation = Mathf.Lerp(saturation2.x, saturation2.y, exposure.Evaluate(timeIn).r);
	}
}