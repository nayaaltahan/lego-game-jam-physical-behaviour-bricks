using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;
using System.Linq;


public class ConnectedDeviceListItem : MonoBehaviour
{

  public Text deviceNameValue;
  public Text batteryLevelValue;
  public Text stateValue;
  public Image centerButtonIndicator;
  public RectTransform servicePanel;

  [SerializeField] GameObject colorSensorServiceUIPrefab;
  [SerializeField] GameObject visionSensorServiceUIPrefab;
  [SerializeField] GameObject orientationServiceUIPrefab;
  [SerializeField] GameObject accelerationServiceUIPrefab;
  [SerializeField] GameObject buttonServiceUIPrefab;
  [SerializeField] GameObject tachoMotorServiceUIPrefab;
  [SerializeField] GameObject motorServiceUIPrefab;
  [SerializeField] GameObject temperatureServiceUIPrefab;
  [SerializeField] GameObject forceSensorServiceUIPrefab;
  [SerializeField] GameObject distanceSensorServiceUIPrefab;
  [SerializeField] GameObject lightServiceUIPrefab;

  DeviceHandler handler;
  ILEGODevice device;
  new LEGORGBLight light;

  Dictionary<ILEGOService, GameObject> serviceUIElements = new Dictionary<ILEGOService, GameObject>();

  public void SetupWithDevice(ILEGODevice device, DeviceHandler handler)
  {
    this.device = device;
    this.handler = handler;

    deviceNameValue.text = device.DeviceID;
    stateValue.text = "Interrogating...";

    this.device.OnDeviceStateUpdated += OnDeviceStateUpdated;
    this.device.OnServiceConnectionChanged += OnServiceConnectionChanged;
    this.device.OnNameUpdated += OnNameUpdated;
    this.device.OnBatteryLevelPercentageUpdated += OnBatteryLevelPercentageUpdated;
    this.device.OnButtonStateUpdated += OnButtonStateUpdated;
  }

  void OnDestroy()
  {
    if (device != null)
    {
      device.OnDeviceStateUpdated -= OnDeviceStateUpdated;
      this.device.OnServiceConnectionChanged -= OnServiceConnectionChanged;
      device.OnNameUpdated -= OnNameUpdated;
      device.OnBatteryLevelPercentageUpdated -= OnBatteryLevelPercentageUpdated;
      device.OnButtonStateUpdated -= OnButtonStateUpdated;
    }
  }

  #region UI Button Handlers

  public void DisconnectDevice()
  {
    handler?.DisconnectDevice(device);
  }

  public void SetLEDColor(int i)
  {
    LEGORGBLight.SetColorIndexCommand colorCmd = new LEGORGBLight.SetColorIndexCommand()
    {
      ColorIndex = i
    };
    light?.SendCommand(colorCmd);
  }

  #endregion


  #region Device Delegates

  void OnDeviceStateUpdated(ILEGODevice device, DeviceState oldState, DeviceState newState)
  {
    stateValue.text = newState.ToString();

    switch (newState)
    {
      case DeviceState.InterrogationFinished:
        SetUpServices();
        break;

      case DeviceState.DisconnectedNotAdvertising:
        DisconnectDevice();
        break;
    }
  }

  void SetUpServices()
  {
    Debug.LogFormat("Setting up services");
    var lightServices = ServiceHelper.GetServicesOfType(device, IOType.LEIOTypeRGBLight);
    if (lightServices == null || lightServices.Count() == 0)
    {
      Debug.LogFormat("No light services found   !");
    }
    else
    {
      light = (LEGORGBLight)(lightServices.First());
      Debug.LogFormat("Has light service {0}", light);
    }
  }

  void OnServiceConnectionChanged(ILEGODevice device, ILEGOService service, bool connected)
  {
    Debug.LogFormat("Service {0} changed state to ({1})", service, connected ? "connected" : "disconnected");

    if (connected)
    {
      if (!serviceUIElements.ContainsKey(service))
      {
        AddServiceUI(service);
      }
      else
      {
        Debug.LogFormat("Service already in UI");
      }
    }
    else
    {
      var uiElem = serviceUIElements[service];
      Destroy(uiElem);
    }
  }

  void OnNameUpdated(ILEGODevice device, string name)
  {
    deviceNameValue.text = name;
  }

  void OnBatteryLevelPercentageUpdated(ILEGODevice device, int level)
  {
    batteryLevelValue.text = level.ToString();
  }

  void OnButtonStateUpdated(ILEGODevice device, bool pressed)
  {
    centerButtonIndicator.color = pressed ? Color.green : Color.gray;
  }

  #endregion

  
  // Add UI for various service types
  void AddServiceUI(ILEGOService service)
  {
    GameObject serviceUI = null;
    if (service.ioType == IOType.LEIOTypeTechnicColorSensor) {
        Debug.Log("Set up color sensor");
        serviceUI = Instantiate(colorSensorServiceUIPrefab, servicePanel);
    } else if (service.ioType == IOType.LEIOTypeVisionSensor) {
        Debug.Log("Set up vision sensor");
        serviceUI = Instantiate(visionSensorServiceUIPrefab, servicePanel);
    } else if (IOTypes.PowerOnlyMotors.Contains(service.ioType)) {
        Debug.Log("Set up motor");
        serviceUI = Instantiate(motorServiceUIPrefab, servicePanel);
    } else if (IOTypes.TachoMotors.Contains(service.ioType)) {
        Debug.Log("Set up tacho motor");
        serviceUI = Instantiate(tachoMotorServiceUIPrefab, servicePanel);
    } else if (service.ioType == IOType.LEIOTypeTechnic3AxisOrientationSensor) {
        Debug.Log("Set up orientation sensor");
        serviceUI = Instantiate(orientationServiceUIPrefab, servicePanel);
    } else if (service.ioType == IOType.LEIOTypeTechnic3AxisAccelerometer) {
        Debug.Log("Set up accelerometer sensor");
        serviceUI = Instantiate(accelerationServiceUIPrefab, servicePanel);
    } else if (service.ioType == IOType.LEIOTypeTechnicTemperatureSensor) {
        Debug.Log("Set up temperature sensor");
        serviceUI = Instantiate(temperatureServiceUIPrefab, servicePanel);
    } else if (service.ioType == IOType.LEIOTypeRemoteControlButtonSensor) {
        Debug.Log("Set up button sensor");
        serviceUI = Instantiate(buttonServiceUIPrefab, servicePanel);
    } else if (service.ioType == IOType.LEIOTypeTechnicForceSensor) {
        Debug.Log("Set up force sensor");
        serviceUI = Instantiate(forceSensorServiceUIPrefab, servicePanel);
    } else if (service.ioType == IOType.LEIOTypeTechnicDistanceSensor) {
        Debug.Log("Set up distance sensor");
        serviceUI = Instantiate(distanceSensorServiceUIPrefab, servicePanel);
    } else if (service.ioType == IOType.LEIOTypeLight) {
        Debug.Log("Set up light");
        serviceUI = Instantiate(lightServiceUIPrefab, servicePanel);
    } 
    else 
    {
        Debug.LogError("Unknown ioType: " + service.ioType);
        return;
    }

    serviceUI.GetComponent<AbstractServiceUI>().SetupWithService(service);
    serviceUIElements[service] = serviceUI;
  }

}

