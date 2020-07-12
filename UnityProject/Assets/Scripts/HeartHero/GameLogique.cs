using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using blueConnect;

public class GameLogique : MonoBehaviour
{
    public Canvas canvas;
    public Camera camera;

    private bool isNotCloseGame = true;

    private LinkedList<ConnectorDeviceBLS> ldb = new LinkedList<ConnectorDeviceBLS>();

    void Start()
    {
        //LinkedList<ConnectorDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
        //LinkedList<ConnectorDeviceBLS> ldb = new LinkedList<ConnectorDeviceBLS>();
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Monstro"));
        int nbPlayer = ldb.Count;

        if(nbPlayer == 0){
            SceneManager.LoadScene(1);
            return;
        }

        float w = 1.0f/nbPlayer;
        int counter = 0;
        foreach (var device in ldb) {
            device.Start(this);

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
                device.Stop();
                if(PlayerPrefs.GetInt("Score" + device.surnameDevice) > PlayerPrefs.GetInt("Highscore" + device.surnameDevice));
                    PlayerPrefs.SetInt("Highscore" + device.surnameDevice, PlayerPrefs.GetInt("Score" + device.surnameDevice));
            } 
            SceneManager.LoadScene("HeartHeroClassement");
        }
    }

    public void AddScore(string nameDevice){
        PlayerPrefs.SetInt("Score" + nameDevice, PlayerPrefs.GetInt("Score"+ nameDevice) + GetScore(nameDevice));
    }

    public int GetScore(string nameDevice){
        return 100 * PlayerPrefs.GetInt("Mult" + nameDevice);
    }
}
