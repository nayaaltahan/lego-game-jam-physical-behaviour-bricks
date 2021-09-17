// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEngine;

namespace LEGOMinifig
{

    public class SpecialAnimationBehavior : StateMachineBehaviour
    {
        int playSpecialHash = Animator.StringToHash("Play Special");

        MinifigController minifigController;

        override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            animator.SetBool(playSpecialHash, false);
            if (!minifigController)
            {
                minifigController = animator.GetComponent<MinifigController>();
            }

        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            minifigController.SpecialAnimationFinished();
        }
    }

}