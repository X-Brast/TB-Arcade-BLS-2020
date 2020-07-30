/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        2 Juillet 2020
 *
 * Fichier :     Mutex.cs
 * Description : Permet de synchroniser plusieurs taché en utilisant des ressources partagés
 */

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BlueConnect {
    public class Mutex 
    {
        private static Mutex instance = null;
        
        private Semaphore monitorMutex;

        /**
        * Constructeur privé
        */
        private Mutex(){
            monitorMutex = new Semaphore(1, 1);
        }

        /**
        * Permet de recupérer l'instance unique de cette objet. Singleton
        */
        public static Mutex Instance{
            get {
                if( instance == null)
                    instance = new Mutex();
                return instance;
            }
        }

        /**
        * Permet l'accès à une section critique du code
        */
        public void MonitorIn(){
            monitorMutex.WaitOne();
            Debug.Log("In");
        }

        /**
        * Libère la section critique du code à un autre thread
        */
        public void MonitorOut(){
            monitorMutex.Release();
            Debug.Log("Out");
        }
    }
}