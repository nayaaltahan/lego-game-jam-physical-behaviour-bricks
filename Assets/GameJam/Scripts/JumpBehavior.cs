using System.Collections;
using System.Collections.Generic;
using LEGOMinifig;
using UnityEngine;

public class JumpBehavior : PlayerBehavior
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
        StartCoroutine(minifigController.TriggerJumpInput(true));
        yield return new WaitForSeconds(duration);
    }
}
