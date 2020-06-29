using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using HoV;

namespace blueConnect {

    class DeviceFinderHelper
    {
        public string transferedText;
        public string surnameDevice;
    }


    public class FinderDevicesBLS
    {
        private static FinderDevicesBLS instance = null;

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

        private bool isDevicesSearching = false;

        private LinkedList<ConnectorDeviceBLS> listDeviceBLS;
        private LinkedList<String> listDeviceChecked;
        private UnityBackgroundWorker ubw;
        private DeviceFinderHelper dfh;

        private String nameGame = "Luffy";

        private FinderDevicesBLS(){
            listDeviceBLS = new LinkedList<ConnectorDeviceBLS>();
            listDeviceChecked = new LinkedList<String>();
            dfh = new DeviceFinderHelper();
        }

        public static FinderDevicesBLS Instance
        {
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

        void AddDeviceBLS(String name){
            if(listDeviceBLS.Count < 6){
                listDeviceBLS.AddFirst(new ConnectorDeviceBLS(name));
            }
        }

        public LinkedList<ConnectorDeviceBLS> GetListDevicesBLS() {
            return listDeviceBLS;
        }

        public bool IsRunning(){
            return isDevicesSearching;
        }

        bool checkBLSDevice(String nameDevice, object CustomData){
            if(nameDevice.Contains("BLS2020HC05HESAV")){
                try {
                    string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                    if(status.Contains("Connected")){
                        Marshal.PtrToStringAnsi(BTM_SendDataFast("Hello, I search BLS device"));
                        bool isFinish = false;
                        String response = "";
                        String lastResponse = "";
                        while(!isFinish){
                            lastResponse += Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                            if(string.Equals(response, lastResponse) && !string.Equals("", response)){
                                isFinish = true;
                            }
                            else{
                                response = lastResponse;
                            }
                            if(response.Contains(" Terminate."))
                                isFinish = true;
                        }
                        response = response.Substring(0, response.Length-2);
                        if(response.Contains("I Am BLS Device. My Name Is ") && response.EndsWith(" Terminate.")){
                            DeviceFinderHelper temp = (DeviceFinderHelper)CustomData;
                            temp.surnameDevice = response.Substring(21);
                            Marshal.PtrToStringAnsi(BTM_SendDataFast("Ok, my name is " + nameGame));
                            return true;
                        }
                        else{
                            Marshal.PtrToStringAnsi(BTM_SendDataFast("Abort"));
                        }
                    }
                } catch (Exception e) {}
                finally {
                    Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                }
            }
            
            return false;
        }

        void FindDevicesBegin(object CustomData, UnityBackgroundWorkerArguments e) {
            try {
                DeviceFinderHelper temp = (DeviceFinderHelper)CustomData;
                temp.transferedText = Marshal.PtrToStringAnsi(BTM_GetDevicesNamesFast());
                e.Progress++;
                
            }
            catch (Exception error) {
                e.HasError = true;
                e.ErrorMessage = error.Message;
            }
        }
        void FindDevicesProgress(object CustomData, int Progress) {
            DeviceFinderHelper temp = (DeviceFinderHelper)CustomData;
            string[] devicesList = temp.transferedText.Split('\n');
            foreach (var device in devicesList) {
                if (device.Length != 0 && !(listDeviceChecked.Contains(device))) {
                    if(checkBLSDevice(device, temp))
                        AddDeviceBLS(device);
                    listDeviceChecked.AddFirst(device);
                }
            }
        }
        void FindDevicesDone(object CustomData, UnityBackgroundWorkerInformation Information) {
            isDevicesSearching = false;
        }
    }
}
