using System.Collections;
using System.Collections.Generic;
using LEGOMinifig;
using UnityEngine;

public class RotateBehavior : PlayerBehavior
{
    private float duration;
    public void SetMinifigController(MinifigController controller)
    {
        this.minifigController = controller;
    }

    public void SetDuration(float duration)
    {
        this.duration = duration;
    }

    public override IEnumerator Execute()
    {
        var pos = minifigController.transform.position - minifigController.transform.forward * 4;
        minifigController.TurnTo(pos);
        yield return new WaitForSeconds(duration);
    }
}
