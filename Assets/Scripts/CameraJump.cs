using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;


[Serializable]
public class CamereItem {
	public Vector3 postion, rotation;
	public bool traversable;
}

[ExecuteInEditMode]
public class CameraJump : MonoBehaviour {
	public List<GameObject> targetToMove;
	public List<GameObject> targetToReset;
	public CameraStealer camStealer;


	public List<Behaviour> playerControllers;
	public CopySceneView targetCopySceneView;

	public bool debug;

	public List<CamereItem> cameraItems;

	[NonSerialized] private bool waitingForInput;

	public void Awake() {
		if (targetCopySceneView == null) {
			targetCopySceneView = FindObjectOfType<CopySceneView>();
			Debug.LogWarning("No CopySceneView Set so using first one I can find", this);
		}
	}

	private void Update() {
		if (!Application.isPlaying) return;
		for (int i = 1; i <= cameraItems.Count; i++) {
			var theKey = i;
			if (theKey == 10) theKey = 0;
			var theKey2 = theKey.ToString();

			//bool requireShift = false;
			//if (theKey > 10) {
			//	if (theKey == 20) theKey = 10;
			//	theKey2 = (theKey - 10).ToString();
			//
			//	requireShift = true;
			//}

			//if ((Input.GetKey(theKey2) && !requireShift) || (Input.GetKey(theKey2) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))) {
			if (Input.GetKey(theKey2) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftAlt)) {
				if (debug) Debug.Log("Key pressed: " + i + ":" + theKey);
				JumpNow(theKey);
			}
		}
		if (waitingForInput) {
			bool moveForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
			bool moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
			bool moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
			bool moveBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
			if (Input.GetKey(KeyCode.P) || moveForward || moveLeft || moveRight || moveBack) {
				if (debug) Debug.Log("Level");
				waitingForInput = false;
				foreach (var o in targetToMove) {
					if (o == null) continue;
					o.transform.rotation = Quaternion.Euler(0, o.transform.rotation.eulerAngles.y, 0);
					//
					var fps = o.GetComponent<FirstPersonController>();
					if (fps != null) {
						if (debug) Debug.Log("Wiping First Person Controller");
						fps.Start(); // Re initialise and wipe internal values
					}
				}


				foreach (var playerController in playerControllers) {
					playerController.enabled = true;
				}
			}
		}
	}

	public void JumpNow(int slot) {
		camStealer.Steal();
		if (debug) Debug.Log("JumpNow: " + slot);
		if (slot > cameraItems.Count - 1) {
			Debug.Log("Slot request is outside range: " + slot + ":" + cameraItems.Count, this);
			return;
		}
		var camItem = cameraItems[slot];

		foreach (var o in targetToMove) {
			if (o == null) continue;
			o.transform.position = camItem.postion;
			o.transform.rotation = Quaternion.Euler(camItem.rotation);
		}

		foreach (var o in targetToReset) {
			if (o == null) continue;
			o.transform.localRotation = new Quaternion();
			o.transform.localPosition = new Vector3();
		}


		targetCopySceneView.enabled = false; // TODO bad

		if (Application.isPlaying) {
			foreach (var playerController in playerControllers) {
				playerController.enabled = false;
			}
		}
		waitingForInput = true;
	}

	public void Store(int slot) {
		var camItem = cameraItems[slot];
		camItem.postion = Camera.main.transform.position;
		camItem.rotation = Camera.main.transform.rotation.eulerAngles;
	}
}


#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof (CameraJump))]
public class LevelScriptEditor : UnityEditor.Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		var cameraJump = (CameraJump) target;

		GUILayout.Space(3);
		if (GUILayout.Button("Follow Scene View")) {
			cameraJump.targetCopySceneView.enabled = true;
			cameraJump.camStealer.Steal();
			if (Application.isPlaying) {
				cameraJump.targetCopySceneView.runInPlayMode = true;
				cameraJump.targetCopySceneView.Update();
			}
		}

		if (GUILayout.Button("First Person Control")) {
			//cameraJump.targetCopySceneView.enabled = true;
			cameraJump.camStealer.Steal();
			if (Application.isPlaying) {
				cameraJump.targetCopySceneView.runInPlayMode = false;
				//cameraJump.targetCopySceneView.Update2();
			}
		}

		GUILayout.Space(3);
		for (int i = 0; i < cameraJump.cameraItems.Count; i++) {
			//var camItem = cameraJump.cameraItems[i];

			GUILayout.Space(3);
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Store " + i)) {
				cameraJump.Store(i);
			}

			if (GUILayout.Button("Jump " + i)) {
				cameraJump.JumpNow(i);
			}

			GUILayout.EndHorizontal();
		}
		//EditorGUILayout.IntField("Experience", myTarget.experience);
		// EditorGUILayout.LabelField("Level", myTarget.Level.ToString());
	}
}
#endif