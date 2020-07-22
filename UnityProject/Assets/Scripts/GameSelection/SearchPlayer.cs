using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlueConnect;

public class SearchPlayer : MonoBehaviour
{
    public GameObject panelPlayer;
    public Canvas canvas;

    private LinkedList<CommunicationDeviceBLS> ldbFinished;
    private Color[] colorsPlayer = {
        new Color(0.91f, 0.72f, 0.71f), 
        new Color(0.09f, 0.37f, 0.29f),
        new Color(0f, 0.85f, 0.61f),
        new Color(1f, 0.69f, 0.36f),
        new Color(1f, 0.62f, 0.52f),
        new Color(0.62f, 0.45f, 1f)};
    private bool[] colorUsed = {false,false,false,false,false,false};
    private string[] spritePlayer = {
        "PlayerCharacter/Bear",
        "PlayerCharacter/Dog",
        "PlayerCharacter/Duck",
        "PlayerCharacter/Panda",
        "PlayerCharacter/Pig",
        "PlayerCharacter/Rabbit",
        "PlayerCharacter/Tiger"
    };
    private bool[] spriteUsed = {false,false,false,false,false,false,false};

    private int nbPlayer = 0;
    private const float delay = 1.0f;
    private float timeCurrent = 0.0f;

    private FinderDevicesBLS fdb;
    private CheckDeviceBLSConnected cdbc;

    void Start(){
        ldbFinished = new LinkedList<CommunicationDeviceBLS>();
        fdb = FinderDevicesBLS.Instance;
        cdbc = CheckDeviceBLSConnected.Instance;
        foreach(var device in fdb.GetListDevicesBLS()){
            CreateCharacterPlayer(device);
        }

        StartCoroutine(FindingDevices());
    }

    void CreateCharacterPlayer(CommunicationDeviceBLS device){
        ldbFinished.AddFirst(device);

        int x = (150 + (nbPlayer / 2) * 300) * (nbPlayer % 2 == 0 ? 1 : -1);
        Vector3 position = new Vector3(x, -375, 0);

        GameObject go = Instantiate(panelPlayer, position, Quaternion.identity);
        go.transform.SetParent(canvas.transform, false);
        
        if(!device.isPlayerDefined){
            int idColor;
            int idCharacter;
            do{
                idColor = Random.Range(0, colorUsed.Length);
            } while(colorUsed[idColor]);
            do{
                idCharacter = Random.Range(0, spriteUsed.Length);
            } while(spriteUsed[idCharacter]);

            colorUsed[idColor] = true;
            spriteUsed[idCharacter] = true;
            device.idColor = idColor;
            device.idCharacter = idCharacter;
            device.colorPlayer = colorsPlayer[idColor];
            device.characterPlayer = Resources.Load<Sprite>(spritePlayer[idCharacter]);
            device.isPlayerDefined = true;
        } else {
            colorUsed[device.idColor] = true;
            spriteUsed[device.idCharacter] = true;
        }

        GameObject imagePlayer  = go.transform.GetChild(0).gameObject;
        GameObject textPlayer   = go.transform.GetChild(1).gameObject;

        go.GetComponent<Image>().color              = device.colorPlayer;
        imagePlayer.GetComponent<Image>().sprite    = device.characterPlayer;
        textPlayer.GetComponent<Text>().text        = device.surnameDevice;

        nbPlayer++;
    }

    IEnumerator FindingDevices(){

        while(true){
            fdb.FindDevices(this);

            while(fdb.IsRunning()){
                yield return new WaitForSeconds(0.3f);
            }

            cdbc.Start(this);

            while(cdbc.IsRunning()){
                yield return new WaitForSeconds(0.3f);
            }

            LinkedList<CommunicationDeviceBLS> ldb = fdb.GetListDevicesBLS();

            foreach(var device in ldb){
                if(!ldbFinished.Contains(device)){
                    CreateCharacterPlayer(device);
                }
            }
        }
    }
}
