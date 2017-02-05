using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using Newtonsoft.Json;
using System.Linq;

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
		new Message{title = "title 0", id = 0, userId = 0, body = "hello world from post 0"},
		new Message{title = "title 1", id = 1, userId = 1, body = "hello world from post 1"},
		new Message{title = "title 2", id = 2, userId = 2, body = "hello world from post 2"},
		new Message{title = "title 3", id = 3, userId = 3, body = "hello world from post 3"},
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
				else if (ctx.Request.HttpMethod == "POST") {
					ProcessPost (ctx);
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
				body = "not found"
			};

			var type = GetType(ctx.Request.Url);
			var itemId = GetId(ctx.Request.Url);
					
			if (type == "posts") {
				response = _messageDb.FirstOrDefault (x=>x.id == itemId) ?? response;
			}

			// return result
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

	void ProcessPost(HttpListenerContext ctx)
	{
		try
		{
			print("recived POST:" + ctx.Request.Url.PathAndQuery);

			using (var reader = new System.IO.StreamReader(ctx.Request.InputStream)){
				string json = reader.ReadToEnd();
				var input = JsonConvert.DeserializeObject<Dictionary<string,Message>>(json);
				var item = input.Values.First<Message>();

				var type = GetType(ctx.Request.Url);
				var itemId = GetId(ctx.Request.Url);
				if (type == "posts") {
					var existing = _messageDb.FirstOrDefault (x=>x.id == itemId);
					if (existing != null)
						_messageDb.Remove(existing);
					_messageDb.Add(item);
				}

				// return result
				json = JsonConvert.SerializeObject(item, _jsonSettings);
				var responseMsgBytes = System.Text.Encoding.UTF8.GetBytes(json);
				ctx.Response.ContentLength64 = responseMsgBytes.Length;  //Response msg size
				ctx.Response.OutputStream.Write(responseMsgBytes, 0, responseMsgBytes.Length);
				ctx.Response.OutputStream.Close();
				ctx.Response.Close();
			}

		}
		catch (Exception ex)
		{
			print (ex.Message);
			ctx.Response.Close();
		}
	}



	string GetType(Uri uri)
	{
		var type = uri.Segments[1];
		type = new string(type.Where(c => c != '/').ToArray());
		return type;
	}
	int GetId(Uri uri)
	{
		int i = Int32.Parse (uri.Segments [2]);
		return i;
	}
//	void ToComplex()
//	{
//		string command = string.Empty;
//		int itemIdx = -1;
//		foreach (var rawSegment in ctx.Request.Url.Segments) {
//
//			var segment = new string(rawSegment.Where(
//				c => c != '/'
//			).ToArray());
//			int i;
//			if (segment == "")
//				continue;
//			if (string.IsNullOrEmpty(command))
//				command = segment;
//			else if (Int32.TryParse(segment, out i)) {
//				itemIdx = i;
//				break;
//			}
//		}
//	}
		


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
