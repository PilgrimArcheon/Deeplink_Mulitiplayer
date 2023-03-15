using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCharMesh : MonoBehaviour
{
    public Renderer[] mesh;
    [SerializeField] Material[] characterMesh;

	void Start()
	{
        int charVal;
        if(PlayerPrefs.HasKey("charID"))
        {
            charVal = PlayerPrefs.GetInt("charID");
        }
        else
        {
            charVal = 0;
        }
        inputChar(charVal);
	}

    void inputChar(int val)
    {
        foreach (var meshItem in mesh)
        {
            meshItem.material = characterMesh[val];
        }
    }
}