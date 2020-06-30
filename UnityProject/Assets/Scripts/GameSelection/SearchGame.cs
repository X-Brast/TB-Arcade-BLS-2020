using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SearchGame : MonoBehaviour
{
    class GameInformation {
        public String nameGame {get;}
        public String nameScene {get;}
        public Image imageGame {get;}

        public GameInformation(String nameGame, String nameScene, Image imageGame){
            this.nameGame = nameGame;
            this.nameScene = nameScene;
            this.imageGame = imageGame;
        }
    }

    public GameObject ButtonStartGame;
    public Canvas canvas;

    private LinkedList<GameInformation> lgi;

    // Start is called before the first frame update
    void Start()
    {
        lgi = new LinkedList<GameInformation>();
        lgi.AddLast(new GameInformation("Heart Hero", "HeartHero", null));
        lgi.AddLast(new GameInformation("Race Heart", "RaceHeart", null));
        
        CreateTileGame();
    }

    private void CreateTileGame(){
        int counter = 0;
        foreach (var gi in lgi) {
            int x = (232 + (counter % 4 / 2) * 464) * (counter % 2 == 0 ? 1 : -1);
            int y = (365 - (counter / 4) * 350);
            Vector3 position = new Vector3(x, y, 0);

            GameObject go = Instantiate(ButtonStartGame, position, Quaternion.identity);
            go.transform.SetParent(canvas.transform, false);
            go.GetComponent<Button>().onClick.AddListener(delegate{
                PlayerPrefs.SetInt("nbPlayer", SearchPlayer.GetNbPlayer());
                SceneManager.LoadScene(gi.nameScene);
            });

            GameObject imageGame = go.transform.GetChild(0).gameObject;
            GameObject textGame = go.transform.GetChild(1).gameObject;
            textGame.GetComponent<Text>().text = gi.nameGame;

            counter++;
        }
    }
}
