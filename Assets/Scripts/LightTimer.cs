using UnityEngine;
using System.Collections;
using System.Linq;


[ExecuteInEditMode]
public class LightTimer : MonoBehaviour {
	public Gradient timer;

	private void OnEnable() {
		if (!enabled) return;
		if (Application.isPlaying) return;
		GetComponentsInChildren<LightProperties>().ToList().ForEach(o => {
			o.activePeriod = timer;
			o.Run();
		});
	}

	public void OnValidate() {
		OnEnable();
	}
}