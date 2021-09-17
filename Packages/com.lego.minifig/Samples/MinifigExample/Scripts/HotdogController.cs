// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEngine;

namespace LEGOMinifig.MinifigExample
{

    public class HotdogController : MonoBehaviour
    {
        public float circleSpeed = 0.5f;
        public float circleRadius = 12.0f;

        public float hoverSpeed = 5.0f;
        public float hoverAmplitude = 0.3f;

        public float rotationSpeed = -60.0f;

        Vector3 orgPosition;
        Vector3 rotationAxis = new Vector3(0.1f, 1.0f, 0.1f);

        // Start is called before the first frame update
        void Start()
        {
            orgPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = orgPosition + new Vector3(Mathf.Sin(Time.time * circleSpeed), 0.0f, Mathf.Cos(Time.time * circleSpeed)) * circleRadius + Vector3.up * Mathf.Cos(Time.time * hoverSpeed) * hoverAmplitude;

            transform.RotateAround(transform.position, rotationAxis, rotationSpeed * Time.deltaTime);
        }
    }

}
