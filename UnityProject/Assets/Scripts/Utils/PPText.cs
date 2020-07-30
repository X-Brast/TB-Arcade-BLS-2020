/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        12 Juillet 2020
 *
 * Fichier :     PPText.cs
 * Description : Permet de mettre à jour dynamiquement le texte d'un objet
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utils {
    public class PPText : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public string nameKey; // la clé pour recupérer l'information

        /**
        * Change le text chaque frame
        */
        void Update() {
            GetComponent<Text>().text = PlayerPrefs.GetInt(nameKey) + "";
        }
    }
}
