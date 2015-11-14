//#define USE_BAKEPOSONLY

using UnityEngine;
using System.Collections.Generic;

public class MoBlur : UnityStandardAssets.ImageEffects.ImageEffectBase {
	static int SID_PREV_MVP;
	static int SID_PREV_VP;
	static int SID_VELOCITY_TEX;
	// TODO: Pre-lookup all shader property ids.

	static readonly int[] s_tapsInPass = { 17, 25, 35, 51 };
	
	[HideInInspector] public Shader velocityRigidShader;
	[HideInInspector] public Shader velocitySkinnedShader;
	[HideInInspector] public Shader velocityStaticShader;
	[HideInInspector] public Shader tileMaxShader;
	[HideInInspector] public Shader neighbourMaxShader;
	
	public float	minBlurRadiusPixels = 0.5f;
	public int		maxBlurRadiusPixels = 20;
	public float	pixelToTapsRatioHint = 15f / 20f;
	public float	velocityScale = 1f;
	public float 	centerVelocityThreshold = 1.25f;
	public bool		treatSkinnedAsRigid;
	
	public int		frameRate = -24;
	public int		shutterAngle = 180;

	public float	jitterStrength = 0.066f;

	public float	dynamicCullingDistance = 15f;

//	public Playable		sequenceRef;
	//public double		sequenceStartTime;
	public bool			useBakedInitialData;
	public Matrix4x4	initialPrevView;
	public Matrix4x4	initialPrevProj;

	public bool		debugFreeze;
	public bool		debugFreezeTick;
	public bool 	debugDynamicVelocities;
	public float	debugTimescale = 1f;

	public enum DebugRenderMode { None, PassThrough, Velocity, TileMax, NeighbourMax, NeighbourJitter }
	public DebugRenderMode debugRenderMode = DebugRenderMode.None;

	public LayerMask dynamicSkinnedLayerMask;
	public LayerMask dynamicRigidLayerMask;

	[HideInInspector] public bool advancedFoldout;

	//static Plane[] ms_cameraPlanes = new Plane[6]; 
	static int ms_charactersLayer = -1;
	static int ms_propsLayer = -1;

	class RigidData {
		public Transform transform;
		public MeshRenderer renderer;
		public Mesh mesh;
		public Material[] materials;
		public MaterialPropertyBlock props;
		public Matrix4x4 prevWorld;

		public RigidData(MeshRenderer mr) {
			var mf = mr.GetComponent<MeshFilter>();
			if(mf == null)
				throw new UnityException("Encountered MeshRenderer without matching MeshFilter! " + mr.name);

			transform = mr.transform;
			renderer = mr;
			mesh = mf.sharedMesh;
			materials = mr.sharedMaterials;
			if(mesh == null || materials == null)
				materials = new Material[0];
			else if(materials.Length > mesh.subMeshCount)	// Cut off any redundant materials in the renderer
				System.Array.Resize(ref materials, mesh.subMeshCount);
			props = new MaterialPropertyBlock();
			props.AddMatrix(SID_PREV_MVP, Matrix4x4.identity);
			prevWorld = transform.localToWorldMatrix;
		}
	}

	class SkinnedData {
		public SkinnedMeshRenderer		renderer;
		public int 						materialCount;
		public Mesh						bakedMesh;
#if !USE_BAKEPOSONLY
		public Vector3[]				bakedPrevPos;
#endif
		public MaterialPropertyBlock	props;
		public Matrix4x4				prevWorld;

		public Transform				motionTransform;
		//public Vector3					prevMotionTranslation;
		
		public SkinnedData(SkinnedMeshRenderer smr) {
			renderer = smr;
			materialCount = renderer.sharedMaterials.Length;
			bakedMesh = new Mesh();
			bakedMesh.MarkDynamic();
#if !USE_BAKEPOSONLY
			smr.BakeMesh(bakedMesh);
			bakedPrevPos = bakedMesh.vertices;
#else
			smr.BakeMeshPositionsOnly(bakedMesh, true);
			Mesh.CopyChannel(bakedMesh, bakedMesh, MeshChannel.Vertex, MeshChannel.Normal);
#endif
			props = new MaterialPropertyBlock();
			props.AddMatrix(SID_PREV_MVP, Matrix4x4.identity);
			prevWorld = renderer.transform.localToWorldMatrix;

			motionTransform = renderer.transform;
			for(var t = motionTransform.parent; t; t = t.parent) {
				var c = t.GetComponent<MoBlurSkinRigidBinding>();
				if(c) {
					motionTransform = c.motionRoot;
					break;
				}
			}
		}
	}

	int m_enabledInFrame;

	Camera m_selfCamera;
	Matrix4x4 m_selfPrevViewMat;
	Matrix4x4 m_selfPrevProjMat;
	Matrix4x4 m_selfPrevViewProjMat;

	Camera m_velocityCamera;
	int m_selfWidth, m_selfHeight;
	RenderTextureFormat m_velocityFormat;
	int m_maxTilesX, m_maxTilesY;
	RenderTexture m_velocityTexture;
	RenderTexture m_tileMaxTexture;
	RenderTexture m_neighbourMaxTexture;

	Material m_velocityRigidMaterial;
	Material m_velocitySkinnedMaterial;
	Material m_velocityStaticMaterial;
	Material m_tileMaxMaterial;
	Material m_neighbourMaxMaterial;

	//List<RigidData> m_dynamicRigidData;
	//List<SkinnedData> m_dynamicSkinnedData;

	bool m_isDebugFrozen, m_isDebugFreezing;

	bool useDebugBuffers { get { return debugDynamicVelocities || m_isDebugFreezing || m_isDebugFrozen; } }

	// Very hacky experiment (probably need to have these register on enable to each component)
	static List<RigidData> ms_dynamicRigidData;
	static List<SkinnedData> ms_dynamicSkinnedData;

	float DeltaTime { get { return Mathf.Max (1f / 30f, Time.deltaTime); } }

	public void SetSkinnedAsRigid(bool b) {
		//treatSkinnedAsRigid = b;

		//if(treatSkinnedAsRigid)
		//	Shader.EnableKeyword("MOBLUR_SKINNED_AS_RIGID");
		//else
		//	Shader.DisableKeyword("MOBLUR_SKINNED_AS_RIGID");
	}

	void OnLevelWasLoaded(int idx) {
		//ms_cameraPlanes = new Plane[6];


		ms_charactersLayer = dynamicSkinnedLayerMask;
		ms_propsLayer = dynamicRigidLayerMask;

		ms_dynamicSkinnedData = new List<SkinnedData>();
		foreach(var r in GameObject.FindObjectsOfType<SkinnedMeshRenderer>()) {
			if(r.gameObject.isStatic)
				continue;
			
			if(r is SkinnedMeshRenderer && ((1 << r.gameObject.layer) & ms_charactersLayer) != 0)
				ms_dynamicSkinnedData.Add(new SkinnedData(r as SkinnedMeshRenderer));
		}
		
		ms_dynamicRigidData = new List<RigidData>();
		foreach(var r in Resources.FindObjectsOfTypeAll<MeshRenderer>()) {
			if(r.gameObject.hideFlags == HideFlags.NotEditable || r.gameObject.hideFlags == HideFlags.HideAndDontSave)
				continue;
			
			if(r.gameObject.isStatic)
				continue;
			
			if(((1 << r.gameObject.layer) & ms_propsLayer) == 0)
				continue;
			
			var rd = new RigidData(r as MeshRenderer);
			if(rd.materials.Length > 0)
				ms_dynamicRigidData.Add(rd);
		}
	}

	void Awake() {
		if(ms_charactersLayer < 0)
			OnLevelWasLoaded(0);
			
		m_selfCamera = GetComponent<Camera>();
		
		if(m_selfCamera.depthTextureMode == DepthTextureMode.None)
			m_selfCamera.depthTextureMode = DepthTextureMode.Depth;		

		m_selfWidth = m_selfCamera.pixelWidth;
		m_selfHeight = m_selfCamera.pixelHeight;
		m_velocityFormat = RenderTextureFormat.RGHalf;
		m_maxTilesX = Mathf.CeilToInt(m_selfWidth / (float)maxBlurRadiusPixels);
		m_maxTilesY = Mathf.CeilToInt(m_selfHeight / (float)maxBlurRadiusPixels);

		m_velocityRigidMaterial = new Material(velocityRigidShader);
		m_velocitySkinnedMaterial = new Material(velocitySkinnedShader);
		m_velocityStaticMaterial = new Material(velocityStaticShader);
		m_tileMaxMaterial = new Material(tileMaxShader);
		m_neighbourMaxMaterial = new Material(neighbourMaxShader);

		SetSkinnedAsRigid(treatSkinnedAsRigid);

		//m_dynamicRigidData = new List<RigidData>();
		//m_dynamicSkinnedData = new List<SkinnedData>();		
	}

	protected override void Start() {
		SID_PREV_MVP = Shader.PropertyToID("u_PreviousMVP");
		SID_PREV_VP = Shader.PropertyToID("u_PreviousVP");
		SID_VELOCITY_TEX = Shader.PropertyToID("u_VelocityTexture");

#if !UNITY_EDITOR
		debugFreeze = debugFreezeTick = debugDynamicVelocities = false;
		debugTimescale = 1f;
#endif

		base.Start();

		if(debugTimescale < 1f)
			Time.timeScale = Mathf.Clamp01(debugTimescale);

		if(frameRate > 0f) {
			Application.targetFrameRate = frameRate;
			QualitySettings.vSyncCount = 0;
		}

		if(debugDynamicVelocities)
			EnsureDebugBuffers();

#if UNITY_EDITOR && _DMP_NAMES
		foreach(var smd in ms_dynamicSkinnedData)
			Debug.Log("MoBlur SkinnedMesh: " + smd.renderer.name);
		foreach (var rmd in ms_dynamicRigidData)
			Debug.Log("MoBlur RigidMesh: " + rmd.transform.name);
#endif
	}

	void OnEnable() {
		//Debug.LogFormat("MoBlur Enable: {0} ({1})", name, Time.frameCount);

		var velocityGO = new GameObject("velocity_camera");
		velocityGO.transform.parent = transform;
		velocityGO.transform.localPosition = Vector3.zero;
		velocityGO.transform.localRotation = Quaternion.identity;
		velocityGO.transform.localScale = Vector3.one;
		velocityGO.SetActive(false);
		m_velocityCamera = velocityGO.AddComponent<Camera>();
		m_velocityCamera.clearFlags = CameraClearFlags.SolidColor;
		m_velocityCamera.backgroundColor = Color.black;
		m_velocityCamera.depth = -100;
		m_velocityCamera.useOcclusionCulling = false;
		m_velocityCamera.cullingMask = 1 << 31;
		m_velocityCamera.renderingPath = RenderingPath.Forward;
		m_velocityCamera.hdr = false;
		m_velocityCamera.enabled = false;

		m_enabledInFrame = Time.frameCount;

		if(useBakedInitialData) {
			m_selfPrevViewMat = initialPrevView;
			m_selfPrevProjMat = initialPrevProj;
		} else {
			m_selfPrevViewMat = m_selfCamera.worldToCameraMatrix;
			m_selfPrevProjMat = GL.GetGPUProjectionMatrix(m_selfCamera.projectionMatrix, true);
		}
		m_selfPrevViewProjMat = m_selfPrevProjMat * m_selfPrevViewMat;
	}

	protected override void OnDisable() {
		if(m_velocityCamera) {
			Object.Destroy(m_velocityCamera.gameObject);
			m_velocityCamera = null;
		}

		base.OnDisable();
	}

	void EnsureDebugBuffers() {
		if(m_velocityTexture != null)
			return;

		m_velocityTexture = new RenderTexture(m_selfWidth, m_selfHeight, 24, m_velocityFormat);
		m_tileMaxTexture = new RenderTexture(m_maxTilesX, m_maxTilesY, 0, m_velocityFormat);
		m_neighbourMaxTexture = new RenderTexture(m_maxTilesX, m_maxTilesY, 0, m_velocityFormat);
	}
	
#if UNITY_EDITOR
	void Update() {
		if(debugFreezeTick && m_isDebugFrozen)
			debugFreeze = false;

		if(debugFreeze != m_isDebugFrozen) {
			if(debugFreeze) {
				m_isDebugFreezing = true;
				Time.timeScale = 0f;
			} else {
				m_isDebugFrozen = false;
			}
		}

		if(!m_isDebugFrozen && !m_isDebugFreezing && Time.timeScale != debugTimescale)
			Time.timeScale = Mathf.Clamp01(debugTimescale);

		if(frameRate > 0f) {
			Application.targetFrameRate = frameRate;
			QualitySettings.vSyncCount = 0;
		} else if(Application.targetFrameRate != -1) {
			//Application.targetFrameRate = -1;
			//QualitySettings.vSyncCount = 1;
		}

		if(m_isDebugFreezing)
			EnsureDebugBuffers();
	}
#endif

	void OnPreCull() {
		bool isFirstFrame = m_enabledInFrame == Time.frameCount;

		var frameRateFraction = 1f / (m_enabledInFrame == Time.frameCount ? 30f : (DeltaTime * Mathf.Abs(frameRate)));

		m_velocityCamera.fieldOfView = m_selfCamera.fieldOfView;
		m_velocityCamera.nearClipPlane = m_selfCamera.nearClipPlane;
		m_velocityCamera.farClipPlane = m_selfCamera.farClipPlane;

		var camPlanes = GeometryUtility.CalculateFrustumPlanes(m_velocityCamera);

		var cullDistSqr = dynamicCullingDistance * dynamicCullingDistance;

		for(int i = 0, n = ms_dynamicRigidData.Count; i < n; ++i) {
			var rd = ms_dynamicRigidData[i];

			//Debug.LogFormat("MoBlur rigid pre: {0} ({1})", rd.renderer.name, Time.frameCount);

			var objectWorldMat = rd.transform.localToWorldMatrix;
			var prevWorld = isFirstFrame ? objectWorldMat : rd.prevWorld;
			rd.prevWorld = objectWorldMat;

			if(!rd.renderer.enabled || !rd.renderer.gameObject.activeInHierarchy)
				continue;

			if(Vector3.SqrMagnitude(m_velocityCamera.transform.position - rd.transform.position) >= cullDistSqr)
				continue;

			if(!GeometryUtility.TestPlanesAABB(camPlanes, rd.renderer.bounds))
				continue;

			//Debug.LogFormat("MoBlur rigid post: {0} ({1})", rd.renderer.name, Time.frameCount);

			var prevMVP = m_selfPrevViewProjMat * prevWorld;
			rd.props.AddMatrix(SID_PREV_MVP, prevMVP);
			rd.props.AddFloat("u_VelocityFramerateFraction", frameRateFraction);

			if(!m_isDebugFrozen)
				for(int j = 0, m = rd.materials.Length; j < m; ++j)
					Graphics.DrawMesh(rd.mesh, objectWorldMat, m_velocityRigidMaterial, 31, m_velocityCamera, j, rd.props, false, false);
		}

		//var oldWeights = QualitySettings.blendWeights;
		//QualitySettings.blendWeights = BlendWeights.TwoBones;

		for(int i = 0, n = ms_dynamicSkinnedData.Count; i < n; ++i) {
			var rd = ms_dynamicSkinnedData[i];

			//Debug.LogFormat("MoBlur skin pre: {0} ({1})", rd.renderer.name, Time.frameCount);

			var objectWorldMat = rd.renderer.transform.localToWorldMatrix;
			var prevWorld = isFirstFrame ? objectWorldMat : rd.prevWorld;
			rd.prevWorld = objectWorldMat;

			if(!rd.renderer.enabled || !rd.renderer.gameObject.activeInHierarchy)
				continue;

			if(Vector3.SqrMagnitude(m_velocityCamera.transform.position - rd.renderer.bounds.center) >= cullDistSqr)
				continue;

			if(!GeometryUtility.TestPlanesAABB(camPlanes, rd.renderer.bounds))
				continue;

			var prevMVP = m_selfPrevViewProjMat * prevWorld;
			rd.props.AddMatrix(SID_PREV_MVP, prevMVP);
			rd.props.AddFloat("u_VelocityFramerateFraction", frameRateFraction);

			if(treatSkinnedAsRigid) {
				//var motionTranslation = rd.motionTransform.position;
				//var prevMotionTranslation = isFirstFrame ? motionTranslation : rd.prevMotionTranslation;
				//rd.prevMotionTranslation = motionTranslation;

				//rd.props.AddVector("u_RootMotionTranslationDelta", motionTranslation - prevMotionTranslation);

				//if(!m_isDebugFrozen)
				//	rd.renderer.DrawMesh(objectWorldMat, m_velocitySkinnedMaterial, 31, m_velocityCamera, 0, rd.props, false, false);
			} else {
#if !USE_BAKEPOSONLY
				rd.renderer.BakeMesh(rd.bakedMesh);

				if(isFirstFrame)
					rd.bakedMesh.normals = rd.bakedMesh.vertices;
				else
					rd.bakedMesh.normals = rd.bakedPrevPos;

				// Leave tangents, UVs etc in the mesh to avoid them re-allocating each bake
#else
				//Debug.LogFormat("MoBlur skin post: {0} ({1})", rd.renderer.name, Time.frameCount);

				if(!isFirstFrame)
					Mesh.CopyChannel(rd.bakedMesh, rd.bakedMesh, MeshChannel.Vertex, MeshChannel.Normal);

				rd.renderer.BakeMeshPositionsOnly(rd.bakedMesh, true);

				if(isFirstFrame)
					Mesh.CopyChannel(rd.bakedMesh, rd.bakedMesh, MeshChannel.Vertex, MeshChannel.Normal);

				rd.bakedMesh.UploadMeshData(false);
#endif

				if(!m_isDebugFrozen)
					for(int j = 0, m = rd.materialCount; j < m; ++j)
						Graphics.DrawMesh(rd.bakedMesh, objectWorldMat, m_velocitySkinnedMaterial, 31, m_velocityCamera, j, rd.props, false, false);

#if !USE_BAKEPOSONLY
				rd.bakedPrevPos = rd.bakedMesh.vertices;
#endif
			}//end skinned render

			rd.prevWorld = objectWorldMat;
		}

		//QualitySettings.blendWeights = oldWeights;
	
		m_velocityCamera.targetTexture = useDebugBuffers ? m_velocityTexture : RenderTexture.GetTemporary(m_selfWidth, m_selfHeight, 24, m_velocityFormat);

		if(!m_isDebugFrozen) {
			var oldShadowDistance = QualitySettings.shadowDistance;
			QualitySettings.shadowDistance = 0f;

			m_velocityCamera.Render();

			QualitySettings.shadowDistance = oldShadowDistance;
		}
	}
	
	void OnRenderImage(RenderTexture source, RenderTexture destination) {
		var selfViewMat = m_selfCamera.worldToCameraMatrix;
		var selfProjMat = GL.GetGPUProjectionMatrix(m_selfCamera.projectionMatrix, true);
		var selfViewProjMat = selfProjMat * selfViewMat;

		var velocityTexture = m_velocityCamera.targetTexture;
		var tileMaxTexture = useDebugBuffers ? m_tileMaxTexture : RenderTexture.GetTemporary(m_maxTilesX, m_maxTilesY, 0, m_velocityFormat);
		var neighbourMaxTexture = useDebugBuffers ? m_neighbourMaxTexture : RenderTexture.GetTemporary(m_maxTilesX, m_maxTilesY, 0, m_velocityFormat);

		// This could be an error.. or it could be the screenshot rendering already having invoked us.
		if(!velocityTexture)
			return;

		source.filterMode = FilterMode.Bilinear;
		velocityTexture.filterMode = FilterMode.Point;
		tileMaxTexture.filterMode = FilterMode.Point;
		neighbourMaxTexture.filterMode = FilterMode.Point;

		source.wrapMode = TextureWrapMode.Clamp;
		velocityTexture.wrapMode = TextureWrapMode.Clamp;
		tileMaxTexture.wrapMode = TextureWrapMode.Clamp;
		neighbourMaxTexture.wrapMode = TextureWrapMode.Clamp;

		var frameRateFraction = 1f / (m_enabledInFrame == Time.frameCount ? 30f : (DeltaTime * Mathf.Abs(frameRate)));

		if(!m_isDebugFrozen) {
			m_velocityStaticMaterial.SetMatrix("u_CurrentClipToPreviousHPos", m_selfPrevViewProjMat * Matrix4x4.Inverse(selfViewProjMat));
			m_velocityStaticMaterial.SetFloat("u_VelocityFramerateFraction", frameRateFraction);
			Graphics.Blit(null, velocityTexture, m_velocityStaticMaterial);
		}

		m_tileMaxMaterial.SetInt("u_TexelsInTile", maxBlurRadiusPixels);
		m_tileMaxMaterial.SetTexture(SID_VELOCITY_TEX, velocityTexture);
		Graphics.Blit(null, tileMaxTexture, m_tileMaxMaterial);

		m_neighbourMaxMaterial.SetTexture("u_TileMaxTexture", tileMaxTexture);
		Graphics.Blit(null, neighbourMaxTexture, m_neighbourMaxMaterial);

		material.SetMatrix(SID_PREV_VP, GL.GetGPUProjectionMatrix(m_selfPrevProjMat, false) * m_selfPrevViewMat);
		material.SetTexture(SID_VELOCITY_TEX, velocityTexture);
		material.SetTexture("u_TileMaxTexture", tileMaxTexture);
		material.SetTexture("u_NeighbourMaxTexture", neighbourMaxTexture);
		material.SetVector("u_MinMaxBlurRadiusPixels", new Vector4(minBlurRadiusPixels, (float)maxBlurRadiusPixels, 0f, 1f));
		var exposureFraction = Mathf.Clamp01(shutterAngle / 360f);
		material.SetVector("u_ExposureFraction", new Vector4(exposureFraction, exposureFraction * 0.5f, exposureFraction * 0.005f, 0f));
		material.SetInt("u_DebugRenderMode", (int)debugRenderMode);
		material.SetFloat("u_JitterStrength", jitterStrength);
		material.SetVector("u_VelocityScale", new Vector4(velocityScale, - velocityScale, 0f, 0f));
		material.SetFloat("u_CenterVelocityThreshold", centerVelocityThreshold);

		int desiredTaps = Mathf.RoundToInt(maxBlurRadiusPixels * pixelToTapsRatioHint);
		int blitPass = s_tapsInPass.Length - 1;
		while(blitPass > 0 && desiredTaps < s_tapsInPass[blitPass - 1]) --blitPass;

		Graphics.Blit(source, destination, material, blitPass);

		m_selfPrevViewMat = selfViewMat;
		m_selfPrevProjMat = selfProjMat;
		m_selfPrevViewProjMat = selfViewProjMat;

		if(!useDebugBuffers) {
			RenderTexture.ReleaseTemporary(neighbourMaxTexture);
			RenderTexture.ReleaseTemporary(tileMaxTexture);
			RenderTexture.ReleaseTemporary(velocityTexture);
		}
		m_velocityCamera.targetTexture = null;

		if(debugFreezeTick && !m_isDebugFrozen) {
			debugFreeze = true;
			debugFreezeTick = false;
		}

		if(m_isDebugFreezing) {
			m_isDebugFrozen = true;
			m_isDebugFreezing = false;
		}
	}
}
