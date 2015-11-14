using UnityEngine;

/// <summary>
/// Simple script sets the playerHeight / Moves the camera, in case you change it by mistake
/// </summary>
public class PlayeHeight : MonoBehaviour {
	public float height = 0.8f;
	
	void Start () {
		if(Application.isPlaying) transform.localPosition = new Vector3(0, height, 0);
	}
}
