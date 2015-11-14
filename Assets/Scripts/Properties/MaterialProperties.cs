using System;
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class MaterialProperties : ObjectProperties {

	private MaterialPropertyBlock m_PropertyBlock;

	public Gradient _color;

	[NonSerialized] private Renderer myRenderer;
	public float multiply;

	public Vector2 randomDelay = new Vector2(0, 0);


	public override void Setup() {
		m_PropertyBlock = new MaterialPropertyBlock();
		myRenderer = GetComponent<Renderer>();
		if (myRenderer == null) {
			Debug.LogWarning("No renderer found, must be on objects with a mesh renderer/material", this);
			return;
		}

		myRenderer.SetPropertyBlock(m_PropertyBlock);

	}

	public override void Run() {
		if (debugLogness) Debug.Log("Run", this);
		if (_color == null) return;
		Color emissionColor = _color.Evaluate(time);
		if (mirrorLight && lightProp != null) {
			if (debugLogness) Debug.Log("MaterialProperties: " + time + ":" + emissionColor + " --- " + lightProp.dimmer, this);
			emissionColor = Color.Lerp(new Color(0, 0, 0), emissionColor, lightProp.dimmer);

			if (debugLogness) Debug.Log("MaterialProperties Mirror: " + time + ":" + emissionColor + " --- " + lightProp.dimmer, this);
		}
		if (myRenderer != null) {
			
			if (debugLogness) Debug.Log("Set Material Property: " +  emissionColor*multiply, this);
			m_PropertyBlock.SetColor("_EmissionColor", emissionColor*multiply);
			myRenderer.SetPropertyBlock(m_PropertyBlock);

			DynamicGI.SetEmissive(myRenderer, emissionColor*multiply);
		}
	}
}