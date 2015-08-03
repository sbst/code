
using UnityEngine;
using System.Collections;

public class FibrumPhoneStart : MonoBehaviour {
	
	public float timeToLoadScene=10f;
	public string sceneNameToLoad;
	public Texture gameTex;
	private GameDisplayScript gds;

	void Start () {
		Invoke ("ReturnTimeScale",timeToLoadScene-3f);
		gds = GameObject.FindObjectOfType<GameDisplayScript>();
		#if UNITY_ANDROID && !UNITY_EDITOR
		FibrumController.Init();
		#endif
	}

	void ReturnTimeScale()
	{
		Time.timeScale = 1f;
		Invoke ("LoadScene",3f);
	}

	void Update()
	{
		if( Input.GetMouseButtonDown(0) )
		{
			Time.timeScale = 100f;
		}
	}

	void OnGUI()
	{
		if( Time.timeScale>1f )
		{
			GUIUtility.RotateAroundPivot(90f, Vector2.zero);
			GUI.DrawTexture(new Rect(Screen.height,0f,-Screen.height,-Screen.width),gds.calibratingTex);
		}
	}

	void LoadScene() {
		Time.timeScale = 1f;
		Application.LoadLevel(sceneNameToLoad);
	}
}
