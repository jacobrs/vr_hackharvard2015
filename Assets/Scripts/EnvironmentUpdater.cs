using UnityEngine;

public class EnvironmentUpdater : MonoBehaviour
{
	public Gradient groundGradient, equatorGradient, skyGradient;
	public float ambientMultiply = 2.0f; // Gradient doesn't support HDR color picker yet, so use a separate multiplier.
	public SunProperties sunProperties;

	private TimeOfDayManager m_TimeOfDayManager;
	private UnityEngine.Rendering.AmbientMode m_PrevAmbientMode;

	void OnEnable ()
	{
		sunProperties = GetComponent<SunProperties> ();
		m_TimeOfDayManager = FindObjectOfType<TimeOfDayManager> ();
		m_PrevAmbientMode = RenderSettings.ambientMode;

		RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
		if (sunProperties != null)
		{
			sunProperties.ambientMultiply = ambientMultiply;
			sunProperties.Run ();
		}
	}

	void OnDisable ()
	{
		if (sunProperties != null)
		{
			sunProperties.ambientMultiply = 1;
			sunProperties.Run ();
		}
		RenderSettings.ambientMode = m_PrevAmbientMode;
	}

	void Update ()
	{
		if (!m_TimeOfDayManager)
		{
			Debug.LogError ("Couldn't find the TimeOfDayManager.");
			return;
		}
		float currentTime = m_TimeOfDayManager.time;

		RenderSettings.ambientGroundColor = groundGradient.Evaluate (currentTime);
		RenderSettings.ambientEquatorColor = equatorGradient.Evaluate (currentTime);
		RenderSettings.ambientSkyColor = skyGradient.Evaluate (currentTime);
	}

}