using UnityEngine;
using UnityEngine.UI;

public class ColorSwatch : MonoBehaviour {

	public Color GetColor(ColorThingThing colorThingThing) {
		var color = colorThingThing.color;
		if ( colorThingThing.gradient != null &&
		    (colorThingThing.gradient.Evaluate(0f) != Color.white &&
		     colorThingThing.gradient.Evaluate(1f) != Color.white)) {
			color = colorThingThing.gradient.Evaluate(Random.Range(0f, 1f));
		}
		return color;
	}

	public void ChangeColor(Toggle toggle) {
		if (!toggle.isOn) return;

		var colorThingy = toggle.gameObject.GetComponent<ColorThingy>();

		colorThingy.OnValidate(); // TODO Badness searches whole scene each button press, just in case things changed

		foreach (var colorThingThing in colorThingy.things) {

			var colorIntensified = colorThingThing.color*colorThingThing.intensity;

			MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
			propertyBlock.SetColor("_EmissionColor", colorIntensified);

			// Debug.Log("Setting color " + color + " on " + renderers.Count + " renderers");

			if (colorThingy.randomDiscoMode) {
				foreach (var thingThing in colorThingy.things) {
					thingThing.color = colorThingy.randomColors.Evaluate(Random.Range(0f, 1f));
				}
			}


			if (colorThingThing.affectMaterial) {
				foreach (Renderer o in colorThingThing.renderers) {
					if (o != null) {
						o.SetPropertyBlock(propertyBlock);
						//Debug.Log("Material uses: " + renderer.material.GetColor("_EmissionColor"));
						//Debug.Log("Setting color: " + color);
						DynamicGI.SetEmissive(o, colorIntensified);
					}
				}
			}

			foreach (var o in  colorThingThing.lights) {
				if (o != null) o.color = GetColor(colorThingThing);
			}

			foreach (var matprop in  colorThingThing.matProps) {
				if (matprop != null) {
					matprop._color = new Gradient {
						colorKeys = new[] {new GradientColorKey(colorThingThing.color, 0), new GradientColorKey(colorThingThing.color, 1)}
					};
					matprop.Run();
				}
			}

			foreach (var lightprop in  colorThingThing.lightProps) {
				if (lightprop != null) {
					lightprop.targetColor = GetColor(colorThingThing);
					lightprop.Run();
				}
			}

			foreach (var agentprop in  colorThingThing.agentProps) {
				if (agentprop != null) {
					agentprop.ColorOverride(GetColor (colorThingThing));
				}
			}

		}
	}

}
