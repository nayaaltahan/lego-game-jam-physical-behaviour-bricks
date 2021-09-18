using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;
using System.Linq;

public class ButtonServiceUI : AbstractServiceUI, ILEGOButtonSensorDelegate
{

  public Image plusButtonIndicator;
  public Image stopButtonIndicator;
  public Image minusButtonIndicator;

  public override void SetupWithService(ILEGOService service)
  {
    base.SetupWithService(service);

    Debug.LogFormat("{0} SetupWithService {1}", this, service);
    
    service.UpdateInputFormat(new LEGOInputFormat(service.ConnectInfo.PortID, service.ioType, (int)LEGOButtonSensor.LEButtonSensorMode.KEYA, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
  }

  #region Remote Control Button Callbacks

  public void DidUpdateButton(LEGOButtonSensor buttonSensor, LEGOValue oldButtonVal, LEGOValue newButtonVal)
  {
      switch (newButtonVal.RawValues[0])
      {
        case 0:
          plusButtonIndicator.color = stopButtonIndicator.color = minusButtonIndicator.color = Color.white;
          break;
        case 1:
          plusButtonIndicator.color = Color.grey;
          break;
        case -1:
          minusButtonIndicator.color = Color.grey;
          break;
        case 127:
          stopButtonIndicator.color = Color.grey;
          break;
      }
  }

  #endregion

}


