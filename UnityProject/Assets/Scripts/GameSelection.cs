using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSelection : MonoBehaviour
{
    // Start is called before the first frame update
    public void StartGameHeartHero()
    {
        PlayerPrefs.SetInt("nbPlayer", 3);
        SceneManager.LoadScene("HeartHero");
    }
}
