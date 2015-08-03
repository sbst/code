using UnityEngine;
using System.Collections;

public class VRCamera : MonoBehaviour {
	
	public Camera[] cameras;
	public float mouseSensitivity=0.2f;
	private Vector3 oldMousePosition;
	#if !UNITY_EDITOR
	private float rotationY=0f,oldRotationY;
	#endif
	
	private  float FPSupdateInterval = 0.5F;
	private float FPSaccum   = 0; // FPS accumulated over the interval
	private int   FPSframes  = 0; // Frames drawn over the interval
	private float FPStimeleft; // Left time for current interval
	public float fps;
	public bool ifGoodFpsSoDoLensCorrection=true;
	public bool useCompassForAntiDrift=false;
	private float blackScreen=0,needBlackScreen=0;
	public Texture blackFadeTex; 
	private Transform vrCameraLocal;
	public bool isHandOriented;
	public Transform vrCameraHeading
	{
		get
		{
			Init ();
			return vrCameraLocal;
		}
	}

	private Vector3 meanAcceleration;


	
	
	#if UNITY_ANDROID && !UNITY_EDITOR
	private float initCompassHeading=0f;
	private float gyroYaccel;
	private float gyroBiasPause;
	private float oldDeltaRotation;
	private float gyroBias;

	void InitCompassHeading()
	{
		initCompassHeading = -rotationY+Input.compass.magneticHeading;
	}

	private int numInGyroBiasArray;
	void AddToGyroBiasArray(float f)
	{
		gyroBiasArray[numInGyroBiasArray]=f;
		numInGyroBiasArray++;
		if( numInGyroBiasArray>=gyroBiasArray.Length ) numInGyroBiasArray=0;
	}
	private float[] gyroBiasArray;
	private int numRecalculatesGyroBias=0;
	void RecalculateGyroBias()
	{
		float tempGyroBias=0;
		int num=0;
		for( int k=0; k<gyroBiasArray.Length; k++ )
		{
			tempGyroBias+=gyroBiasArray[k];
			num++;
		}
		gyroBias = tempGyroBias/(float)num;
		numRecalculatesGyroBias++;
	}

	#endif
	
	
	#if UNITY_IPHONE
	private float q0,q1,q2,q3;
	Quaternion rot;
	#elif UNITY_ANDROID
	private float meanDeltaGyroY;
	#endif
	
	bool initialized=false;
	public void Init()
	{
		if( initialized ) return;

		FPStimeleft = FPSupdateInterval;
		if( ifGoodFpsSoDoLensCorrection ) 
		{
			blackScreen = 1f;
			needBlackScreen = 1f;
			Invoke ("CheckFPS",1.6f);
		} 
		
		vrCameraLocal = transform.FindChild("VRCamera").transform;
				
		#if UNITY_ANDROID && !UNITY_EDITOR
		gyroBiasArray = new float[64]; for( int k=0; k<gyroBiasArray.Length; k++) gyroBiasArray[k]=0f;
		FibrumController.Init();
		if( !SystemInfo.supportsGyroscope ) transform.Find("VRCamera/WarningPlane").gameObject.SetActive(true);

		if( useCompassForAntiDrift )
		{
			Input.compass.enabled = true;
			Invoke ("InitCompassHeading",1f);
		}
		else
		{
			InvokeRepeating("RecalculateGyroBias",0.1f,0.2f);
		}

		#endif
		
		#if UNITY_IPHONE
		transform.localRotation = Quaternion.Euler(90, 0 ,0);
		#endif

		FibrumController.vrCamera = this;
		
		Invoke("SensorCalibration", 0.1f);
		FibrumInput.LoadJoystickPrefs();
		
		meanAcceleration = Input.acceleration;

		initialized = true;
	}
	
	void Start () {
		
		Init ();
	}
	
	void CheckFPS()
	{
		needBlackScreen = 0f;
		Invoke ("DisableBlackScreen",2f);
		if( fps<30f )
		{
			for( int k=0; k<cameras.Length; k++ )
			{
				if( cameras[k] != null )	cameras[k].SendMessage("DisableLensCorrection");
				else Debug.Log("VRCamera - cameras["+k+"] not assigned!");
			}
		}
	} 
	
	void DisableBlackScreen()
	{
		ifGoodFpsSoDoLensCorrection = false;
	} 
	
	void SensorCalibration ()
	{
		#if UNITY_IPHONE
		transform.localRotation = Quaternion.Euler(90f, -vrCameraLocal.localEulerAngles.y, 0f);
		#else
		transform.localRotation = Quaternion.Euler(0f, -vrCameraLocal.localEulerAngles.y, 0f);
		#endif
	}
	
	void Update () {
		
		
		#if UNITY_EDITOR
		Vector3 euler = vrCameraLocal.localRotation.eulerAngles;
		vrCameraLocal.localRotation = Quaternion.Euler(euler.x+mouseSensitivity*(oldMousePosition.y-Input.mousePosition.y),euler.y-mouseSensitivity*(oldMousePosition.x-Input.mousePosition.x),0f);
		oldMousePosition = Input.mousePosition;
		
		#elif UNITY_ANDROID && !UNITY_EDITOR
		Matrix4x4 matrix = new Matrix4x4();
		
		float[] M = FibrumController.vrs._ao.CallStatic<float[]>("getHeadMatrix");
		
		matrix.SetColumn(0, new Vector4(M[0], M[4], -M[8], M[12]) );
		matrix.SetColumn(1, new Vector4(M[1], M[5], -M[9], M[13]) );
		matrix.SetColumn(2, new Vector4(-M[2], -M[6], M[10], M[14]) );
		matrix.SetColumn(3, new Vector4(M[3], M[7], M[11], M[15]) );
		
		TransformFromMatrix (matrix, vrCameraLocal);
		float deltaRotation = vrCameraLocal.localRotation.eulerAngles.y-oldRotationY;
		while( deltaRotation>180f ) deltaRotation -= 360f;
		while( deltaRotation<-180f ) deltaRotation += 360f;
		gyroYaccel = Mathf.Lerp(gyroYaccel,(deltaRotation-oldDeltaRotation)/Time.deltaTime,Time.deltaTime);
		oldDeltaRotation = deltaRotation;
		oldRotationY = vrCameraLocal.localRotation.eulerAngles.y;

		if( Mathf.Abs(gyroYaccel)>0.2f ) gyroBiasPause = Time.realtimeSinceStartup+1f;
		if( Time.realtimeSinceStartup>gyroBiasPause )
		{
			meanDeltaGyroY = Mathf.Lerp(meanDeltaGyroY,deltaRotation/Time.deltaTime,Time.deltaTime*10f);
			AddToGyroBiasArray(meanDeltaGyroY);
		}

		rotationY += deltaRotation - gyroBias*Time.deltaTime; 
		while( rotationY>180f ) rotationY -= 360f;
		while( rotationY<-180f ) rotationY += 360f;
		if( useCompassForAntiDrift )
		{
			if( Time.realtimeSinceStartup>gyroBiasPause-0.7f )
			{
				float compassDeltaRotationY = rotationY-(Input.compass.magneticHeading-initCompassHeading);
				while( compassDeltaRotationY>180f ) compassDeltaRotationY -= 360f;
				while( compassDeltaRotationY<-180f ) compassDeltaRotationY += 360f;
				rotationY -= compassDeltaRotationY*Time.deltaTime*0.5f;
			}
		}
				
		vrCameraLocal.localRotation = Quaternion.Euler(vrCameraLocal.localRotation.eulerAngles.x,rotationY,vrCameraLocal.localRotation.eulerAngles.z);
		#elif UNITY_IPHONE
		rot = ConvertRotation(Input.gyro.attitude);
		vrCameraLocal.localRotation = rot;
		#endif


		meanAcceleration = Vector3.Lerp(meanAcceleration,Input.acceleration,Time.deltaTime*2.0f);
		if( meanAcceleration.x>0.25f && Mathf.Abs (meanAcceleration.y)<0.1f && (meanAcceleration-Input.acceleration).magnitude<0.5f )
		{
			isHandOriented=true;
		}
		else
		{
			isHandOriented=false;
		}
		
		////////////////////////////////////////
		// MEASURE FPS
		////////////////////////////////////////
		FPStimeleft -= Time.deltaTime;
		FPSaccum += Time.timeScale/Time.deltaTime;
		++FPSframes;
		if( FPStimeleft <= 0.0 )
		{
			fps = FPSaccum/FPSframes;
			FPStimeleft = FPSupdateInterval;
			FPSaccum = 0.0F;
			FPSframes = 0;
		}
		
		if( ifGoodFpsSoDoLensCorrection )
		{
			blackScreen = Mathf.Lerp(blackScreen,needBlackScreen,Time.deltaTime*5f);
		} 
	}
	
	void OnGUI()
	{
		if( ifGoodFpsSoDoLensCorrection )
		{
			if (Event.current.type.Equals(EventType.Repaint))
			{
				Graphics.DrawTexture (new Rect(0f,0f,Screen.width,Screen.height),blackFadeTex,new Rect(0f,0f,1f,1f),0,0,0,0,new Color(1f,1f,1f,blackScreen),null);
			}
		}
		#if UNITY_ANDROID && !UNITY_EDITOR
		//GUI.Box (new Rect(0f,0f,Screen.width,30f),"   rotationY="+(int)rotationY+"   deltaCompassHeading="+(int)(rotationY-(Input.compass.magneticHeading-initCompassHeading)));
		#endif
	}
	
	#if UNITY_IPHONE
	private static Quaternion ConvertRotation(Quaternion q)
	{
		return new Quaternion(q.x, q.y, -q.z, -q.w);
	}
	#endif
	
	#if UNITY_ANDROID
	public static void TransformFromMatrix(Matrix4x4 matrix, Transform trans) {
		
		trans.localRotation = QuaternionFromMatrix(matrix);
		
	}
	
	public static Quaternion QuaternionFromMatrix(Matrix4x4 m) {
		
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
		q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
		q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2;
		q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
		q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
		q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
		q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
		
		return q;
		
	}
	#endif
	
}