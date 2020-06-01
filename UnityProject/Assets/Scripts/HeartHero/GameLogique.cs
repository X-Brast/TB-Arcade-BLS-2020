using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLogique : MonoBehaviour
{
    public Canvas canvas;
    public Camera camera;

    private KeyCode[] keys = new KeyCode[] {KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H};

    void Start()
    {
        
        int nbPlayer = PlayerPrefs.GetInt("nbPlayer");
        float w = 1.0f/nbPlayer;

        for(int i = 0; i < nbPlayer && i < 6; ++i){
            int layer = LayerMask.NameToLayer("Player" + (i+1).ToString());

            PlayerPrefs.SetInt("Highstreak" + layer, 0);
            PlayerPrefs.SetInt("Score" + layer, 0);
            PlayerPrefs.SetInt("NotesHit" + layer, 0);
            PlayerPrefs.SetInt("Streak" + layer, 0);
            PlayerPrefs.SetInt("Mult" + layer, 1);

            Camera cam = Instantiate(camera, new Vector3(0, 0,-10), Quaternion.identity);
            cam.rect = new Rect(w * i, 0.0f, w, 1.0f);
            cam.backgroundColor = new Color(
                                        Random.Range(0f, 1f), 
                                        Random.Range(0f, 1f), 
                                        Random.Range(0f, 1f)
                                    );
            cam.cullingMask = (1 << layer) + (1 << LayerMask.NameToLayer("UI"));

            Canvas can = Instantiate(canvas, new Vector3(0, 0, 0), Quaternion.identity);
            can.renderMode = RenderMode.ScreenSpaceCamera;
            can.worldCamera = cam;
            can.transform.GetChild(0).gameObject.GetComponent< PPtext >().name = "Score" + layer;
            can.transform.GetChild(1).gameObject.GetComponent< PPtext >().name = "Mult" + layer;
            can.transform.GetChild(2).gameObject.GetComponent< PPtext >().name = "Streak" + layer;

            can.SendMessage("InitKey", keys[i]);
            can.SendMessage("TheStart", layer);
        }
    }

    public void AddStreak(int layer){
        PlayerPrefs.SetInt("Streak" + layer, PlayerPrefs.GetInt("Streak" + layer)+1);
        PlayerPrefs.SetInt("NotesHit" + layer, PlayerPrefs.GetInt("NotesHit" + layer)+1);
        
        int streak = PlayerPrefs.GetInt("Streak" + layer);
        int multiplier = PlayerPrefs.GetInt("Mult" + layer);

        if(streak > 50)
            multiplier = 10;
        else if(streak > 40)
            multiplier = 5;
        else if(streak > 30)
            multiplier = 4;
        else if(streak > 20)
            multiplier = 3;
        else if(streak > 10)
            multiplier = 2;
        else
            multiplier = 1;

        PlayerPrefs.SetInt("Mult" + layer, multiplier);

        if(streak > PlayerPrefs.GetInt("HighStreak" + layer))
            PlayerPrefs.SetInt("Highstreak" + layer, streak);
    }

    public void ResetStreak(int layer){
        PlayerPrefs.SetInt("Mult" + layer, 1);
        PlayerPrefs.SetInt("Streak" + layer, 0);
    }

    public void EndGame(){
        // if(PlayerPrefs.GetInt("Score" + layer) > PlayerPrefs.GetInt("Highscore"));
        //     PlayerPrefs.SetInt("Highscore", PlayerPrefs.GetInt("Score" + layer));
        SceneManager.LoadScene("HeartHeroClassement");
    }

    public void AddScore(int layer){
        PlayerPrefs.SetInt("Score" + layer, PlayerPrefs.GetInt("Score"+ layer) + GetScore(layer));
    }

    public int GetScore(int layer){
        return 100 * PlayerPrefs.GetInt("Mult" + layer);
    }
}
