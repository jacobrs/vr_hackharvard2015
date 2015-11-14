using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[ExecuteInEditMode]
public class ColorThingy : MonoBehaviour {
	public List<ColorThingThing> things;
	public bool randomDiscoMode;
	public Gradient randomColors;
	[SerializeField]
	private List<AgentProperties> masterListagentProps;
	public void Awake() {
		Setup();
	}

	public void Setup() {
#if UNITY_EDITOR
		// Runs in Editor only, so I can grab all AgentProperties even ones in disabled objects, messy :-(
		//foreach (var colorThingThing in things) {
			masterListagentProps = (Resources.FindObjectsOfTypeAll(typeof (AgentProperties)) as AgentProperties[]).ToList();
		//}
#endif
	}

	public void OnEnable() {
		Setup();
	}

	public void OnValidate() {
		foreach (var colorThingThing in things) {
			// TODO this is terrible code, sorry in a rush!

			colorThingThing.renderers = GameObject.FindGameObjectsWithTag(colorThingThing.tag).ToList().
				Select(x => x.GetComponent<Renderer>()).ToList();

			colorThingThing.lights = GameObject.FindGameObjectsWithTag(colorThingThing.tag).ToList().
				Select(x => x.GetComponent<Light>()).ToList();

			colorThingThing.matProps = GameObject.FindGameObjectsWithTag(colorThingThing.tag).ToList().
				Select(x => x.GetComponent<MaterialProperties>()).ToList();

			colorThingThing.lightProps = GameObject.FindGameObjectsWithTag(colorThingThing.tag).ToList().
				Select(x => x.GetComponent<LightProperties>()).ToList();

			colorThingThing.agentProps = new List<AgentProperties>();
			if (masterListagentProps == null || masterListagentProps.Count == 0) {
				Setup();
			}
			if (masterListagentProps != null) {
				foreach (var masterListagentProp in masterListagentProps) {
					if (masterListagentProp == null) continue;
					
					if (masterListagentProp.tag == colorThingThing.tag) {
						colorThingThing.agentProps.Add(masterListagentProp);
					}
				}
			}

			//colorThingThing.agentProps = GameObject.FindGameObjectsWithTag(colorThingThing.tag).ToList().
				//Select(x => x.GetComponent<AgentProperties>()).ToList();

			
			// TODO sure there is a way to do above and remove null in one go surely?
			colorThingThing.renderers.RemoveAll(item => item == null);
			colorThingThing.lights.RemoveAll(item => item == null);
			colorThingThing.matProps.RemoveAll(item => item == null);
			colorThingThing.lightProps.RemoveAll(item => item == null);
			colorThingThing.agentProps.RemoveAll(item => item == null);
		}
	}
}

[Serializable]
public class ColorThingThing {
	public string tag;
	public Color color;
	public Gradient gradient;
	public float intensity;
	public List<AgentProperties> agentProps;
	public List<MaterialProperties> matProps;
	public List<Renderer> renderers;
	public List<Light> lights;
	public List<LightProperties> lightProps;
	public bool affectMaterial;
}