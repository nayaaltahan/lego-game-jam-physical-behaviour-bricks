using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;
using System.Linq;
using UnityEngine.EventSystems;

public class LightServiceUI : AbstractServiceUI
{

  public Text percentageValue;
  public Slider percentageSlider;

  new LEGOSingleColorLight light;

  public override void SetupWithService(ILEGOService service)
  {
    base.SetupWithService(service);

    Debug.LogFormat("{0} SetupWithService {1}", this, service);

    light = (LEGOSingleColorLight)service;
  }

  #region Percentage slider handling

  public void SetLightIntensity(int newPercentage)
  {
    if (light == null)
    {
      Debug.LogFormat("No light");
      return;
    }

    var percentageCmd = new LEGOSingleColorLight.SetPercentCommand()
    {
      Percentage = newPercentage
    };

    light.SendCommand(percentageCmd);

    percentageValue.text = newPercentage.ToString();

  }

  public void PercentageSliderChanged()
  {
    SetLightIntensity((int)percentageSlider.value);
  }

  #endregion
}
