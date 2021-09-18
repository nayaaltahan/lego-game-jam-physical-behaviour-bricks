using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;
using System.Linq;
using UnityEngine.EventSystems;

public class MotorServiceUI : AbstractServiceUI
{
  public Text powerValue;
  public Slider powerSlider;

  LEGOMotor motor;

  public override void SetupWithService(ILEGOService service)
  {
    base.SetupWithService(service);

    Debug.LogFormat("{0} SetupWithService {1}", this, service);

    motor = (LEGOMotor)service;
  }

  #region Power slider handling

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

      powerValue.text = newPower.ToString();
    }
  }

  public void PowerSliderChanged()
  {
    SetMotorPower((int)powerSlider.value);
  }

  public void PowerSliderReleased(BaseEventData evt)
  {
    evt.selectedObject.GetComponent<Slider>().value = 0;
  }

  #endregion

}
