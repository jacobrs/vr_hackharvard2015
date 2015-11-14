using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (LightProbesTetrahedralGrid))]
public class LightProbesTetrahedralGridEditor : Editor
{

	public void OnEnable ()
	{
		LightProbesTetrahedralGrid grid = target as LightProbesTetrahedralGrid;
		if (grid)
			grid.Generate ();
	}

	public override void OnInspectorGUI ()
	{
		EditorGUI.BeginChangeCheck ();
		base.OnInspectorGUI ();

		if (EditorGUI.EndChangeCheck ())
		{
			LightProbesTetrahedralGrid grid = target as LightProbesTetrahedralGrid;
			if (grid)
				grid.Generate ();
		}
	}

	float DrawSlider (Vector3 basePosition, Vector3 direction, float radius)
	{

		Vector3 position = basePosition + direction * radius;
		float handleSize = 0.03f * HandleUtility.GetHandleSize (position);
		Vector3 radiusPosition = Handles.Slider (position, direction, handleSize, Handles.DotCap, 0.0f);
		return (radiusPosition - basePosition).magnitude;
	}

	public void OnSceneGUI ()
	{
		LightProbesTetrahedralGrid grid = target as LightProbesTetrahedralGrid;

		if (!grid)
			return;

		Handles.color = Color.yellow;
		Handles.DrawWireDisc (grid.transform.position, grid.transform.up, grid.m_Radius);
		Handles.DrawWireDisc (grid.transform.position + grid.transform.up * grid.m_Height, grid.transform.up, grid.m_Radius);


		float oldRadius = grid.m_Radius;
		float radius = oldRadius;

		radius = DrawSlider (grid.transform.position, grid.transform.right, radius);
		radius = DrawSlider (grid.transform.position, -grid.transform.right, radius);
		radius = DrawSlider (grid.transform.position, grid.transform.forward, radius);
		radius = DrawSlider (grid.transform.position, -grid.transform.forward, radius);

		float oldHeight = grid.m_Height;
		float height = oldHeight;

		height = DrawSlider (grid.transform.position + grid.transform.right * radius, grid.transform.up, height);
		height = DrawSlider (grid.transform.position - grid.transform.right * radius, grid.transform.up, height);
		height = DrawSlider (grid.transform.position + grid.transform.forward * radius, grid.transform.up, height);
		height = DrawSlider (grid.transform.position - grid.transform.forward * radius, grid.transform.up, height);

		if (radius != oldRadius || height != oldHeight)
		{
			grid.m_Radius = radius;
			grid.m_Height = height;
			grid.OnValidate ();
			grid.Generate ();
		}
	}
}
