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
        private UnityBackgroundWorker dataReceiver;
        private Boolean isRunning = false;
        private HoareMonitor hm = HoareMonitor.Instance;

        private static CheckDeviceBLSConnected instance = null;

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

        private CheckDeviceBLSConnected(){}

        public static CheckDeviceBLSConnected Instance {
            get {
                if( instance == null)
                    instance = new CheckDeviceBLSConnected();
                return instance;
            }
        }

        public void Start(MonoBehaviour caller){
            dataReceiver = new UnityBackgroundWorker(caller, CheckDeviceBegin, CheckDeviceProgress, CheckDeviceDone, null);
            try {
                isRunning = true;
                dataReceiver.Run();                
            } catch (Exception e) {}
        }

        public bool IsRunning(){
            return isRunning;
        }

        void CheckDeviceBegin(object CustomData, UnityBackgroundWorkerArguments e)
        {
            FinderDevicesBLS fdb =  FinderDevicesBLS.Instance;
            LinkedList<ConnectorDeviceBLS> ldb = new LinkedList<ConnectorDeviceBLS>(fdb.GetListDevicesBLS());
            hm.MonitorIn();
            foreach(var device in ldb){
                try {
                    Thread.Sleep(100);
                    string status;
                    do{
                        status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(device.nameDevice));
                        Debug.Log("check " + status);
                    } while(! status.Contains("Connected"));

                    string data = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(device.nameDevice));
                    if(!data.Contains("I am connected with " + FinderDevicesBLS.nameGame)){
                        Debug.Log("delete device");
                        fdb.RemoveDevice(device);
                    }

                    Debug.Log("check " + data);

                    Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                }
                catch (Exception error) {
                    e.HasError = true;
                    e.ErrorMessage = error.Message;
                    Debug.Log("delete device");
                    fdb.RemoveDevice(device);
                }

                Debug.Log("end one device");
            }
            hm.MonitorOut();

        }
        void CheckDeviceProgress(object CustomData, int Progress) { }
        void CheckDeviceDone(object CustomData, UnityBackgroundWorkerInformation Information) {
            if (Information.Status == UnityBackgroundWorkerStatus.HasError)
                Debug.Log(Information.ErrorMessage);
            isRunning = false;
            Debug.Log("End check");
        }
    }
}