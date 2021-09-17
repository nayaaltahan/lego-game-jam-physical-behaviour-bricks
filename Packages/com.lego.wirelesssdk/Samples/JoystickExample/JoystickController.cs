using System.Collections;
using UnityEngine;
using LEGODeviceUnitySDK;
using System.Linq;

public class JoystickController : MonoBehaviour, ILEGOGeneralServiceDelegate
{

    public Transform planeTransform;
    public Thrust planeThrust;

    [SerializeField] float rollFactor = -10f;
    [SerializeField] float pitchFactor = 10f;

    LEGOTechnicMotor pitchMotor;
    LEGOTechnicMotor rollMotor;
    LEGORGBLight rgbLight;
    LEGOTechnicForceSensor forceSensor;

    ILEGODevice device;

    float currentRollValue = 0;
    float currentPitchValue = 0;
    float currentBoostValue = 0;

    // Update is called once per frame
    void Update()
    {
        planeTransform.Rotate(
          currentPitchValue * pitchFactor * Time.deltaTime,
          0,
          currentRollValue * rollFactor * Time.deltaTime,
          Space.Self);

        planeThrust.boost = currentBoostValue;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shake();
        }
    }

    public void Shake()
    {
        if (rollMotor != null && pitchMotor != null)
            StartCoroutine(DoShake((int)rollMotor.Position.RawValues[0], (int)pitchMotor.Position.RawValues[0]));
    }

    IEnumerator DoShake(int startRoll, int startPitch)
    {
        var rollCmd = new LEGOTachoMotorCommon.SetSpeedPositionCommand()
        {
            Position = startRoll - 5,
            Speed = 100
        };
        rollCmd.SetEndState(MotorWithTachoEndState.Drifting);

        var pitchCmd = new LEGOTachoMotorCommon.SetSpeedPositionCommand()
        {
            Position = startPitch - 5,
            Speed = 100
        };
        pitchCmd.SetEndState(MotorWithTachoEndState.Drifting);

        rollMotor.SendCommand(rollCmd);
        yield return new WaitForSeconds(0.05f);

        pitchMotor.SendCommand(pitchCmd);

        yield return new WaitForSeconds(0.05f);

        rollCmd.Position = startRoll + 5;
        rollMotor.SendCommand(rollCmd);

        yield return new WaitForSeconds(0.05f);

        pitchCmd.Position = startPitch + 5;
        pitchMotor.SendCommand(pitchCmd);

        yield return new WaitForSeconds(0.05f);

        rollCmd.Position = startRoll - 5;
        rollMotor.SendCommand(rollCmd);

        yield return new WaitForSeconds(0.05f);

        pitchCmd.Position = startPitch + 5;
        pitchMotor.SendCommand(pitchCmd);

        yield return new WaitForSeconds(0.05f);

        var driftCmd = new LEGOTachoMotorCommon.DriftCommand();
        rollMotor.SendCommand(driftCmd);
        pitchMotor.SendCommand(driftCmd);

    }

    public void Blink()
    {
        if (rgbLight != null)
            StartCoroutine(DoBlink());
    }

    IEnumerator DoBlink()
    {
        LEGORGBLight.SetColorIndexCommand colorIndexCmd = new LEGORGBLight.SetColorIndexCommand()
        {
            ColorIndex = 9
        };
        LEGORGBLight.SwitchOffCommand switchOffCmd = new LEGORGBLight.SwitchOffCommand();

        for (var i = 0; i < 5; ++i)
        {
            rgbLight.SendCommand(colorIndexCmd);

            yield return new WaitForSeconds(0.1f);

            rgbLight.SendCommand(switchOffCmd);

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SetUpWithDevice(ILEGODevice device)
    {

        this.device = device;
        Debug.LogFormat("Setting up light");
        var lightServices = ServiceHelper.GetServicesOfType(device, IOType.LEIOTypeRGBLight);
        if (lightServices == null || lightServices.Count() == 0)
        {
            Debug.LogFormat("No light services found!");
        }
        else
        {
            rgbLight = (LEGORGBLight)(lightServices.First());
            Debug.LogFormat("Has light service {0}", rgbLight);
            rgbLight.UpdateCurrentInputFormatWithNewMode((int)LEGORGBLight.RGBLightMode.Discrete);

            var cmd = new LEGORGBLight.SwitchOffCommand();
            rgbLight.SendCommand(cmd);
        }

        Debug.LogFormat("Setting roll motor"); // Must be connected to port A.
        var rollMotors = ServiceHelper.GetServicesOfTypeOnPort(device, IOType.LEIOTypeTechnicMotorXL, 0);
        if (rollMotors == null || rollMotors.Count() == 0)
        {
            Debug.LogFormat("No rollMotors found!");
        }
        else
        {
            rollMotor = (LEGOTechnicMotor)rollMotors.First();
            Debug.LogFormat("Has motor service {0}", rollMotor);
            rollMotor.UpdateInputFormat(new LEGOInputFormat(rollMotor.ConnectInfo.PortID, rollMotor.ioType, rollMotor.PositionModeNo, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
            rollMotor.RegisterDelegate(this);

            // var cmd = new LEGOTachoMotorCommon.SetSpeedPositionCommand()
            // {
            //   Position = 0,
            //   Speed = 80
            // };
            // cmd.SetEndState(MotorWithTachoEndState.Drifting);
            // rollMotor.SendCommand(cmd);
        }

        Debug.LogFormat("Setting pitch motor");// Must be connected to port C.
        var pitchMotors = ServiceHelper.GetServicesOfTypeOnPort(device, IOType.LEIOTypeTechnicMotorXL, 2);
        if (pitchMotors == null || pitchMotors.Count() == 0)
        {
            Debug.LogFormat("No pitchMotors found!");
        }
        else
        {
            pitchMotor = (LEGOTechnicMotor)pitchMotors.First();
            Debug.LogFormat("Has motor service {0}", pitchMotor);
            pitchMotor.UpdateInputFormat(new LEGOInputFormat(pitchMotor.ConnectInfo.PortID, pitchMotor.ioType, pitchMotor.PositionModeNo, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
            pitchMotor.RegisterDelegate(this);
            // var cmd = new LEGOTachoMotorCommon.SetSpeedPositionCommand()
            // {
            //   Position = -90,
            //   Speed = 80
            // };
            // cmd.SetEndState(MotorWithTachoEndState.Drifting);
            // pitchMotor.SendCommand(cmd);
        }

        var forceSensorServices = ServiceHelper.GetServicesOfType(device, IOType.LEIOTypeTechnicForceSensor);
        if (forceSensorServices == null || forceSensorServices.Count() == 0)
        {
            Debug.LogFormat("No force sensor services found   !");
        }
        else
        {
            forceSensor = (LEGOTechnicForceSensor)(forceSensorServices.First());
            Debug.LogFormat("Has forceSensor service {0}", forceSensor);
            // Mode 0 - Variable force
            // Mode 1 - Binary pressed/not pressed
            // Mode 2 - not sure what this is...
            int mode = 0;
            forceSensor.UpdateInputFormat(new LEGOInputFormat(forceSensor.ConnectInfo.PortID, forceSensor.ioType, mode, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
            forceSensor.RegisterDelegate(this);
        }
    }

    #region ILEGOGeneralService Delegates

    public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue)
    {
        if (service == pitchMotor)
        {
            currentPitchValue = newValue.RawValues[0];

        }
        else if (service == rollMotor)
        {
            currentRollValue = newValue.RawValues[0];

        }
        else if (service == forceSensor)
        {
            currentBoostValue = newValue.RawValues[0];
        }
    }

    public void DidChangeState(ILEGOService service, ServiceState oldState, ServiceState newState)
    {
        //    Debug.LogFormat("DidChangeState {0} to {1}", service, newState);
    }

    public void DidUpdateInputFormat(ILEGOService service, LEGOInputFormat oldFormat, LEGOInputFormat newFormat)
    {
        //    Debug.LogFormat("DidUpdateInputFormat {0} to {1}", service, newFormat);
    }

    public void DidUpdateInputFormatCombined(ILEGOService service, LEGOInputFormatCombined oldFormat, LEGOInputFormatCombined newFormat)
    {
        //    Debug.LogFormat("DidUpdateInputFormatCombined {0} to {1}", service, newFormat);
    }

    #endregion
}
