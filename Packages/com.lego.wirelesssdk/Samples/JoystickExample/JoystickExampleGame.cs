using UnityEngine;
using LEGODeviceUnitySDK;

public class JoystickExampleGame : MonoBehaviour
{
    [SerializeField] bool connectToBLE;
    public DeviceHandler deviceHandler;
    JoystickController joystickController;

    // Start is called before the first frame update
    void Start()
    {
        joystickController = GetComponent<JoystickController>();
        deviceHandler.OnDeviceInitialized += OnDeviceInitialized;
        if (connectToBLE == true)
            deviceHandler.AutoConnectToDeviceOfType(HubType.Technic);
    }

    public void OnDeviceInitialized(ILEGODevice device)
    {
        Debug.LogFormat("OnDeviceInitialized {0}", device);
        joystickController.SetUpWithDevice(device);
    }
}
