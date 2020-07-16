using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BlueConnect;

public class RaceGameLogic : MonoBehaviour
{
    public Canvas canvas;
    public Camera camera;

    private LinkedList<ConnectorDeviceBLS> ldb = new LinkedList<ConnectorDeviceBLS>();
    private int nbPlayer;
    private int nbPlayerFinish = 0;

    // Start is called before the first frame update
    void Start() {
        //LinkedList<ConnectorDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Monstro"));

        
        nbPlayer = ldb.Count;

        if(nbPlayer == 0){
            SceneManager.LoadScene(1);
            return;
        }

        float w = 1.0f/nbPlayer;
        float speed = 0.1f * nbPlayer;
        int counter = 0;
        foreach (var device in ldb) {
            //device.StartGame(this);

            int layer = LayerMask.NameToLayer("Player" + (counter+1).ToString());
            
            PlayerPrefs.SetInt("RaceHighstreak" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("RaceScore" + device.surnameDevice, (int)Time.fixedTime);
            PlayerPrefs.SetInt("RaceStreak" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("RaceGoodHit" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("RaceBadHit" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("RaceMult" + device.surnameDevice, 0);

            Camera cam = Instantiate(camera, new Vector3(0, 0,-10), Quaternion.identity);
            cam.rect = new Rect(0.0f, w * counter, 1.0f, w);
            cam.backgroundColor = new Color(
                                        Random.Range(0f, 1f), 
                                        Random.Range(0f, 1f), 
                                        Random.Range(0f, 1f)
                                    );
            cam.cullingMask = (1 << layer) + (1 << LayerMask.NameToLayer("UI"));

            Canvas can = Instantiate(canvas, new Vector3(0, 0, 0), Quaternion.identity);
            can.renderMode = RenderMode.ScreenSpaceCamera;
            can.worldCamera = cam;

            can.transform.GetChild(0).gameObject.layer = layer;
            can.transform.GetChild(0).GetChild(0).gameObject.layer = layer;
            can.transform.GetChild(0).GetChild(1).gameObject.layer = layer;
            can.transform.GetChild(0).GetChild(1).gameObject.GetComponent< Ambulance >().speed = speed;
            can.transform.GetChild(0).GetChild(1).gameObject.GetComponent< Ambulance >().device = device;

            ++counter;
        }
    }

    public void GoodStreak(string nameDevice){
        PlayerPrefs.SetInt("RaceGoodHit" + nameDevice, PlayerPrefs.GetInt("RaceGoodHit" + nameDevice)+1);
        PlayerPrefs.SetInt("RaceStreak" + nameDevice, PlayerPrefs.GetInt("RaceStreak" + nameDevice)+1);

        int streak = PlayerPrefs.GetInt("RaceStreak" + nameDevice);
        int multiplier = PlayerPrefs.GetInt("RaceMult" + nameDevice);

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

        PlayerPrefs.SetInt("RaceMult" + nameDevice, multiplier);

        if(streak > PlayerPrefs.GetInt("RaceHighstreak" + nameDevice))
            PlayerPrefs.SetInt("RaceHighstreak" + nameDevice, streak);
    }

    public void BadStreak(string nameDevice){
        PlayerPrefs.SetInt("RaceBadHit" + nameDevice, PlayerPrefs.GetInt("RaceBadHit" + nameDevice)+1);
        PlayerPrefs.SetInt("RaceMult" + nameDevice, 1);
        PlayerPrefs.SetInt("RaceStreak" + nameDevice, 0);
    }

    public int GetMult(string nameDevice){
        return PlayerPrefs.GetInt("RaceMult" + nameDevice);
    }

    public void RaceFinish(){
        ++nbPlayerFinish;
        Debug.Log(nbPlayerFinish);
        if(nbPlayerFinish == nbPlayer){
            //LinkedList<ConnectorDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
            foreach (var device in ldb){
                //device.StopGame();
                PlayerPrefs.SetInt("RaceScore" + device.surnameDevice, (int)Time.fixedTime - PlayerPrefs.GetInt("RaceScore" + device.surnameDevice));
                if(PlayerPrefs.GetInt("RaceScore" + device.surnameDevice) > PlayerPrefs.GetInt("RaceHighscore" + device.surnameDevice));
                    PlayerPrefs.SetInt("RaceHighscore" + device.surnameDevice, PlayerPrefs.GetInt("RaceScore" + device.surnameDevice));
            } 
            SceneManager.LoadScene("RaceHeartClassement");
        }
    }
}
