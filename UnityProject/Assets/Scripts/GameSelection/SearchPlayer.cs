using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using blueConnect;

public class SearchPlayer : MonoBehaviour
{
    public GameObject panelPlayer;
    public Canvas canvas;

    private LinkedList<ConnectorDeviceBLS> ldbFinished;
    private int[] colorChoosen;
    private int[] spriteChoosen;

    private static int nbPlayer = 0;
    private const float delay = 5.0f;
    private float timeCurrent = 0.0f;

    public static int GetNbPlayer(){
        return nbPlayer;
    }

    void Start(){
        ldbFinished = new LinkedList<ConnectorDeviceBLS>();
    }


    void Update()
    {
        if(timeCurrent + delay < Time.fixedTime){
            StartCoroutine(FindingDevices());
            timeCurrent = Time.fixedTime;
        }
    }

    IEnumerator FindingDevices(){
        FinderDevicesBLS fdb = FinderDevicesBLS.Instance;

        fdb.FindDevices(this);

        while(fdb.IsRunning()){
            yield return new WaitForSeconds(.3f);
        }

        LinkedList<ConnectorDeviceBLS> ldb2 = fdb.GetListDevicesBLS();

        yield return new WaitForSeconds(1.0f);

        CheckDeviceBLSConnected cdbc = CheckDeviceBLSConnected.Instance;

        //cdbc.Start(this);

        while(cdbc.IsRunning()){
            yield return new WaitForSeconds(.3f);
        }

        LinkedList<ConnectorDeviceBLS> ldb = fdb.GetListDevicesBLS();
        Debug.Log(ldb.Count + " " + ldbFinished.Count);


        foreach(var device in ldb){
            if(!ldbFinished.Contains(device)){
                ldbFinished.AddFirst(device);

                int x = (150 + (nbPlayer / 2) * 300) * (nbPlayer % 2 == 0 ? 1 : -1);
                Vector3 position = new Vector3(x, -375, 0);

                GameObject go = Instantiate(panelPlayer, position, Quaternion.identity);
                go.transform.SetParent(canvas.transform, false);
                
                go.GetComponent<Image>().color = new Color(
                                                    Random.Range(0f, 1f), 
                                                    Random.Range(0f, 1f), 
                                                    Random.Range(0f, 1f)
                                                );
                GameObject imagePlayer = go.transform.GetChild(0).gameObject;
                GameObject textPlayer = go.transform.GetChild(1).gameObject;
                
                textPlayer.GetComponent<Text>().text = device.surnameDevice;

                nbPlayer++;
            }
        }
    }
}
