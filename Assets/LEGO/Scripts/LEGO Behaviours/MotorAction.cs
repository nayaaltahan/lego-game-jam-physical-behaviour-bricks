using LEGODeviceUnitySDK;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.LEGO.Behaviours.Actions
{
    public class MotorAction : RepeatableAction
    {
        [SerializeField, Tooltip("The speed of the motor.")]
        int m_Speed = 100;

        enum Direction
        {
            Clockwise,
            Anticlockwise
        }

        [SerializeField, Tooltip("The direction to run the motor")]
        Direction m_Direction;

        [SerializeField, Tooltip("The time in seconds to run the motor.")]
        float m_Time = 2.0f;

        enum Port
        {
            A,
            B,
            C,
            D,
        }

        [SerializeField, Tooltip("The port to use.")]
        Port m_Port;

        enum State
        {
            Running,
            WaitingToRun
        }

        State m_State;

        DeviceHandler m_DeviceHandler;
        LEGOTechnicMotor m_Motor;
        LEGOTachoMotorCommon.SetSpeedCommand m_SetSpeedCommand = new LEGOTachoMotorCommon.SetSpeedCommand();
        LEGOTachoMotorCommon.BrakeCommand m_BrakeCommand = new LEGOTachoMotorCommon.BrakeCommand();

        float m_CurrentTime;

        protected override void Reset()
        {
            base.Reset();

            m_IconPath = "Assets/LEGO/Gizmos/LEGO Behaviour Icons/Motor Action.png";
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            m_Speed = Mathf.Clamp(m_Speed, 10, 200);
            m_Time = Mathf.Max(0.1f, m_Time);
        }

        protected override void Start()
        {
            base.Start();

            m_DeviceHandler = GameObject.FindObjectOfType<DeviceHandler>();
            if (m_DeviceHandler)
            {
                m_DeviceHandler.OnDeviceInitialized += SetupWithDevice;
                if (!m_DeviceHandler.isScanning)
                {
                    m_DeviceHandler.AutoConnectToDeviceOfType(HubType.Technic);
                }
            }
        }

        void SetupWithDevice(ILEGODevice device)
        {
            device.OnServiceConnectionChanged += ServiceConnectionChanged;

            var motorServices = ServiceHelper.GetServicesInTypeCollection(device, new List<IOType> { IOType.LEIOTypeTechnicMotorL, IOType.LEIOTypeTechnicMotorXL } );
            if (motorServices == null || motorServices.Count() == 0)
            {
                Debug.LogFormat("No motor services found.");
            }
            else
            {
                foreach (var motorService in motorServices)
                {
                    if (motorService.ConnectInfo.PortID == (int)m_Port && !motorService.ConnectInfo.VirtualConnection)
                    {
                        m_Motor = (LEGOTechnicMotor)motorService;

                        break;
                    }
                }

                if (m_Motor == null)
                {
                    Debug.LogFormat("No motor service found on port " + m_Port + ".");
                }
            }

            m_DeviceHandler.OnDeviceInitialized -= SetupWithDevice;
        }

        void ServiceConnectionChanged(ILEGODevice device, ILEGOService service, bool b)
        {
            device.OnServiceConnectionChanged -= ServiceConnectionChanged;

            SetupWithDevice(device);
        }

        void Update()
        {
            if (m_Active)
            {
                // Update time.
                m_CurrentTime += Time.deltaTime;

                // Run.
                if (m_State == State.Running)
                {
                    // FIXME Only send command once?

                    if (m_Motor != null)
                    {
                        StopAllCoroutines();
                        m_SetSpeedCommand.Speed = m_Speed * (m_Direction == Direction.Clockwise ? 1 : -1);
                        m_Motor.SendCommand(m_SetSpeedCommand);
                    }

                    // Check if we are done running.
                    if (m_CurrentTime >= m_Time)
                    {
                        m_CurrentTime -= m_Time;
                        m_State = State.WaitingToRun;
                    }
                }

                // Waiting to run.
                if (m_State == State.WaitingToRun)
                {
                    // FIXME Only send command once?

                    if (m_Motor != null)
                    {
                        StartCoroutine(BrakeMotor());
                    }

                    if (m_CurrentTime >= m_Pause)
                    {
                        m_CurrentTime -= m_Pause;
                        m_State = State.Running;
                        m_Active = m_Repeat;
                    }
                }
            }
        }

        IEnumerator BrakeMotor()
        {
            // Wait one frame before braking to avoid stuttering for short pauses.
            yield return null;

            m_Motor.SendCommand(m_BrakeCommand);
        }
    }
}
