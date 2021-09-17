using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LEGODeviceUnitySDK;

public abstract class AbstractServiceUI : MonoBehaviour, ILEGOServiceDelegate
{

  public Text serviceNameValue;
  public Text portValue;
  public Text stateValue;

  protected ILEGOService service;

  public virtual void SetupWithService(ILEGOService service)
  {
    this.service = service;

    serviceNameValue.text = service.ServiceName;
    stateValue.text = service.State.ToString();
    portValue.text = service.ConnectInfo.PortID.ToString();

    service.RegisterDelegate(this);
  }

  void OnDestroy()
  {
    service.UnregisterDelegate(this);
  }
}