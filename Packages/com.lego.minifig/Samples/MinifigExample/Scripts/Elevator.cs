// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System;
using UnityEngine;

namespace LEGOMinifig.MinifigExample
{

    public class Elevator : MonoBehaviour
    {
        [SerializeField, Range(-100f, 100f)]
        float m_Height = 0.96f * 8f;

        [SerializeField, Range(0.1f, 10f)]
        float m_TravelTime = 2f;

        [SerializeField, Range(0f, 10f)]
        float m_Pause = 1f;

        enum State
        {
            waitingToGoUp,
            goingUp,
            waitingToGoDown,
            goingDown
        };

        State m_State;
        float m_NextMovementStartTime;
        float m_Time;
        Vector3 m_OldOffset;

        void Awake()
        {
            m_Time = UnityEngine.Random.Range(0f, m_Pause);
        }

        void Update()
        {
            m_Time += Time.deltaTime;

            switch (m_State)
            {
                case State.waitingToGoUp:
                    if (m_Time > m_Pause)
                    {
                        m_Time = m_Time - m_Pause + m_NextMovementStartTime;
                        m_State = State.goingUp;
                    }
                    break;

                case State.goingUp:
                    var newOffset = Vector3.up * Mathf.Clamp(m_Height * m_Time / m_TravelTime, Mathf.Min(-m_Height, m_Height), Mathf.Max(-m_Height, m_Height));

                    transform.position += newOffset - m_OldOffset;

                    m_OldOffset = newOffset;

                    if (m_Time > m_TravelTime)
                    {
                        m_Time -= m_TravelTime;
                        m_NextMovementStartTime = m_Time;
                        m_State = State.waitingToGoDown;
                    }
                    break;

                case State.waitingToGoDown:
                    if (m_Time > m_Pause)
                    {
                        m_Time = m_Time - m_Pause + m_NextMovementStartTime;
                        m_State = State.goingDown;
                    }
                    break;

                case State.goingDown:
                    newOffset = Vector3.up * (m_Height - m_Height * m_Time / m_TravelTime);
                    if (Math.Sign(newOffset.y) != Math.Sign(m_Height))
                    {
                        newOffset = Vector3.zero;
                    }

                    transform.position += newOffset - m_OldOffset;

                    m_OldOffset = newOffset;

                    if (m_Time > m_TravelTime)
                    {
                        m_Time -= m_TravelTime;
                        m_NextMovementStartTime = m_Time;
                        m_State = State.waitingToGoUp;
                    }
                    break;
            }
        }
    }
}