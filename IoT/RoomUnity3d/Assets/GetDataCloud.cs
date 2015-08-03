using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.IO;
using UnityEngine.UI;

public class GetDataCloud : MonoBehaviour {
	
	public string temperature;
	Text textField;

	void Start () {

//		textField = GetComponent<Text> ();
//		textField.text = "";

		temperature = "unknown";
//		textField.text = temperature;
		//GetData ();
		//textField.text = temperature;
	}

	public void GetData()
	{
		Debug.Log (temperature);
		Text text2 = GetComponent<Text> ();
		text2.text = "";
		text2.text = "NENORMAL";
		WebRequest request = WebRequest.Create("http://168.63.82.20/api/login?login=sbst&password=ebc1628c26f8515f81a5178a5abfcbd9");
		DoWithResponse(request, (response) => {
			var body = new StreamReader(response.GetResponseStream()).ReadToEnd();
			string[] resp = body.Split('\"');

			GetNext(resp[7], resp[3]);
		});
		//textField.text = temperature;
	}

	void GetNext(string user_id,string token)
	{
		WebRequest requestSecond = WebRequest.Create ("http://168.63.82.20/api/thing?user_id=" + user_id + "&token=" + token + "&did=yk5ynj69aw7z");
		DoWithResponse (requestSecond, (response) => {
			var body = new StreamReader (response.GetResponseStream ()).ReadToEnd ();
			string[] resp = body.Split('\"');
			temperature = resp[15] + " C";

		});
		//Debug.Log (temperature);
		//textField.text = temperature;
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
	/*void Update()
	{
		textField.text = temperature;
	}*/
}
