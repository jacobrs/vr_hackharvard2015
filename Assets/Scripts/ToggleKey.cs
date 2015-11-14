using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;

public class ToggleKey : MonoBehaviour {
	public List<keyListStruct> keyList;
	// Use this for initialization
	private void Start() {}


	private void Update() {
		foreach (var item in keyList) {
			if (CrossPlatformInputManager.GetButtonDown(item.keyName)) {
				Debug.Log("key: " + item.keyName);
				foreach (var item2 in item.behaviour) {
					//var go = item2.gameObject;
					if (item2 != null) item2.enabled = !item2.enabled;
				}
				foreach (var item2 in item.gameObject) {
					//var go = item2.gameObject;
					if (item2 != null) item2.SetActive(!item2.activeSelf);
				}
			}
		}
	}
}


[Serializable]
public struct keyListStruct {
	[SerializeField] public string keyName;
	public List<Behaviour> behaviour;
	public List<GameObject> gameObject;
}