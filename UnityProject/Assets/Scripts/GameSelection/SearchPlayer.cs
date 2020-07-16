using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlueConnect;

public class SearchPlayer : MonoBehaviour
{
    public GameObject panelPlayer;
    public Canvas canvas;

    private LinkedList<ConnectorDeviceBLS> ldbFinished;
    private Color[] colorsPlayer = {
        new Color(0.8f, 0.92f, 0.88f), 
        new Color(0.09f, 0.37f, 0.29f),
        new Color(0f, 0.85f, 0.61f),
        new Color(0.95f, 0.2f, 0f),
        new Color(1f, 0.62f, 0.52f),
        new Color(0.62f, 0.45f, 1f)};
    private bool[] colorUsed = {false,false,false,false,false,false};
    private Color[] spriteForUse;
    private Color[] spriteUsed;

    private int nbPlayer = 0;
    private const float delay = 5.0f;
    private float timeCurrent = 0.0f;

    private FinderDevicesBLS fdb;
    private CheckDeviceBLSConnected cdbc;

    void Start(){
        ldbFinished = new LinkedList<ConnectorDeviceBLS>();
        fdb = FinderDevicesBLS.Instance;
        cdbc = CheckDeviceBLSConnected.Instance;
        foreach(var device in fdb.GetListDevicesBLS()){
            CreateCharacterPlayer(device);
        }
    }


    void Update()
    {
        if(timeCurrent + delay < Time.fixedTime && !fdb.IsRunning() && !cdbc.IsRunning()){
            StartCoroutine(FindingDevices());
            timeCurrent = Time.fixedTime;
        }
    }

    void CreateCharacterPlayer(ConnectorDeviceBLS device){
        ldbFinished.AddFirst(device);

        int x = (150 + (nbPlayer / 2) * 300) * (nbPlayer % 2 == 0 ? 1 : -1);
        Vector3 position = new Vector3(x, -375, 0);

        GameObject go = Instantiate(panelPlayer, position, Quaternion.identity);
        go.transform.SetParent(canvas.transform, false);
        
        if(device.isColorDefined){
            int idColor;
            do{
                idColor = Random.Range(0, colorUsed.Length);
            } while(colorUsed[idColor]);
            colorUsed[idColor] = true;
            device.idColor = idColor;
            device.isColorDefined = true;
            device.colorPlayer = colorsPlayer[idColor];
            go.GetComponent<Image>().color = colorsPlayer[idColor];
        } else
        {
            go.GetComponent<Image>().color = device.colorPlayer;
            colorUsed[device.idColor] = true;
        }

        GameObject imagePlayer = go.transform.GetChild(0).gameObject;
        GameObject textPlayer = go.transform.GetChild(1).gameObject;
        
        textPlayer.GetComponent<Text>().text = device.surnameDevice;

        nbPlayer++;
    }

    IEnumerator FindingDevices(){

        fdb.FindDevices(this);

        while(fdb.IsRunning()){
            yield return new WaitForSeconds(.3f);
        }

        //cdbc.Start(this);

        while(cdbc.IsRunning()){
            yield return new WaitForSeconds(.3f);
        }

        LinkedList<ConnectorDeviceBLS> ldb = fdb.GetListDevicesBLS();
        Debug.Log(ldb.Count + " " + ldbFinished.Count);

        foreach(var device in ldb){
            if(!ldbFinished.Contains(device)){
                CreateCharacterPlayer(device);
            }
        }
    }
}
