using UnityEngine;

/// <summary>
/// Grabs the targetCamera and relocates it to the object this component is called from
/// Put Back will send it back to where you took it from
/// You can steal the Camera at runtime also so when the parent is enabled it will trigger and steal then, handy to move camera about say from first person controller to a saved viewpoint or jump into an animation
/// </summary>
public class CameraStealer : MonoBehaviour {
	public bool stealRunTime = false;
	public bool debug;
	
	public Camera targetCamera;
	public Transform oldTransform;

	public void Update() {}

	private void OnEnable() {
		if (stealRunTime && Application.isPlaying) {
			Steal();
		}
	}

	public void Setup() {
		if (targetCamera == null) targetCamera = Camera.main;
		if (targetCamera == null) targetCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}

	public void Steal() {
		if (!enabled) return;
		//
		Setup();
		if (targetCamera.transform.parent != transform) {
			oldTransform = targetCamera.transform.parent;
			if (debug) Debug.Log("cameraStealer Stealing!", this);
			targetCamera.transform.parent = transform;
			targetCamera.transform.localRotation = new Quaternion();
			targetCamera.transform.localPosition = new Vector3();
		}
	}

	public void PutBack() {
		if (!enabled) return;
		//
		Setup();
		if (targetCamera.transform.parent != oldTransform) {
			if (debug) Debug.Log("cameraStealer Putting Back", this);
			targetCamera.transform.parent = oldTransform;
			targetCamera.transform.localRotation = new Quaternion();
			targetCamera.transform.localPosition = new Vector3();
		}

	}
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof (CameraStealer))]
public class CameraStealerEditor : UnityEditor.Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		var targetCast = (CameraStealer) target;

		GUILayout.Space(3);

		if (GUILayout.Button("Steal")) {
			targetCast.Steal();
		}
		if (GUILayout.Button("Put Back")) {
			targetCast.PutBack();
		}
	}
}
#endif