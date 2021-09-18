using System.Collections;
using System.Collections.Generic;
using LEGOMinifig;
using UnityEngine;

public class JumpBehavior : PlayerBehavior
{
    private float duration;
    private CharacterBehavior character;
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
    public override IEnumerator Execute()
    {
        character.ready = false;
        minifigController.jumpIn = true;
        yield return new WaitForSeconds(0.1f);
        minifigController.jumpIn = false;
        yield return new WaitForSeconds(duration);
        character.ready = true;
    }
}
