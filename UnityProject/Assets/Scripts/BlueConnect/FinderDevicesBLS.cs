/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        29 Juin 2020
 *
 * Fichier :     FinderDevicesBLS.cs
 * Description : Permet de trouver des devices avec un nom commencant par BLS2020HC05HESAV
 *
 *               Basé sur le modèle Singleton
 */

using HoV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace BlueConnect {
    public class FinderDevicesBLS {
        // contient des informations lié au bluetooth
        class DeviceFinderHelper {
            public string transferedText; // list de tous les devices Bluetooth trouvé
            public string surnameDevice; // le nom du device actuellement utilisé
        }

        // Ces fonctions de librairie permettent de réaliser une communication Bluetooth
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_GetDevicesNamesFast();
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ConnectToDevice(string data);
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ReceiveDataFast(string data);
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_SendDataFast(string data);
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_DisconnectFromDevice();

        private const string BEGIN_NAME_DEVICE = "BLS2020HC05HESAV";

        public static readonly string NAME_GAME = "Luffy"; // nom de la station accueillant ce jeu
        public static readonly byte NB_MAX_PLAYER = 6;

        private static FinderDevicesBLS instance = null;
        private Mutex hm = Mutex.Instance;
        private bool isDevicesSearching = false;

        private LinkedList<CommunicationDeviceBLS> listDeviceBLS;
        private LinkedList<string> listDeviceChecked; // list de tous les devices que le programme a tenté une comunication Bluetooth
        private UnityBackgroundWorker ubw; // Thread pour la recherche des devices
        private DeviceFinderHelper dfh; 

        /**
        * Constructeur privé
        */
        private FinderDevicesBLS(){
            listDeviceBLS = new LinkedList<CommunicationDeviceBLS>();
            listDeviceChecked = new LinkedList<String>();
            dfh = new DeviceFinderHelper();
        }

        /**
        * Permet de recupérer l'instance unique de cette objet. Singleton
        */
        public static FinderDevicesBLS Instance {
            get {
                if( instance == null)
                    instance = new FinderDevicesBLS();
                return instance;
            }
        }

        /**
        * Permet d'initier la recherche
        * @param    caller  Le thread courant
        */
        public void FindDevices(MonoBehaviour caller) {
            if(!isDevicesSearching) { 
                ubw = new UnityBackgroundWorker(caller, FindDevicesBegin, FindDevicesProgress, FindDevicesDone, dfh);
                isDevicesSearching = true;
                ubw.Run();
            }
        }

        /**
        * Permet d'arreter la recherche
        */
        public void StopFindDevices() {
            if(isDevicesSearching)
                ubw.Abort();
            isDevicesSearching = false;
        }

        /**
        * Permet de supprimer un device Arduino de la liste
        * @param    cdb  L'objet à supprimer
        */
        public void RemoveDevice(CommunicationDeviceBLS cdb) {
            Debug.Log(listDeviceBLS.Contains(cdb));
            if(listDeviceBLS.Contains(cdb)) {
                listDeviceBLS.Remove(cdb);
                listDeviceChecked.Remove(cdb.nameDevice);
            }
            Debug.Log(listDeviceBLS.Contains(cdb));
        }

        /**
        * Permet de vérifier si un device est déjà associé à un objet existant.
        * @param    name    nom du device
        * @param    surname surnom du device
        * @return   True si device est associé à un objet existant
        */
        private bool isExistDeviceConnect(string name, string surname) {
            foreach(var cdb in listDeviceBLS) {
                if(cdb.nameDevice.Equals(name) && cdb.surnameDevice.Equals(surname))
                    return true;
            } 
            return false;
        }

        /**
        * Créer un objet qui sera lié au device.
        * @param    name    nom du device
        * @param    surname surnom du device
        */
        private void AddDeviceBLS(string name, string surname) {
            if(listDeviceBLS.Count < NB_MAX_PLAYER && !isExistDeviceConnect(name, surname)) {
                listDeviceBLS.AddFirst(new CommunicationDeviceBLS(name, surname));
                listDeviceChecked.AddFirst(name);
            }
        }

        /**
        * Retourne la liste des devices
        */
        public LinkedList<CommunicationDeviceBLS> GetListDevicesBLS() {
            return listDeviceBLS;
        }

        /**
        * retourne le nombre de device trouver
        */
        public int NbDevicesBLS() {
            return listDeviceBLS.Count;
        }

        /**
        * Indique si la recherche est en cours
        */
        public bool IsRunning(){
            return isDevicesSearching;
        }

        /**
        * Initie le protocole avec un device compatible
        * @param    name        nom du device
        * @param    CustomData  Permet de recupérer le surnom du device si le protocole est terminé
        * @return   True si le protocole est effectué jusqu'au bout
        */
        bool checkBLSDevice(string nameDevice, object CustomData){
            hm.MonitorIn();
            try {
                string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                if(status.Contains("Connected")) {
                    string available = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                    if(available.Contains("I am available")) {
                        string response = "";
                        do {
                            Marshal.PtrToStringAnsi(BTM_SendDataFast("Hello, I search BLS device"));
                            Thread.Sleep(2000);
                            response = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                        } while(!response.Contains(" Terminate."));
                        response = response.Substring(0, response.Length-2);
                        if(response.Contains("I Am BLS Device. My Name Is ") && response.EndsWith(" Terminate.")) {
                            DeviceFinderHelper temp = (DeviceFinderHelper)CustomData;
                            string[] splitReponse = response.Split(' ');
                            temp.surnameDevice = splitReponse[splitReponse.Length - 2];
                            Marshal.PtrToStringAnsi(BTM_SendDataFast("Ok, my name is " + NAME_GAME));
                            Thread.Sleep(2000);
                            response = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                            Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                            if(response.Contains("I am connected with " + NAME_GAME))
                                return true;
                            else
                                return false;
                        }
                        else{
                            Marshal.PtrToStringAnsi(BTM_SendDataFast("Abort"));
                        }
                    }
                }
            } catch (Exception e) {
                Debug.Log(e.Message);
            }
            finally{
                hm.MonitorOut();
            }
            
            return false;
        }

        /**
        * Cherche les devices et tente un connexion
        * @param    CustomData  information lié au Bluetooth
        * @param    e           indique au thread qu'il y a eu une modification
        */
        void FindDevicesBegin(object CustomData, UnityBackgroundWorkerArguments e) {
            try {
                DeviceFinderHelper temp = (DeviceFinderHelper)CustomData;
                temp.transferedText = Marshal.PtrToStringAnsi(BTM_GetDevicesNamesFast());
                string[] devicesList = temp.transferedText.Split('\n');
                foreach (var device in devicesList) {
                    if (device.Length != 0 && !(listDeviceChecked.Contains(device))) {
                        if(device.Contains(BEGIN_NAME_DEVICE)){
                            if(checkBLSDevice(device, temp))
                                AddDeviceBLS(device, temp.surnameDevice);
                        }
                        else {
                            listDeviceChecked.AddFirst(device);
                        }
                    }
                }
                e.Progress++;
            }
            catch (Exception error) {
                e.HasError = true;
                e.ErrorMessage = error.Message;
            }
        }

        /**
        * Ne fait rien
        */
        void FindDevicesProgress(object CustomData, int Progress) { }
        /**
        * Cloture la fin de la recherche
        */
        void FindDevicesDone(object CustomData, UnityBackgroundWorkerInformation Information) {
            isDevicesSearching = false;
        }
    }
}
