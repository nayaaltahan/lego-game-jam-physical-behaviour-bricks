using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;
using System.Linq;

public class ColorSensorServiceUI : AbstractServiceUI, ILEGOGeneralServiceDelegate
{
  public Image colorIndicator;

  public override void SetupWithService(ILEGOService service)
  {
    base.SetupWithService(service);

    Debug.LogFormat("{0} SetupWithService {1}", this, service);

    service.UpdateInputFormat(new LEGOInputFormat(service.ConnectInfo.PortID, service.ioType, (int)LEGOColorSensor.LEColorSensorMode.Color, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
  }

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
      colorIndicator.color = new Color(0,0,0,0);
    } else {
      colorIndicator.color = _defaultColorSet[index];
    }
  }

  #endregion

  // Copied from LEGORGBLight
  private Color[] _defaultColorSet = new Color[]
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

}
