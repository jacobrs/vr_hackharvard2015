using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class ToggleReflections : MonoBehaviour {
	[Header("You only need one or the other, but you can enable both")]
	public bool useSsrr;
	[Header("At runtime you can toggle with default keypress R and Y")]
	public bool usePlanar;
	
	[Space(10)]
	public Behaviour ssrr;
	public GameObject planarReflection;
	
	void Start () {
		Run();
	}
	
	
	void Update () {
		if (CrossPlatformInputManager.GetButtonDown("PlanarReflections")) {
			useSsrr = !useSsrr;
			Run();
		}
		if (CrossPlatformInputManager.GetButtonDown("ToggleSSRR")) {
			usePlanar = !usePlanar;
			Run();
		}
	}



	private void Run() {
		if (ssrr != null) ssrr.enabled = useSsrr;
		if (planarReflection != null) planarReflection.SetActive(usePlanar);
	}

	private void OnValidate() {
		//if (useSsrr == false && usePlanar == false) useSsrr = true;
		Run();
	}
}
