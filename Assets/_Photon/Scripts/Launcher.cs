using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks
{
	public static Launcher Instance;

	[SerializeField] Text roomNameText;
	[SerializeField] Text roomInfoText;
	[SerializeField] Text gameName;

	public GameObject RoomMenu;
	public Transform playerListContent;
	public GameObject PlayerListItemPrefab;
	public ServerSettings settings;
	public int roomPlayersNum;

	int[] maxPlayers = new int[3]{2, 5, 10};
	bool waitingForClients = false;

	public string currentMatchedRoom;

	[SerializeField] public List<RoomInfo> availableRooms = new List<RoomInfo>();

	void Awake()
	{
		Instance = this;
	}

	public void ConnectToPhoton()
	{
		PhotonNetwork.Disconnect();
		settings.AppSettings.FixedRegion = "us";
		PhotonNetwork.ConnectUsingSettings();
		
		roomPlayersNum = 2;
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected To Master");
		roomInfoText.text = "Connected to Master";
		PhotonNetwork.JoinLobby();
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public override void OnJoinedLobby()
	{
		SearchForRoom();
	}

	public void CreateRoom(string roomName)
	{
		if(string.IsNullOrEmpty(roomName))
		{
			roomInfoText.text = "Create Room Failed...";
			return;
		}
		roomInfoText.text = "Created Room...";
		PhotonNetwork.CreateRoom(currentMatchedRoom, new RoomOptions() { MaxPlayers = (byte)roomPlayersNum});
	}

	public void JoinRandomRoom()
	{
    	PhotonNetwork.JoinRandomRoom();
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
    {
		string _roomName = "_ROOM" + Random.Range(0, 10000).ToString("00");
		int _mp = Random.Range(0, (maxPlayers.Length - 1));
		roomPlayersNum = maxPlayers[_mp];
        PhotonNetwork.CreateRoom(_roomName, new RoomOptions() { MaxPlayers = (byte)roomPlayersNum}, null);
    }

	public override void OnJoinedRoom()
	{
		roomInfoText.text = "Joined Room...";
		Player[] players = PhotonNetwork.PlayerList;
		roomNameText.text = PhotonNetwork.CurrentRoom.Name;
		gameName.text = DeepLinkManager.Instance.GameName;

		foreach(Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}

		for(int i = 0; i < players.Count(); i++)
		{
			Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
		}

		if(PhotonNetwork.IsMasterClient)
		{
			waitingForClients = true;
		}
		// startGameButton.SetActive(PhotonNetwork.IsMasterClient);
		// leaveButton.SetActive(!PhotonNetwork.IsMasterClient);
	}

	public void Update()
	{
		if(waitingForClients)
		{
			if(PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && PhotonNetwork.IsMasterClient)
			{
				PhotonNetwork.CurrentRoom.IsOpen = false;
				Invoke("StartGame", 1f);
				waitingForClients = false;
			}
		}
		if(PhotonNetwork.CurrentRoom != null)
		{
			roomInfoText.text = "Waiting For Players To Join... (" + PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString() + " Players)";
		}
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		if(PhotonNetwork.IsMasterClient)
		{
			waitingForClients = true;
		}
		// startGameButton.SetActive(PhotonNetwork.IsMasterClient);
		// leaveButton.SetActive(!PhotonNetwork.IsMasterClient);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		SearchForRoom();
	}

	public void StartGame()
	{
		if(PhotonNetwork.CurrentRoom.PlayerCount >= 1)
		{
			PhotonNetwork.LoadLevel(5);
			PhotonNetwork.CurrentRoom.IsVisible = false;
			waitingForClients = false;
		}
	}

	public void LeaveRoom()
	{
		if(PhotonNetwork.IsMasterClient)
		{
			waitingForClients = false;
		}

		//MenuManager.Instance.OpenMenu("title");
		PhotonNetwork.Disconnect();
		PhotonNetwork.ConnectUsingSettings();
	}

	public void SearchForRoom()
	{
		currentMatchedRoom = DeepLinkManager.Instance.roomName;
		RoomMenu.SetActive(true);
		PhotonNetwork.NickName = DeepLinkManager.Instance.Email;
		
		if(availableRooms.Count != 0)
		{
			foreach (RoomInfo _room in availableRooms)
			{
				if(_room.Name == currentMatchedRoom && _room.IsOpen)
				{
					JoinRoom(_room);
				}
				else
				{
					CreateRoom(currentMatchedRoom);
				}
			}
		}
		else
		{
			CreateRoom(currentMatchedRoom);
		}
	}

	public void JoinRoom(RoomInfo info)
	{
		PhotonNetwork.JoinRoom(info.Name);
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		availableRooms.Clear();

		for(int i = 0; i < roomList.Count; i++)
		{
			if(roomList[i].RemovedFromList)
				continue;
			availableRooms.Add(roomList[i]);
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
	}
}