using UnityEngine;
using System.Linq;

/// <summary>
/// Simple batch script to find all child AgentProperties and set guardian to true/false
/// </summary>
[ExecuteInEditMode]
public class Guardian : MonoBehaviour {
	public bool guardian = true;

	private void OnEnable() {
		//if(!Application.isPlaying)
		GetComponentsInChildren<AgentProperties>().ToList().ForEach(o => o.guardian = guardian);
	}
}
