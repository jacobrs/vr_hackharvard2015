using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode]
public class LightTimerOverride : MonoBehaviour {
	public Gradient activePeriod;
	public void Run() {
		if (activePeriod != null) {
			GetComponentsInChildren<LightProperties>().ToList().ForEach(o => {
				o.activePeriod = activePeriod;
				o.Run();
			});
		}

	}

	public void OnValidate() {
		Run();
	}

	public void OnEnable() {
		Run();
	}
}
