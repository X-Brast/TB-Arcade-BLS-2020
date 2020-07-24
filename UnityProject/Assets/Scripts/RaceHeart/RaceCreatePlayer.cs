/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        23 Juillet 2020
 *
 * Fichier :     RaceCreatePlayer.cs
 * Description : Il va permettre de créer pour chaque joueur un panel et une camera qui lui seront dédié.
                 Il y a un vérification pour savoir si l'arduino du joueur est toujours connecté
                 Si aucun joueur n'est connecté, retourne à la selection de jeu
 */

using BlueConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace RaceHeart {
    public class RaceCreatePlayer : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public Canvas canvas; // Le canvas qui contient les objets nécéssaire au joueur.
        public Camera camera; // La caméra destiné au joueur
        public GameObject loaderScene; // Permet de changer de Scene 
        public Canvas loadCanvas; // Canvas d'attente avant le démarage du jeu

        /**
        * Verifie si aucun joueur est connecté
        * @return True si aucun joueur n'est connecté
        */
        private bool IsNoPlayer() {
            if(FinderDevicesBLS.Instance.NbDevicesBLS() == 0) {
                loaderScene.GetComponent<LoaderScene>().LoadLevelSelection(0);
                return true;
            }
            return false;
        }

        /**
        * Start est appelé avant la première actualisation de la frame
        */
        void Start() {
            if(IsNoPlayer())
                return;

            StartCoroutine(LoadPanelPlayerGaming());
        }

        /**
        * Créer les panels pour chacun des joueurs
        */
        IEnumerator LoadPanelPlayerGaming() {
            LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();

            // on lance toutes les connexions Si problème on ne va pas créer un panneau pour le joueur
            foreach (var device in ldb)
                device.StartGame(this);

            if(IsNoPlayer())
                yield return null;

            RaceGameLogic rgl = GameObject.Find("GameLogic").GetComponent<RaceGameLogic>();

            ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
            int nbPlayer = ldb.Count;
            float w = 1.0f/nbPlayer;
            float speed = 0;
            int counter = 0;

            foreach (var device in ldb) {
                int layer = LayerMask.NameToLayer("Player" + (counter+1).ToString());
                
                rgl.ResetDataPlayer(device.surnameDevice);

                Camera cam = Instantiate(camera, new Vector3(0, 0,-10), Quaternion.identity);
                cam.rect = new Rect(0.0f, w * counter, 1.0f, w);
                cam.cullingMask = (1 << layer) + (1 << LayerMask.NameToLayer("UI")); // attribu deux layer à la caméra

                Canvas can = Instantiate(canvas, new Vector3(0, 0, 0), Quaternion.identity);
                can.renderMode = RenderMode.ScreenSpaceCamera;
                can.worldCamera = cam;

                can.transform.GetChild(0).gameObject.layer = layer;
                can.transform.GetChild(0).gameObject.GetComponent< Image >().color = device.colorPlayer;
                can.transform.GetChild(0).GetChild(0).gameObject.layer = layer;
                can.transform.GetChild(0).GetChild(1).gameObject.layer = layer;
                can.transform.GetChild(0).GetChild(1).gameObject.GetComponent< Ambulance >().speed = speed;
                can.transform.GetChild(0).GetChild(1).gameObject.GetComponent< Ambulance >().device = device;

                ++counter;
            }

            // Temps d'attente pour que le joueur se prepare pour jouer
            yield return new WaitForSeconds(3.0f);

            loadCanvas.enabled = false;
        }
    }
}
