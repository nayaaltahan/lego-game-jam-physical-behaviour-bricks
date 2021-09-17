using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;
using System.Linq;

public class AccelerationServiceUI : AbstractServiceUI, ILEGOGeneralServiceDelegate
{

  public Transform accXUI;
  public Transform accYUI;
  public Transform accZUI;

  public override void SetupWithService(ILEGOService service)
  {
    base.SetupWithService(service);

    Debug.LogFormat("{0} SetupWithService {1}", this, service);

    service.UpdateInputFormat(new LEGOInputFormat(service.ConnectInfo.PortID, service.ioType, 0, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
  }

  #region Generic data Callback

  public void DidChangeState(ILEGOService service, ServiceState oldState, ServiceState newState) {}

  public void DidUpdateInputFormat(ILEGOService service, LEGOInputFormat oldFormat, LEGOInputFormat newFormat) { }

  public void DidUpdateInputFormatCombined(ILEGOService service, LEGOInputFormatCombined oldFormat, LEGOInputFormatCombined newFormat) { }

  public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue) {
    if (newValue.RawValues.Length == 3) {
      accXUI.localScale = new Vector3(newValue.RawValues[0]/4096f, 1f, 1f);
      accYUI.localScale = new Vector3(1f, newValue.RawValues[1]/4096f, 1f);
      accZUI.localScale = new Vector3(1f, 1f, newValue.RawValues[2]/4096f);

      // Debug.LogFormat("Acc [{0} : {1} : {2}]", newValue.RawValues[0], newValue.RawValues[1], newValue.RawValues[2]);
    }
   }


  #endregion

}
