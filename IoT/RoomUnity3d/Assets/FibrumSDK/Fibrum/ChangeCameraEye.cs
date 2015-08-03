using UnityEngine;
using System.Collections;

public class ChangeCameraEye : MonoBehaviour {

	public float distanceBetweenLens;
	public float lastScreenWidth;
	private float initCameraRectXmin;

	// Use this for initialization
	void Start () {
		GetComponent<Camera>().layerCullSpherical = true;
		initCameraRectXmin = GetComponent<Camera>().rect.xMin;
		InvokeRepeating("ProcessCameraView",0.5f,2f);
	}

	void ProcessCameraView()
	{
		if( Screen.width==lastScreenWidth ) return;
		float deviceDiagonal = Mathf.Sqrt((float)(Screen.width*Screen.width)+(float)(Screen.height*Screen.height))/Screen.dpi;
		//Debug.Log("Diagonal="+deviceDiagonal);
		if (Application.isEditor || (deviceDiagonal<3.9f && deviceDiagonal>7.1f) )
		{
			transform.localPosition = new Vector3(-transform.localPosition.x,transform.localPosition.y,transform.localPosition.z);
			GetComponent<Camera>().rect = new Rect(0.5f-initCameraRectXmin,0,0.5f,GetComponent<Camera>().rect.height);
		}
		else
		{
			float screenLength = (Screen.width/Screen.dpi)*25.4f;
			float viewPortCenter = (distanceBetweenLens/2f)/screenLength;
			float viewPortHalfSize = Mathf.Min (viewPortCenter,0.5f-viewPortCenter);
			float screenHeight = (Screen.height/Screen.dpi)*25.4f;
			float viewPortYHalfSize = Mathf.Min (0.5f,0.5f*distanceBetweenLens/screenHeight);
			GetComponent<Camera>().rect = new Rect(0.5f+(initCameraRectXmin*4f-1f)*viewPortCenter-viewPortHalfSize,0.5f-viewPortYHalfSize,viewPortHalfSize*2f,viewPortYHalfSize*2f);
		}
		lastScreenWidth = Screen.width;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
