// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEngine;

namespace LEGOMinifig.MinifigExample
{
    public class Platform : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)]
        float m_Distance = 0.8f * 10f;

        [SerializeField, Range(0.1f, 10f)]
        float m_TravelTime = 2f;

        [SerializeField, Range(0.1f, 10f)]
        float m_Pause = 1f;

        enum State
        {
            waitingToMoveOut,
            movingOut,
            waitingToMoveBack,
            movingBack
        };

        State m_State;
        float m_NextMovementStartTime;
        float m_Time;
        Vector3 m_Direction;
        Vector3 m_OldOffset;

        void Awake()
        {
            m_Time = Random.Range(0f, m_Pause);
            m_Direction = transform.right;
        }

        void Update()
        {
            m_Time += Time.deltaTime;

            switch (m_State)
            {
                case State.waitingToMoveOut:
                    if (m_Time > m_Pause)
                    {
                        m_Time = m_Time - m_Pause + m_NextMovementStartTime;
                        m_State = State.movingOut;
                    }
                    break;

                case State.movingOut:
                    var newOffset = m_Direction * Mathf.Min(m_Distance * m_Time / m_TravelTime, m_Distance);

                    transform.position += newOffset - m_OldOffset;
                    
                    m_OldOffset = newOffset;

                    if (m_Time > m_TravelTime)
                    {
                        m_Time -= m_TravelTime;
                        m_NextMovementStartTime = m_Time;
                        m_State = State.waitingToMoveBack;
                    }
                    break;

                case State.waitingToMoveBack:
                    if (m_Time > m_Pause)
                    {
                        m_Time = m_Time - m_Pause + m_NextMovementStartTime;
                        m_State = State.movingBack;
                    }
                    break;

                case State.movingBack:
                    newOffset = m_Direction * Mathf.Max(m_Distance - m_Distance * m_Time / m_TravelTime, 0);

                    transform.position += newOffset - m_OldOffset;
                    
                    m_OldOffset = newOffset;

                    if (m_Time > m_TravelTime)
                    {
                        m_Time -= m_TravelTime;
                        m_NextMovementStartTime = m_Time;
                        m_State = State.waitingToMoveOut;
                    }
                    break;
            }
        }
    }
}
