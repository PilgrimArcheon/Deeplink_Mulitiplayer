using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance;
    public string serverUrl = "http://156.67.217.206:7000/apis/";
    public Text loginText;
    public GameObject loaderScreen;
    [SerializeField] string uId;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void DoLogin()
    {
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        loaderScreen.SetActive(true);

        uId = SystemInfo.deviceUniqueIdentifier;
// #if UNITY_ANDROID && !UNITY_EDITOR
//         uId = androidUnique();
// #endif
        WWWForm form = new WWWForm();
        if(DeepLinkManager.Instance.ifOpenedFromDeepLink)
        {
            form.AddField("email", DeepLinkManager.Instance.Email);
            form.AddField("pin", DeepLinkManager.Instance.Pin);
        }

        form.AddField("device_id", uId);
        UnityWebRequest www = UnityWebRequest.Post(serverUrl + "login", form);
        www.timeout = 120;
        yield return www.SendWebRequest();

        DeepLinkManager.Instance.deepLinkOverlay.SetActive(false);

        if(www.result == UnityWebRequest.Result.ConnectionError)
        {
            loginText.text = www.error;
        }
        else
        {
            loginText.text = "LOGIN SUCCESSFUL: " + www.downloadHandler.text;
        }
    
    }
}
