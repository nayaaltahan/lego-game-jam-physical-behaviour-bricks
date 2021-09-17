// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System;
using System.Collections.Generic;
using UnityEngine;

namespace LEGOMinifig.MinifigExample
{
    public class SurfaceMaterialChecker : MonoBehaviour
    {
        [Serializable]
        class SurfaceMaterialSounds
        {
            public string materialName = "";

            public List<AudioClip> stepAudioClips = default;
            public List<AudioClip> landAudioClips = default;
            public AudioClip jumpSound = default;
        }

        [SerializeField]
        List<SurfaceMaterialSounds> surfaceMaterialSounds = new List<SurfaceMaterialSounds>();

        MinifigController minifigController;

        string currentMaterialName;

        void Awake()
        {
            minifigController = GetComponent<MinifigController>();
        }

        void Update()
        {
            Ray ray = new Ray(transform.position + Vector3.up * 2.0f, Vector3.down);
            RaycastHit hit;

            // Cast ray from minifig center to ground.
            if (Physics.Raycast(ray, out hit, 4.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                var surfaceMaterial = hit.transform.GetComponent<SurfaceMaterial>();

                if (surfaceMaterial)
                {
                    if (currentMaterialName != surfaceMaterial.materialName)
                    {
                        var surfaceMaterialMatch = surfaceMaterialSounds.Find(item => item.materialName == surfaceMaterial.materialName);

                        if (surfaceMaterialMatch != null)
                        {
                            currentMaterialName = surfaceMaterialMatch.materialName;

                            minifigController.stepAudioClips = surfaceMaterialMatch.stepAudioClips;
                            minifigController.jumpAudioClip = surfaceMaterialMatch.jumpSound;
                            minifigController.landAudioClips = surfaceMaterialMatch.landAudioClips;
                        }
                    }
                }
            }
        }
    }
}
