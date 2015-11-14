using UnityEngine;
using System.Collections;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ResetPrefabs : MonoBehaviour {

	
	private void OnEnable() {
		if (!enabled) return;
		#if UNITY_EDITOR
		
		if (Application.isPlaying) return;
		
		//transform.GetComponentsInChildren<GameObject>().ToList().ForEach(o => { PrefabUtility.RevertPrefabInstance(o); });
		var i = 0;
		foreach (var t in transform) {
			PrefabUtility.RevertPrefabInstance(((Transform) t).gameObject);
			i++;
		}
		Debug.Log("ResetPrefabs: " + name + " : " + i,this);
#endif

	}

	public void OnValidate() {
		OnEnable();
	}
}

