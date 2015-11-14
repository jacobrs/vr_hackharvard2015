using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ToggleShadows : MonoBehaviour {
	public List<AlterRange> alterRanges;
	public bool debug;

	private void OnEnable() {
		Run();
	}
	private void OnDisable() {
		Run();
	}

	private void Run() {
		if(debug)Debug.Log("toggleShadows: " + enabled);
		if (enabled) {
			alterRanges.ForEach(o => o.Run(AlterRange.Eshadow.none));
		} else {
		alterRanges.ForEach(o=>o.Run());}
	}

	public void OnValidate() {
		Run();
	}
}
