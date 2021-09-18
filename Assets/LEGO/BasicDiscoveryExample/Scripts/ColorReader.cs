using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using LEGODeviceUnitySDK;
using Debug = UnityEngine.Debug;


public class ColorReader : MonoBehaviour
{
    public DeviceHandler deviceHandler;
    ILEGODevice device;
    LEGOTechnicColorSensor colorSensorService;
    ColorSensor colorSensor;
    LEGOSingleTechnicMotor motorService;
    MotorIO motor;
    LEGOTechnicForceSensor forceService;
    ForceIO force;
    private bool isScanning;
    private bool isReady;
    public static Queue<Color> colorList;
    public static List<Color> scannedColors;

    private bool running;
    private int speed;
    private Dictionary<Color, Color> dict;


    // Start is called before the first frame update
    void Start()
    {
        deviceHandler.OnDeviceInitialized += OnDeviceInit;
        deviceHandler.OnDeviceAppeared += onDeviceAppear;
        deviceHandler.OnDeviceDisconnected += onDeviceDisconnect;
        deviceHandler.StartScanning();
        colorList = new Queue<Color>();
        scannedColors = new List<Color>();
        running = false;
        speed = 50;
        dict = new Dictionary<Color, Color>();
        dict.Add(ColorSensor._defaultColorSet[0], Color.black);
        dict.Add(ColorSensor._defaultColorSet[1], Color.red );
        dict.Add( ColorSensor._defaultColorSet[2], Color.red);
        dict.Add(ColorSensor._defaultColorSet[3], Color.blue);
        dict.Add(ColorSensor._defaultColorSet[4], Color.blue);
        dict.Add(ColorSensor._defaultColorSet[5], Color.green);
        dict.Add(ColorSensor._defaultColorSet[6], Color.green);
        dict.Add(ColorSensor._defaultColorSet[7], Color.red);
        dict.Add(ColorSensor._defaultColorSet[8], Color.red);
        dict.Add(ColorSensor._defaultColorSet[9], Color.red);
        dict.Add(ColorSensor._defaultColorSet[10], Color.red);

    }

    void onDeviceDisconnect(ILEGODevice device)
    {
        Debug.Log("disconnected");
    }

    void onDeviceAppear(ILEGODevice device)
    {
        Debug.Log("just appeared");
        Debug.Log(device.DeviceID);
        //if (device.DeviceID == "7E09662B84900000")
        if (device.DeviceID == "FCBA662B84900000")
        {
            deviceHandler.ConnectToDevice(device);
        }
    }

    void OnDeviceInit(ILEGODevice device)
    {
        Debug.Log("just inited");
        foreach (ILEGOService service in device.Services)
        {
            Debug.Log("Cutie Service: " + service);
            if (service is LEGOTechnicColorSensor)
            {
                colorSensorService = (LEGOTechnicColorSensor)service;
                colorSensor = gameObject.AddComponent<ColorSensor>();
                colorSensorService.RegisterDelegate(colorSensor);
                colorSensorService.UpdateInputFormat(new LEGOInputFormat(colorSensorService.ConnectInfo.PortID, colorSensorService.ioType, (int)LEGOColorSensor.LEColorSensorMode.Color, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
                Debug.Log("FOUND THE COLOR SENSOR!!! WOW AMAZINNG");
            }
            if (service is LEGOSingleTechnicMotor)
            {
                motorService = (LEGOSingleTechnicMotor) service;
                motor = gameObject.AddComponent<MotorIO>();
                motorService.RegisterDelegate(motor);
                motorService.UpdateInputFormat(new LEGOInputFormat(motorService.ConnectInfo.PortID, motorService.ioType, motorService.PositionModeNo, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));

                             motor.motor = motorService;
                             Debug.Log("FOUND A MOTOR");
               }
            var pushMotor = ServiceHelper.GetServicesOfTypeOnPort(device, IOType.LEIOTypeTechnicMotorXL, 1);
            if (pushMotor == null || pushMotor.Count()==0)
                Debug.Log("No motor founds");
            else
            {

            }

            if (service is LEGOTechnicForceSensor)
            {
                forceService = (LEGOTechnicForceSensor)service;
                force = gameObject.AddComponent<ForceIO>();
                forceService.RegisterDelegate(force);
                forceService.UpdateInputFormat(new LEGOInputFormat(forceService.ConnectInfo.PortID, forceService.ioType, 0, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
                Debug.Log("FOUND A Force");
            }

        }

        isReady = true;
    }

    void Update()
    {
        if (isScanning && motor)
        {
            motor.SetMotorPower(speed);

        }
        else if (motor)
        {
            motor.SetMotorPower(0);
        }

        if (isReady && force.forceValue > 0)
        {
            if (!isScanning)
            {
                Debug.Log("Force is:" + force.forceValue);
                isScanning = true;
                StartCoroutine(Scan());
                StartCoroutine(Colory());
            }

        }

    }

    public IEnumerator Colory()
    {
        yield return new WaitForSeconds(3.4f);
        Color newColor = dict[scannedColors.Last()];
        colorList.Enqueue(newColor);
        Debug.Log("just added " + newColor);

        yield return new WaitForSeconds(3.7f);
        newColor = dict[scannedColors.Last()];
        colorList.Enqueue(newColor);

        yield return new WaitForSeconds(4.7f);
        newColor = dict[scannedColors.Last()];
        colorList.Enqueue(newColor);

        yield return new WaitForSeconds(4.2f);
        newColor = dict[scannedColors.Last()];
        colorList.Enqueue(newColor);


        scannedColors.Clear();

    }

    public IEnumerator Scan()
    {
        yield return new WaitForSeconds(18);
        speed *= -2;
        yield return new WaitForSeconds(9);
        isScanning = false;
        speed /= -2;
    }
}

public class ColorSensor : MonoBehaviour, ILEGOGeneralServiceDelegate
{

    // #region Color Sensor Callbacks


  // public void DidUpdateColorIndexFrom(LEGOColorSensor colorSensor, LEGOValue oldColorIndex, LEGOValue newColorIndex)
  // {
  //   Debug.LogFormat("DidUpdateColorIndexFrom {0} {1}", newColorIndex.RawValues, newColorIndex.RawValues.Length);

  //   colorIndicator.color = _defaultColorSet[(int)newColorIndex.RawValues[0]];
  // }

  // public void DidUpdateTagFrom(LEGOColorSensor colorSensor, LEGOValue oldColorIndex, LEGOValue newColorIndex)
  // {
  //   Debug.LogFormat("DidUpdateTagFrom {0} {1}", newColorIndex.RawValues, newColorIndex.RawValues.Length);
  // }

  // public void DidUpdateReflectionFrom(LEGOColorSensor colorSensor, LEGOValue oldReflection, LEGOValue newReflection)
  // {
  //   Debug.LogFormat("DidUpdateReflectionFrom {0} {1}", newReflection.RawValues, newReflection.RawValues.Length);
  // }

  // #endregion

  private void Update()
  {

  }

  #region Generic data Callback

  public void DidChangeState(ILEGOService service, ServiceState oldState, ServiceState newState) { }

  public void DidUpdateInputFormat(ILEGOService service, LEGOInputFormat oldFormat, LEGOInputFormat newFormat) { }

  public void DidUpdateInputFormatCombined(ILEGOService service, LEGOInputFormatCombined oldFormat, LEGOInputFormatCombined newFormat) { }

  public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue)
  {
    // forceValue.text = newValue.RawValues[0].ToString();
    // Debug.LogFormat("DidUpdateValue {0} {1}", newValue.RawValues, newValue.RawValues[0]);
    int index = (int)newValue.RawValues[0];
    if (index == -1) {
        Debug.Log("No color detected");
    } else {
        /*Debug.LogFormat("DidUpdateValue {0} {1}", newValue.RawValues, newValue.RawValues[0]);
        Debug.Log("Current color is " + _defaultColorSet[index]);*/
        ColorReader.scannedColors.Add(_defaultColorSet[index]);
    }
  }

  #endregion

  #region Color mapping

  public static Color[] _defaultColorSet = new Color[]
  {
      Color0(),
      Color1(),
      Color2(),
      Color3(),
      Color4(),
      Color5(),
      Color6(),
      Color7(),
      Color8(),
      Color9(),
      Color10(),
  };
  private static Color Color10()
  {
      return new Color32(255, 110, 60, 255);
  }

  private static Color Color9()
  {
      return new Color32(255, 0, 0, 255);
  }

  private static Color Color8()
  {
      return new Color32(255, 20, 0, 255);
  }

  private static Color Color7()
  {
      return new Color32(255, 55, 0, 255);
  }

  private static Color Color6()
  {
      return new Color32(0, 200, 5, 255);
  }

  private static Color Color5()
  {
      return new Color32(0, 255, 60, 255);
  }

  private static Color Color4()
  {
      return new Color32(70, 155, 140, 255);
  }

  private static Color Color3()
  {
      return new Color32(0, 0, 255, 255);
  }

  private static Color Color2()
  {
      return new Color32(145, 0, 130, 255);
  }

  private static Color Color1()
  {
      return new Color32(255, 10, 18, 255);
  }

  private static Color Color0()
  {
      //aka. off
      return new Color32(0, 0, 0, 255);
  }


  #endregion
    // Copied from LEGORGBLight

}


class MotorIO : MonoBehaviour, ILEGOGeneralServiceDelegate
{
    public LEGOSingleTechnicMotor motor { set; get; }

    public void DidChangeState(ILEGOService service, ServiceState oldState, ServiceState newState)
    {
        throw new System.NotImplementedException();
    }

    public void DidUpdateInputFormat(ILEGOService service, LEGOInputFormat oldFormat, LEGOInputFormat newFormat)
    {
        throw new System.NotImplementedException();
    }

    public void DidUpdateInputFormatCombined(ILEGOService service, LEGOInputFormatCombined oldFormat,
        LEGOInputFormatCombined newFormat)
    {
        throw new System.NotImplementedException();
    }

    public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue)
    {
        throw new System.NotImplementedException();
    }

    public void SetMotorPower(int newPower)
    {
        if (motor == null)
        {
            Debug.LogFormat("No motor");
            return;
        }

        if (newPower == 0)
        {
            motor?.SendCommand(new LEGOMotor.DriftCommand());
        }
        else
        {
            var powerCmd = new LEGOMotor.SetPowerCommand()
            {
                Power = newPower
            };

            motor.SendCommand(powerCmd);

        }
    }
}

public class ForceIO : MonoBehaviour, ILEGOGeneralServiceDelegate
{
    public float forceValue = 0;

    #region Generic data Callback

    public void DidChangeState(ILEGOService service, ServiceState oldState, ServiceState newState) { }

    public void DidUpdateInputFormat(ILEGOService service, LEGOInputFormat oldFormat, LEGOInputFormat newFormat) { }

    public void DidUpdateInputFormatCombined(ILEGOService service, LEGOInputFormatCombined oldFormat, LEGOInputFormatCombined newFormat) { }

    public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue)
    {
        forceValue = newValue.RawValues[0];
    }

    #endregion

}



