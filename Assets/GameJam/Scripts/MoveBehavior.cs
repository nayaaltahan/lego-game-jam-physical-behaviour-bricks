using System.Collections;
using System.Collections.Generic;
using LEGOMinifig;
using UnityEngine;

public class MoveBehavior : PlayerBehavior
{
    private float moveDist;

    private float duration;

    public void SetMinifigController(MinifigController controller)
    {
        this.minifigController = controller;
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
        var pos = minifigController.transform.position;
        var newPos = pos + minifigController.transform.forward * moveDist;
        minifigController.MoveTo(newPos);
        yield return new WaitForSeconds(duration);


    }

    public void Abort()
    {
        StopAllCoroutines();
    }
}
