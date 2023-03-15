using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToMenu : MonoBehaviour
{
    public void ToMenuFunction()
    {
        RoomManager.Instance.ReturnToMenu();
    }
}
