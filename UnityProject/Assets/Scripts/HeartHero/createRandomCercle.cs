using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using blueConnect;

public class createRandomCercle : MonoBehaviour
{
    public GameObject targets;
    public GameObject note;
    public GameObject finalNote;
    private float delay = 1.0f;
    private float timeCurrent = 0.0f;
    private Queue<GameObject> list = new Queue<GameObject>();
    private bool isFinish = true;

    private int z = 0; // car nous travaillons en 2D
    private int xBegin = 0;
    private int yBegin = 5;
    private int xTarget = 0;
    private int yTarget = -4;
    private ConnectorDeviceBLS device; 
    private int layer;

    void InitKey(ConnectorDeviceBLS device){
        Debug.Log(device);
        this.device = device;
    }

    void TheStart(int layer)
    {
        this.layer = layer;
        Vector3 position = new Vector3(xTarget, yTarget, z);
        targets.layer = layer;
        GameObject go = Instantiate(targets, position, Quaternion.identity);
        go.GetComponent< Activator >().device = device;
        AnalyseSound();

        isFinish = false;
    }

    private void AnalyseSound(){
        for (int i = 0; i < 10; i++)
        {
            list.Enqueue(note);
        }
        list.Enqueue(finalNote);
        PlayerPrefs.SetInt("NotesMax", 100);
    }

    // Update is called once per frame
    void Update()
    {
        if(!isFinish && timeCurrent + delay < Time.fixedTime){
            Vector3 position = new Vector3(xBegin, yBegin, z);
            GameObject o = list.Dequeue();
            o.layer = layer;
            Instantiate(o, position, Quaternion.identity);
            timeCurrent = Time.fixedTime;
            isFinish = list.Count == 0;
        }
    }
}
