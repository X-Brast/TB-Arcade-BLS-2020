using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlueConnect;

public class HeroGameLogique : MonoBehaviour
{
    public Canvas canvas;
    public Camera camera;
    public GameObject loaderScene;
    public Canvas loadCanvas;

    private bool isNotCloseGame = true;
    public bool isLoading = true;

    void Start()
    {
        if(FinderDevicesBLS.Instance.NbDevicesBLS() == 0){
            loaderScene.GetComponent<LoaderScene>().LoadLevelSelection(0);
            return;
        }
        
        StartCoroutine(LoadPanelPlayerGaming());
    }

    IEnumerator LoadPanelPlayerGaming(){
        LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
        
        // on lance toutes les connexions Si problème on ne va pas créer un panneau pour le joueur
        foreach (var device in ldb) {
            device.StartGame(this);
        }


        ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
        int nbPlayer = ldb.Count;
        float w = 1.0f/nbPlayer;
        int counter = 0;

        foreach(var device in ldb) {
            int layer = LayerMask.NameToLayer("Player" + (counter+1).ToString());

            PlayerPrefs.SetInt("Highstreak" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("Score" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("NotesHit" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("Streak" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("Mult" + device.surnameDevice, 1);

            Camera cam = Instantiate(camera, new Vector3(0, 0,-10), Quaternion.identity);
            cam.rect = new Rect(w * counter, 0.0f, w, 1.0f);
            cam.backgroundColor = device.colorPlayer;
            cam.cullingMask = (1 << layer) + (1 << LayerMask.NameToLayer("UI"));

            Canvas can = Instantiate(canvas, new Vector3(0, 0, 0), Quaternion.identity);
            can.renderMode = RenderMode.ScreenSpaceCamera;
            can.worldCamera = cam;

            can.transform.GetChild(0).gameObject.GetComponent< PPText >().name = "Score" + device.surnameDevice;
            can.transform.GetChild(1).gameObject.GetComponent< Text >().text = device.surnameDevice;

            can.SendMessage("InitKey", device);
            can.SendMessage("TheStart", layer);

            ++counter;

            yield return null;
        }

        yield return new WaitForSeconds(3.0f);

        loadCanvas.enabled = false;
        isLoading = false;
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
            LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
            foreach (var device in ldb){
                device.StopGame();
                if(PlayerPrefs.GetInt("Score" + device.surnameDevice) > PlayerPrefs.GetInt("Highscore" + device.surnameDevice));
                    PlayerPrefs.SetInt("Highscore" + device.surnameDevice, PlayerPrefs.GetInt("Score" + device.surnameDevice));
            } 
            loaderScene.GetComponent<LoaderScene>().LoadLevelScore(2);
        }
    }

    public void AddScore(string nameDevice, int typePrecision){
        int score = 0;
        switch (typePrecision) {
            case 1:
                score = 200;
                break;
            case 2:
                score = 150;
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
