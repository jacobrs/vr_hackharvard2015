using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor;

[ExecuteInEditMode]
public class GlobalIllumOnProps : MonoBehaviour {
	public List<GlobalIllumOnPropsRecord> props;
	public bool reset;
	public bool enableGIonProps;
	public bool warnOnZerobounce;
	public bool resetPrefabs;

	private void Awake() {
		Setup();
	}

	private void Setup() {
		if (props == null) props = FindObjectsOfType<GlobalIllumOnPropsRecord>().ToList();
	}

	private void OnEnable() {
		Run();
	}

	private void OnDisable() {
		ActivateProps();
	}

	public void Run() {
		if (enableGIonProps) {
			ActivateProps();
		} else {
			DeActivateProps();
		}
	}

	public void ActivateProps() {
		//if(props!=null)props.ForEach(o => o.Activate());
		if (props == null) return;
		foreach (var o in props) {
			if (o == null) continue;
			o.warnOnZerobounce = warnOnZerobounce;

			o.Activate();
		}
	}

	public void DeActivateProps() {
		if (props == null) return;
		//props.ForEach(o => o.DeActivate());
		foreach (var o in props) {
			if (o == null) continue;
			o.DeActivate();
		}
	}

	private void OnValidate() {
		if (reset) {
			reset = false;
			props = null;
			Setup();
		}

#if UNITY_EDITOR
		if (resetPrefabs) {
			resetPrefabs = false;
			if (props != null) {
				foreach (var o in props) {
					if (o == null) continue;
					UnityEditor.PrefabUtility.RevertPrefabInstance(o.gameObject);
				}
			}

		}
#endif

		Run();
	}

}
