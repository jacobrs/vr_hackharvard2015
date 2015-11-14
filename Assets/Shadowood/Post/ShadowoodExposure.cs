using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

/// <summary>
/// Code by Alex Lovett ( HeliosDoubleSix )
/// Contact alex@shadowood.uk
/// www.shadowood.uk
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof (Camera))]
[AddComponentMenu("Image Effects/Color Adjustments/Shadowood Exposure")]
public class ShadowoodExposure : PostEffectsBase {
	public Shader exposureShader;
	public float exposure = 1;
	private Material m_ExposureMaterial;

	public override bool CheckResources() {
		CheckSupport(false, true);
		m_ExposureMaterial = CheckShaderAndCreateMaterial(exposureShader, m_ExposureMaterial);
		if (!isSupported) ReportAutoDisable();
		return isSupported;
	}


	private void OnRenderImage(RenderTexture source, RenderTexture destination) {
		if (CheckResources() == false) {
			Graphics.Blit(source, destination);
			return;
		}

		m_ExposureMaterial.SetFloat("_Exposure", exposure);

		//if (doPrepass) color.wrapMode = TextureWrapMode.Clamp;
		// else
		source.wrapMode = TextureWrapMode.Clamp;
		//Graphics.Blit (doPrepass ? color : source, destination, m_ChromAberrationMaterial, mode == AberrationMode.Advanced ? 2 : 1);
		Graphics.Blit(source, destination, m_ExposureMaterial);
	}
}