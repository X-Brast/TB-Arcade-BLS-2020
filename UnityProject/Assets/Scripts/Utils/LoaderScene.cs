/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        21 Juillet 2020
 *
 * Fichier :     LoaderScene.cs
 * Description : Permet de changer de scene en mettant un canvas d'attente
 */

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utils {
    public class LoaderScene : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public GameObject loadGameCanvas;
        public GameObject loadScoreCanvas;

        /**
        * Charge un scene avec une attente de preparation de jeu
        * @param    idScene  id de la scene voulant être chargé
        */
        public void LoadLevelGame(int idScene) {
            loadGameCanvas.SetActive(true);
            StartCoroutine(LoadScene(idScene));
        }

        /**
        * Charge un scene avec une attente de recolte des scores
        * @param    idScene  id de la scene voulant être chargé
        */
        public void LoadLevelScore(int idScene) {
            loadScoreCanvas.SetActive(true);
            StartCoroutine(LoadScene(idScene));
        }

        /**
        * Charge un scene sans canvas d'attente
        * @param    idScene  id de la scene voulant être chargé
        */
        public void LoadLevelSelection(int idScene) {
            StartCoroutine(LoadScene(idScene));
        }

        /**
        * Charge la scene
        * @param    idScene  id de la scene voulant être chargé
        */
        IEnumerator LoadScene(int idScene) {
            AsyncOperation op = SceneManager.LoadSceneAsync(idScene);

            while(!op.isDone){
                yield return null;
            }
        }
    }
}