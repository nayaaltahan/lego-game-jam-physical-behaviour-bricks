using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using LEGODeviceUnitySDK;

public class ConnectedDeviceList : MonoBehaviour
{
  public DeviceHandler deviceHandler;
  public RectTransform connectedDeviceContent;
  public GameObject listItemPrefab;

  Dictionary<string, GameObject> listItems = new Dictionary<string, GameObject>();

  void Start()
  {
    if (deviceHandler != null) {
      deviceHandler.OnDeviceDisconnected += OnDeviceDisconnected;
      deviceHandler.OnDeviceInterrogating += OnDeviceInterrogating;
            // if device disconnects and is advertising, OnDeviceAppeared is called
            // we still want to remove it from connected devices
            deviceHandler.OnDeviceAppeared += OnDeviceDisconnected;
    }
  }

  void OnDestroy()
  {
    if (deviceHandler != null) {
      deviceHandler.OnDeviceDisconnected -= OnDeviceDisconnected;
      deviceHandler.OnDeviceInterrogating -= OnDeviceInterrogating;
            deviceHandler.OnDeviceAppeared -= OnDeviceDisconnected;
        }
  }

  void OnDeviceInterrogating(ILEGODevice device)
  {
    Debug.LogFormat("Device connecting {0} {1} {2}", device, device.DeviceID, device.SystemType);
    var listItem = Instantiate(listItemPrefab, connectedDeviceContent);
    var cdli = listItem.GetComponent<ConnectedDeviceListItem>();
    cdli.SetupWithDevice(device, deviceHandler);

    listItems.Add(device.DeviceID, listItem);
  }

  void OnDeviceDisconnected(ILEGODevice device)
  {
    Debug.LogFormat("Device disconnected {0}", device);
    if (listItems.ContainsKey(device.DeviceID))
    {
      var listItem = listItems[device.DeviceID];
      listItems.Remove(device.DeviceID);
      Destroy(listItem);
    }
  }
}
