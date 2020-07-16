using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BlueConnect;

public class GameLogique : MonoBehaviour
{
    public Canvas canvas;
    public Camera camera;

    private bool isNotCloseGame = true;

    private LinkedList<ConnectorDeviceBLS> ldb = new LinkedList<ConnectorDeviceBLS>();

    void Start()
    {
        //LinkedList<ConnectorDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Monstro"));
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Bobo"));
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Carole"));
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Inconnu"));
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Richard"));
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Jessica"));

        int nbPlayer = ldb.Count;

        if(nbPlayer == 0){
            SceneManager.LoadScene(1);
            return;
        }

        float w = 1.0f/nbPlayer;
        int counter = 0;
        foreach (var device in ldb) {
            //idevice.StartGame(this);

            int layer = LayerMask.NameToLayer("Player" + (counter+1).ToString());

            PlayerPrefs.SetInt("Highstreak" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("Score" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("NotesHit" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("Streak" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("Mult" + device.surnameDevice, 1);

            Camera cam = Instantiate(camera, new Vector3(0, 0,-10), Quaternion.identity);
            cam.rect = new Rect(w * counter, 0.0f, w, 1.0f);
            cam.backgroundColor = new Color(
                                        Random.Range(0f, 1f), 
                                        Random.Range(0f, 1f), 
                                        Random.Range(0f, 1f)
                                    );
            cam.cullingMask = (1 << layer) + (1 << LayerMask.NameToLayer("UI"));

            Canvas can = Instantiate(canvas, new Vector3(0, 0, 0), Quaternion.identity);
            can.renderMode = RenderMode.ScreenSpaceCamera;
            can.worldCamera = cam;

            can.transform.GetChild(0).gameObject.GetComponent< PPText >().name = "Score" + device.surnameDevice;
            can.transform.GetChild(1).gameObject.GetComponent< Text >().text = device.surnameDevice;

            can.SendMessage("InitKey", device);
            can.SendMessage("TheStart", layer);

            ++counter;
        }
    }

    public void AddStreak(string nameDevice){
        PlayerPrefs.SetInt("Streak" + nameDevice, PlayerPrefs.GetInt("Streak" + nameDevice)+1);
        PlayerPrefs.SetInt("NotesHit" + nameDevice, PlayerPrefs.GetInt("NotesHit" + nameDevice)+1);
        
        int streak = PlayerPrefs.GetInt("Streak" + nameDevice);
        int multiplier = PlayerPrefs.GetInt("Mult" + nameDevice);

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

        PlayerPrefs.SetInt("Mult" + nameDevice, multiplier);

        if(streak > PlayerPrefs.GetInt("HighStreak" + nameDevice))
            PlayerPrefs.SetInt("Highstreak" + nameDevice, streak);
    }

    public void ResetStreak(string nameDevice){
        PlayerPrefs.SetInt("Mult" + nameDevice, 1);
        PlayerPrefs.SetInt("Streak" + nameDevice, 0);
    }

    public void EndGame(){
        if(isNotCloseGame){
            isNotCloseGame = false;
            //LinkedList<ConnectorDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
            foreach (var device in ldb){
                //device.StopGame();
                if(PlayerPrefs.GetInt("Score" + device.surnameDevice) > PlayerPrefs.GetInt("Highscore" + device.surnameDevice));
                    PlayerPrefs.SetInt("Highscore" + device.surnameDevice, PlayerPrefs.GetInt("Score" + device.surnameDevice));
            } 
            SceneManager.LoadScene("HeartHeroClassement");
        }
    }

    public void AddScore(string nameDevice, int typePrecision){
        int score = 0;
        switch (typePrecision) {
            case 1:
                score = 200;
                break;
            case 2:
                score = 100;
                break;
            case 3:
                score = 75;
                break;
            case 4:
                score = 50;
                break;
            default:
                score = 0;
                break;
        }
        score *= PlayerPrefs.GetInt("Mult" + nameDevice);
        PlayerPrefs.SetInt("Score" + nameDevice, PlayerPrefs.GetInt("Score"+ nameDevice) + score);
    }
}
