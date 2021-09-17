// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    GameObject playerObjectToFollow;
    
    Vector3 offset;
    Vector3 initialPosition;

    enum ZoomState
    {
        NotZooming,
        ZoomingIn,
        ZoomingOut
    }
    ZoomState zoomState = ZoomState.NotZooming;

    float zoomScalar = 0.0f;

    const float maxZoom = -7.5f;
    const float minZoom = 10.0f;
    const float zoomSpeed = 8.0f;

    public void TargetCameraOnPlayer(GameObject playerObject)
    {
        // Register player object.
        playerObjectToFollow = playerObject;

        // Reset camera position.
        transform.position = initialPosition;

        // Store initial offset.
        offset = playerObjectToFollow.transform.position - transform.position;

        // Adjust for current zoom level.
        PositionCamera();
    }

    public void ZoomIn(bool isOn)
    {
        zoomState = isOn ? ZoomState.ZoomingIn : ZoomState.NotZooming;
    }

    public void ZoomOut(bool isOn)
    {
        zoomState = isOn ? ZoomState.ZoomingOut : ZoomState.NotZooming;
    }

    void Start()
    {
        initialPosition = transform.position;

        TargetCameraOnPlayer(GameObject.FindGameObjectWithTag("Player"));
    }

    void Update()
    {
        PositionCamera();

        switch (zoomState)
        {
            case ZoomState.ZoomingIn:
                if (zoomScalar >= maxZoom)
                {
                    zoomScalar -= zoomSpeed * Time.deltaTime;
                }
                break;
            case ZoomState.ZoomingOut:
                if (zoomScalar <= minZoom)
                {
                    zoomScalar += zoomSpeed * Time.deltaTime;
                }
                break;
        }
    }

    void PositionCamera()
    {
        if (playerObjectToFollow)
        {
            var currentTargetPosition = playerObjectToFollow.transform.position;
            currentTargetPosition.x -= offset.x;
            currentTargetPosition.y = transform.position.y;
            currentTargetPosition.z -= offset.z + zoomScalar;

            transform.position = currentTargetPosition;
        }
    }
}
