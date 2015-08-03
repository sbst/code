using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System;

public class GetData : MonoBehaviour {

	public int TempData;
	// Use this for initialization
	void Start () {
		TempData = 0;	
	}

	void GetInf () {
		WebRequest request = WebRequest.Create("http://168.63.82.20/api/login?login=sbst&password=ebc1628c26f8515f81a5178a5abfcbd9");
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
}
