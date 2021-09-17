using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LEGOMinifig
{
    public class MinifigAnimationControllerUpdater
    {
        static readonly string animationPath = "Packages/com.lego.minifig/Animations";
        static readonly string animationPrefix = "Minifig@";
        static readonly string animationControllerFile = "AnimationController.controller";

        [MenuItem("LEGO Tools/Dev/Rebuild Specials of Minifig Animation Controller")]
        static void RebuildSpecials()
        {
            var specialAnimations = Enum.GetValues(typeof(MinifigController.SpecialAnimation));
            var newStates = new AnimatorState[specialAnimations.Length];

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Path.Combine(animationPath, animationControllerFile));

            // Assume that there is one layer and that layer has only one substate.
            var specialStateMachine = controller.layers[0].stateMachine.stateMachines[0].stateMachine;

            // Remove previous entry transitions.
            foreach (var transition in specialStateMachine.entryTransitions)
            {
                specialStateMachine.RemoveEntryTransition(transition);
            }

            // Remove previous states.
            foreach (var state in specialStateMachine.states)
            {
                specialStateMachine.RemoveState(state.state);
            }

            // Add states and exit transitions.
            for (var i = 0; i < specialAnimations.Length; ++i)
            {
                var name = Enum.GetName(typeof(MinifigController.SpecialAnimation), specialAnimations.GetValue(i));
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(animationPath, animationPrefix + name + ".fbx"));

                var state = specialStateMachine.AddState(name, Vector3.right * 500.0f + Vector3.up * 50.0f * (i - (specialAnimations.Length - 1) * 0.5f));
                state.motion = clip;

                var cancelSpecialTransition = state.AddExitTransition();
                cancelSpecialTransition.hasExitTime = false;
                cancelSpecialTransition.interruptionSource = TransitionInterruptionSource.Destination;
                cancelSpecialTransition.AddCondition(AnimatorConditionMode.If, 1, "Cancel Special");

                var notEqualTransition = state.AddExitTransition();
                notEqualTransition.hasExitTime = false;
                notEqualTransition.interruptionSource = TransitionInterruptionSource.Destination;
                notEqualTransition.AddCondition(AnimatorConditionMode.NotEqual, (int)specialAnimations.GetValue(i), "Special Id");

                if (!clip.isLooping)
                {
                    var exitTimeTransition = state.AddExitTransition();
                    exitTimeTransition.hasExitTime = true;
                    exitTimeTransition.interruptionSource = TransitionInterruptionSource.Destination;
                    exitTimeTransition.exitTime = 0.9f;
                    exitTimeTransition.hasFixedDuration = false;
                    exitTimeTransition.duration = 0.1f;
                }

                newStates[i] = state;
            }

            // Add default state and place entry and exit states.
            specialStateMachine.defaultState = newStates[0];
            specialStateMachine.entryPosition = Vector3.zero;
            specialStateMachine.exitPosition = Vector3.right * 1000.0f;

            // Add entry transitions.
            for (var i = 1; i < newStates.Length; ++i)
            {
                var entryTransition = specialStateMachine.AddEntryTransition(newStates[i]);
                entryTransition.AddCondition(AnimatorConditionMode.Equals, (int)specialAnimations.GetValue(i), "Special Id");
            }

            AssetDatabase.SaveAssets();
        }
    }
}