using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BlueConnect;

public class RaceDisplayScore : MonoBehaviour
{
    public GameObject panelScorePlayer;
    public Canvas canvas;

    void Start()
    {
        //LinkedList<ConnectorDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
        LinkedList<ConnectorDeviceBLS> ldb = new LinkedList<ConnectorDeviceBLS>();
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Monstro"));
        ldb.First.Value.colorPlayer = new Color(0.8f, 0.92f, 0.88f);
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Bobo"));
        ldb.First.Value.colorPlayer = new Color(0.09f, 0.37f, 0.29f);
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Carole"));
        ldb.First.Value.colorPlayer = new Color(0f, 0.85f, 0.61f);
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Inconnu"));
        ldb.First.Value.colorPlayer = new Color(0.95f, 0.2f, 0f);
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Richard"));
        ldb.First.Value.colorPlayer = new Color(1f, 0.62f, 0.52f);
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Jessica"));
        ldb.First.Value.colorPlayer = new Color(0.62f, 0.45f, 1f);
        int nbPlayer = ldb.Count;

        if(nbPlayer == 0){
            SceneManager.LoadScene(1);
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
            
            go.GetComponent<Image>().color = device.colorPlayer;
            GameObject namePlayer       = go.transform.GetChild(0).gameObject;
            GameObject timePlayer       = go.transform.GetChild(2).gameObject;
            GameObject goodHitPlayer    = go.transform.GetChild(5).gameObject;
            GameObject badHitPlayer     = go.transform.GetChild(7).gameObject;
            GameObject comboPlayer      = go.transform.GetChild(9).gameObject;
            
            namePlayer.GetComponent<Text>().text    = device.surnameDevice;
            timePlayer.GetComponent<Text>().text    = PlayerPrefs.GetInt("RaceScore" + device.surnameDevice) + "";
            goodHitPlayer.GetComponent<Text>().text = PlayerPrefs.GetInt("RaceGoodHit" + device.surnameDevice) + "";
            badHitPlayer.GetComponent<Text>().text  = PlayerPrefs.GetInt("RaceBadHit" + device.surnameDevice) + "";
            comboPlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("RaceHighstreak" + device.surnameDevice) + "";
        }
    }
}
