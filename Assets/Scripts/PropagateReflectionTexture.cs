using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This facilitates copying a reflection from one probe to many others, thus you incure the texture memory and refresh cost for only one probe,
/// allowing you to use many many probes to paint in the reflection onto e.g. props. Especially useful for reflection probes in deferred,
/// as anchor override is not supported there.
/// To refresh probes toggle component on and off
/// </summary>

[ExecuteInEditMode]
public class PropagateReflectionTexture : MonoBehaviour
{

	public bool debug;
	[Tooltip ("the source Probe used to copy reflection to all DestinationProbes")]
	public ReflectionProbe m_SourceProbe;

	[Tooltip ("The gameObject to search within and finds all Probes to populate DestinationProbes with")]
	[NonSerialized]
	public GameObject m_Parent;

	[Tooltip ("Do not hand edit this as it is auto generated, used for debugging and curisoity")]
	public ReflectionProbe[] m_DestinationProbes;

	[Tooltip ("when true copies the intensity from the SourceProbe to all DestinationProbes")]
	public bool copyIntensity = false;
	[Tooltip ("when true overrides intensity ( using below intensity value ) on all DestinationProbes")]
	public bool overrideIntensity = false;
	[Tooltip ("Intensity value used for above overrideIntensity")]
	public float intensity = 1f;

	[NonSerialized]
	public bool valid;

	private int hackyDelay = 0; // TODO reflection probes during edit mode do not update properly, so I count a few frames and retry

	public void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			UnityEditor.EditorApplication.update -= Update;
			UnityEditor.EditorApplication.update += Update;
		}
#endif
		hackyDelay = 0;
		valid = false;
		if (!Application.isPlaying)
		{
			Start ();
			Run ();
		}
	}

	public void OnDisable ()
	{
		valid = false;
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			UnityEditor.EditorApplication.update -= Update;
		}
#endif

	}

	public void Update ()
	{
		if (hackyDelay < 10)
			hackyDelay++;

		if (m_SourceProbe == null)
			return;
		if (lastTextureName != m_SourceProbe.texture)
		{
			// At start on Scene load in editor the sourceProbe will have wrong texture, so if and when the sourceProbes texture changes it will re Run()

			if (lastTextureName != null)
			{
				if (debug)
					Debug.Log ("Texture changed from: " + lastTextureName.name + " -> " + m_SourceProbe.texture.name, this);
				//Debug.Log("Texture changed from: " + lastTextureName.width + " -> " + m_SourceProbe.texture.width, this);
			}
			else
			{
				if (debug)
					Debug.Log ("Texture changed: " + name + " -> " + m_SourceProbe.texture.name, this);
			}
			valid = false;
		}
		if (!valid)
			Run ();
	}

	private void Awake ()
	{
		valid = false;

	}

	void Start ()
	{
		m_Parent = m_Parent != null ? m_Parent : gameObject;
		m_DestinationProbes = m_Parent.GetComponentsInChildren<ReflectionProbe> ();
	}

	public Texture lastTextureName;
	void Run ()
	{
		if (!Application.isPlaying)
		{
			if (hackyDelay < 10)
				return;
			hackyDelay = -120; // TODO Will retry every 120 frames as leaving playback and saving scene can cause it to fail to update
		}
		//hackyDelay = 0;
		//Debug.Log("RUN PropagateReflectionTexture: valid: " + valid,this);
		if (m_DestinationProbes == null || m_SourceProbe == null)
			return;

		Texture srcTexture = m_SourceProbe.texture;
		if (m_SourceProbe.texture != null)
		{
			lastTextureName = m_SourceProbe.texture;
			//valid = true; // TODO never valid as I cannot guarantee it is set properly due to Unity bug, why I use hackyDelay above
			//int alteredCount=0;
			int debugProbes = 0;
			//Debug.Log("VALID PropagateReflectionTexture",this);
			//Debug.Log("srcTexture:"+srcTexture+":"+srcTexture.name+":"+srcTexture.width+":"+srcTexture.height);
			foreach (ReflectionProbe dstReflectionProbe in m_DestinationProbes)
			{
				if (dstReflectionProbe == null)
					continue;
				if (!dstReflectionProbe.gameObject.activeInHierarchy)
					continue;
				if (!dstReflectionProbe.gameObject.activeSelf)
					continue;

				if (copyIntensity && dstReflectionProbe.intensity != m_SourceProbe.intensity)
					dstReflectionProbe.intensity = m_SourceProbe.intensity;
				if (overrideIntensity && dstReflectionProbe.intensity != intensity)
					dstReflectionProbe.intensity = intensity;

				if (dstReflectionProbe.customBakedTexture == srcTexture)
					continue;
				//alteredCount++;
				if (!Application.isPlaying || debug)
					debugProbes++;

				//Debug.Log("setting probe: " +dstReflectionProbe.name+":"+name ,dstReflectionProbe);

				dstReflectionProbe.mode = ReflectionProbeMode.Custom;
				dstReflectionProbe.customBakedTexture = srcTexture;
			}
			if (debugProbes > 0)
			{
				Debug.Log ("PropagateReflection to Probes: " + debugProbes + " - " + name, this);
			}
			//Debug.Log(" COUNT PropagateReflectionTexture: " + alteredCount,this);

		}
	}

	private void OnValidate ()
	{
		Run ();
	}
}
