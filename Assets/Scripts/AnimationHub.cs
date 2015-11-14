
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteInEditMode]
public class AnimationHub : MonoBehaviour {
	public TimeOfDayManager timeOfDayManager;
	public ColorGradingProperties colorGradingProperties;
	public GameObject guardians, guardiansFixedOn,guardiansFixedOff;
	public List<GameObject> hideGameObjects;
	public CameraStealer cameraStealerThis,cameraStealerController;
	public bool guardianInstantOn;
	public bool runInEditMode = false;
	public bool debug;
	public float unityLogoFade;
	public Renderer unityLogo;
	public Color unityLogoColor;
	public List<Behaviour> unityLogoHide;

	//[Range(0,1)]
	public float CrossFade;
	//[Range(0,10)]
	public float TimeOfDay;

	public bool enableGuardians = true;
	public bool enableGuardiansFixed = false;

	
	public bool previewMode; // Weakens the crossFade so you can see even when it's at max
	private MaterialPropertyBlock propertyBlock;

	public void Awake() {
		if (Application.isPlaying) previewMode = false;
		#if UNITY_EDITOR
		if (!Application.isPlaying) {
			EditorApplication.update -= Update;
			EditorApplication.update += Update;
		}
#endif
		if (timeOfDayManager == null) timeOfDayManager = FindObjectOfType<TimeOfDayManager>();
		if (colorGradingProperties == null) colorGradingProperties = FindObjectOfType<ColorGradingProperties>();
	}
	
	// Update is called once per frame to keep up with animation being scrubbed
	void Update () { //Called by  OnValidate
		if (runInEditMode || Application.isPlaying) {
			//if(debug)Debug.Log("AnimationHub Update");
			if(cameraStealerThis!=null)cameraStealerThis.Steal();
			//if (CrossFade > 1) CrossFade = 1;
			if (CrossFade < -1) CrossFade = -1;

			if (unityLogo != null) {
				if(propertyBlock==null)  propertyBlock = new MaterialPropertyBlock();
				propertyBlock.SetColor("_EmissionColor", unityLogoColor*(15f*unityLogoFade));
				//TODO switch to material prop block

				//Material mat = unityLogo.sharedMaterial;
				//mat.SetColor("_EmissionColor",unityLogoColor*(15f*unityLogoFade));
				unityLogo.SetPropertyBlock(propertyBlock);
				//Debug.Log("unityLogoFade  0:"+unityLogoFade+":"+(unityLogoFade>0));
				unityLogo.enabled = unityLogoFade > 0;
				

				
				foreach (var behaviour in unityLogoHide) {
					if(behaviour==null) continue;
#if !UNITY_EDITOR
					// Hack, disables anti aliasing when capturing to video to keep Unity Logo sharp
					#endif
					behaviour.enabled = !unityLogo.enabled;
				}


			}

			
			if (timeOfDayManager == null || colorGradingProperties == null) return;
			timeOfDayManager.SetTime(TimeOfDay*0.8f); // TODO remove hardcoded 0.8 and find max in Slider

			colorGradingProperties.crossFade = CrossFade;
			if (previewMode) colorGradingProperties.crossFade = CrossFade*0.9f;
			if (enableGuardians && enableGuardiansFixed) Debug.LogError("you should not have both enabled: enableGuardiansFixed and enableGuardians", this);

			//guardians.SetActive(enableGuardians);
			guardians.GetComponent<GuardianStatic>().Visible = enableGuardians;

			if (enableGuardians) {
				if (Application.isPlaying) {
					FindObjectsOfType<AgentProperties>().ToList().ForEach(o => {
						o.forceAwake = guardianInstantOn;
					});
				}
				guardiansFixedOn.GetComponent<GuardianStatic>().Visible = false;
				guardiansFixedOff.GetComponent<GuardianStatic>().Visible = false;
				//guardiansFixedOn.SetActive(false);
				//guardiansFixedOff.SetActive(false);
			} else {
				//guardiansFixedOn.SetActive(enableGuardiansFixed);
				//guardiansFixedOff.SetActive(!enableGuardiansFixed);
				guardiansFixedOn.GetComponent<GuardianStatic>().Visible = enableGuardiansFixed;
				guardiansFixedOff.GetComponent<GuardianStatic>().Visible = !enableGuardiansFixed;
			}
		}
	}


	public void OnEnable() {
		if(debug)Debug.Log("AnimationHub OnEnable");
		if (runInEditMode || Application.isPlaying) {
			
			if (timeOfDayManager == null || colorGradingProperties == null) return;
			timeOfDayManager.animationControl = true;
			OnValidate();
			hideGameObjects.ForEach(o => o.SetActive(false));
		} else {
			OnDisable();
		}
	}

	public void OnDisable() { // Called by OnEnable and OnValidate if runInEditMode is false
		if(debug)Debug.Log("AnimationHub OnDisable");
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			EditorApplication.update -= Update;
		}
#endif
		if (timeOfDayManager == null ) return;
		timeOfDayManager.animationControl = false;


		if (unityLogo != null) {
			unityLogo.enabled = false;
			//#if UNITY_EDITOR
			foreach (var behaviour in unityLogoHide) {
				if(behaviour==null) continue;
#if !UNITY_EDITOR
				// Hack, disables anti aliasing when capturing to video to keep Unity Logo sharp
					if(behaviour.name.Contains("Aliasing")) continue;
					#endif
				 behaviour.enabled = !unityLogo.enabled;
			}
//#endif
		}

		//if(guardians!=null)guardians.SetActive(true);
		if (guardians != null) guardians.GetComponent<GuardianStatic>().Visible = true;
		if (guardiansFixedOn != null) guardiansFixedOn.GetComponent<GuardianStatic>().Visible = false;
		if (guardiansFixedOff != null) guardiansFixedOff.GetComponent<GuardianStatic>().Visible = false;
		
		//if(guardiansFixedOn!=null)guardiansFixedOn.SetActive(false);
		//if(guardiansFixedOff!=null)guardiansFixedOff.SetActive(false);

		if(hideGameObjects!=null)hideGameObjects.ForEach(o => { if(o!=null)o.SetActive(true); });

		if (Application.isPlaying) {
			FindObjectsOfType<AgentProperties>().ToList().ForEach(o => {
				o.forceAwake = false;
			});
		}

		TimeOfDay = timeOfDayManager.defaultTime;
		CrossFade = 0f;
		//timeOfDayManager.time = timeOfDayManager.defaultTime;
		

		if (colorGradingProperties != null) colorGradingProperties.crossFade = 0;

		timeOfDayManager.SetTime(timeOfDayManager.defaultTime);

		//colorGradingProperties.Run();

		//timeOfDayManager.OnValidate();
		//timeOfDayManager.ForceUpdate();

		//colorGradingProperties.Run();
		
		if(cameraStealerController!=null)cameraStealerController.Steal();
	}



	private void OnValidate() {
		if (gameObject.activeInHierarchy && ( runInEditMode || Application.isPlaying)) {
			Update();
		} else {
			OnDisable();
		}
	}
}
