using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;
using System.Linq;
using UnityEngine.EventSystems;

public class TachoMotorServiceUI : AbstractServiceUI, /*ILEGOMotorWithTachoDelegate,*/ ILEGOGeneralServiceDelegate
{

  public RectTransform positionIndicator;
  public Text positionValue;
  public Text speedValue;
  public Slider speedSlider;

  LEGOTachoMotorCommon motor;

  public override void SetupWithService(ILEGOService service)
  {
    base.SetupWithService(service);

    Debug.LogFormat("{0} SetupWithService {1}", this, service);

    service.ResetCombinedModesConfiguration();
    service.AddCombinedMode((int)MotorWithTachoMode.Speed, 1);
    service.AddCombinedMode((int)MotorWithTachoMode.Position, 1);
    service.ActivateCombinedModes();

    motor = (LEGOTachoMotorCommon)service;
  }


  #region Speed slider handling

  public void SetMotorSpeed(int newSpeed)
  {
    if (motor == null)
    {
      Debug.LogFormat("No motor");
      return;
    }

    if (newSpeed == 0)
    {
      motor?.SendCommand(new LEGOTachoMotorCommon.DriftCommand());
    }
    else
    {
      var speedCmd = new LEGOTachoMotorCommon.SetSpeedCommand()
      {
        Speed = newSpeed
      };

      motor.SendCommand(speedCmd);

    }
  }

  public void SpeedSliderChanged()
  {
    SetMotorSpeed((int)speedSlider.value);
  }

  public void SpeedSliderReleased(BaseEventData evt)
  {
    evt.selectedObject.GetComponent<Slider>().value = 0;
  }

  #endregion


/*
  #region Tacho Motor Callbacks - these are never being called by the motor service

  public void DidUpdatePower(LEGOMotorWithTacho motor, LEGOValue oldPower, LEGOValue newPower)
  {
    Debug.LogFormat("DidUpdatePower {0} {1}", oldPower.RawValues[0], newPower.RawValues[0]);
  }

  public void DidUpdateSpeed(LEGOMotorWithTacho motor, LEGOValue oldSpeed, LEGOValue newSpeed)
  {
    Debug.LogFormat("DidUpdateSpeed {0} {1}", oldSpeed.RawValues[0], newSpeed.RawValues[0]);
  }

  public void DidUpdatePosition(LEGOMotorWithTacho motor, LEGOValue oldPosition, LEGOValue newPosition)
  {
    Debug.LogFormat("DidUpdatePosition {0} {1}", oldPosition.RawValues[0], newPosition.RawValues[0]);
  }

  #endregion
  */


  #region Generic data Callbacks

  public void DidChangeState(ILEGOService service, ServiceState oldState, ServiceState newState) { }
  public void DidUpdateInputFormat(ILEGOService service, LEGOInputFormat oldFormat, LEGOInputFormat newFormat)
  {
    // Debug.LogFormat("Service {0} DidUpdateInputFormat to {1}", service, newFormat);
  }
  public void DidUpdateInputFormatCombined(ILEGOService service, LEGOInputFormatCombined oldFormat, LEGOInputFormatCombined newFormat)
  {
    // Debug.LogFormat("Service {0} DidUpdateInputFormatCombined to {1}", service, newFormat);
  }

  public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue)
  {
    // Debug.LogFormat("Service {0} DidUpdateValueData to {1} {2}", service, newValue.RawValues[0], newValue.Mode);
    if (newValue.Mode == (int)MotorWithTachoMode.Position)
    {
      positionValue.text = newValue.RawValues[0].ToString();
      positionIndicator.eulerAngles = Vector3.forward * newValue.RawValues[0];
    }
    else if (newValue.Mode == (int)MotorWithTachoMode.Speed)
    {
      speedValue.text = newValue.RawValues[0].ToString();
    }
  }

  #endregion

}
