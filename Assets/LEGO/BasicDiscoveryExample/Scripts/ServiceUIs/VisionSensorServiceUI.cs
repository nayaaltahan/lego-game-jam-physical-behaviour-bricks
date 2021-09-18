using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;

public class VisionSensorServiceUI : AbstractServiceUI, ILEGOVisionSensorDelegate
{

  public Image colorIndicator;

  public override void SetupWithService(ILEGOService service)
  {
    base.SetupWithService(service);

    Debug.LogFormat("{0} SetupWithService {1}", this, service);

    service.UpdateInputFormat(new LEGOInputFormat(service.ConnectInfo.PortID, service.ioType, (int)LEVisionSensorMode.RGBRaw, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
  }

  #region Vision Sensor Callbacks

  public void DidUpdateColorIndexFrom(LEGOVisionSensor visionSensor, LEGOValue oldColorIndex, LEGOValue newColorIndex) {
    Debug.LogFormat("DidUpdateColorIndexFrom {0} {1}", oldColorIndex.RawValues, newColorIndex.RawValues);
  }
		public void DidUpdateMeasuredColorFrom(LEGOVisionSensor visionSensor, LEGOValue oldColor, LEGOValue newColor) {
      Debug.LogFormat("DidUpdateMeasuredColorFrom {0} {1}", oldColor.RawValues, newColor.RawValues);
    }
		public void DidUpdateDetectFrom(LEGOVisionSensor visionSensor, LEGOValue oldDetect, LEGOValue newDetect) {
      Debug.LogFormat("DidUpdateDetectFrom {0} {1}", oldDetect.RawValues, newDetect.RawValues);
    }
		public void DidUpdateCountFrom(LEGOVisionSensor visionSensor, LEGOValue oldCount, LEGOValue newCount) {
      Debug.LogFormat("DidUpdateCountFrom {0} {1}", oldCount.RawValues, newCount.RawValues);
    }
		public void DidUpdateReflectionFrom(LEGOVisionSensor visionSensor, LEGOValue oldReflection, LEGOValue newReflection) {
      Debug.LogFormat("DidUpdateReflectionFrom {0} {1}", oldReflection.RawValues, newReflection.RawValues);
    }
		public void DidUpdateAmbientFrom(LEGOVisionSensor visionSensor, LEGOValue oldAmbient, LEGOValue newAmbient) {
      Debug.LogFormat("DidUpdateAmbientFrom {0} {1}", oldAmbient.RawValues, newAmbient.RawValues);
    }
		public void DidUpdateRGBFrom(LEGOVisionSensor visionSensor, LEGOValue oldRGB, LEGOValue newRGB) {
      // Debug.LogFormat("DidUpdateRGBFrom {0} {1}", oldRGB.RawValues, newRGB.RawValues);
      colorIndicator.color = new Color(newRGB.RawValues[0]/255f, newRGB.RawValues[1]/255f, newRGB.RawValues[2]/255f);
    }

  #endregion

}
