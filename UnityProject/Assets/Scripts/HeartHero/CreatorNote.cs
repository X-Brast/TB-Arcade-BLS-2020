/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        25 Mai 2020
 *
 * Fichier :     CreatorNote.cs
 * Description : Il crée les notes et la main du joueur.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueConnect;

namespace HeartHero {
    public class CreatorNote : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public GameObject targets; // represente la main
        public GameObject note;
        public GameObject finalNote;

        private const float DELAY       = 0.55f; // represente 110 compressions par minute
        private const int NB_MAX_NOTE   = 100;

        private CommunicationDeviceBLS device; 
        private int layer; //  niveau de la strate où sera crée les objets

        private int counterNoteCreate   = 0;

        /**
        * Initialise le device qui sera associé à la main 
        * @param    device  Un device
        */
        void InitDevice(CommunicationDeviceBLS device){
            this.device = device;
        }

        /**
        * Crée la main, recupère les informations d'une note et crée les notes.
        * @param    layer  niveau de la strate
        */
        void TheStart(int layer)
        {
            this.layer = layer;

            float xTarget = targets.transform.position.x;
            float yTarget = targets.transform.position.y;
            float zTarget = targets.transform.position.z;
            Vector3 position = new Vector3(xTarget, yTarget, zTarget);
            targets.layer = layer;
            GameObject go = Instantiate(targets, position, Quaternion.identity);
            go.transform.SetParent(gameObject.transform, false);
            go.GetComponent< Activator >().device = device;

            PlayerPrefs.SetInt("HeroNotesMax", NB_MAX_NOTE);

            StartCoroutine(CreateNote());
        }

        /**
        * Crée les notes et la note final.
        */
        IEnumerator CreateNote(){
            HeroCreatePlayer hcp = GameObject.Find("CreatePlayer").GetComponent<HeroCreatePlayer>();

            // ne crée les notes que quand les devices seront prêts.
            while(hcp.isLoading){
                yield return new WaitForSeconds(1.0f);
            }

            float xBegin = note.transform.position.x;
            float yBegin = note.transform.position.y;
            float zBegin = note.transform.position.z;

            Vector3 position;
            GameObject go;
            while(counterNoteCreate < NB_MAX_NOTE){
                position = new Vector3(xBegin, yBegin, zBegin);
                note.layer = layer;
                go = Instantiate(note, position, Quaternion.identity);
                go.transform.SetParent(gameObject.transform, false);

                Color color = go.GetComponent<SpriteRenderer>().material.color;

                if(counterNoteCreate > NB_MAX_NOTE / 2 + 10 && counterNoteCreate < NB_MAX_NOTE)
                    color.a = 0.1f;
                else if(counterNoteCreate > NB_MAX_NOTE / 2)
                    color.a = 0.5f;

                go.GetComponent<SpriteRenderer>().material.color = color;

                ++counterNoteCreate;
                yield return new WaitForSeconds(DELAY);
            }

            // Création de la note final
            position = new Vector3(xBegin, yBegin, zBegin);
            finalNote.layer = layer;
            go = Instantiate(finalNote, position, Quaternion.identity);
            go.transform.SetParent(gameObject.transform, false);
        }
    }
}