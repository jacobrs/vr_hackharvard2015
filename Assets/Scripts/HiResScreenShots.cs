using System;
using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;


[ExecuteInEditMode]
public class HiResScreenShots : MonoBehaviour {
	public int resWidth = 2550;
	public int resHeight = 3300;
	public float resMult = 1;
	public Camera targetCamera;
	public eImageFormat imageFormat = eImageFormat.png;
	public int jpegQuality = 80;
	public string outputFolder;
	public int waitFrames = 2;
	public int delayAfterShot = 2;

	[Range(0, 1)] public float rateOfTime = 1;

	public bool pauseTime = true;

	public List<Behaviour> enableBehaviours;
	public List<Behaviour> disableBehaviours;
	public List<GameObject> enableGameObjects;
	public List<GameObject> disableGameObjects;

	public List<Behaviour> toggleBack;
	public List<GameObject> toggleBackGameObjects;

	public enum eImageFormat {
		jpg,
		png
	}

	[NonSerialized] private bool takeHiResShot = false;


	public static string outputPath;

	public string ScreenShotName(int width, int height) {
		#if UNITY_EDITOR
		var sceneName = System.IO.Path.GetFileNameWithoutExtension(UnityEditor.EditorApplication.currentScene);		
		
		System.IO.Directory.CreateDirectory(outputFolder);
		
		return string.Format("{0}/{1} - {2}x{3} - {4}.png",
			outputFolder,
			sceneName,
			width, height,
			DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
			
		#else
		
			return "nopeOnlyInEditor"; // TODO Screenshot work outside of Editor
			
		#endif
	}

	public void OnDisable() {
		if (togglingBack) {
			Debug.Log("Force Toggleback now");
			//CancelInvoke("ToggleBack");
			ToggleBack();
		}
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			UnityEditor.EditorApplication.update -= LateUpdate;
		}
#endif
	}


	public void OnEnable() {
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			UnityEditor.EditorApplication.update -= LateUpdate;
			UnityEditor.EditorApplication.update += LateUpdate;
		}
#endif
	}

	public void TakeHiResShot() {
		//outputPath = path;
		toggleBack = new List<Behaviour>();
		toggleBackGameObjects = new List<GameObject>();
		foreach (var o in enableBehaviours) {
			if (o != null && o.enabled == false) {
				toggleBack.Add(o);
				o.enabled = true;
			}
		}
		foreach (var o in disableBehaviours) {
			if (o != null && o.enabled == true) {
				toggleBack.Add(o);
				o.enabled = false;
			}
		}
		foreach (var o in enableGameObjects) {
			if (o != null && o.activeSelf == false) {
				toggleBackGameObjects.Add(o);
				o.SetActive(true);
			}
		}
		foreach (var o in disableGameObjects) {
			if (o != null && o.activeSelf == true) {
				toggleBackGameObjects.Add(o);
				o.SetActive(false);
			}
		}

		takeHiResShot = true;
	}

	public void Awake() {
		if (targetCamera == null) targetCamera = Camera.main;
	}

	[NonSerialized] private int framesWaited = 0;

	[NonSerialized] private float toggleBackStartTime;

	[NonSerialized] private bool togglingBack;

	public void ToggleBack() {
		Debug.Log("ToggleBack", this);
		togglingBack = false;
		toggleBackStartTime = -1;
		framesWaited = 0;
		foreach (var o in toggleBack) {
			o.enabled = !o.enabled;
		}
		foreach (var o in toggleBackGameObjects) {
			o.SetActive(!o.activeSelf);
		}
		if (pauseTime) Time.timeScale = 1; // TODO add easing time back
	}

	private float timeCrossFade = 1; // For above future timeScale fading back in

	public void Update() {
		Time.timeScale = rateOfTime*timeCrossFade;
	}

	public void LateUpdate() {
		if (togglingBack) {
			//Debug.Log(toggleBackStartTime + delayAfterShot + ":" + Time.realtimeSinceStartup);
			if (toggleBackStartTime + delayAfterShot > Time.realtimeSinceStartup) {
				return;
			} else {
				ToggleBack();
			}
		}
		if (togglingBack) return;

		if (!takeHiResShot && CrossPlatformInputManager.GetButtonDown("Screenshot") ) { 
			TakeHiResShot();
		}

		if (takeHiResShot) {
			if (pauseTime) Time.timeScale = 0;
		}
		if (takeHiResShot) framesWaited++;
		if (takeHiResShot && framesWaited < waitFrames) Debug.Log("Waiting:" + framesWaited + " out of " + waitFrames);
		if (takeHiResShot && framesWaited >= waitFrames) {
			int w = (int) (resWidth*resMult);
			int h = (int) (resHeight*resMult);
			RenderTexture rt = new RenderTexture(w, h, 24);
			targetCamera.targetTexture = rt;
			Texture2D screenShot = new Texture2D(w, h, TextureFormat.RGB24, false);
			targetCamera.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
			targetCamera.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors

#if UNITY_EDITOR
			if (!Application.isPlaying) {
				DestroyImmediate(rt);
			} else {
				Destroy(rt);
			}

#else
Destroy(rt);
#endif


			byte[] bytes = null;
			switch (imageFormat) {
				case eImageFormat.jpg:
					bytes = screenShot.EncodeToJPG(jpegQuality);
					break;
				case eImageFormat.png:
					bytes = screenShot.EncodeToPNG();
					break;
			}

			toggleBackStartTime = Time.realtimeSinceStartup;
			togglingBack = true;
			//Invoke("ToggleBack", delayAfterShot);

#if !UNITY_WEBPLAYER
			string filename = ScreenShotName(resWidth, resHeight);
			System.IO.File.WriteAllBytes(filename, bytes);
			Debug.Log(string.Format("Took screenshot to: {0}", filename));
			takeHiResShot = false;
#else
			Debug.Log("Saving screenshots is not avaiable in the webplayer.");
#endif
		}
	}
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof (HiResScreenShots))]
public class HiResScreenShotsEditor : UnityEditor.Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		var targetCast = (HiResScreenShots) target;

		GUILayout.Space(3);

		if (GUILayout.Button("Screenshot")) {
			targetCast.TakeHiResShot();
			targetCast.LateUpdate();
		}
		if (targetCast.rateOfTime >= 1) {
			if (GUILayout.Button("Pause Time")) {
				targetCast.rateOfTime = 0;
			}
		} else {
			if (GUILayout.Button("Resume Time")) {
				targetCast.rateOfTime = 1;
			}
		}
	}
}
#endif