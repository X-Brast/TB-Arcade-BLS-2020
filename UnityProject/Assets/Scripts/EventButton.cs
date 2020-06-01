using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventButton : MonoBehaviour
{
    public void start(){
        SceneManager.LoadScene("MainGame");
    }

    public void stop(){
        Application.Quit();
    }
}
