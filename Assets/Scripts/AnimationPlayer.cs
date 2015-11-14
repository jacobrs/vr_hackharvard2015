using UnityEngine;
using System.Collections.Generic;

public class AnimationPlayer : MonoBehaviour {

	public bool startDisabled = true;
	public List<GameObject> disableList;
	public List<GameObject> enableList;
	public List<Behaviour> disableList2;
	public List<Behaviour> enableList2;
	
	private void OnEnable() {
		if (startDisabled) {
			gameObject.SetActive(false);
			startDisabled = false;
			return;
		}
		disableList.ForEach(o => {if(o!=null) o.SetActive(false); });
		enableList.ForEach(o=>{if(o!=null)o.SetActive(true); });
		disableList2.ForEach(o=>{if(o!=null)o.enabled=false; });
		enableList2.ForEach(o=>{if(o!=null)o.enabled=true; });
	}

	private void OnDisable() {
		disableList.ForEach(o=>{if(o!=null)o.SetActive(true); });
		enableList.ForEach(o=>{if(o!=null)o.SetActive(false); });
		disableList2.ForEach(o=>{if(o!=null)o.enabled=true; });
		enableList2.ForEach(o=>{if(o!=null)o.enabled=false; });
	}
}
