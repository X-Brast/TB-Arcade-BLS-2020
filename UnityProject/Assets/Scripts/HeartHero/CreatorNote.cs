using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueConnect;

public class CreatorNote : MonoBehaviour
{
    public GameObject targets;
    public GameObject note;
    public GameObject finalNote;

    private float delay = 0.55f;
    private float timeCurrent = 0.0f;
    private Queue<GameObject> list = new Queue<GameObject>();
    private bool isFinish = false;
    private const int NB_MAX_NOTE = 30;
    private int counterNoteCreate = 0;

    private int xBegin;
    private int yBegin;
    private int zBegin;
    private CommunicationDeviceBLS device; 
    private int layer;

    void InitDevice(CommunicationDeviceBLS device){
        this.device = device;
    }

    void TheStart(int layer)
    {
        this.layer = layer;
        xBegin = (int)note.transform.position.x;
        yBegin = (int)note.transform.position.y;
        zBegin = (int)note.transform.position.z;

        int xTarget = (int)targets.transform.position.x;
        int yTarget = (int)targets.transform.position.y;
        int zTarget = (int)targets.transform.position.z;
        Vector3 position = new Vector3(xTarget, yTarget, zTarget);
        targets.layer = layer;
        GameObject go = Instantiate(targets, position, Quaternion.identity);
        go.transform.SetParent(gameObject.transform, false);
        go.GetComponent< Activator >().device = device;
        AnalyseSound();

        StartCoroutine(CreateNote());
    }

    private void AnalyseSound(){
        for (int i = 0; i < NB_MAX_NOTE; i++)
        {
            list.Enqueue(note);
        }
        list.Enqueue(finalNote);
        PlayerPrefs.SetInt("NotesMax", NB_MAX_NOTE);
    }

    IEnumerator CreateNote(){
        HeroGameLogique gm = GameObject.Find("HeroGameLogic").GetComponent<HeroGameLogique>();

        while(gm.isLoading){
            yield return new WaitForSeconds(1.0f);
        }

        while(!isFinish){
            if(timeCurrent + delay < Time.fixedTime){
                Vector3 position = new Vector3(xBegin, yBegin, zBegin);
                GameObject o = list.Dequeue();
                o.layer = layer;
                GameObject go = Instantiate(o, position, Quaternion.identity);
                go.transform.SetParent(gameObject.transform, false);

                if(counterNoteCreate > NB_MAX_NOTE / 2 + 10 && counterNoteCreate < NB_MAX_NOTE){
                    Color color = go.GetComponent<SpriteRenderer>().material.color;
                    color.a = 0.1f;
                    go.GetComponent<SpriteRenderer>().material.color = color;
                }
                else if(counterNoteCreate > NB_MAX_NOTE / 2){
                    Color color = go.GetComponent<SpriteRenderer>().material.color;
                    color.a = 0.5f;
                    go.GetComponent<SpriteRenderer>().material.color = color;
                }

                isFinish = list.Count == 0;
                ++counterNoteCreate;
                timeCurrent = Time.fixedTime;
                
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
}
