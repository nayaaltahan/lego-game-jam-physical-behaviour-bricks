// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEditor;
using UnityEngine.Rendering;

namespace LEGOMaterials
{

    public class CheckRenderPipeline
    {
        static readonly string pipelinePath = "Packages/com.lego.materials/Rendering/UniversalRenderPipelineAsset.asset";
        static readonly string pipelineNoPromptPrefsKey = "com.lego.materials.noPromptForMissingRenderPipeline";

        [InitializeOnLoadMethod]
        static void DoCheckRenderPipeline()
        {
            // Do not perform the check when playing.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var existingRenderPipeline = GraphicsSettings.renderPipelineAsset;
            var noPrompt = EditorPrefs.GetBool(pipelineNoPromptPrefsKey, false);

            if (!existingRenderPipeline && !noPrompt)
            {
                EditorApplication.delayCall += PromptToEnableRenderPipeline;
            }
        }

        static void PromptToEnableRenderPipeline()
        {
            // Prompt user.
            var answer = EditorUtility.DisplayDialogComplex("Universal Render Pipeline required by LEGO packages", "Do you want to enable Universal Render Pipeline?", "Yes", "No", "No, Don't Show Again");
            switch (answer)
            {
                // Yes
                case 0:
                    {
                        EnableRenderPipeline();
                        break;
                    }
                // No, Don't Show Again
                case 2:
                    {
                        EditorPrefs.SetBool(pipelineNoPromptPrefsKey, true);
                        break;
                    }
            }
        }

        [MenuItem("LEGO Tools/Enable Universal Render Pipeline", priority = 100)]
        static void EnableRenderPipeline()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(pipelinePath);
            GraphicsSettings.renderPipelineAsset = pipeline;
        }
    }

}
