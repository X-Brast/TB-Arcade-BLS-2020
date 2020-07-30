/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        30 Juin 2020
 *
 * Fichier :     SearchGame.cs
 * Description : Permet de créer une tuile pour chaque jeu
 */

using BlueConnect;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace GameSelection {
    public class SearchGame : MonoBehaviour
    {
        // contient les informations des différents jeux proposé dans l'application
        class GameInformation {
            public string   nameGame    {get;}
            public byte     idScene     {get;}
            public Sprite   imageGame   {get;}

            public GameInformation(string nameGame, byte idScene, Sprite imageGame){
                this.nameGame = nameGame;
                this.idScene = idScene;
                this.imageGame = imageGame;
            }
        }

        // Definit dans l'editeur Unity
        public GameObject buttonStartGame; 
        public Canvas canvas; // permet de position correctement les boutons

        private LinkedList<GameInformation> lgi;
        private List<GameObject> games;

        private int idGameSelect = 0;
        private CheckDeviceBLSConnected cdbc = CheckDeviceBLSConnected.Instance;

        /**
        * Recupère les jeux et crée pour chacun une tuile correspondante
        */
        void Start()
        {
            lgi = new LinkedList<GameInformation>();
            games = new List<GameObject>();

            lgi.AddLast(new GameInformation("Heart Hero", (byte)1, Resources.Load<Sprite>("GameSelection/HeartHero_ImageGame")));
            lgi.AddLast(new GameInformation("Race Heart", (byte)3, Resources.Load<Sprite>("GameSelection/RaceHeart_ImageGame")));
            
            CreateTileGame();

            // permet de définir le focus d'un jeu
            EventSystem.current.SetSelectedGameObject(games[idGameSelect], null);
        }

        /**
        * Vérifie si un joueur a effectué un tache
        */
        void Update(){
            if(cdbc.GetIsSelect()){
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            }
            if(cdbc.GetIsNeedMove()){
                idGameSelect = (++idGameSelect) % games.Count;
                EventSystem.current.SetSelectedGameObject(games[idGameSelect], null);
            }
        }

        /**
        * Crée la tuile correspondante au jeu
        */
        private void CreateTileGame(){

            float widthCanvas = canvas.GetComponent<RectTransform>().rect.width;
            float sizeXButton = widthCanvas / lgi.Count;
            float x = (widthCanvas - sizeXButton) / -2;
            float y = buttonStartGame.transform.position.y;
            float z = buttonStartGame.transform.position.z;

            int counter = 0;
            foreach (var gi in lgi) {
                Vector3 position = new Vector3(x, y, z);
                x += sizeXButton;

                GameObject go = Instantiate(buttonStartGame, position, Quaternion.identity);
                go.transform.SetParent(canvas.transform, false);
                go.GetComponent<Button>().onClick.AddListener(delegate{
                    GameObject.Find("LoaderScene").GetComponent<LoaderScene>().LoadLevelGame(gi.idScene);
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
}
