using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using Newtonsoft.Json;

public class RestServer : MonoBehaviour {
//	public string LocalURL = "127.0.0.1";
	public int LocalPort = 5000; // 0 = no port
//	public string RemoteURL = "jsonplaceholder.typicode.com";
	public int RemotePort = 0; //0 = no port
	public bool UseLocal = true;

	HttpListener _httpListener;
	object _listenLock = new object ();

	JsonSerializerSettings _jsonSettings = new JsonSerializerSettings{
		//			TypeNameHandling = TypeNameHandling.Arrays,
	};

	List<Message> _messageDb = new List<Message>{
	};

	// Use this for initialization
	void Start () {
		ServerStart ();

	}
	
	// Update is called once per frame
	void Update () {
		
	}


	void ServerStart()
	{
		var port = UseLocal ? LocalPort : RemotePort;
		var url = "http://*:";
		if (port != 0)
			url += port.ToString () + "/";
		_httpListener = new HttpListener();
		_httpListener.Prefixes.Add(url);
//		if (_auth != null) _httpListener.AuthenticationSchemes = AuthenticationSchemes.Basic;

		_httpListener.Start();
		if (!_httpListener.IsListening) {
			print("ERROR: Server could not start on port " + port);
		}

		_httpListener.BeginGetContext(Process, _httpListener);
		print("Server is up on port " + port);

	}

	void Process(IAsyncResult result)
	{
		lock (_listenLock) {
			try {
				HttpListener listener = (HttpListener)result.AsyncState;

				HttpListenerContext ctx = listener.EndGetContext (result);

				if (ctx.Request.HttpMethod == "GET") {
					ProcessGet (ctx);
				}
				else {
					print("HttpMethod not implement: " + ctx.Request.HttpMethod);

				}

				listener.BeginGetContext (Process, listener);
			} catch (Exception ex) {
				print (ex.Message);
				throw ex;
			}
		}
	}

	void ProcessGet(HttpListenerContext ctx)
	{
		try
		{
			print("recived GET:" + ctx.Request.Url.PathAndQuery);
			var response = new Message{
				body = "hello world"
			};
			var json = JsonConvert.SerializeObject(response, _jsonSettings);

			var responseMsgBytes = System.Text.Encoding.UTF8.GetBytes(json);
			ctx.Response.ContentLength64 = responseMsgBytes.Length;  //Response msg size
			ctx.Response.OutputStream.Write(responseMsgBytes, 0, responseMsgBytes.Length);
			ctx.Response.OutputStream.Close();
			ctx.Response.Close();
		}
		catch (Exception ex)
		{
			print (ex.Message);
			ctx.Response.Close();
		}
	}

	void OnDestroy()
	{
		if (_httpListener != null) {
			lock (_listenLock) {
				_httpListener.Stop ();
				_httpListener.Close ();
				_httpListener = null;
			}
		}
	}

}
