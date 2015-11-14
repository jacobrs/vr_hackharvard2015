using UnityEngine;

public class FrameRate : MonoBehaviour
{
	public int frameRate = 60;
	void Start ()
	{
		Application.targetFrameRate = frameRate;
	}
}
