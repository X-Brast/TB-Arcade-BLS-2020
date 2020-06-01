using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private KeyCode key;
    private int layer;

    void InitKey(KeyCode key){
        this.key = key;
    }

    void TheStart(int layer)
    {
        this.layer = layer;
        Vector3 position = new Vector3(xTarget, yTarget, z);
        targets.layer = layer;
        targets.GetComponent< Activator >().key = key;
        Instantiate(targets, position, Quaternion.identity);
        AnalyseSound();

        isFinish = false;
    }

    private void AnalyseSound(){
        for (int i = 0; i < 100; i++)
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
