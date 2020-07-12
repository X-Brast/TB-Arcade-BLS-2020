using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using blueConnect;

public class Activator : MonoBehaviour
{
    public ConnectorDeviceBLS device;
    
    private GameObject note;
    private GameLogique gm;
    private bool active = false;
    private bool noteExist = false;
    private Color old;
    private SpriteRenderer sr;

    void Awake(){
        sr = GetComponent<SpriteRenderer>();
        gm = GameObject.Find("GameLogique").GetComponent<GameLogique>();
    }

    // Start is called before the first frame update
    void Start()
    {
        old = sr.color;
    }

    // Update is called once per frame
    void Update()
    {
        if(device != null && device.data.Count > 0) {
            StartCoroutine(Pressed());
            int value = device.data.Dequeue();

            if(active) {
                noteExist = false;
                AddScore();
                gm.AddStreak(device.surnameDevice);
                Destroy(note);
            }
            else 
                gm.ResetStreak(device.surnameDevice);
            
        }
    }

    void OnTriggerEnter2D(Collider2D col){
        if(col.gameObject.tag=="Note"){
            noteExist = true;
            active = true;
            note=col.gameObject;
        }

        if(col.gameObject.tag=="Final"){
            gm.EndGame();
        }
    }

    void OnTriggerExit2D(Collider2D col){
        active = false;
        if(noteExist){
            gm.ResetStreak(device.surnameDevice);
            noteExist = false;
        }
    }

    void AddScore(){
        gm.AddScore(device.surnameDevice);
    }

    IEnumerator Pressed(){
        sr.color = new Color(0,0,0);
        yield return new WaitForSeconds(0.05f);
        sr.color = old;
    }
}
