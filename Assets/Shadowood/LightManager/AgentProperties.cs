using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class AgentProperties : ObjectProperties
{
	public AudioSource sound;
	public Vector2 soundPitch;
	private float soundPitchRandom;
	private MaterialPropertyBlock m_PropertyBlock;
	public bool guardianStatic;
	public bool guardianDefaultEnabled;
	public static WayPoints wayPoints;
	public float speed = 8f;
	public float acceleration = 6f;
	public Vector2 speedMinMax = new Vector2 (6, 8);
	public Gradient activeZone;

	private float wanderDistance = 50f;

	public Vector2 awakeTime = new Vector2 (4, 4);
	public Vector2 napTime = new Vector2 (1, 1);

	private NavMeshAgent agent;
	public bool activePeriod = false;
	public bool freeSpirit = false;
	public bool randomWaypoint = false;
	public bool guardian = false;
	public Vector3 startLocation;
	public Color32 color;
	public Gradient colorRand;

	public Gradient colorFreeSpritRand = new Gradient { colorKeys = new[] { new GradientColorKey (new Color (0, 0, 1), 0f), new GradientColorKey (new Color (0.1f, 0.1f, 1), 1f) } };
	public Gradient colorWandererRand = new Gradient { colorKeys = new[] { new GradientColorKey (new Color (0, 1, 0), 0f), new GradientColorKey (new Color (0.1f, 1, 0.1f), 1f) } };

	public float randomSize;

	public Vector2 randomSizeMinMax = new Vector2 (0.2f, 1.2f);
	private Rigidbody rigigBody;

	private Color32 currentColor, colorWanderer, colorFreeSpirit;

	public bool visibleAndActive;
	public void Visible (bool visible)
	{
		visibleAndActive = visible;
		if (!visibleAndActive)
			InstantSleep ();

		if (myRenderer == null)
			myRenderer = GetComponentInChildren<Renderer> ();
		if (myRenderer != null)
			myRenderer.enabled = visibleAndActive;
		foreach (var o in lights)
		{
			o.enabled = visibleAndActive;
		}
	}

	public new void Awake ()
	{
		agent = GetComponentInChildren<NavMeshAgent> ();
		lights = GetComponentsInChildren<Light> (includeInactive: true).ToList ();
		myRenderer = GetComponentInChildren<Renderer> ();
		wayPoints = FindObjectOfType<WayPoints> ();
		rigigBody = GetComponentInChildren<Rigidbody> ();

		allAgents = FindObjectsOfType<AgentProperties> ().ToList ();
		foundAgents = allAgents.Count;
		notreadyAgents = 0;
		waitingAgents = 0;
		startLocation = transform.position;

		base.Awake ();
	}

	public void OnDisable ()
	{
		InstantSleep ();
	}

	[NonSerialized]
	public static List<AgentProperties> allAgents = null;

	[NonSerialized]
	public int foundAgents, waitingAgents, notreadyAgents;

	/// <summary>
	/// Setup some random start values for speed, size, if it is a 'freesprit' or 'randomWaypoint' style of agent
	/// </summary>
	private void Start ()
	{
		if (!Application.isPlaying)
			return;

		//DeepSleep(); // Orbs all start in DeepSleep doing nothing, no physics

		if (!guardian)
		{
			if (Random.Range (0f, 1f) > 0.8f)
				freeSpirit = true;
			if (Random.Range (0f, 1f) > 0.9f)
				randomWaypoint = true;
			randomSize = Random.Range (randomSizeMinMax.x, randomSizeMinMax.y);
			transform.localScale = new Vector3 (randomSize, randomSize, randomSize);

		}
		if (sound != null && guardianStatic)
			sound.enabled = false;
		speed = Random.Range (speedMinMax.x, speedMinMax.y);
		soundPitchRandom = Random.Range (soundPitch.x, soundPitch.y);
		//soundPitchRandom = Random.Range(soundPitch.x,soundPitch.y) + sound.pitch;
		waiting = false;
		//placeInWaypointList = -1;
		currentWaypoint = null;

	}


	public bool waiting;
	public bool forceAwake;

	public override void Run ()
	{
		//activePeriod = time > _scatterTime ? true : false;
		if (!forceAwake && activeZone != null)
			activePeriod = activeZone.Evaluate (time).r > 0.9f;
		if (forceAwake && !activePeriod)
		{
			if (debugLogness)
				Debug.Log ("forceAwake");
			activePeriod = true;
		}
		//if (forceSleep) activePeriod = false;
	}

	private bool randomPicked = false;
	public void PickRandomColorsOnce ()
	{
		if (!randomPicked)
		{
			randomPicked = true;
			if (debugLogness)
				Debug.Log ("PickRandomColorsOnce", this);
			if (!colorOveridden && colorRand != null)
				currentColor = colorRand.Evaluate (Random.Range (0f, 1f));
			else
				currentColor = color;

			colorFreeSpirit = colorFreeSpritRand.Evaluate (Random.Range (0f, 1f));
			colorWanderer = colorWandererRand.Evaluate (Random.Range (0f, 1f));
		}
	}

	public override void Setup ()
	{
		//base.Setup();
		if (debugLogness)
			Debug.Log ("Setup: guardianDefaultEnabled: " + guardianDefaultEnabled, this);
		PickRandomColorsOnce ();

		if (guardianDefaultEnabled)
		{
			WakeUp ();
		}
		else
		{
			DeepSleep ();
		}
		Update ();
	}

	private bool _wasActive = false;

	public new void OnValidate ()
	{
		if (debugLogness)
			Debug.Log ("ValidateAgent", this);
		Awake ();

		if (guardianDefaultEnabled)
			ColorLight (color); // Will update color as you change it in inspector, ( only if guardianDefaultEnabled so it doesnt interfere with agents that are set to black/sleep by default )
		if (gameObject.activeInHierarchy)
			base.OnValidate ();
	}

	public bool colorOveridden; // TODO quick hack to stop Color defaulting if you used color picker in UI
								// Called by the UI color palette
	public void ColorOverride (Color c)
	{
		currentColor = c;
		if (debugLogness)
			Debug.Log ("ColorOverride: " + currentColor + " - guardianDefaultEnabled: " + guardianDefaultEnabled, this);
		ColorLight (currentColor);
		colorOveridden = true;
		// TODO integrate this properly!
	}

	private void Update ()
	{
		if (!Application.isPlaying)
			return;
		if (!visibleAndActive)
			return;
		if (guardianStatic)
			return;
		if (sound != null && rigigBody != null)
		{
			// TODO only update pitch if enabled/active with volume?
			float vel;
			vel = rigigBody.velocity.magnitude;
			vel = agent.velocity.sqrMagnitude / 12;
			if (debugLogness)
				Debug.Log ("rigidbody.velocity.magnitude:" + vel);
			sound.pitch = Mathf.Clamp (vel, 0.1f, soundPitchRandom + 0.5f);
		}

		if (activePeriod)
		{ // If this is the time of day they should be active

			if (!_wasActive)
			{ // If this agent was not active before then WakeUp now
				if (debugLogness)
					Debug.Log ("I Awaken!", this);
				_wasActive = true;
				waiting = true;
				WakeUp ();
			}

			if (agent != null && agent.enabled && !agent.pathPending)
			{ // Code to determine if it has reached it's destination
				if (agent.remainingDistance <= agent.stoppingDistance)
				{
					if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.05f)
					{
						GetNewDestination (); // Agent has reached its waypoint so find a new destination
											  // TODO desination reached is checked every frame, maybe check every second for performance
					}
				}
			}
		}
		else
		{ // This is the time of day they should be asleep
			if (_wasActive)
			{ // If agent was active before then send it to 'GetBackToStart' now
				_wasActive = false;
				if (debugLogness)
					Debug.Log ("Hiding Time!", this);

				GetBackToStart ();
			}
			else
			{
				if (agent != null && agent.enabled && !agent.pathPending)
				{ // Code to determine if it has reached it's destination ( and is back where it started )
					if (agent.remainingDistance <= agent.stoppingDistance)
					{
						if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.05f)
						{
							if (debugLogness)
								Debug.Log ("I have returned! guardian:" + guardian, this);
							StartCoroutine (WaitingForFamily ());
							// TODO desination reached is checked every frame, maybe check every second for performance
						}
					}
				}
			}
		}
	}


	public List<AgentProperties> notReadyList;

	// TODO should be a manager running this for allAgents not each agent running its own coroutine, a lot of waste
	public IEnumerator WaitingForFamily ()
	{

		agent.enabled = false;
		waiting = true;

		if (!guardian)
		{ // Guardians float above the ground in the outer ring, none guardians float above water then sink all together
			agent.transform.position = startLocation + new Vector3 (0, 1f, 0f); // None guardian will float high above its start point above the water line
		}
		else
		{
			agent.transform.position = startLocation;
		}

		var hoverPos = agent.transform.position;

		// Loops and waits floating above water till the time of day is past a certain range or till most are ready
		while (!activePeriod && (time < 0.01 || time > 0.3))
		{ // They all sleep at set time // TODO hardcoded values atm so add inspector gradient to specify this range
			var wiggle = new Vector3 (0, Random.Range (0f, 0.3f), 0); // Causes a slight wiggle up and down as they wait to drop down
			agent.transform.position = hoverPos + wiggle;

			int notready = 0; // Local counter for notreadyAgents
#if UNITY_EDITOR
			notReadyList = new List<AgentProperties> (); // For debug in editor only
#endif
			if (allAgents != null)
			{
				foreach (var agentProperties in allAgents)
				{
					if (agentProperties == null)
						continue;
					if (agentProperties.guardianStatic)
						continue; // guardianStatic do not move just for animation shown still, so ignore them

					if (!agentProperties.waiting)
					{
						notready++;
						notreadyAgents++; // Global counter only for debug
#if UNITY_EDITOR
						notReadyList.Add (agentProperties); // For debug in editor only
#endif
					}
					else
					{
						waitingAgents++; // Global counter only for debug
					}
				}
			}

			//TODO have below work only if dayLight 
			if (notready < 5)
			{ // Needed if you have no autoCycle of time of Day, say you just dragging slider around freely and leave it at night
				if (debugLogness)
					Debug.Log ("Most are ready now: " + notready);
				break; // Leave this 'while' loop now
			}

			yield return new WaitForSeconds (0.4f); // loop after a delay
		}

		// Reset
		waitingAgents = 0;
		notreadyAgents = 0;
#if UNITY_EDITOR
		notReadyList = new List<AgentProperties> ();
#endif

		if (debugLogness)
			Debug.Log ("Left while loop: time: " + time + " activePeriod: " + activePeriod, this);

		if (activePeriod)
		{ // Time of day changed while waiting to sleep, must now reactivate! ( when user drags slider around causing great distress )
			if (debugLogness)
				Debug.Log ("Leave me alone!", this);
			WakeUp ();
			yield return null;
		}
		else
		{
			if (debugLogness)
				Debug.Log ("Time to go");
			yield return new WaitForSeconds (1f); // Wait and then drop, causes staggered dropping and means late arrivals still pause a bit before dropping
			agent.transform.position = startLocation; // Drop, AgentTween will smooth this to a steady drop
			DeepSleep ();
			yield return null;
		}
	}

	public bool inDeepSleep; // Purely informative bool - Resting under water with no physics

	/// <summary>
	/// Disable Physics and Blackout setting state sleeping to true, counter this with WakeFromDeepSleep()
	/// </summary>
	public void DeepSleep ()
	{
		if (debugLogness)
			Debug.Log ("DeepSleep");
		ColorLight (Color.black);
		_wasActive = false;
		inDeepSleep = true;
		if (rigigBody != null)
		{
			rigigBody.useGravity = false;
			rigigBody.isKinematic = false;
			rigigBody.detectCollisions = false;
		}
	}

	/// <summary>
	/// Jumps instantly to original position and DeepSleep()'s
	/// Animation will disable this for example so needs to jump instantly with no transition )
	/// </summary>
	public void InstantSleep ()
	{
		if (agent != null)
		{
			agent.enabled = false;
			agent.Warp (startLocation);
			agent.transform.position = startLocation;
		}
		_wasActive = false;
		transform.position = startLocation;
		DeepSleep ();
	}

	/// <summary>
	/// Starts urgent journey back to the start position to hide
	/// </summary>
	private void GetBackToStart ()
	{ // This runs when activePeriod was true and is now false
		if (debugLogness)
			Debug.Log ("Eeep Hide!", this);
		if (agent != null && agent.enabled)
		{
			agent.SetDestination (startLocation);
			agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
			agent.avoidancePriority = Random.Range (1, 2);
			agent.radius = 0.1f;
			agent.speed = speed * 2f;
			agent.acceleration = acceleration * 3f;
		}
	}

	/// <summary>
	/// Wake enable Physics and Navagent and set Color
	/// </summary>
	public void WakeUp ()
	{
		if (debugLogness)
			Debug.Log ("I think therefore I am", this);

		if (randomWaypoint)
			currentColor = colorWanderer;
		if (freeSpirit)
			currentColor = colorFreeSpirit;

		ColorLight (currentColor);

		waiting = false;
		inDeepSleep = false;

		if (!guardianStatic)
		{
			// enable physics
			if (rigigBody != null)
			{
				rigigBody.useGravity = true;
				rigigBody.isKinematic = true;
				rigigBody.detectCollisions = true;
			}

			// enable nav agent
			if (agent != null)
			{
				agent.enabled = true;
				agent.speed = speed;
				agent.acceleration = acceleration;
				agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
				agent.avoidancePriority = Random.Range (60, 99);
				agent.radius = 0.4f;
			}
		}
	}

	public List<Light> lights;

	private Renderer myRenderer;
	public Material originalMaterial; // TODO Not needed now using Material property block

	// TODO ease in and out Color and Sound change
	private void ColorLight (Color colorin)
	{
		if (debugLogness)
			Debug.Log ("Coloring: " + colorin, this);

		if (lights == null || lights.Count == 0)
			lights = GetComponentsInChildren<Light> (includeInactive: true).ToList ();
		if (lights == null || lights.Count == 0)
			Debug.LogWarning ("No Lights Founds", this);

		lights.ForEach (o => o.color = colorin);

		if (myRenderer == null)
			myRenderer = GetComponentInChildren<Renderer> ();
		if (myRenderer != null)
		{
			if (m_PropertyBlock == null)
				m_PropertyBlock = new MaterialPropertyBlock ();
			m_PropertyBlock.SetColor ("_EmissionColor", colorin * 7.8f);
			myRenderer.SetPropertyBlock (m_PropertyBlock);
			DynamicGI.SetEmissive (myRenderer, colorin * 7.8f);

		}
		Sound ((colorin.r + colorin.g + colorin.b) / 3);

	}

	private void Sound (float val)
	{
		if (!Application.isPlaying)
			return;
		if (sound == null)
			return;
		sound.volume = val;
		if (val == 0)
		{
			sound.Stop ();
		}
		else
		{
			if (sound.gameObject.activeInHierarchy && sound.enabled)
				sound.PlayScheduled (Random.Range (0f, sound.clip.length));
		}
		sound.pitch = soundPitchRandom;
		//agent.velocity.sqrMagnitude

	}

	public WayPoint currentWaypoint;
	//public int placeInWaypointList = -1;

	private void GetNewDestination ()
	{
		//Debug.Log("GetNewDestination", this);
		if (!agent.enabled)
			return;
		if (freeSpirit || wayPoints == null || wayPoints.wayPoints.Count == 0)
		{
			float randomX = Random.Range (-wanderDistance, wanderDistance);
			float randomZ = Random.Range (-wanderDistance, wanderDistance);
			agent.SetDestination (new Vector3 (randomX, 0, randomZ));
			if (debugLogness)
				Debug.Log ("I have no idea where I am going!", this);
		}
		else
		{
			//if (placeInWaypointList != -1) {
			//	placeInWaypointList++;
			//	if (placeInWaypointList > wayPoints.wayPoints.Count) placeInWaypointList = 0;
			//	_agent.SetDestination(wayPoints.wayPoints[placeInWaypointList].transform.position);
			//}
			if (currentWaypoint == null || randomWaypoint)
			{
				//	int randomStart = Random.Range(0, wayPoints.wayPoints.Count);
				//	_agent.SetDestination(wayPoints.wayPoints[randomStart].transform.position);
				//}
				if (debugLogness)
					Debug.Log ("new first point", this);

				int randomStart = Random.Range (0, wayPoints.wayPoints.Count);

				//placeInWaypointList = randomStart;
				currentWaypoint = wayPoints.wayPoints[randomStart];
				bool looking = true;
				int lookCount = 0;
				Vector2 randomRange = new Vector2 (0.5f, 4f);
				while (looking)
				{
					lookCount++;

					randomRange.y = randomRange.y / lookCount; // Reduces random if it fails to find a valid destination
															   //if (lookCount > 1) Debug.Log("Noooo: " + lookCount + ":" + randomRange, this);
					looking = agent.SetDestination (currentWaypoint.transform.position + new Vector3 (Random.Range (randomRange.x, randomRange.y), 0, Random.Range (randomRange.x, randomRange.y)));
					if (lookCount > 30)
					{
						looking = false;
						//Debug.Log("Bailed: " + lookCount + ":" + randomRange, this);
					}
				}
			}
			else
			{
				var offset = Random.Range (0, currentWaypoint.connectedWayPoints.Count);
				if (debugLogness)
					Debug.Log ("new point:" + currentWaypoint.connectedWayPoints.Count + ":" + offset, this);

				var currentWaypoint2 = currentWaypoint.connectedWayPoints[offset];
				currentWaypoint = currentWaypoint2;
				agent.SetDestination (currentWaypoint.transform.position + new Vector3 (Random.Range (0.1f, 5f), 0, Random.Range (0.1f, 5f)));
			}
		}

	}


}