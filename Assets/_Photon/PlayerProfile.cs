using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    [SerializeField] int currentChar = 0;
    [SerializeField] MeshRenderer mesh;
    [SerializeField] Material[] characterMeshColor;

	void Start()
	{
        if(PlayerPrefs.HasKey("charID"))
        {
            currentChar = PlayerPrefs.GetInt("charID");
        }
        else
        {
            currentChar = 0;
        }
        inputChar(currentChar);
	}


    public void OnChangeCharacterForward()
    {
        if (currentChar == (characterMeshColor.Length - 1))
        {
            currentChar = 0;
        }
        else
        {
            currentChar += 1;
        }
        inputChar(currentChar);
    }

    public void OnChangeCharacterBackward()
    {
        if (currentChar == 0)
        {
            currentChar = (characterMeshColor.Length - 1);
        }
        else
        {
            currentChar -= 1;
        }
        inputChar(currentChar);
    }

    void inputChar(int charVal)
    {
        mesh.material = characterMeshColor[charVal];
        SaveProfile();
    }

    public void SaveProfile()
    {
        PlayerPrefs.SetInt("charID", currentChar);
    }

    public void ResetSave()
    {
        PlayerPrefs.DeleteAll();
    }        
}
