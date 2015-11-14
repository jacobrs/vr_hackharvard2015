using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class GuardianStatic : MonoBehaviour {

	
	public bool visible;
	public bool guardianStatic = true;
	public bool guardianDefaultEnabled = true;
	public List<AgentProperties> agents;

	public bool Visible {
		get { return visible; }
		set { visible = value; Run(); }
	}

	private void OnEnable() {
		Run();
	}

	private void OnValidate() {
		Run();
	}

	public void Awake() {
		Setup();
	}

	public void Setup() {
		agents = GetComponentsInChildren<AgentProperties>().ToList();
	}

	private void Run() {
		//GetComponentsInChildren<Rigidbody>().ToList().ForEach(o => {
		//	o.isKinematic = false;
		//	o.useGravity = false;
		//	o.detectCollisions = false;
		//} );
		if (agents == null|| agents.Count ==0) Setup();
		if (agents == null || agents.Count ==0) {
			Debug.LogError("agents list empty",this);
			return;
		}
		agents.ForEach(o => { 
			if (o != null) {
				o.Visible(Visible);
				o.guardianStatic = guardianStatic;
				o.guardianDefaultEnabled = guardianDefaultEnabled;
				o.Setup(); // AgentProperties may run Enable() before this and miss the change of the above bools, so forcer Setup() now to run it again
			}
		});
	}

	
}
