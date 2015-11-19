using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class BoltTest : Bolt.GlobalEventListener
{
	public const ushort LAN_PORT = 27001;

	private bool _continueSearching;

	public Button _ServerButton;
	public Button _ClientButton;

	public Text _StatusText;
	List<string> _statuses;

	public Button _SendEvent;
	public Button _QuitButton;

	public GameObject _Waiting;

	void Start()
	{
		LogMe();

		setFor(true);

		_ServerButton.onClick.AddListener( () => startBolt(true) );
		_ClientButton.onClick.AddListener( () => startBolt(false) );

		_SendEvent.onClick.AddListener( () => sendEvent() );
		_QuitButton.onClick.AddListener( () => StartCoroutine( quitBolt() ) );
	}

	void setFor(bool initial)
	{
		LogMe();

		if(initial)
		{
			_statuses = new List<string>();
		}

		_Waiting.SetActive(false);
		_ServerButton.transform.parent.gameObject.SetActive(initial);
		_StatusText.transform.parent.gameObject.SetActive(!initial);
	}

	public override void BoltStartBegin()
	{
		base.BoltStartBegin();
		LogMe();

		setFor(false);
	}

	public override void BoltStartFailed()
	{
		base.BoltStartFailed();
		LogMe();

		updateStatus( "Bolt start has failed" );
	}

	public override void BoltStartDone()
	{
		base.BoltStartBegin();
		LogMe();

		string bit;

		BoltNetwork.EnableLanBroadcast();

		if(BoltNetwork.isServer)
		{
			bit = "server";
		}
		else
		{
			bit = "client";

			_continueSearching = true;

			StartCoroutine( startSearching() );
		}

		updateStatus("Bolt has begun, I'm a " + bit);
	}

	public override void Connected(BoltConnection connection)
	{
		base.Connected(connection);
		LogMe();

		updateStatus(connString(connection) + " Connected");
	}

	public override void Disconnected(BoltConnection connection)
	{
		base.Disconnected(connection);
		LogMe();

		updateStatus(connString(connection) + " Disconnected");
	}

	public override void OnEvent(TestyBoltEvent evnt)
	{
		base.OnEvent(evnt);
		if(!evnt.FromSelf)
		{
			updateStatus("event received: " + evnt);
		}
	}

	void sendEvent()
	{
		TestyBoltEvent evnt = TestyBoltEvent.Create();
		evnt.RandomValue = UnityEngine.Random.Range(0, 100);
		evnt.Send();
		updateStatus("event sent: " + evnt);
	}

	string connString(BoltConnection conn)
	{
		return conn.RemoteEndPoint.ToString().Split('|')[0] + "]";
	}

	void updateStatus(string s)
	{
		_statuses.Add(s);
		while(5 < _statuses.Count)
		{
			_statuses.RemoveAt(0);
		}

		_StatusText.text = "Status\n" + string.Join( "\n", _statuses.ToArray () );
	}

	void startWaiting()
	{
		LogMe();

		_Waiting.SetActive(true);
	}

	void startBolt(bool server)
	{
		LogMe();

		startWaiting();

		if(server)
		{
			BoltLauncher.StartServer( new UdpKit.UdpEndPoint(UdpKit.UdpIPv4Address.Any, LAN_PORT) );
		}
		else
		{
			BoltLauncher.StartClient();
		}
	}

	IEnumerator startSearching()
	{
		LogMe("startSearching");

		yield return null;
		int tries = 10;

		while(_continueSearching)
		{
			yield return new WaitForSeconds(1.0f);

			//if we have a session then auto-connect
			if(BoltNetwork.SessionList.Count > 0)
			{
				foreach(var s in BoltNetwork.SessionList)
				{
					BoltNetwork.Connect(s.Value.LanEndPoint);

					_continueSearching = false;
					yield break;
				}
			}

			tries--;
			print("search failed, num tries remaining: " + tries);
			if(0 == tries)
			{
				StartCoroutine( quitBolt() );

				yield break;
			}
		}
	}

	IEnumerator quitBolt()
	{
		LogMe("quitBolt");

		startWaiting();

		tryBoltBit(BoltNetwork.DisableLanBroadcast);
		tryBoltBit(BoltLauncher.Shutdown);

		yield return new WaitForSeconds(0.5f);

		setFor(true);
	}

	void tryBoltBit(Action action)
	{
		if(BoltNetwork.isRunning)
		{
			try
			{
				action.Invoke();
			}
			catch(Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}
	}

	void LogMe(string s=null)
	{
		UnityEngine.Debug.Log( string.Format(
			"[ {0} ]",
			s ?? new System.Diagnostics.StackFrame(1, true).GetMethod().Name
		) );
	}
}
