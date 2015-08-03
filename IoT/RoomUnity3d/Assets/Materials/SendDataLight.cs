using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.IO;

public class SendDataLight : MonoBehaviour
{
	GameObject bedlighter1;
	GameObject bedlighter2;
	GameObject mainlighter;
	public bool stateLight;
	
	public void Start()
	{
		bedlighter1 = GameObject.Find("BedLight");
		bedlighter2 = GameObject.Find("BedLight 1");
		mainlighter = GameObject.Find ("Point light");
		stateLight = false;
		//bedlighter1.SetActive (false);
		//bedlighter2.SetActive (false);
		mainlighter.SetActive (false);
	}

	public void Toggle()
	{
		if (stateLight == true)
		{
			Send ("501");
			stateLight = false;
		} else 
		{
			Send ("458");
			stateLight = true;
		}
	}

	void Send(string arg)
	{

		if (arg == "458")
		{
			bedlighter1.SetActive (true);
			bedlighter2.SetActive (true);
			mainlighter.SetActive (true);
		}
		else
		{
			bedlighter1.SetActive (false);
			bedlighter2.SetActive (false);
			mainlighter.SetActive (false);
		}
		WebRequest request = WebRequest.Create("http://168.63.82.20/server/income/?did=51pgvpo06ue2&action=put&value=" + arg);
		DoWithResponse(request, (response) => {
			var body = new StreamReader(response.GetResponseStream()).ReadToEnd();
		});
	}
	
	void DoWithResponse(WebRequest request, Action<HttpWebResponse> responseAction)
	{
		Action wrapperAction = () =>
		{
			request.BeginGetResponse(new AsyncCallback((iar) =>
			                                           {
				var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
				responseAction(response);
			}), request);
		};
		wrapperAction.BeginInvoke(new AsyncCallback((iar) =>
		                                            {
			var action = (Action)iar.AsyncState;
			action.EndInvoke(iar);
		}), wrapperAction);
	}
	void OnApplicationQuit()
	{
		this.Send ("501");
	}
}
