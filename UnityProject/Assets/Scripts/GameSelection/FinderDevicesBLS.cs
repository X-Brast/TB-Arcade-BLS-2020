using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using HoV;

namespace BlueConnect {

    class DeviceFinderHelper
    {
        public string transferedText;
        public string surnameDevice;
    }


    public class FinderDevicesBLS
    {
        private static FinderDevicesBLS instance = null;
        public static String nameGame = "Luffy";

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_GetDevicesNamesFast();

        [DllImport("BTManagerLibrary")]
        private static extern bool BTM_IsConnected();

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ConnectToDevice(String data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_DisconnectFromDevice();

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ReceiveDataFast(String data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_SendDataFast(string data);

        [DllImport("BTManagerLibrary")]
        private static extern bool BTM_IsReceiving();

        private LinkedList<ConnectorDeviceBLS> listDeviceBLS;
        private LinkedList<String> listDeviceChecked;
        private UnityBackgroundWorker ubw;
        private DeviceFinderHelper dfh;

        private HoareMonitor hm = HoareMonitor.Instance;
        private bool isDevicesSearching = false;

        private FinderDevicesBLS(){
            listDeviceBLS = new LinkedList<ConnectorDeviceBLS>();
            listDeviceChecked = new LinkedList<String>();
            dfh = new DeviceFinderHelper();
        }

        public static FinderDevicesBLS Instance {
            get {
                if( instance == null)
                    instance = new FinderDevicesBLS();
                return instance;
            }
        }

        public void FindDevices(MonoBehaviour caller)
        {
            if(!isDevicesSearching){ 
                ubw = new UnityBackgroundWorker(caller, FindDevicesBegin, FindDevicesProgress, FindDevicesDone, dfh);
                isDevicesSearching = true;
                ubw.Run();
            }
        }

        public void StopFindDevices(){
            if(isDevicesSearching)
                ubw.Abort();
            isDevicesSearching = false;
        }

        public void RemoveDevice(ConnectorDeviceBLS cdb){
            if(listDeviceBLS.Contains(cdb))
                listDeviceBLS.Remove(cdb);
        }

        private bool isExistDeviceConnect(String name, String surname){
            foreach(var cdb in listDeviceBLS){
                if(cdb.nameDevice.Equals(name) && cdb.surnameDevice.Equals(surname))
                    return true;
            } 
            return false;
        }

        void AddDeviceBLS(String name, String surname){
            if(listDeviceBLS.Count < 6 && !isExistDeviceConnect(name, surname)){
                listDeviceBLS.AddFirst(new ConnectorDeviceBLS(name, surname));
            }
        }

        public LinkedList<ConnectorDeviceBLS> GetListDevicesBLS() {
            return listDeviceBLS;
        }

        public int NbDevicesBLS() {
            return listDeviceBLS.Count;
        }

        public bool IsRunning(){
            return isDevicesSearching;
        }

        bool checkBLSDevice(String nameDevice, object CustomData){
            try {
                hm.MonitorIn();
                string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                Debug.Log("finding device : " + status);
                if(status.Contains("Connected")){
                    String available = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                    if(available.Contains("I am available")){
                        string response = "";
                        do{
                            Marshal.PtrToStringAnsi(BTM_SendDataFast("Hello, I search BLS device"));
                            Thread.Sleep(2000);
                            response = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                            Debug.Log(response + response.Contains(" Terminate."));
                        } while(!response.Contains(" Terminate."));
                        response = response.Substring(0, response.Length-2);
                        Debug.Log("End Loop  " + response + response.Contains("I Am BLS Device. My Name Is ") + response.EndsWith(" Terminate."));
                        if(response.Contains("I Am BLS Device. My Name Is ") && response.EndsWith(" Terminate.")){
                            DeviceFinderHelper temp = (DeviceFinderHelper)CustomData;
                            String[] splitReponse = response.Split(' ');
                            temp.surnameDevice = splitReponse[splitReponse.Length - 2];
                            Marshal.PtrToStringAnsi(BTM_SendDataFast("Ok, my name is " + nameGame));
                            Thread.Sleep(2000);
                            response = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                            Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                            if(response.Contains("I am connected with " + nameGame))
                                return true;
                            else
                                return false;
                        }
                        else{
                            Marshal.PtrToStringAnsi(BTM_SendDataFast("Abort"));
                        }
                    }
                }
            } catch (Exception e) {}
            finally {
                Thread.Sleep(200);
                if(BTM_IsConnected())
                    Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                hm.MonitorOut();
            }
            
            return false;
        }

        void FindDevicesBegin(object CustomData, UnityBackgroundWorkerArguments e) {
            try {
                DeviceFinderHelper temp = (DeviceFinderHelper)CustomData;
                temp.transferedText = Marshal.PtrToStringAnsi(BTM_GetDevicesNamesFast());
                string[] devicesList = temp.transferedText.Split('\n');
                foreach (var device in devicesList) {
                    if (device.Length != 0 && !(listDeviceChecked.Contains(device))) {
                        if(device.Contains("BLS2020HC05HESAV")){
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
        void FindDevicesProgress(object CustomData, int Progress) { }
        void FindDevicesDone(object CustomData, UnityBackgroundWorkerInformation Information) {
            isDevicesSearching = false;
            if (Information.Status == UnityBackgroundWorkerStatus.HasError)
                Debug.Log(Information.ErrorMessage);
        }
    }
}
