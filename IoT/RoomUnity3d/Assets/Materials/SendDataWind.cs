using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.IO;


public class SendDataWind : MonoBehaviour
{
	public bool stateWind;

	void Start()
	{
		stateWind = false;
	}

	public void Toggle()
	{
		if (stateWind == true)
		{
			Send ("501");
			stateWind = false;
		} else 
		{
			Send ("458");
			stateWind = true;
		}
	}

	public void Send(string arg)
	{
		WebRequest request = WebRequest.Create("http://168.63.82.20/server/income/?did=ictvj8authr4&action=put&value=" + arg);
		DoWithResponse(request, (response) => {
			var body = new StreamReader(response.GetResponseStream()).ReadToEnd();
			Debug.Log(body);
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

