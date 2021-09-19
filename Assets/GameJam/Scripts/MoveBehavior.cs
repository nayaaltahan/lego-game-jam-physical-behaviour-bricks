using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using LEGOMinifig;
using Unity.LEGO.Behaviours;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Vector3 = UnityEngine.Vector3;

public class MoveBehavior : PlayerBehavior
{
    private float moveDist;

    private float duration;
    private Vector3 initialPos;
    private Vector3 finalPos;
    private float threshold = 0.2f;

    private bool moving = false;

    private CharacterBehavior character;

    private void Start()
    {
        moving = false;
        minifigController.isControlledBySensors = true;
    }

    public void SetMinifigController(MinifigController controller)
    {
        this.minifigController = controller;
    }

    public void SetCharacter(CharacterBehavior characterBehavior)
    {
        character = characterBehavior;
    }

    public void SetDistance(float dist)
    {
        this.moveDist = dist;
    }

    public void SetDuration(float duration)
    {
        this.duration = duration;
    }

    private void Update()
    {
        var distToFinalPos = (finalPos - minifigController.transform.position).magnitude;
        if (distToFinalPos < threshold && moving)
        {
            minifigController.SetSensorInput(Vector3.zero);
            moving = false;
        }



    }
    public override IEnumerator Execute()
    {
        character.ready = false;
        moving = true;
        Debug.Log("Executed");
        finalPos = minifigController.transform.forward * moveDist;
        minifigController.SetSensorInput(finalPos);

        yield return new WaitForSeconds(duration);
        character.ready = true;
    }
}
