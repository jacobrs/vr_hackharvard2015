using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode]
public class LightAlwaysOn : MonoBehaviour {
	public bool alwaysOn;
	public bool debug;

	private void OnAwake() {
		//OnEnable();
	}

	private void OnEnable() {
		
		if (!enabled) return;
		//if (Application.isPlaying) return;
		

		var i = 0;
		GetComponentsInChildren<LightProperties>().ToList().ForEach(o => {
			i++;
			if (o != null) {
				o.alwaysOn = alwaysOn;
				//Debug.Log("	" + o.name, o.gameObject);
				o.Run();
			}
		});
		if(debug)Debug.Log("AlwaysOn: " + name + "-> " + alwaysOn + " : " + i ,this);
	}

	public void OnValidate() {
		OnEnable();
	}
}
