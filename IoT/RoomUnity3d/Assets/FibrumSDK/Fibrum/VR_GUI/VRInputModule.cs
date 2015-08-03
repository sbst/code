using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class VRInputModule : BaseInputModule {
	
	public const int kLookId = -3;
	private PointerEventData lookData;
	private GameObject lastActiveButton;
	private float lookTimer;
	public float timeToLookPress=2f;
	private GameObject circleProgressBar;

	private bool initialized=false;
	void Init()
	{
		if( initialized ) return;
		circleProgressBar = GameObject.Instantiate((GameObject)Resources.Load("FibrumResources/VR_UI_ProgressBar",typeof(GameObject))) as GameObject;
		circleProgressBar.GetComponent<Renderer>().enabled=false;
		circleProgressBar.GetComponent<Renderer>().material.SetFloat("_Cutoff",1f);
		initialized=true;
	}

	public void SetProgressBarTexture(Texture pbtex)
	{
		Init ();
		circleProgressBar.GetComponent<Renderer>().material.SetTexture("_MainTex",pbtex);
	}


		
	// name of button to use for click/submit
	public string submitButtonName = "Fire1";
		
	// name of axis to use for scrolling/sliders
	//public string controlAxisName = "Horizontal";
		
	// smooth axis - default UI move handlers do things in steps, meaning you can smooth scroll a slider or scrollbar
	// with axis control. This option allows setting value of scrollbar/slider directly as opposed to using move handler
	// to avoid this
	//public bool useSmoothAxis = true;
	// multiplier controls how fast slider/scrollbar moves with respect to input axis value
	//public float smoothAxisMultiplier = 0.01f;
	// if useSmoothAxis is off, this next field controls how many steps per second are done when axis is on
	//public float steppedAxisStepsPerSecond = 10f;
		
	// guiRaycastHit is helpful if you have other places you want to use look input outside of UI system
	// you can use this to tell if the UI raycaster hit a UI element
	private bool _guiRaycastHit;
	public bool guiRaycastHit {
		get {
			return _guiRaycastHit;
		}
	}
		
	// the UI element to use for the cursor
	// the cursor will appear on the plane of the current UI element being looked at - so it adjusts to depth correctly
	// recommended to use a simple Image component (typical mouse cursor works pretty well) and you MUST add the 
	// Unity created IgnoreRaycast component (script included in example) so that the cursor will not be see by the UI
	// event system
	public RectTransform cursor;
		
	// ignore input when looking away from all UI elements
	// useful if you want to use buttons/axis for other controls
	public bool ignoreInputsWhenLookAway = true;

	// interal vars
	//private PointerEventData lookData;
	private GameObject currentLook;
	private GameObject currentPressed;
	private GameObject currentDragging;
	private float nextAxisActionTime;

	// use screen midpoint as locked pointer location, enabling look location to be the "mouse"
	private PointerEventData GetLookPointerEventData() {
		Vector2 lookPosition;
		lookPosition.x = Screen.width/2;
		lookPosition.y = Screen.height/2;
		if (lookData == null) {
			lookData = new PointerEventData(eventSystem);
		}
		lookData.Reset();
		lookData.delta = Vector2.zero;
		lookData.position = lookPosition;
		lookData.scrollDelta = Vector2.zero;
		eventSystem.RaycastAll(lookData, m_RaycastResultCache);
		lookData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
		if (lookData.pointerCurrentRaycast.gameObject != null) {
			_guiRaycastHit = true;
		} else {
			_guiRaycastHit = false;
		}
		m_RaycastResultCache.Clear();
		return lookData;
	}
		
	// update the cursor location and whether it is enabled
	// this code is based on Unity's DragMe.cs code provided in the UI drag and drop example
	private void UpdateCursor(PointerEventData lookData) {
		if (cursor != null) {
			if (lookData.pointerCurrentRaycast.gameObject!=null) {
				RectTransform draggingPlane = lookData.pointerCurrentRaycast.gameObject.GetComponent<RectTransform>();
				Vector3 globalLookPos;
				if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, lookData.position, lookData.enterEventCamera, out globalLookPos)) {
					cursor.gameObject.SetActive(true);
					cursor.position = globalLookPos;
					cursor.rotation = draggingPlane.rotation;
				} else {
					cursor.gameObject.SetActive(false);
				}
			} else {
				cursor.gameObject.SetActive(false);
			}
		}
	}

			
	// clear the current selection
	private void ClearSelection() {
		if (eventSystem.currentSelectedGameObject) {
			eventSystem.SetSelectedGameObject(null);
		}
	}

	// select a game object
	private void Select(GameObject go) {
		ClearSelection();
		if (ExecuteEvents.GetEventHandler<ISelectHandler> (go)) {
			eventSystem.SetSelectedGameObject(go);
		}
	}
		
	// send update event to selected object
	// needed for InputField to receive keyboard input
	private bool SendUpdateEventToSelectedObject() {
		if (eventSystem.currentSelectedGameObject == null)
			return false;
		BaseEventData data = GetBaseEventData ();
		ExecuteEvents.Execute (eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
		return data.used;
	}

	void SimulatePress()
	{
		ClearSelection();
		lookData.pressPosition = lookData.position;
		lookData.pointerPressRaycast = lookData.pointerCurrentRaycast;
		lookData.pointerPress = null;
		if (currentLook != null) {
			currentPressed = currentLook;
			GameObject newPressed = null;
			newPressed = ExecuteEvents.ExecuteHierarchy (currentPressed, lookData, ExecuteEvents.pointerDownHandler);
			if (newPressed == null) {
				// some UI elements might only have click handler and not pointer down handler
				newPressed = ExecuteEvents.ExecuteHierarchy (currentPressed, lookData, ExecuteEvents.pointerClickHandler);
				if (newPressed != null) {
					currentPressed = newPressed;
				}
			} else {
				currentPressed = newPressed;
				// we want to do click on button down at same time, unlike regular mouse processing
				// which does click when mouse goes up over same object it went down on
				// reason to do this is head tracking might be jittery and this makes it easier to click buttons
				ExecuteEvents.Execute (newPressed, lookData, ExecuteEvents.pointerClickHandler);
			}
			ExecuteEvents.ExecuteHierarchy (currentPressed, lookData, ExecuteEvents.pointerUpHandler);
			if (newPressed != null) {
				lookData.pointerPress = newPressed;
				currentPressed = newPressed;
				Select(currentPressed);
			}
			// the following is for scrollbars to work right
			// apparently they go into an odd drag mode when pointerDownHandler is called
			// a begin/end drag fixes that
			if (ExecuteEvents.Execute(currentPressed,lookData, ExecuteEvents.beginDragHandler)) {
				ExecuteEvents.Execute(currentPressed,lookData,ExecuteEvents.endDragHandler);
			}
			if (ExecuteEvents.Execute(currentPressed,lookData, ExecuteEvents.beginDragHandler)) {
				lookData.pointerDrag = currentPressed;
				currentDragging = currentPressed;
			}
		}
	}

	GameObject GetCanvasOfUI(GameObject ui)
	{
		if( ui.GetComponent<Canvas>() != null ) return ui;
		else if( ui.transform.parent!=null ) return GetCanvasOfUI(ui.transform.parent.gameObject);
		else return null;
	}

	// Process is called by UI system to process events
	public override void Process() {

		bool needActivateCursor=false;
		if( cursor != null )
		{
			needActivateCursor = cursor.gameObject.activeSelf;
			cursor.gameObject.SetActive(false);
		}
		// send update events if there is a selected object - this is important for InputField to receive keyboard events
		SendUpdateEventToSelectedObject();

		// see if there is a UI element that is currently being looked at
		PointerEventData lookData = GetLookPointerEventData();
		currentLook = lookData.pointerCurrentRaycast.gameObject;

		if (needActivateCursor && cursor!=null) cursor.gameObject.SetActive(true);

		// deselect when look away
		if ( currentLook == null) {
			ClearSelection();
		}

		// handle enter and exit events (highlight)
		// using the function that is already defined in BaseInputModule
		HandlePointerExitAndEnter(lookData,currentLook);

		// update cursor
		UpdateCursor(lookData);

		if (Input.GetButtonDown (submitButtonName) && currentLook != null) {
			SimulatePress();
		}
		if( currentLook != null )
		{
			//print (currentLook.name);
			bool clickable=false;
			if(	currentLook.transform.gameObject.GetComponent<Button>()!=null ) clickable=true;
			if( currentLook.transform.parent!=null )
			{
				if( currentLook.transform.parent.gameObject.GetComponent<Button>()!=null ) clickable=true;
				if( currentLook.transform.parent.gameObject.GetComponent<Toggle>()!=null ) clickable=true;
				if( currentLook.transform.parent.gameObject.GetComponent<Slider>()!=null ) clickable=true;
				if( currentLook.transform.parent.parent!=null )
				{
					if( currentLook.transform.parent.parent.gameObject.GetComponent<Slider>()!=null )
					{
						if( currentLook.name != "Handle" )	clickable=true;
					}
					if( currentLook.transform.parent.parent.gameObject.GetComponent<Toggle>()!=null ) clickable=true;					
				}
			}

			if( clickable )
			{
				if( lastActiveButton==currentLook )
				{
					if(circleProgressBar.GetComponent<Renderer>().enabled)
					{
						RectTransform draggingPlane = lookData.pointerCurrentRaycast.gameObject.GetComponent<RectTransform>();
						Vector3 globalLookPos;
						if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, lookData.position, lookData.enterEventCamera, out globalLookPos))
						{
							circleProgressBar.transform.position = globalLookPos;
							circleProgressBar.transform.rotation = draggingPlane.rotation;
							circleProgressBar.transform.Translate(-Vector3.forward*0.1f,Space.Self);
						}
						circleProgressBar.GetComponent<Renderer>().material.SetFloat("_Cutoff",1f-((Time.realtimeSinceStartup-lookTimer)/timeToLookPress));
					}
					else if( Time.realtimeSinceStartup-lookTimer>0 ) circleProgressBar.GetComponent<Renderer>().enabled=true;
					if( Time.realtimeSinceStartup-lookTimer>timeToLookPress )
					{
						circleProgressBar.GetComponent<Renderer>().enabled=false;
						circleProgressBar.GetComponent<Renderer>().material.SetFloat("_Cutoff",1f);
						SimulatePress();
						lookTimer = Time.realtimeSinceStartup+timeToLookPress*3f;
					}
					GameObject canvasGO = GetCanvasOfUI(currentLook);
					if( canvasGO!=null )
					{
						if( canvasGO.transform.FindChild("VRpointer")!=null )	cursor = canvasGO.transform.FindChild("VRpointer").gameObject.GetComponent<RectTransform>();
					}
				}
				else
				{
					lastActiveButton=currentLook;
					lookTimer = Time.realtimeSinceStartup;
					circleProgressBar.GetComponent<Renderer>().enabled=true;
					RectTransform draggingPlane = lookData.pointerCurrentRaycast.gameObject.GetComponent<RectTransform>();
					Vector3 globalLookPos;
					if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, lookData.position, lookData.enterEventCamera, out globalLookPos))
					{
						circleProgressBar.transform.position = globalLookPos;
						circleProgressBar.transform.rotation = draggingPlane.rotation;
						circleProgressBar.transform.Translate(-Vector3.forward*0.1f,Space.Self);
					}
				}
			}
			else
			{
				lastActiveButton = null;
				circleProgressBar.GetComponent<Renderer>().enabled=false;
				circleProgressBar.GetComponent<Renderer>().material.SetFloat("_Cutoff",1f);
				eventSystem.SetSelectedGameObject(null);
			}
		}
		else
		{
			lastActiveButton = null;
			circleProgressBar.GetComponent<Renderer>().enabled=false;
			circleProgressBar.GetComponent<Renderer>().material.SetFloat("_Cutoff",1f);
			eventSystem.SetSelectedGameObject(null);
		}

		// have to handle button up even if looking away
		/*if (Input.GetButtonUp(submitButtonName)) {
			if (currentDragging) {
				ExecuteEvents.Execute(currentDragging,lookData,ExecuteEvents.endDragHandler);
				if (currentLook != null) {
					ExecuteEvents.ExecuteHierarchy(currentLook,lookData,ExecuteEvents.dropHandler);
				}
				lookData.pointerDrag = null;
				currentDragging = null;
			}
			if (currentPressed) {
				ExecuteEvents.Execute(currentPressed,lookData,ExecuteEvents.pointerUpHandler);
				lookData.rawPointerPress = null;
				lookData.pointerPress = null;
				currentPressed = null;
			}
		}*/

		// drag handling
		if (currentDragging != null) {
			ExecuteEvents.Execute (currentDragging,lookData,ExecuteEvents.dragHandler);
		}

		/*if (!ignoreInputsWhenLookAway || ignoreInputsWhenLookAway && currentLook != null) {
			// control axis handling
			_controlAxisUsed = false;
			if (eventSystem.currentSelectedGameObject && controlAxisName != null && controlAxisName != "") {
				float newVal = Input.GetAxis (controlAxisName);
				if (newVal > 0.01f || newVal < -0.01f) {
					if (useSmoothAxis) {
						Slider sl = eventSystem.currentSelectedGameObject.GetComponent<Slider>();
						if (sl != null) {
							float mult = sl.maxValue - sl.minValue;
							sl.value += newVal*smoothAxisMultiplier*mult;
							_controlAxisUsed = true;
						} else {
							Scrollbar sb = eventSystem.currentSelectedGameObject.GetComponent<Scrollbar>();
							if (sb != null) {
								sb.value += newVal*smoothAxisMultiplier;
								_controlAxisUsed = true;
							}
						}
					} else {
						_controlAxisUsed = true;
						float time = Time.unscaledTime;
						if (time > nextAxisActionTime) {
							nextAxisActionTime = time + 1f/steppedAxisStepsPerSecond;
							AxisEventData axisData = GetAxisEventData(newVal,0.0f,0.0f);
							if (!ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisData, ExecuteEvents.moveHandler)) {
								_controlAxisUsed = false;
							} 
						}
					}
				}
			}
		}*/
	}   

	
	/*// use screen midpoint as locked pointer location, enabling look location to be the "mouse"
	private PointerEventData GetLookPointerEventData() {
		Vector2 lookPosition;
		lookPosition.x = Screen.width/2;
		lookPosition.y = Screen.height/2;
		if (lookData == null) {
			lookData = new PointerEventData(eventSystem);
		}
		lookData.Reset();
		lookData.delta = Vector2.zero;
		lookData.position = lookPosition;
		lookData.scrollDelta = Vector2.zero;
		eventSystem.RaycastAll(lookData, m_RaycastResultCache);
		lookData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
		m_RaycastResultCache.Clear();
		return lookData;
	}
	
	private bool SendUpdateEventToSelectedObject() {
		if (eventSystem.currentSelectedGameObject == null)
			return false;
		BaseEventData data = GetBaseEventData ();
		ExecuteEvents.Execute (eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
		return data.used;
	}
	
	void SimulatePress()
	{
		//print ("SimulatePress");
		eventSystem.SetSelectedGameObject(null);
		if (lookData.pointerCurrentRaycast.gameObject != null) {
			GameObject go = lookData.pointerCurrentRaycast.gameObject;
			GameObject newPressed = ExecuteEvents.ExecuteHierarchy (go, lookData, ExecuteEvents.submitHandler);
			ExecuteEvents.ExecuteHierarchy (go, lookData, ExecuteEvents.beginDragHandler);
			if (newPressed == null) {
				// submit handler not found, try select handler instead
				newPressed = ExecuteEvents.ExecuteHierarchy (go, lookData, ExecuteEvents.selectHandler);
			}
			if (newPressed != null) {
				eventSystem.SetSelectedGameObject(newPressed);
			}
		}
	}
	
	public override void Process() {
		// send update events if there is a selected object - this is important for InputField to receive keyboard events
		SendUpdateEventToSelectedObject();
		PointerEventData lookData = GetLookPointerEventData();
		// use built-in enter/exit highlight handler
		HandlePointerExitAndEnter(lookData,lookData.pointerCurrentRaycast.gameObject);
		if( lookData.pointerCurrentRaycast.gameObject!=null )
		{
			print (lookData.pointerCurrentRaycast.gameObject.name);
			if( lookData.pointerCurrentRaycast.gameObject.transform.parent.gameObject.GetComponent<Button>()!=null )
			{
				if( lastActiveButton==lookData.pointerCurrentRaycast.gameObject )
				{
					if(circleProgressBar.renderer.enabled) circleProgressBar.renderer.material.SetFloat("_Cutoff",1f-((Time.realtimeSinceStartup-lookTimer)/timeToLookPress));
					else if( Time.realtimeSinceStartup-lookTimer>0 ) circleProgressBar.renderer.enabled=true;
					if( Time.realtimeSinceStartup-lookTimer>timeToLookPress )
					{
						circleProgressBar.renderer.enabled=false;
						circleProgressBar.renderer.material.SetFloat("_Cutoff",1f);
						SimulatePress();
						lookTimer = Time.realtimeSinceStartup+timeToLookPress*3f;
					}
				}
				else
				{
					lastActiveButton=lookData.pointerCurrentRaycast.gameObject;
					lookTimer = Time.realtimeSinceStartup;
					circleProgressBar.renderer.enabled=true;
					circleProgressBar.transform.position = lastActiveButton.transform.position;
					circleProgressBar.transform.rotation = lastActiveButton.transform.rotation;
				}
			}
			if( lookData.pointerCurrentRaycast.gameObject.transform.parent.parent.gameObject.GetComponent<Toggle>()!=null )
			{
				if( lastActiveButton==lookData.pointerCurrentRaycast.gameObject )
				{
					if(circleProgressBar.renderer.enabled) circleProgressBar.renderer.material.SetFloat("_Cutoff",1f-((Time.realtimeSinceStartup-lookTimer)/timeToLookPress));
					else if( Time.realtimeSinceStartup-lookTimer>0 ) circleProgressBar.renderer.enabled=true;
					if( Time.realtimeSinceStartup-lookTimer>timeToLookPress )
					{
						circleProgressBar.renderer.enabled=false;
						circleProgressBar.renderer.material.SetFloat("_Cutoff",1f);
						SimulatePress();
						lookTimer = Time.realtimeSinceStartup+timeToLookPress*3f;
					}
				}
				else
				{
					lastActiveButton=lookData.pointerCurrentRaycast.gameObject;
					lookTimer = Time.realtimeSinceStartup;
					circleProgressBar.renderer.enabled=true;
					circleProgressBar.transform.position = lastActiveButton.transform.position;
					circleProgressBar.transform.rotation = lastActiveButton.transform.rotation;
				}
			}
			if( lookData.pointerCurrentRaycast.gameObject.transform.parent.parent.gameObject.GetComponent<Slider>()!=null )
			{
				if( lastActiveButton==lookData.pointerCurrentRaycast.gameObject )
				{
					if(circleProgressBar.renderer.enabled) circleProgressBar.renderer.material.SetFloat("_Cutoff",1f-((Time.realtimeSinceStartup-lookTimer)/timeToLookPress));
					else if( Time.realtimeSinceStartup-lookTimer>0 ) circleProgressBar.renderer.enabled=true;
					if( Time.realtimeSinceStartup-lookTimer>timeToLookPress )
					{
						circleProgressBar.renderer.enabled=false;
						circleProgressBar.renderer.material.SetFloat("_Cutoff",1f);
						SimulatePress();
						lookTimer = Time.realtimeSinceStartup+timeToLookPress*3f;
					}
				}
				else
				{
					lastActiveButton=lookData.pointerCurrentRaycast.gameObject;
					lookTimer = Time.realtimeSinceStartup;
					circleProgressBar.renderer.enabled=true;
					circleProgressBar.transform.position = lastActiveButton.transform.position;
					circleProgressBar.transform.rotation = lastActiveButton.transform.rotation;
				}
			}
		}
		else
		{
			lastActiveButton = null;
			circleProgressBar.renderer.enabled=false;
			circleProgressBar.renderer.material.SetFloat("_Cutoff",1f);
			eventSystem.SetSelectedGameObject(null);
		}
	}
	*/
}