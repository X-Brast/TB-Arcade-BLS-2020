using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BlueConnect;

public class HeroDisplayScore : MonoBehaviour
{
    public GameObject panelScorePlayer;
    public Canvas canvas;
    public GameObject loaderScene;

    private CheckDeviceBLSConnected cdbc = CheckDeviceBLSConnected.Instance;
    private List<GameObject> buttons;
    private int idButtonSelect = 0;
    private bool isFinishLoopFindCheck = false;

    void Awake(){
        buttons = new List<GameObject>();
        buttons.Add(GameObject.Find("RestartButton")); 
        buttons.Add(GameObject.Find("SelectionGameButton"));
        buttons.Add(GameObject.Find("QuitButton"));
        EventSystem.current.SetSelectedGameObject(buttons[idButtonSelect], null);
    }

    void Start()
    {
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
            
            GameObject namePlayer   = go.transform.GetChild(0).gameObject;
            GameObject scorePlayer  = go.transform.GetChild(2).gameObject;
            GameObject hitPlayer    = go.transform.GetChild(4).gameObject;
            GameObject maxHit       = go.transform.GetChild(6).gameObject;
            GameObject comboPlayer  = go.transform.GetChild(8).gameObject;
            GameObject imagePlayer  = go.transform.GetChild(9).gameObject;
            
            go.GetComponent<Image>().color          = device.colorPlayer;
            namePlayer.GetComponent<Text>().text    = device.surnameDevice;
            scorePlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("Score" + device.surnameDevice) + "";
            hitPlayer.GetComponent<Text>().text     = PlayerPrefs.GetInt("NotesHit" + device.surnameDevice) + "";
            maxHit.GetComponent<Text>().text        = PlayerPrefs.GetInt("NotesMax") + "";
            comboPlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("Highstreak" + device.surnameDevice) + "";
            imagePlayer.GetComponent<Image>().sprite= device.characterPlayer;
        }

        isFinishLoopFindCheck = true;
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
