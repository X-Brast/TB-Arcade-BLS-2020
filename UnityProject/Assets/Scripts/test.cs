using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    public void StartGameHeartHero()
    {
        PlayerPrefs.SetInt("nbPlayer", 2);
        SceneManager.LoadScene("HeartHero");
    }
}
