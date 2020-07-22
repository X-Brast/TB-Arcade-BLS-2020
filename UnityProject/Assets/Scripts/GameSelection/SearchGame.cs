using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BlueConnect;

public class SearchGame : MonoBehaviour
{
    class GameInformation {
        public String nameGame {get;}
        public int idScene {get;}
        public Sprite imageGame {get;}

        public GameInformation(String nameGame, int idScene, Sprite imageGame){
            this.nameGame = nameGame;
            this.idScene = idScene;
            this.imageGame = imageGame;
        }
    }

    public GameObject ButtonStartGame;
    public Canvas canvas;
    public GameObject loaderScene;

    private LinkedList<GameInformation> lgi;
    private List<GameObject> games;
    private int idGameSelect = 0;
    private CheckDeviceBLSConnected cdbc = CheckDeviceBLSConnected.Instance;

    // Start is called before the first frame update
    void Start()
    {
        lgi = new LinkedList<GameInformation>();
        games = new List<GameObject>();

        lgi.AddLast(new GameInformation("Heart Hero", 1, Resources.Load<Sprite>("GameSelection/HeartHero_ImageGame")));
        lgi.AddLast(new GameInformation("Race Heart", 3, Resources.Load<Sprite>("GameSelection/RaceHeart_ImageGame")));
        
        CreateTileGame();

        EventSystem.current.SetSelectedGameObject(games[idGameSelect], null);
    }

    void Update(){
        if(cdbc.GetIsSelect()){
            EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
        }
        if(cdbc.GetIsNeedMove()){
            idGameSelect = (++idGameSelect) % games.Count;
            EventSystem.current.SetSelectedGameObject(games[idGameSelect], null);
        }
    }

    private void CreateTileGame(){

        float widthCanvas = canvas.GetComponent<RectTransform>().rect.width;
        float sizeXButton = widthCanvas / lgi.Count;
        float x = (widthCanvas - sizeXButton) / -2;
        float y = ButtonStartGame.transform.position.y;
        float z = ButtonStartGame.transform.position.z;

        int counter = 0;
        foreach (var gi in lgi) {
            Vector3 position = new Vector3(x, y, z);
            x += sizeXButton;

            GameObject go = Instantiate(ButtonStartGame, position, Quaternion.identity);
            go.transform.SetParent(canvas.transform, false);
            go.GetComponent<Button>().onClick.AddListener(delegate{
                loaderScene.GetComponent<LoaderScene>().LoadLevelGame(gi.idScene);
            });
            games.Add(go);

            GameObject imageGame = go.transform.GetChild(0).gameObject;
            GameObject textGame = go.transform.GetChild(1).gameObject;

            imageGame.GetComponent<Image>().sprite = gi.imageGame;
            textGame.GetComponent<Text>().text = gi.nameGame;

            counter++;
        }
    }
}
