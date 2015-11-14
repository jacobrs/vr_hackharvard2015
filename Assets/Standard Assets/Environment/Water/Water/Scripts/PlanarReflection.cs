using System;
using System.Collections.Generic;

using UnityEngine;

namespace UnityStandardAssets.Water
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(WaterBase))]
    public class PlanarReflection : MonoBehaviour
    {
        public LayerMask reflectionMask;
        public bool reflectSkybox = false;
        public Color clearColor = Color.grey;
        public String reflectionSampler = "_ReflectionTex";
        public float clipPlaneOffset = 0.07F;
	    public Vector2 renderTextureSize = new Vector2(1920,1080);
	    public float renderTextureSizeMult = 1;
	    public bool renderTextureSizeFromCamera = true; 
	   public Vector2 editorRTSize = new Vector2(1920*0.5f,1080*0.5f);
		public RenderingPath renderingPath = RenderingPath.DeferredShading;
	    
        Vector3 m_Oldpos;
        Camera m_ReflectionCamera;
        Material m_SharedMaterial;
        Dictionary<Camera, bool> m_HelperCameras;

	    private int rtWidth, rtHeight;

        public void Start()
        {
			
            m_SharedMaterial = ((WaterBase)gameObject.GetComponent(typeof(WaterBase))).sharedMaterial;
        }


        Camera CreateReflectionCameraFor(Camera cam)
        {
            String reflName = gameObject.name + "Reflection" + cam.name;
            GameObject go = GameObject.Find(reflName);

            if (!go)
            {
                go = new GameObject(reflName, typeof(Camera));
            }
            if (!go.GetComponent(typeof(Camera)))
            {
                go.AddComponent(typeof(Camera));
            }
            Camera reflectCamera = go.GetComponent<Camera>();

            reflectCamera.backgroundColor = clearColor;
            reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;

            SetStandardCameraParameter(reflectCamera, reflectionMask);

			//rtWidth = (int) (renderTextureSize.x*renderTextureSizeMult);
			//rtHeight = (int) (renderTextureSize.y*renderTextureSizeMult);

			//if (renderTextureSizeFromCamera) {
				
				//rtWidth = (int)(cam.pixelWidth * renderTextureSizeMult);
				//rtHeight = (int)(cam.pixelHeight * renderTextureSizeMult);

			//}
	       


            if (!reflectCamera.targetTexture||reflectCamera.targetTexture.width != rtWidth ||reflectCamera.targetTexture.height != rtHeight)
            {
                reflectCamera.targetTexture = CreateTextureFor(cam);
            }

            return reflectCamera;
        }


        void SetStandardCameraParameter(Camera cam, LayerMask mask)
        {
            cam.cullingMask = mask & ~(1 << LayerMask.NameToLayer("Water"));
            cam.backgroundColor = Color.black;
            cam.enabled = false;
        }


        RenderTexture CreateTextureFor(Camera cam)
        {
			//RenderTexture rt = new RenderTexture(Mathf.FloorToInt(cam.pixelWidth * 0.5F),
			// Mathf.FloorToInt(cam.pixelHeight * 0.5F), 24, RenderTextureFormat.ARGBFloat);



			Debug.Log("PlanarReflection - Creating new RenderTexture: " + rtWidth + " x " + rtHeight,this);
			RenderTexture rt = new RenderTexture(rtWidth,rtHeight, 24, RenderTextureFormat.ARGBFloat);

            rt.hideFlags = HideFlags.DontSave;
            return rt;
        }


        public void RenderHelpCameras(Camera currentCam) {
	       // cameraStore = currentCam;
			//CheckRTSize(currentCam);

	        //if (rtWidth == 0 || rtHeight ==0) return; // not ready yet come back after CheckRTSize has ran
	       // cameraStore= currentCam;
            if (null == m_HelperCameras)
            {
                m_HelperCameras = new Dictionary<Camera, bool>();
            }

            if (!m_HelperCameras.ContainsKey(currentCam))
            {
                m_HelperCameras.Add(currentCam, false);
            }
            if (m_HelperCameras[currentCam])
            {
                return;
            }

            if (!m_ReflectionCamera)
            {
                m_ReflectionCamera = CreateReflectionCameraFor(currentCam);
            }

			CheckRTSize(currentCam);
            RenderReflectionFor(currentCam, m_ReflectionCamera);


            m_HelperCameras[currentCam] = true;
        }

	   // private Camera cameraStore;
	    public void CheckRTSize(Camera currentCam ) {
			//Debug.Log("CheckRTSize: " + currentCam.pixelWidth);
			//if ( currentCam != null) {
			// if (currentCam != SceneView.lastActiveSceneView.camera) return;
			//

				var rtWidth2 = 0;
				var rtHeight2 = 0;
				if (renderTextureSizeFromCamera) {
					rtWidth2 = (int) (currentCam.pixelWidth*renderTextureSizeMult);
					rtHeight2 = (int) (currentCam.pixelHeight*renderTextureSizeMult);
				} else {
					rtWidth2 = (int)renderTextureSize.x;
					rtHeight2 = (int)renderTextureSize.y;
				}
		  

				if (rtWidth2 < 32) rtWidth2 = 32;
				if (rtHeight2 < 32) rtHeight2 = 32;
				if (rtWidth2 < 512) rtWidth2 = 512; // Do not go below 512 ever, as will be ugly ;-)
				if (rtHeight2 < 512) rtHeight2 = 512;


			// TODO hack so in editor mode it sticks to one res even if camera change due to it having multiple scene cameras/views flip flopping
#if UNITY_EDITOR
		    if (!Application.isPlaying) {
			    rtWidth2 = (int) editorRTSize.x;
			    rtHeight2 = (int) editorRTSize.y;
		    } else {
			    if (renderTextureSizeFromCamera) {
					// If in Editor and playing but renderTextureSizeFromCamera was set ( renderTextureSizeFromCamera is false when capturing to video )
					rtWidth2 = (int) editorRTSize.x;
			    rtHeight2 = (int) editorRTSize.y;
				}
		    }
#endif



			if (rtWidth != rtWidth2 || rtHeight != rtHeight2) {
				    rtWidth = rtWidth2;
				    rtHeight = rtHeight2;
				    Debug.Log("camera (" +currentCam.name +") changed at runtime, creating new RenderTexture",currentCam);
				    //	if (m_ReflectionCamera != null) {
				   if(m_ReflectionCamera) DestroyImmediate(m_ReflectionCamera.gameObject);
				   m_ReflectionCamera = CreateReflectionCameraFor(currentCam);
				   // currentCam = null;
				    //}
			    }
		   // }
	    }

	    public void LateUpdate() {

		   // CheckRTSize();
			if (destroyNextUpdate && m_ReflectionCamera != null) {
				destroyNextUpdate = false;
		        DestroyImmediate(m_ReflectionCamera.gameObject);
	        }
            if (null != m_HelperCameras)
            {
                m_HelperCameras.Clear();
            }
        }


        public void WaterTileBeingRendered(Transform tr, Camera currentCam)
        {
            RenderHelpCameras(currentCam);

            if (m_ReflectionCamera && m_SharedMaterial)
            {
                m_SharedMaterial.SetTexture(reflectionSampler, m_ReflectionCamera.targetTexture);
            }
        }

		
	    private bool destroyNextUpdate;
	    public void OnValidate() {
			destroyNextUpdate =true;
			
		}

	    public void OnEnable() {
		   // CheckRTSize();
	        

	        //if (m_ReflectionCamera != null) {
		    //    DestroyImmediate(m_ReflectionCamera.gameObject);
			//	 cameraStore = null;
	        //}
	        
            Shader.EnableKeyword("WATER_REFLECTIVE");
            Shader.DisableKeyword("WATER_SIMPLE");
        }


        public void OnDisable()
        {
            Shader.EnableKeyword("WATER_SIMPLE");
            Shader.DisableKeyword("WATER_REFLECTIVE");
        }


        void RenderReflectionFor(Camera cam, Camera reflectCamera)
        {
            if (!reflectCamera)
            {
                return;
            }

            if (m_SharedMaterial && !m_SharedMaterial.HasProperty(reflectionSampler))
            {
                return;
            }

            reflectCamera.cullingMask = reflectionMask & ~(1 << LayerMask.NameToLayer("Water"));

            SaneCameraSettings(reflectCamera);

            reflectCamera.backgroundColor = clearColor;
            reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            if (reflectSkybox)
            {
                if (cam.gameObject.GetComponent(typeof(Skybox)))
                {
                    Skybox sb = (Skybox)reflectCamera.gameObject.GetComponent(typeof(Skybox));
                    if (!sb)
                    {
                        sb = (Skybox)reflectCamera.gameObject.AddComponent(typeof(Skybox));
                    }
                    sb.material = ((Skybox)cam.GetComponent(typeof(Skybox))).material;
                }
            }

            GL.invertCulling = true;

            Transform reflectiveSurface = transform; //waterHeight;

            Vector3 eulerA = cam.transform.eulerAngles;

            reflectCamera.transform.eulerAngles = new Vector3(-eulerA.x, eulerA.y, eulerA.z);
            reflectCamera.transform.position = cam.transform.position;

            Vector3 pos = reflectiveSurface.transform.position;
            pos.y = reflectiveSurface.position.y;
            Vector3 normal = reflectiveSurface.transform.up;
            float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            reflection = CalculateReflectionMatrix(reflection, reflectionPlane);
            m_Oldpos = cam.transform.position;
            Vector3 newpos = reflection.MultiplyPoint(m_Oldpos);

            reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

            Vector4 clipPlane = CameraSpacePlane(reflectCamera, pos, normal, 1.0f);

            Matrix4x4 projection = cam.projectionMatrix;
            projection = CalculateObliqueMatrix(projection, clipPlane);
            reflectCamera.projectionMatrix = projection;

            reflectCamera.transform.position = newpos;
            Vector3 euler = cam.transform.eulerAngles;
            reflectCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

            reflectCamera.Render();

            GL.invertCulling = false;
        }


        void SaneCameraSettings(Camera helperCam)
        {
            helperCam.depthTextureMode = DepthTextureMode.DepthNormals;
            helperCam.backgroundColor = Color.black;
            helperCam.clearFlags = CameraClearFlags.SolidColor;
	        helperCam.renderingPath = renderingPath;
            helperCam.hdr = true;
            helperCam.useOcclusionCulling = false;
			
        }


        static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
        {
            Vector4 q = projection.inverse * new Vector4(
                Sgn(clipPlane.x),
                Sgn(clipPlane.y),
                1.0F,
                1.0F
                );
            Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
            // third row = clip plane - fourth row
            projection[2] = c.x - projection[3];
            projection[6] = c.y - projection[7];
            projection[10] = c.z - projection[11];
            projection[14] = c.w - projection[15];

            return projection;
        }


        static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1.0F - 2.0F * plane[0] * plane[0]);
            reflectionMat.m01 = (- 2.0F * plane[0] * plane[1]);
            reflectionMat.m02 = (- 2.0F * plane[0] * plane[2]);
            reflectionMat.m03 = (- 2.0F * plane[3] * plane[0]);

            reflectionMat.m10 = (- 2.0F * plane[1] * plane[0]);
            reflectionMat.m11 = (1.0F - 2.0F * plane[1] * plane[1]);
            reflectionMat.m12 = (- 2.0F * plane[1] * plane[2]);
            reflectionMat.m13 = (- 2.0F * plane[3] * plane[1]);

            reflectionMat.m20 = (- 2.0F * plane[2] * plane[0]);
            reflectionMat.m21 = (- 2.0F * plane[2] * plane[1]);
            reflectionMat.m22 = (1.0F - 2.0F * plane[2] * plane[2]);
            reflectionMat.m23 = (- 2.0F * plane[3] * plane[2]);

            reflectionMat.m30 = 0.0F;
            reflectionMat.m31 = 0.0F;
            reflectionMat.m32 = 0.0F;
            reflectionMat.m33 = 1.0F;

            return reflectionMat;
        }


        static float Sgn(float a)
        {
            if (a > 0.0F)
            {
                return 1.0F;
            }
            if (a < 0.0F)
            {
                return -1.0F;
            }
            return 0.0F;
        }


        Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * clipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;

            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }
    }
}