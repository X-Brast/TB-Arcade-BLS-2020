﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using blueConnect;

public class DisplayScore : MonoBehaviour
{
    public GameObject panelScorePlayer;
    public Canvas canvas;

    void Start()
    {
        //LinkedList<ConnectorDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
        LinkedList<ConnectorDeviceBLS> ldb = new LinkedList<ConnectorDeviceBLS>();
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Monstro"));
        int nbPlayer = ldb.Count;

        if(nbPlayer == 0){
            //SceneManager.LoadScene(1);
            return;
        }

        int widthCanvas = (int)canvas.GetComponent<RectTransform>().rect.width;
        int sizeXPanel = widthCanvas / nbPlayer;
        int x = (widthCanvas - sizeXPanel) / -2;
        int y = (int)panelScorePlayer.transform.position.y;
        int z = (int)panelScorePlayer.transform.position.z;

        foreach (var device in ldb) {

            Vector3 position = new Vector3(x, y, z);
            x += sizeXPanel;

            GameObject go = Instantiate(panelScorePlayer, position, Quaternion.identity);
            go.transform.SetParent(canvas.transform, false);
            //go.GetComponent<RectTransform>().sizeDelta = new Vector2 (sizeXPanel, go.RectTransform.sizeDelta.y);
            
            go.GetComponent<Image>().color = new Color(
                                                Random.Range(0f, 1f), 
                                                Random.Range(0f, 1f), 
                                                Random.Range(0f, 1f)
                                            );
            GameObject namePlayer   = go.transform.GetChild(0).gameObject;
            GameObject scorePlayer  = go.transform.GetChild(2).gameObject;
            GameObject hitPlayer    = go.transform.GetChild(4).gameObject;
            GameObject maxHit       = go.transform.GetChild(6).gameObject;
            GameObject comboPlayer  = go.transform.GetChild(8).gameObject;
            
            namePlayer.GetComponent<Text>().text    = device.surnameDevice;
            scorePlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("Score" + device.surnameDevice) + "";
            //scorePlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("Highscore" + device.surnameDevice) + "";
            hitPlayer.GetComponent<Text>().text     = PlayerPrefs.GetInt("NotesHit" + device.surnameDevice) + "";
            maxHit.GetComponent<Text>().text        = PlayerPrefs.GetInt("NotesMax") + "";
            comboPlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("Streak" + device.surnameDevice) + "";
        }
    }
}
