/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        29 Mai 2020
 *
 * Fichier :     Note.cs
 * Description : Definit des opérations d'une note
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeartHero {
    public class Note : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public float speed;

        private Rigidbody2D rb; // un gameobject qui permet d'appliquer la gravité à la note

        void Awake(){
            rb = GetComponent<Rigidbody2D>();
        }

        /**
        * Defini la vitesse de la note
        */
        void Start()
        {
            rb.velocity = new Vector2(0, -speed);
        }

        /**
        * Detruit la note quand elle sort d'une collision
        * @param    col  l'objet qui a effectué la collision
        */
        void OnTriggerExit2D(Collider2D col){
            Destroy(gameObject);
        }
    }
}
