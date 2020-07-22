using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BlueConnect;

public class RaceDisplayScore : MonoBehaviour
{
    public GameObject panelScorePlayer;
    public Canvas canvas;
    public GameObject loaderScene;

    private CheckDeviceBLSConnected cdbc = CheckDeviceBLSConnected.Instance;
    private List<GameObject> buttons;
    private int idButtonSelect = 0;
    private bool isFinishLoopFindCheck = false;

    void Start()
    {
        buttons = new List<GameObject>();
        buttons.Add(GameObject.Find("RestartGame")); 
        buttons.Add(GameObject.Find("SelectionGameButton"));
        buttons.Add(GameObject.Find("QuitButton"));
        EventSystem.current.SetSelectedGameObject(buttons[idButtonSelect], null);

        LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();

        int nbPlayer = ldb.Count;

        if(nbPlayer == 0){
            loaderScene.GetComponent<LoaderScene>().LoadLevelSelection(0);
            return;
        }

        bool isDeviceStopped = true;
        while (isDeviceStopped) {
            isDeviceStopped = false;
            foreach(var device in ldb) {
                isDeviceStopped |= device.isRunning;
            }
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
            
            GameObject namePlayer       = go.transform.GetChild(0).gameObject;
            GameObject timePlayer       = go.transform.GetChild(2).gameObject;
            GameObject goodHitPlayer    = go.transform.GetChild(5).gameObject;
            GameObject badHitPlayer     = go.transform.GetChild(7).gameObject;
            GameObject comboPlayer      = go.transform.GetChild(9).gameObject;
            GameObject imagePlayer      = go.transform.GetChild(10).gameObject;
            
            go.GetComponent<Image>().color          = device.colorPlayer;
            namePlayer.GetComponent<Text>().text    = device.surnameDevice;
            timePlayer.GetComponent<Text>().text    = ConvertSecondToHMS(PlayerPrefs.GetInt("RaceScore" + device.surnameDevice));
            goodHitPlayer.GetComponent<Text>().text = PlayerPrefs.GetInt("RaceGoodHit" + device.surnameDevice) + "";
            badHitPlayer.GetComponent<Text>().text  = PlayerPrefs.GetInt("RaceBadHit" + device.surnameDevice) + "";
            comboPlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("RaceHighstreak" + device.surnameDevice) + "";
            imagePlayer.GetComponent<Image>().sprite= device.characterPlayer;
        }

        isFinishLoopFindCheck = true;
    }

    private string ConvertSecondToHMS(int second){
        string hours = "";
        int s = second % 60;
        int m = second / 60;
        int h = second / 3600;

        if(h > 0)
            hours += h + "h";
        if(m > 0)
            hours += m + "'";
        hours += s + "''";

        return hours;
    }

    void Update(){
        if(isFinishLoopFindCheck){
            StartCoroutine(CheckDevices());
            isFinishLoopFindCheck = false;
        }
    }

    IEnumerator CheckDevices(){
        cdbc.Start(this);

        while(cdbc.IsRunning()){
            yield return new WaitForSeconds(.3f);
        }

        if(cdbc.GetIsSelect()){
            EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
        }
        if(cdbc.GetIsNeedMove()){
            idButtonSelect = (++idButtonSelect) % buttons.Count;
            EventSystem.current.SetSelectedGameObject(buttons[idButtonSelect], null);
        }

        isFinishLoopFindCheck = true;
    }
}
