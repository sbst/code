using UnityEngine;
using System.Collections;

static public class FibrumController {

	static public VRSensor vrs;
	static public VRCamera vrCamera;

	static public void Init()
	{
		if( vrs == null  )
		{
			GameObject vrsGO = GameObject.Instantiate((GameObject)Resources.Load("FibrumResources/VRSensor",typeof(GameObject))) as GameObject;
			vrs = vrsGO.GetComponent<VRSensor>();
		}
	}
}
