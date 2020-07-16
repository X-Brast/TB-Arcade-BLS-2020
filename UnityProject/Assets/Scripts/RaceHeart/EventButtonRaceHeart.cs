﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BlueConnect;

public class EventButtonRaceHeart : MonoBehaviour
{
    public void RestartGame(){
        SceneManager.LoadScene("RaceHeart");
    } 

    public void GameSelection(){
        SceneManager.LoadScene("GameSelection");
    }

    public void QuitGame(){
        /*LinkedList<ConnectorDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
        foreach (var device in ldb) {
            device.Deconnect();
        }*/

        Application.Quit();
    }
}
