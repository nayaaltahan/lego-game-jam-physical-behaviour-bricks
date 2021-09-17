using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreUnityBleBridge.Model;
using CoreUnityBleBridge.ToUnity;
using CoreUnityBleBridge.Utilities;
using LEGO.Logger;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CoreUnityBleBridge.ToNative
{
    public class EditorUnityToNative : IUnityToNative
    {
        private static readonly ILog Logger = LogManager.GetLogger<EditorUnityToNative>();
        private readonly NativeToUnity nativeToUnity;
 
        private const byte BoostHubID = 0x40;
        private const byte CityHubID = 0x41;
        private bool isDevicesAdvertising;

        private readonly List<FakeDevice> fakeDevices = new List<FakeDevice>();
        private readonly Dictionary<FakeDevice, Coroutine> advertiseRoutines = new Dictionary<FakeDevice, Coroutine>();
        
        public EditorUnityToNative(NativeToUnity nativeToUnity)
        {
            instance = this;

            CreateAndTrackDevice("FakeBoost1", BoostHubID);
            CreateAndTrackDevice("FakeBoost2 \u2609", BoostHubID);
            CreateAndTrackDevice("FakeCity1", CityHubID);
            CreateAndTrackDevice("FakeCity2 \u2609", CityHubID);

            this.nativeToUnity = nativeToUnity;
        }

        public void Initialize(string services)
        {
            if (!Application.isEditor)
                Logger.Error("Using the EditorUnityToNative while not in editor");

            Logger.Debug("Initialize with services: " + services);

            nativeToUnity.OnAdapterScanStateChanged(AdapterScanState.NotScanning);
        }

        private void CreateAndTrackDevice(string name, byte systemID)
        {
            var newDevice = new FakeDevice(name, systemID);

            fakeDevices.Add(newDevice);

            if (isDevicesAdvertising)
                StartAdvertisingDevice(newDevice);
        }

        public void SetScanState(bool enabled)
        {
            Logger.Debug("SetScanState: " + enabled);
            
            if (enabled)
            {
                nativeToUnity.OnAdapterScanStateChanged(AdapterScanState.TurningOnScanning);
                nativeToUnity.OnAdapterScanStateChanged(AdapterScanState.Scanning);

                StopAllAdvertisingRoutines();
                StartAdvertisingAllDevices();
            } 
            else 
            {
                nativeToUnity.OnAdapterScanStateChanged(AdapterScanState.NotScanning);
                
                StopAllAdvertisingRoutines();

                //TODO: disabled in pup. Figure out why 
                //foreach (var fakeDevice in fakeDevices)
                //{
                //    nativeToUnity.OnDeviceDisappeared(fakeDevice.ID);
                //}
            }
        }

        private void StartAdvertisingAllDevices()
        {
            fakeDevices.ForEach(StartAdvertisingDevice);
            isDevicesAdvertising = true;
        }

        private void StartAdvertisingDevice(FakeDevice device)
        {
            var routine = BleCoroutineRunner.GetDefault()
                                            .StartCoroutine(AdvertiseRoutine(device));

            advertiseRoutines.Add(device, routine);
        }

        public void ConnectToDevice(string deviceID, SendStrategy? sendStrategy)
        {
            Logger.Debug("ConnectToDevice(s): " + deviceID + " sendStrategy: " + sendStrategy);

            var matches = fakeDevices.Where(d => d.ID == deviceID);
            foreach (var match in matches)
            {
                StartCoroutine(ConnectRoutine(match.ID));
            }
        }

        private IEnumerator AdvertiseRoutine(FakeDevice device)
        {
            //fake a start delay
            yield return new WaitForSeconds(Random.Range(0.0f, 5.0f));

            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                var rssi = Random.Range(-250, 250);
                nativeToUnity.OnDeviceStateChanged(device.ID, device.VisibilityState, device.Name, device.ServiceGuid, rssi, device.ManufacturerData);

                if (rssi < -240)
                { 
                    // Becomes invisible for a little while.
                    Logger.Verbose(string.Format("Device '{0}' changes visibility state to: {1}", device.Name, DeviceVisibilityState.Invisible));
                    nativeToUnity.OnDeviceStateChanged(device.ID, DeviceVisibilityState.Invisible, device.Name, device.ServiceGuid, rssi, device.ManufacturerData);

                    yield return new WaitForSeconds(2.0f);
                }
            }
        }

        private void StopAllAdvertisingRoutines()
        {
            var copy = advertiseRoutines.ToArray();
            foreach (var routine in copy)
            {
                BleCoroutineRunner.GetDefault().StopCoroutine(routine.Value);
                advertiseRoutines.Remove(routine.Key);
            }
            isDevicesAdvertising = false;
        }
        
        private IEnumerator ConnectRoutine(string deviceID)
        {
            Logger.Debug("ConnectRoutine: " + deviceID);
            
            nativeToUnity.OnDeviceConnectionStateChanged(deviceID, DeviceConnectionState.Connecting, "");
            yield return new WaitForSeconds(1.5f);
            
            nativeToUnity.OnDeviceConnectionStateChanged(deviceID, DeviceConnectionState.Connected, "");
            yield return new WaitForSeconds(0.2f);
            
            nativeToUnity.OnMtuSizeChanged(new MtuSizeChangedEventArgs()
            {
                DeviceID = deviceID,
                MtuSize = 512
            });

            for (var i = 0; i <= 40; i++) { 
                fakePacketFromDevice(deviceID, new byte[] { (byte)i });
                yield return new WaitForSeconds(0.2f);
            }

            /*TODO: fake packets from device:
                fakePacketFromDevice(deviceID,
                    new MessageHubAttachedIOAttached(0, 1, dk.lego.devicesdk.device.IOType.IO_TYPE_VISION_SENSOR,
                        new LEGORevision(1,2,3,4), new LEGORevision(1,2,3,5)));
                fakePacketFromDevice(deviceID,
                    new MessageHubAttachedIOAttached(0, 2, dk.lego.devicesdk.device.IOType.IO_TYPE_MOTOR_WITH_TACHO,
                        new LEGORevision(1,2,3,6), new LEGORevision(1,2,3,7)));
                fakePacketFromDevice(deviceID,
                    new MessageHubAttachedIOVirtualAttached(0, 50, dk.lego.devicesdk.device.IOType.IO_TYPE_MOTOR_WITH_TACHO, 3,2));
                */
        }

        public void DisconnectFromDevice(string deviceID)
        {
            Logger.Debug("DisconnectFromDevice: " + deviceID);
            
            nativeToUnity.OnDeviceConnectionStateChanged(deviceID, DeviceConnectionState.Disconnected, "");
        }

        public void SetNoAckParameters(string deviceID, int packetCount, int windowLengthMs)
        {
            Logger.Debug("SetNoAckParameters: deviceID: " + deviceID + " packetCount=" + packetCount + " windowLengthMs=" + windowLengthMs);
        }
        
        public void GetWriteMTUSize(string deviceID, string service, string gattChar )
        {
            Logger.Debug("GetWriteMTUSize: deviceID: " + deviceID + " service: " + service + " gattChar: " + gattChar);
        }
        
        public void SendPacket(string deviceID, string service, string characteristic, byte[] data, int group, SendFlags sendFlags, int packetID)
        {
            if (service != Constants.LDSDK.V3_SERVICE || characteristic != Constants.LDSDK.V3_CHARACTERISTIC) {
                Logger.Error("Send packet on unknown service/characteristic: "+service+"/"+characteristic+" to device "+deviceID);
                return;
            }

            Logger.Debug("SendPacket: " + BitConverter.ToString(data));
            if (data.Length >= 3) {
                var msgType = data[2];
                if (msgType >= 0x90) {
                    // Invalid msgType. Send error response:
                    var drop = data[2] != 0xdf && UnityEngine.Random.Range(0, 2) == 0;
                    if (packetID >= 0 && drop) {
                        PacketDropped(deviceID, packetID);
                    } else {
                        fakePacketFromDevice(deviceID, new byte[] {
                            0x05, 0x00, // Length=5
                            0x05, // MsgType "Error"
                            data[2], // Command ID = the invalid MsgType
                            0x05 // Error = "Command not recognized"
                        });
                    }
                }
            }

            nativeToUnity.OnPacketSent(deviceID, service, characteristic);
        }

        public void SendPacketNotifyOnDataTransmitted(string deviceID, string service, string gattChar, byte[] data, int SeqNr, bool softAck)
        {
            SendPacket(deviceID, service, gattChar, data, 0, SendFlags.None, -1);
            if (SeqNr >= 0) {
                Delay(softAck ? 0.2f : 0.3f, () => nativeToUnity.OnPacketTransmitted(deviceID, service, gattChar, SeqNr));
            }
        }

        public void SetLogLevel(int logLevel)
        {
            Logger.Info("LogLevel set to: " + logLevel);
        }

        public void RequestMtuSize(string deviceID, int mtuSize)
        {
            Logger.Debug($"RequestMtuSize(deviceID: {deviceID}, mtuSize: {mtuSize}");
        }

        private void PacketDropped(string deviceID, int packetID)
        {
            Delay(0.0f, ()=>nativeToUnity.OnPacketDropped(new PacketDroppedEventArgs {
                DeviceID = deviceID,
                Service = Constants.LDSDK.V3_SERVICE,
                GattCharacteristic = Constants.LDSDK.V3_CHARACTERISTIC,
                PacketID = packetID
            }));
        }
        private void WriteMTUSize(string deviceID, int writeMTUSize)
        {
            Delay(0.0f, ()=>nativeToUnity.OnWriteMTUSize(new WriteMTUSizeEventArgs {
                DeviceID = deviceID,
                Service = Constants.LDSDK.V3_SERVICE,
                GattCharacteristic = Constants.LDSDK.V3_CHARACTERISTIC,
                WriteMTUSize = writeMTUSize
            }));
        }

        private void fakePacketFromDevice(string deviceID, byte[] message)
        {
            Delay(0.1f, ()=>nativeToUnity.OnPacketReceived(deviceID, Constants.LDSDK.V3_SERVICE, Constants.LDSDK.V3_CHARACTERISTIC, message));
        }

        private void Delay(float delay, Action action)
        {
            StartCoroutine(DelayHelper(delay, action));
        }

        private void StartCoroutine(IEnumerator enumerator)
        {
            BleCoroutineRunner.GetDefault().StartCoroutine(enumerator);
        }

        private static IEnumerator DelayHelper(float delay, Action action) 
        {
            yield return new WaitForSeconds(delay);
                
            action();
        }

        private sealed class FakeDevice
        {
            public readonly string ID = Guid.NewGuid().ToString();
            public DeviceVisibilityState VisibilityState = DeviceVisibilityState.Visible;
            public string Name;
            public Guid ServiceGuid = new Guid(Constants.LDSDK.V3_SERVICE);
            public int Rssi = Random.Range(-250, 250);
            public byte[] ManufacturerData;

            public FakeDevice(string name, byte systemID)
            {
                this.Name = name;
                this.ManufacturerData = new byte[] {
                    0/*Button pressed*/,
                    systemID,
                    0 /*DeviceFunction.CentralMode*/,
                    123,/*Network ID*/
                    0,
                    0
                };
            }
        }

        private static EditorUnityToNative instance = null;
        public static void CreateFakeDevice(string name, byte systemID)
        {
            if (instance == null)
                Logger.Error("Please create an instance of EditorUnityToNative before creating devices");

            instance.CreateAndTrackDevice(name, systemID);
        }
    }
}