/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        12 Juillet 2020
 *
 * Fichier :     EventButton.cs
 * Description : Contient tous les evenements que les boutons peuvent invoquer
 */

using BlueConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils {
    public class EventButton : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public GameObject loaderScene; // Permet de changer de Scene  

        /**
        * Permet de recommencer le jeu
        */
        public void RestartGame(int idLevel) {
            loaderScene.GetComponent<LoaderScene>().LoadLevelGame(idLevel);
        } 

        /**
        * Permet de revenir à la sélection des jeux
        */
        public void GameSelection() {
            loaderScene.GetComponent<LoaderScene>().LoadLevelSelection(0);
        }

        /**
        * Permet de quitter le jeu en déconnectant tous les appareils.
        * Ne quitte pas le jeu si vous êtes en mode Debug
        */
        public void QuitGame() {
            LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
            foreach (var device in ldb)
                device.Deconnect();

            Application.Quit();
        }
    }
}