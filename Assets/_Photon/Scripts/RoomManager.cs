using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks
{
	public static RoomManager Instance;
	PhotonView PV;

	List<PhotonView> CurWonPlayer = new List<PhotonView>();
	public int SpotsLeft;
	public int MaxSpotsLeft;
	Text SpotsLeftText;

	Text PlayerCountText;

	public List<int> MapIDs = new List<int>();

	bool HasMatchStarted = false;

	public int playableRoom;
	public int spotVal;

	void Awake()
	{
		if(Instance)
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		Instance = this;

		playableRoom = MapIDs[Random.Range(0, MapIDs.Count)];

		PV = GetComponent<PhotonView>();
	}

	public void ResetAll()
	{
		CurWonPlayer.Clear();
		SpotsLeft = MaxSpotsLeft;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if(scene.name.Contains("Game")) // We're in the game scene
		{
			Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
			HasMatchStarted = true;
			spotVal = Random.Range(1, 5);
			FindSpotVal(PhotonNetwork.CurrentRoom.PlayerCount);
			UpdateMaxSpotsLeft();
			
			PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
			SpotsLeftText = GameObject.FindGameObjectWithTag("SpotsLeft").GetComponent<Text>();
		}
		if(scene.name == "Winner")
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
			Invoke("StopAllMove", 0.2f);
		}
	}

	void Update()
	{
		if(SceneManager.GetActiveScene().name.Contains("Game"))
		{
			if(SpotsLeftText != null)
			{
				SpotsLeftText.text = $"Spots left: {SpotsLeft}";
			}
			if(HasMatchStarted)
			{
				if(PhotonNetwork.CurrentRoom.PlayerCount < 2)
				{
					SceneManager.LoadScene("Winner");
				}
			}
		}

		//Lock and Unlock Cursor
        if(Input.GetKeyDown(KeyCode.Escape))
        {
			GameObject exitGame = GameObject.FindGameObjectWithTag("ExitGame");
            if(Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked; 
				
				if(exitGame != null)
				{
					exitGame.SetActive(true);
				}
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

				if(exitGame != null)
				{
					exitGame.SetActive(false);
				}
            }
        }
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("OnDisconnected(" + cause + ")");
		SceneManager.LoadScene(1);
	}
		
	public void ReturnToMenu()
	{
		PhotonNetwork.LeaveRoom();
		SceneManager.LoadScene(1);
	}

	public override void OnLeftRoom()
	{
		HasMatchStarted = false;
		SceneManager.LoadScene(1);
	} 

	public void UpdateMaxSpotsLeft()
	{
		PV.RPC("RPC_MaxSpotsLeft", RpcTarget.AllBuffered);
	}

	public void AddPlayerToSpot(PhotonView view)
	{
		PV.RPC("RPC_AddPlayerToSpot", RpcTarget.AllBuffered, view.ViewID);
	}

	[PunRPC]
	void RPC_AddPlayerToSpot(int id)
	{
		PhotonView player = PhotonView.Find(id);
		if(!CurWonPlayer.Contains(player))
		{
			SpotsLeft--;
			CurWonPlayer.Add(player);
			if(SpotsLeft <= 0)
			{
				foreach (PlayerManager pm in GameObject.FindObjectsOfType<PlayerManager>())
				{
					pm.CheckFinish();
				}
				Invoke("ShowNextRound", 1.5f);
				Invoke("StartNewMap", 5f);
			}
		}
	}

	void ShowNextRound()
	{
		GameObject nextRound = GameObject.FindGameObjectWithTag("NextRound");
		if(nextRound != null)
			nextRound.transform.GetChild(0).gameObject.SetActive(true);
	}

	void StartNewMap()
	{
		playableRoom = MapIDs[Random.Range(0, MapIDs.Count)];
		PV.RPC("NewMap", RpcTarget.Others, playableRoom);
	}
	
	[PunRPC]
	void NewMap(int mapId)
	{
		PhotonNetwork.LoadLevel(mapId);
	}

	[PunRPC]
	void RPC_MaxSpotsLeft()
	{
		MaxSpotsLeft = PhotonNetwork.CurrentRoom.PlayerCount - spotVal;
		SpotsLeft = MaxSpotsLeft;
		Debug.Log("Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/SpotsToGo: " + spotVal);
		Debug.Log("MAXSPOTS LEFT: " + MaxSpotsLeft);
	}

	public void FindSpotVal(int number)
	{
		var factors = new List<int>();
    	int max = (int)Mathf.Sqrt(number);  // Round down

		for (int factor = 2; factor <= number; ++factor)
		{  
			if (number % factor == 0) 
			{
				factors.Add(factor);
			}
		}
		
		if(factors.Count < 3)
		{
			spotVal = factors[factors.Count - 1] - 1;
			if(spotVal <= 1)
			{
				spotVal = 1;
			}
			else
			{
				spotVal = spotVal/2;
			}
		}
		else
		{
			spotVal = factors[1];
		}
	}
}