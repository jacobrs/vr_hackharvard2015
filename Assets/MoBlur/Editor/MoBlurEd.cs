using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MoBlur))]
[CanEditMultipleObjects]
public class MoBlurEd : Editor {
	new MoBlur target { get { return base.target as MoBlur; } }

	public override void OnInspectorGUI () {
		EditorGUILayout.PropertyField(serializedObject.FindProperty("shutterAngle"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("frameRate"));

		if(GUI.changed)
			serializedObject.ApplyModifiedProperties();

		EditorGUILayout.Space();

		target.advancedFoldout = EditorGUILayout.Foldout(target.advancedFoldout, "Advanced");

		if(target.advancedFoldout)
			DrawDefaultInspector();

		//EditorGUILayout.Space();
		
		//if(GUILayout.Button("Capture Initial Data"))
		//	CaptureInitial();
	}

	//void CaptureInitial() {
	//	if(!target.sequenceRef) {
	//		Debug.LogError("Cannot capture initial data, no sequence reference set.");
	//		return;
	//	}

	//	var cam = target.GetComponent<Camera>();

	//	var oldCursor = target.sequenceRef.GetTime();
	//	target.sequenceRef.ScrubTo(target.sequenceStartTime);

	//	//Debug.Log(cam.worldToCameraMatrix);
	//	//Debug.Log(cam.projectionMatrix);

	//	var oldPos = target.transform.localPosition;
	//	var oldRot = target.transform.localEulerAngles;

	//	var pos1 = target.transform.position;
	//	var rot1 = target.transform.eulerAngles;

	//	target.sequenceRef.ScrubTo(target.sequenceStartTime + 1f/30f);

	//	var pos2 = target.transform.position;
	//	var rot2 = target.transform.eulerAngles;

	//	target.transform.position = pos1 * 2f - pos2;
	//	target.transform.eulerAngles = rot1 * 2f - rot2;

	//	target.useBakedInitialData = true;
	//	target.initialPrevView = cam.worldToCameraMatrix;
	//	target.initialPrevProj = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);

	//	//Debug.Log(cam.worldToCameraMatrix);
	//	//Debug.Log(cam.projectionMatrix);

	//	target.sequenceRef.ScrubTo(oldCursor);
	//	target.transform.localPosition = oldPos;
	//	target.transform.localEulerAngles = oldRot;
	//}
}
