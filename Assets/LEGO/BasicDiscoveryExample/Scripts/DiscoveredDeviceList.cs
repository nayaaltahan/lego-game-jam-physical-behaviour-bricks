using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using LEGODeviceUnitySDK;

public class DiscoveredDeviceList : MonoBehaviour
{

  public DeviceHandler deviceHandler;
  public RectTransform discoveredDeviceContent;
  public GameObject listItemPrefab;
  public Button scanButton;

  Dictionary<string, GameObject> listItems = new Dictionary<string, GameObject>();

  // Start is called before the first frame update
  void Start()
  {
    if (deviceHandler != null) {
      deviceHandler.OnDeviceAppeared += OnDeviceAppeared;
      deviceHandler.OnDeviceDisappeared += OnDeviceDisappeared;
    }
  }

  void OnDestroy()
  {
    if (deviceHandler != null) {
      deviceHandler.OnDeviceAppeared -= OnDeviceAppeared;
      deviceHandler.OnDeviceDisappeared -= OnDeviceDisappeared;
    }
  }

  public void ToggleScan()
  {
    if (deviceHandler.isScanning)
    {
      deviceHandler.StopScanning();
      foreach (Transform item in discoveredDeviceContent)
      {
        Destroy(item.gameObject);
      }
      listItems.Clear();

      scanButton.GetComponentInChildren<Text>().text = "Start scan";
    }
    else
    {
      deviceHandler.StartScanning();
      scanButton.GetComponentInChildren<Text>().text = "Stop scan";
    }
  }

  void OnDeviceAppeared(ILEGODevice device)
  {
    Debug.LogFormat("Device appeared {0} {1} {2}", device, device.DeviceName, device.SystemType);
    if (!listItems.ContainsKey(device.DeviceID))
    {

      var listItem = Instantiate(listItemPrefab, discoveredDeviceContent);
      var ddli = listItem.GetComponent<DiscoveredDeviceListItem>();
      ddli.SetupWithDevice(device, deviceHandler);

      listItems.Add(device.DeviceID, listItem);
    }
  }

  void OnDeviceDisappeared(ILEGODevice device)
  {
    Debug.LogFormat("Device disappeared/connected {0}", device.DeviceName);
        if(listItems.ContainsKey(device.DeviceID)) {
            var listItem = listItems[device.DeviceID];
            listItems.Remove(device.DeviceID);
            Destroy(listItem);
        }
        else {
            // todo i just added the check, probably there is issue to be fixed
            Debug.LogWarning("DiscoveredDeviceList OnDeviceDisappeared, device id not in dictionary " + device.DeviceID);
        }

  }
}
