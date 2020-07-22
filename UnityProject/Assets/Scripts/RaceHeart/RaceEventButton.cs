using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueConnect;

public class RaceEventButton : MonoBehaviour
{
    public GameObject loaderScene;

    public void RestartGame(){
        loaderScene.GetComponent<LoaderScene>().LoadLevelGame(3);
    } 

    public void GameSelection(){
        loaderScene.GetComponent<LoaderScene>().LoadLevelSelection(0);
    }

    public void QuitGame(){
        LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
        foreach (var device in ldb) {
            device.Deconnect();
        }

        Application.Quit();
    }
}
