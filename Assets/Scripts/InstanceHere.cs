using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class InstanceHere : MonoBehaviour {
	public GameObject instanceGo;


	public void OnEnable() {
#if UNITY_EDITOR
		if (Application.isPlaying) return;
		if (!enabled) return;

		if (instanceGo != null) {
			//for (int i = 0; i < transform.childCount; i++) {
			//	//Destroy(transform.GetChild(i));
			//	DestroyImmediate(transform.GetChild(i));
			//}
			foreach (Transform child in transform) {
				DestroyImmediate(child.gameObject);
			}
			//var instance = Instantiate(instanceGo);

			var instance = PrefabUtility.InstantiatePrefab(instanceGo) as GameObject;
			instance.transform.parent = transform;
			instance.transform.localRotation = new Quaternion();
			instance.transform.localPosition = new Vector3();
			instance.transform.localScale = new Vector3(1, 1, 1);
		}
#endif
	}
}