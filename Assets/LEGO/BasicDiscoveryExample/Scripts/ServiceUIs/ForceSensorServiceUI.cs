using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;
using System;

public class ForceSensorServiceUI : AbstractServiceUI, ILEGOGeneralServiceDelegate
{

  public Text forceValue;

  public override void SetupWithService(ILEGOService service)
  {
    base.SetupWithService(service);

    Debug.LogFormat("{0} SetupWithService {1}", this, service);

    service.UpdateInputFormat(new LEGOInputFormat(service.ConnectInfo.PortID, service.ioType, 0, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
  }

  #region Generic data Callback

  public void DidChangeState(ILEGOService service, ServiceState oldState, ServiceState newState) { }

  public void DidUpdateInputFormat(ILEGOService service, LEGOInputFormat oldFormat, LEGOInputFormat newFormat) { }

  public void DidUpdateInputFormatCombined(ILEGOService service, LEGOInputFormatCombined oldFormat, LEGOInputFormatCombined newFormat) { }

  public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue)
  {
    forceValue.text = newValue.RawValues[0].ToString();  
  }

  #endregion

}
