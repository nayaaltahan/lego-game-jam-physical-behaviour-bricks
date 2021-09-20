using System;
using System.Collections;
using System.Collections.Generic;
using LEGOMinifig;
using Unity.LEGO.Behaviours;
using UnityEngine;

public class JumpBehavior : PlayerBehavior
{
    private float duration;
    private CharacterBehavior character;

    private Vector3 finalPos;
    private bool jumping;
    private float threshold = 0.2f;

    private void Awake()
    {
        jumping = false;
    }

    private float moveDist;
    public void SetMinifigController(MinifigController controller)
    {
        this.minifigController = controller;
    }

    public void SetCharacter(CharacterBehavior characterBehavior)
    {
        character = characterBehavior;
    }

    public void SetDuration(float duration)
    {
        this.duration = duration;
    }

    public void SetDistance(float dist)
    {
        this.moveDist = dist;
    }

    private void Update()
    {
        var distToFinalPos = (finalPos - minifigController.transform.position).magnitude;
        if (distToFinalPos < threshold && jumping)
        {
            jumping = false;
            minifigController.SetSensorInput(Vector3.zero);
        }



    }
    public override IEnumerator Execute()
    {
        character.ready = false;
        minifigController.jumpIn = true;
        jumping = true;
        yield return new WaitForSeconds(0.1f);

        finalPos = minifigController.transform.forward * moveDist;
        minifigController.SetSensorInput(finalPos);

        minifigController.jumpIn = false;
        yield return new WaitForSeconds(duration);
        jumping = false;
        character.ready = true;
    }
}
