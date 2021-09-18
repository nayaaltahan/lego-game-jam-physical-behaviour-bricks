using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using LEGOMinifig;
using UnityEngine;

public class MoveBehavior : PlayerBehavior
{
    private float moveDist;

    private float duration;

    private CharacterBehavior character;

    private void Start()
    {
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

    public override IEnumerator Execute()
    {
        Debug.Log("Move");
        character.ready = false;
        var pos = minifigController.transform.position;
        var newPos = pos + minifigController.transform.forward * moveDist;

        minifigController.SetSensorInput(UnityEngine.Vector3.one);

        yield return new WaitForSeconds(duration);

        character.ready = true;
    }

    public void Abort()
    {
        StopAllCoroutines();
    }
}
