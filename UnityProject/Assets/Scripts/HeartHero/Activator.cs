using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
    public KeyCode key;
    
    private GameObject note;
    private GameLogique gm;
    private bool active = false;
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
        if(Input.GetKeyDown(key))
            StartCoroutine(Pressed());

        if(Input.GetKeyDown(key) && !active){
            gm.ResetStreak();
        }

        if(Input.GetKeyDown(key) && active){
            AddScore();
            gm.AddStreak();
            Destroy(note);
        }
    }

    void OnTriggerEnter2D(Collider2D col){
        if(col.gameObject.tag=="Note"){
            active = true;
            note=col.gameObject;
        }

        if(col.gameObject.tag=="Final"){
            gm.EndGame();
        }
    }

    void OnTriggerExit2D(Collider2D col){
        active = false;
        if(! note)
            gm.ResetStreak();
    }

    void AddScore(){
        PlayerPrefs.SetInt("Score", PlayerPrefs.GetInt("Score") + gm.GetScore());
    }

    IEnumerator Pressed(){
        sr.color = new Color(0,0,0);
        yield return new WaitForSeconds(0.05f);
        sr.color = old;
    }
}
