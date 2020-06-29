using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using blueConnect;

public class SearchPlayer : MonoBehaviour
{
    private LinkedList<ConnectorDeviceBLS> ldbFinished;
    private int[] colorChoosen;
    private int[] spriteChoosen;
    private float delay = 5.0f;
    private float timeCurrent = 0.0f;

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

        fdb.StopFindDevices();
        fdb.FindDevices(this);

        while(fdb.IsRunning()){
            yield return new WaitForSeconds(.3f);
        }

        LinkedList<ConnectorDeviceBLS> ldb = fdb.GetListDevicesBLS();

        foreach(var device in ldb){
            if(!ldbFinished.Contains(device)){
                ldbFinished.AddFirst(device);
                // TODO
            }
            print(device.surnameDevice);
        }
    }
}
