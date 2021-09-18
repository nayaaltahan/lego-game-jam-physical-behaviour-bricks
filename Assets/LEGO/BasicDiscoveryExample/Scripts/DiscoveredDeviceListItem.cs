using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;


public class DiscoveredDeviceListItem : MonoBehaviour
{

  public Text deviceNameValue;
  public Text deviceTypeValue;
  public Text rssiValue;
  public Image pressedIndicator;

  ILEGODevice device;
  DeviceHandler handler;

  public void SetupWithDevice(ILEGODevice device, DeviceHandler handler)
  {
    this.device = device;
    this.handler = handler;

    deviceNameValue.text = device.DeviceID;
    deviceTypeValue.text = device.SystemType.ToString();

    this.device.OnButtonStateUpdated += OnButtonStateUpdated;
    this.device.OnRSSIValueUpdated += OnRSSIValueUpdated;
  }

  void OnDestroy()
  {
    device.OnButtonStateUpdated -= OnButtonStateUpdated;
    device.OnRSSIValueUpdated -= OnRSSIValueUpdated;
  }

  public void ConnectDevice()
  {
    handler.ConnectToDevice(device);
  }

  void OnButtonStateUpdated(ILEGODevice device, bool pressed)
  {
    Debug.Log("Device button is " + pressed);
    pressedIndicator.color = pressed ? Color.green : Color.gray;
  }

  void OnRSSIValueUpdated(ILEGODevice device, int rssi)
  {
    rssiValue.text = rssi.ToString();
  }
}

