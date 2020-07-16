using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BlueConnect;

public class DisplayScore : MonoBehaviour
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

        float widthCanvas = canvas.GetComponent<RectTransform>().rect.width;
        float sizeXPanel = widthCanvas / nbPlayer;
        float x = (widthCanvas - sizeXPanel) / -2;
        float y = panelScorePlayer.transform.position.y;
        float z = panelScorePlayer.transform.position.z;

        foreach (var device in ldb) {
            Vector3 position = new Vector3(x, y, z);
            x += sizeXPanel;

            GameObject go = Instantiate(panelScorePlayer, position, Quaternion.identity);
            go.transform.SetParent(canvas.transform, false);
            
            go.GetComponent<Image>().color = device.colorPlayer;
            GameObject namePlayer   = go.transform.GetChild(0).gameObject;
            GameObject scorePlayer  = go.transform.GetChild(2).gameObject;
            GameObject hitPlayer    = go.transform.GetChild(4).gameObject;
            GameObject maxHit       = go.transform.GetChild(6).gameObject;
            GameObject comboPlayer  = go.transform.GetChild(8).gameObject;
            
            namePlayer.GetComponent<Text>().text    = device.surnameDevice;
            scorePlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("Score" + device.surnameDevice) + "";
            hitPlayer.GetComponent<Text>().text     = PlayerPrefs.GetInt("NotesHit" + device.surnameDevice) + "";
            maxHit.GetComponent<Text>().text        = PlayerPrefs.GetInt("NotesMax") + "";
            comboPlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("Highstreak" + device.surnameDevice) + "";
        }
    }
}
