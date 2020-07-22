using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using HoV;

namespace BlueConnect {

    public class CheckDeviceBLSConnected
    {
        class DataCommunicationHelper
        {
            public string receivedData;
            public CommunicationDeviceBLS device;
        }

        private UnityBackgroundWorker dataReceiver;
        private DataCommunicationHelper dataReceiverHelper;
        private Boolean isRunning = false;
        private HoareMonitor hm = HoareMonitor.Instance;
        private FinderDevicesBLS fdb =  FinderDevicesBLS.Instance;

        private static CheckDeviceBLSConnected instance = null;

        private bool isNeedMove = false;
        private bool isSelect = false;

        [DllImport("BTManagerLibrary")]
        private static extern bool BTM_IsConnected();

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ConnectToDevice(String data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ReceiveDataFast(String data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_SendDataFast(string data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_DisconnectFromDevice();

        private CheckDeviceBLSConnected(){
            dataReceiverHelper = new DataCommunicationHelper();
        }

        public static CheckDeviceBLSConnected Instance {
            get {
                if( instance == null)
                    instance = new CheckDeviceBLSConnected();
                return instance;
            }
        }

        public void Start(MonoBehaviour caller){
            dataReceiver = new UnityBackgroundWorker(caller, CheckDeviceBegin, CheckDeviceProgress, CheckDeviceDone, dataReceiverHelper);
            try {
                isRunning = true;
                dataReceiver.Run();                
            } catch (Exception e) {}
        }

        public bool IsRunning(){
            return isRunning;
        }

        public bool GetIsNeedMove(){
            if(isNeedMove){
                isNeedMove = false;
                return true;
            }
            return false;
        }

        public bool GetIsSelect(){
            if(isSelect){
                isSelect = false;
                return true;
            }
            return false;
        }

        private string data;

        void CheckDeviceBegin(object CustomData, UnityBackgroundWorkerArguments e)
        {
            LinkedList<CommunicationDeviceBLS> ldb = new LinkedList<CommunicationDeviceBLS>(fdb.GetListDevicesBLS());
            hm.MonitorIn();
            foreach(var device in ldb){
                if(!device.isRunning){
                    try {
                        Thread.Sleep(50);
                        string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(device.nameDevice));;
                        if(status.Contains("Connected")){
                            DataCommunicationHelper temp = (DataCommunicationHelper)CustomData;
                            data = "";
                            do{
                                data += Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(device.nameDevice));
                            } while(data.Length % 127 == 0);

                            temp.receivedData = data; 
                            temp.device = device; 
                            e.Progress++;
                        Thread.Sleep(50);
                        Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                        }
                    }
                    catch (Exception error) {
                        e.HasError = true;
                        e.ErrorMessage = error.Message;
                        Debug.Log(error.Message);
                    }
                }
            }
            hm.MonitorOut();

        }
        void CheckDeviceProgress(object CustomData, int Progress) {
            DataCommunicationHelper temp = (DataCommunicationHelper)CustomData;
            if(!temp.receivedData.Contains("I am connected with " + FinderDevicesBLS.nameGame)){
                Debug.Log("delete device");
                fdb.RemoveDevice(temp.device);
            }
            if(temp.receivedData.Contains("MOVE")){
                isNeedMove = true;
                Debug.Log("Move");
            }
            if(temp.receivedData.Contains("PRESS")) {
                isSelect = true;
                Debug.Log("PRESS");
            }
        }
        void CheckDeviceDone(object CustomData, UnityBackgroundWorkerInformation Information) {
            isRunning = false;
        }
    }
}