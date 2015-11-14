using UnityEngine;

[RequireComponent (typeof (Renderer))]
[ExecuteInEditMode]
public class ScrollDetailMap : MonoBehaviour
{
	public bool runInEditor;
	public Vector2 m_Speed = new Vector2 (1.0f, 1.0f);
	public float m_SpeedScale = 0.01f;

	[Tooltip ("Material property to alter, if blank defaults to '_DetailAlbedoMap_ST' ")]
	public string m_PropertyName = "";

	private Renderer m_Renderer;
	private Vector4 m_DetailAlbedoMap_ST;
	private MaterialPropertyBlock m_PropertyBlock;
	private float TimedeltaTime, lastTime;
	private string m_DetailPropertyName = "";

	private void OnEnable ()
	{
		m_DetailPropertyName = m_PropertyName != "" ? m_PropertyName : "_DetailAlbedoMap_ST";

		m_Renderer = GetComponent<Renderer> ();
		m_DetailAlbedoMap_ST = m_Renderer.sharedMaterial.GetVector (m_DetailPropertyName);
		m_PropertyBlock = new MaterialPropertyBlock ();
#if UNITY_EDITOR
		if (!Application.isPlaying && runInEditor)
		{
			UnityEditor.EditorApplication.update -= Update;
			UnityEditor.EditorApplication.update += Update;
		}
#endif
	}

	private void Update ()
	{
		if (Application.isPlaying || runInEditor)
		{
			if (runInEditor && !Application.isPlaying)
			{
				TimedeltaTime = Time.realtimeSinceStartup - lastTime;
				lastTime = Time.realtimeSinceStartup;
			}
			else
			{
				TimedeltaTime = Time.deltaTime;
			}
			m_DetailAlbedoMap_ST.z += m_SpeedScale * m_Speed.x * TimedeltaTime;
			m_DetailAlbedoMap_ST.w += m_SpeedScale * m_Speed.y * TimedeltaTime;

			m_DetailAlbedoMap_ST.z = m_DetailAlbedoMap_ST.z % 1.0f;
			m_DetailAlbedoMap_ST.w = m_DetailAlbedoMap_ST.w % 1.0f;

			m_PropertyBlock.SetVector (m_DetailPropertyName, m_DetailAlbedoMap_ST);
			m_Renderer.SetPropertyBlock (m_PropertyBlock);
		}
	}

	private void OnDisable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			UnityEditor.EditorApplication.update -= Update;
		}
#endif
	}
}