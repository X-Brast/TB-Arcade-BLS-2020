using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueConnect;

public class Ambulance : MonoBehaviour
{
    public ConnectorDeviceBLS device;
    public float speed;

    private RaceGameLogic gl;
    private Rigidbody2D rb;

    private bool isCorrectTempo = true;

    private float delay = 1.0f;
    private float timeCurrent = 0.0f;

    void Awake(){
        gl = GameObject.Find("GameLogic").GetComponent<RaceGameLogic>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start() {
        rb.velocity = new Vector2(speed, 0);
    }

    void Update(){
        if(device != null && device.data.Count > 0) {
            int value = device.data.Dequeue();

            if(isCorrectTempo){
                gl.GoodStreak(device.surnameDevice);
            }
            else {
                gl.BadStreak(device.surnameDevice);
            }  

            Moving(value); 
            isCorrectTempo = false;   
        }
        if(timeCurrent + delay < Time.fixedTime){
            isCorrectTempo = true;
            rb.velocity = new Vector2(speed, 0);
            timeCurrent = Time.fixedTime; 
        }
    }

    void Moving(int value){
        int mult = gl.GetMult(device.surnameDevice);
        switch(value){
            case 1:
                rb.velocity = new Vector2(speed + 2 * mult, 0);
                break;
            case 2:
                rb.velocity = new Vector2(speed + 1 * mult, 0);
                break;
            case 3:
                rb.velocity = new Vector2(speed + 0.75f * mult, 0);
                break;
            case 4:
                rb.velocity = new Vector2(speed + 0.5f * mult, 0);
                break;
            default:
                rb.velocity = new Vector2(speed, 0);
                break;
        }
    }

    void OnTriggerExit2D(Collider2D col){
        if(col.gameObject.tag == "FinishLine"){
            rb.velocity = new Vector2(0, 0);
            gl.RaceFinish();
        }
    }
}
