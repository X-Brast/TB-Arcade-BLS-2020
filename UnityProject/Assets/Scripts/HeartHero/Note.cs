using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed;
    Rigidbody2D rb;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = new Vector2(0, -speed);
    }

    void OnTriggerExit2D(Collider2D col){
        Destroy(gameObject);
    }
}
