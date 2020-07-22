using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueConnect;

public class Ambulance : MonoBehaviour
{
    public CommunicationDeviceBLS device;
    public float speed;

    private RaceGameLogic gl;
    private Rigidbody2D rb;

    private bool isCorrectTempo = true;
    private bool isFinish = false;

    private float delay = 0.55f;
    private float timeCurrent = 0.0f;
    private float badDelay = 1.5f;

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
        if(timeCurrent + badDelay < Time.fixedTime && !isFinish){
            isCorrectTempo = false;
            rb.velocity = new Vector2(speed, 0);
            gl.BadStreak(device.surnameDevice);
        }
        if(timeCurrent + delay < Time.fixedTime && !isFinish){
            isCorrectTempo = true;
            rb.velocity = new Vector2(speed, 0);
            timeCurrent = Time.fixedTime; 
        }
    }

    void Moving(int value){
        int mult = gl.GetMult(device.surnameDevice);
        float newSpeed = speed;
        switch(value){
            case 1:
                newSpeed += 1f * mult;
                break;
            case 2:
                newSpeed += 0.7f * mult;
                break;
            case 3:
                newSpeed += 0.5f * mult;
                break;
            case 4:
                newSpeed += 0.2f * mult;
                break;
            default:
                newSpeed += 0;
                break;
        }
        newSpeed /= (isCorrectTempo ? 1 : 2);
        rb.velocity = new Vector2(newSpeed , 0);
    }

    void OnTriggerExit2D(Collider2D col){
        if(col.gameObject.tag == "FinishLine" && !isFinish){
            isFinish = true;
            rb.velocity = new Vector2(0, 0);
            gl.RaceFinish(this.transform.parent.gameObject, device.surnameDevice);
        }
    }
}
