using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class AlterRange : MonoBehaviour {
	public bool alterRange = false;
	public bool debug;
	public enum Eshadow {
		leave,soft,hard,none
	}

	public Eshadow shadows = Eshadow.leave;

	public float range;
	public List<string> gameTags;
	public List<string> names;

	public void OnEnable() {
		if (!enabled) return;
		if (Application.isPlaying) return;
		Run(shadows);
	}

	public void Run() {
		if(debug)Debug.Log("AlterRange: ONE: " + enabled+" : " + shadows,this);
		Run(shadows);
	}

	public void Run(Eshadow shadows) {
		if(debug)Debug.Log("AlterRange: TWO: " + enabled+" : " + shadows);
		//if (!enabled) return;
		//if (Application.isPlaying) return;
		GetComponentsInChildren<Light>().ToList().ForEach(o => {
			if (gameTags != null&&gameTags.Count==0) {
				foreach (var gameTag in gameTags) {
					if (o.gameObject.tag == gameTag) {
						if(alterRange)o.range = range;
						if(shadows== Eshadow.soft)o.shadows = LightShadows.Soft;
						if(shadows== Eshadow.hard)o.shadows = LightShadows.Hard;
						if(shadows== Eshadow.none)o.shadows = LightShadows.None;
						
					}
				}
			}
			if (names != null) {
				foreach (var iname in names) {
					if (o.gameObject.name.Contains(iname)) {
						if(alterRange)o.range = range;
						if(shadows== Eshadow.soft)o.shadows = LightShadows.Soft;
						if(shadows== Eshadow.hard)o.shadows = LightShadows.Hard;
						if(shadows== Eshadow.none)o.shadows = LightShadows.None;
					}
				}
			}

		} );

	}

	
	public void OnValidate() {
		OnEnable();
	}
}
