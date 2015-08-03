using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class VR_canvasUIcontroller : MonoBehaviour {

	//Transform vrCamera;
	Camera UI_dummyCamera;
	public float lookToPressTime=2f;
	public Texture progressBarTex;

	public void UpdatesSceneCanvases()
	{
		UI_dummyCamera = transform.Find("VRCamera/VR_UI_dummyCamera").GetComponent<Camera>();
		GameObject eventSystem = GameObject.Find("EventSystem");
		if( eventSystem!=null )
		{
			if( eventSystem.GetComponent<VRInputModule>() == null )
			{
				if( eventSystem.GetComponent<StandaloneInputModule>()!=null ) eventSystem.GetComponent<StandaloneInputModule>().enabled=false;
				if( eventSystem.GetComponent<TouchInputModule>()!=null ) eventSystem.GetComponent<TouchInputModule>().enabled=false;
				VRInputModule vrim = eventSystem.AddComponent<VRInputModule>();
				vrim.timeToLookPress = lookToPressTime;
				vrim.SetProgressBarTexture(progressBarTex);
			}
			Canvas[] cvs = GameObject.FindObjectsOfType<Canvas>();
			for( int k=0; k<cvs.Length; k++ )
			{
				if( cvs[k].renderMode == RenderMode.WorldSpace )	cvs[k].worldCamera = UI_dummyCamera;
			}
		}
	}

	// Use this for initialization
	void Start () {
		InvokeRepeating("UpdatesSceneCanvases",0f,5f);
	}
}
