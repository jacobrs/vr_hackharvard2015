using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

class FindLightsWithShadows : EditorWindow
{
    private Light[] m_DirectionalLights;
    private Light[] m_SpotLights;
    private Light[] m_PointLights;
    private Vector2 m_ScrollPos;

    [MenuItem("Window/Find Lights with Shadows")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FindLightsWithShadows));
    }

    void OnEnable ()
    {
        m_DirectionalLights = new Light[0];
        m_SpotLights = new Light[0];
        m_PointLights = new Light[0];
    }

    void DrawLights (Light[] lights, ref int i)
    {
        GUIContent tooltip = new GUIContent("", "Is the game object active in hierarchy and the light component enabled.");
        foreach (Light light in lights)
        {
            if (light == null)
                continue;

            EditorGUI.BeginDisabledGroup(light.shadows == LightShadows.None);

            EditorGUILayout.BeginHorizontal();

            string controlName = "ObjectField-" + i;
            GUI.SetNextControlName(controlName);
            EditorGUILayout.ObjectField(light, typeof(Light), true);

            if (GUILayout.Button("Select"))
            {
                Selection.activeGameObject = light.gameObject;
                SceneView.lastActiveSceneView.FrameSelected();
                GUI.FocusControl(controlName);
            }
            if (GUILayout.Button("Disable shadows"))
            {
                light.shadows = LightShadows.None;
            }
            GUILayout.Toggle(light.gameObject.activeInHierarchy && light.enabled, tooltip, GUILayout.ExpandWidth (false));
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }
    }

    void FetchLightsWithShadows ()
    {
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        List<Light> directionalLightsWithShadows = new List<Light>();
        List<Light> spotLightsWithShadows = new List<Light>();
        List<Light> pointLightsWithShadows = new List<Light>();
        foreach (Light light in lights)
        {
            if (light.shadows != LightShadows.None)
            {
                switch (light.type)
                {
                    case LightType.Directional:
                        directionalLightsWithShadows.Add(light);
                        break;
                    case LightType.Spot:
                        spotLightsWithShadows.Add(light);
                        break;
                    case LightType.Point:
                        pointLightsWithShadows.Add(light);
                        break;
                    case LightType.Area:
                        break;
                }
            }

            m_DirectionalLights = directionalLightsWithShadows.ToArray();
            m_SpotLights = spotLightsWithShadows.ToArray();
            m_PointLights = pointLightsWithShadows.ToArray();
        }
    }

    void OnGUI()
    {
        if (GUILayout.Button ("Find lights with shadows"))
            FetchLightsWithShadows();

        m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

        int i = 0;
        if (m_DirectionalLights.Length > 0)
            EditorGUILayout.LabelField("Directional lights:");
        DrawLights(m_DirectionalLights, ref i);

        if (m_SpotLights.Length > 0)
            EditorGUILayout.LabelField("Spot lights:");
        DrawLights(m_SpotLights, ref i);

        if (m_PointLights.Length > 0)
            EditorGUILayout.LabelField("Point lights:");
        DrawLights(m_PointLights, ref i);

        EditorGUILayout.EndScrollView();
    }
}