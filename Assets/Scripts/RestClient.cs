using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;

public class RestClient : MonoBehaviour {
	public string LocalURL = "127.0.0.1";
	public int LocalPort = 5000; // 0 = no port
	public string RemoteURL = "jsonplaceholder.typicode.com";
	public int RemotePort = 0; //0 = no port
	public bool UseLocal = false;
	public bool UseUnityWebRequest = true;

	JsonSerializerSettings _jsonSettings = new JsonSerializerSettings{
		//			TypeNameHandling = TypeNameHandling.Arrays,
	};

	string BuildUrl(string withCall)
	{
		var url = UseLocal ? LocalURL : RemoteURL;
		var port = UseLocal ? LocalPort : RemotePort;
		string fullUrl;
		if (port != 0)
			fullUrl = "http://" + url + ":" + port + "/" + withCall;
		else
			fullUrl = "http://" + url + "/" + withCall;
		return fullUrl;
	}

	// Use this for initialization
	void Start () {
		
	}


	
	// Update is called once per frame
	void Update () {
//		return;

		// Get example
		var URL = BuildUrl ("posts/1");		// requesting post with id 1
		var message = Get<Message>(URL);
		if (message != null)
			print (message.title + ": " + message.body);

		// TODO post example
//		var json = JsonConvert.SerializeObject(objectToPost, _jsonSettings);

	}

	//		const float kTimeOut = 30f;
	const float kTimeOut = float.MaxValue;
	private T Get<T>(string url) where T: class
	{
		UnityEngine.Profiling.Profiler.BeginSample("PostServer");
		T res;
		if (UseUnityWebRequest)
			res = GetUsingUnityWebRequest<T>(url); // New Unity
		else
			res = GetUsingWWW<T>(url);  // Old Unity
		UnityEngine.Profiling.Profiler.EndSample();
		return res;
	}


	private T Post<T>(string url, string json) where T: class
	{
		UnityEngine.Profiling.Profiler.BeginSample("PostServer");
		T res;
		if (UseUnityWebRequest)
			res = PostUsingUnityWebRequest<T>(url, json); // New Unity
		else
			res = PostUsingWWW<T>(url, json);  // Old Unity
		UnityEngine.Profiling.Profiler.EndSample();
		return res;
	}

	private T GetUsingUnityWebRequest<T>(string url) where T: class
	{
		var www = UnityWebRequest.Get(url);
		www.SetRequestHeader( "Content-Type", "application/json");
//		www.SetRequestHeader( "Connection", "Keep-Alive"); // not suported :(
		www.Send();
		var startTime = DateTime.UtcNow;
		while (!www.isDone) { 
			var elpased = DateTime.UtcNow-startTime;
			if (elpased.TotalSeconds >=kTimeOut) {
				// time out
				Debug.Log("time out");
				return null;
			}
		}
		if (www.error == null){
			//convert server response
			var response = JsonConvert.DeserializeObject<T>(www.downloadHandler.text);
			www.Dispose();
			return response;
		} else {
			//Something goes wrong, print the error response
			Debug.Log(www.error);
		}
		www.Dispose();
		return null;

	}
	private T PostUsingUnityWebRequest<T>(string url, string json) where T: class
	{
		json = "{\"requestData\":" + json + "}";
		WWWForm form = new WWWForm();
		form.AddField("username", "name");
		var www = UnityWebRequest.Post(url,form);
		www.SetRequestHeader( "Content-Type", "application/json");
		//			www.SetRequestHeader( "Connection", "Keep-Alive"); // not suported :(
		byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
		UploadHandlerRaw upHandler = new UploadHandlerRaw(data);
		upHandler.contentType = "application/json";
		www.uploadHandler = upHandler;
		www.Send();
		var startTime = DateTime.UtcNow;
		while (!www.isDone) { 
			var elpased = DateTime.UtcNow-startTime;
			if (elpased.TotalSeconds >=kTimeOut) {
				// time out
				Debug.Log("time out");
				return null;
			}
		}
		if (www.error == null){
			//convert server response
			var response = JsonConvert.DeserializeObject<T>(www.downloadHandler.text);
			www.Dispose();
			return response;
		} else {
			//Something goes wrong, print the error response
			Debug.Log(www.error);
		}
		www.Dispose();
		return null;

	}
	private T GetUsingWWW<T>(string url) where T: class
	{
		Dictionary<string,string> parameters = new Dictionary<string, string>();
		parameters.Add( "Content-Type", "application/json" );
		parameters.Add( "Connection", "Keep-Alive");
		var www = new WWW(url, null, parameters);
		var startTime = DateTime.UtcNow;
		while (!www.isDone) { 
			var elpased = DateTime.UtcNow-startTime;
			if (elpased.TotalSeconds >=kTimeOut) {
				// time out
				Debug.Log("time out");
				return null;
			}
		}
		if (www.error == null){
			//convert server response
			var response = JsonConvert.DeserializeObject<T>(www.text);
			www.Dispose();
			return response;
		} else {
			//Something goes wrong, print the error response
			Debug.Log("WWW.error: " + www.error);
		}
		www.Dispose();
		return null;
	}
	private T PostUsingWWW<T>(string url, string json) where T: class
	{
		Dictionary<string,string> parameters = new Dictionary<string, string>();
		parameters.Add( "Content-Type", "application/json" );
		parameters.Add( "Connection", "Keep-Alive");
		byte[] postData = System.Text.Encoding.UTF8.GetBytes (json);
		var www = new WWW(url, postData, parameters);
		var startTime = DateTime.UtcNow;
		while (!www.isDone) { 
			var elpased = DateTime.UtcNow-startTime;
			if (elpased.TotalSeconds >=kTimeOut) {
				// time out
				Debug.Log("time out");
				return null;
			}
		}
		if (www.error == null){
			//convert server response
			var response = JsonConvert.DeserializeObject<T>(www.text);
			www.Dispose();
			return response;
		} else {
			//Something goes wrong, print the error response
			Debug.Log("WWW.error: " + www.error);
		}
		www.Dispose();
		return null;
	}
}

