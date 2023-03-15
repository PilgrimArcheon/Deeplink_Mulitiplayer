using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using Photon.Pun;

public class DeepLinkManager : MonoBehaviour
{
    public static DeepLinkManager Instance { get; private set; }

    public Transform textHolder;
    public Text textPrefab;
    public string serverUrl = "http://156.67.217.206:7000/apis/";

    public Text deepLinkLogText;
    public GameObject deepLinkOverlay;

    public string deeplinkURL;

    public string MatchID = "";
    public int GameID = -1;
    public int UserID = -1;
    public int BetAmount = -1;
    public string Email = "";
    public string Pin = "";
    public string GameName = "";
    public string Token = "";
    public bool ifOpenedFromDeepLink = false;
    
    public string roomName = "_ROOM75"; 

    private void Awake()
    {
        Time.timeScale = 1.0f;
        if (Instance == null)
        {
            Instance = this;                
            Application.deepLinkActivated += onDeepLinkActivated;
            ifOpenedFromDeepLink = false;

            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else deeplinkURL = "[none]";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    void OnDestroy() 
    {
        Application.deepLinkActivated -= onDeepLinkActivated;
    }

    private void onDeepLinkActivated(string url)
    {
        //SceneManager.LoadScene("LoginSplash");
        DeepLinkActivated(url);
    }

    void DeepLinkActivated(string url)
    {
        Time.timeScale = 1.0f;
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }
        ifOpenedFromDeepLink = true;
        deepLinkOverlay.SetActive(true);
        deepLinkLogText.text = "Logging In...";

        // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
        deeplinkURL = url;
        string matchID, gameID, userID, betAmount, email, pin, gameName, token = "";

        //Decode url and get player Data
        string data = url.Split("?"[0])[1];
        if(!String.IsNullOrEmpty(data))
        {
            string[] dataArray = data.Split(new string[] {"&"}, StringSplitOptions.None);

            matchID = dataArray[0].Split("="[0])[1];
            MatchID = matchID;

            if(MatchID == "null")
            {
                MatchID = "";
            }

            gameID = dataArray[1].Split("="[0])[1];
            ShowData(gameID);
            GameID = int.Parse(gameID);

            userID = dataArray[2].Split("="[0])[1];
            ShowData(userID);
            UserID = int.Parse(userID);

            betAmount = dataArray[3].Split("="[0])[1];
            ShowData(betAmount);
            BetAmount = int.Parse(betAmount);

            email = dataArray[4].Split("="[0])[1];
            ShowData(email);
            Email = email;

            pin = dataArray[5].Split("="[0])[1];
            ShowData(pin);
            Pin = pin;

            gameName = dataArray[6].Split("="[0])[1];
            ShowData(gameName);
            GameName = gameName;

            token = dataArray[7].Split("="[0])[1];
            ShowData(token);
            Token = token;

            //StartCoroutine(Login());
            StartCoroutine(CreateMatchRequest());
        }
    }

    void ShowData(string str)
    {
        Instantiate(textPrefab, textHolder).GetComponent<Text>().text = str;
    }

    IEnumerator CreateMatchRequest()
    {
        string url_1 = serverUrl + "update-matchRequest";
        string uId = SystemInfo.deviceUniqueIdentifier;

        WWWForm form = new WWWForm();
        form.AddField("matchId", MatchID);

        UnityWebRequest www = UnityWebRequest.Post(url_1, form); // Put address here
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + Token);
        www.timeout = 120;

        yield return www.SendWebRequest();
        
        string results = "";

        if(www.result == UnityWebRequest.Result.ConnectionError)
        {
            results = www.error;
        }
        else
        {
            if(www.downloadHandler.text.Contains("error"))
            {
                results = "Match Request Error: " + www.downloadHandler.text;
            }
            else
            {
                results = "Match Request Success: " + www.downloadHandler.text;
                Invoke("OpenGameRoom", 2f);
            }
        }
        deepLinkLogText.text = results;
    }

    void OpenGameRoom()
    {
        deepLinkOverlay.SetActive(false);
        roomName = "_ROOM" + MatchID;
        Launcher.Instance.ConnectToPhoton();
    }
}