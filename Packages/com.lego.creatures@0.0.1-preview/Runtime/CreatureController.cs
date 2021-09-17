using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LEGO.Creatures
{
    [RequireComponent(typeof(AudioSource), typeof(Animator))]
    public class CreatureController : MonoBehaviour
    {
        [SerializeField]
        bool addBoundsCollider = false;

        [SerializeField] 
        float boundsColliderSizeModifier = 0.8f;

        [Serializable]
        public class Clip
        {
            public AnimationClip AnimationClip = default;
            public AudioClip AudioClip = default;
            public float AudioDelay = 0.0f;
        }

        [SerializeField]
        List<Clip> clips = new List<Clip>();

        Clip currentClip;

        enum AudioState
        {
            NotPlaying,
            WaitingToStart,
            IsPlaying
        }

        AudioState audioState = AudioState.NotPlaying;

        AudioSource audioSourceComponent;
        Animator animatorComponent;

        Bounds objectBounds;

        float audioTime;

        void Awake()
        {
            animatorComponent = GetComponent<Animator>();
            audioSourceComponent = GetComponent<AudioSource>();
            audioSourceComponent.playOnAwake = false;
        }

        void Start()
        {
            Play("idle");
            SetAudioVolume(0.0f);

            objectBounds = GetRendererBounds();

            if (addBoundsCollider)
            {
                var boxCollider = gameObject.AddComponent<BoxCollider>();

                boxCollider.center = objectBounds.center - transform.position;
                boxCollider.size = objectBounds.size * boundsColliderSizeModifier;
            }
        }

        void Update()
        {
            if (audioState == AudioState.NotPlaying)
                return;

            audioTime += Time.deltaTime;

            if (audioState == AudioState.WaitingToStart)
            {
                if (audioTime >= currentClip.AudioDelay)
                {
                    audioSourceComponent.Play();

                    audioTime = 0.0f;
                    audioState = AudioState.IsPlaying;
                }
            }

            if (audioState == AudioState.IsPlaying)
            {
                if (audioTime >= currentClip.AnimationClip.length)
                {
                    audioState = currentClip.AnimationClip.isLooping ? AudioState.WaitingToStart : AudioState.NotPlaying;
                }
            }
        }

        public void Play(string animationName)
        {
            currentClip = clips.Find(clip => clip.AnimationClip.name == animationName);

            if (currentClip != null)
            {
                animatorComponent.SetTrigger(animationName);
                audioSourceComponent.clip = currentClip.AudioClip;

                audioState = AudioState.WaitingToStart;
                audioTime = 0.0f;
            }
        }

        public void SetAudioVolume(float volume)
        {
            audioSourceComponent.volume = volume;
        }

        // FIXME: Temporarily disabled - Make this work for more precise bounds.
        //public Bounds GetBoundsFromVertices()
        //{
        //    var result = new Bounds();
        //    var vertices = new List<Vector3>();

        //    var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        //    foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        //    {
        //        var mesh = skinnedMeshRenderer.sharedMesh;

        //        foreach (var vertex in mesh.vertices)
        //        {
        //            vertices.Add(vertex);
        //        }
        //    }

        //    var meshFilters = GetComponentsInChildren<MeshFilter>();
        //    foreach (var meshFilter in meshFilters)
        //    {
        //        vertices.AddRange(meshFilter.mesh.vertices);
        //    }

        //    if (vertices.Count > 0)
        //    {
        //        var minimum = transform.TransformPoint(vertices[0]);
        //        var maximum = minimum;

        //        foreach (var vertex in vertices)
        //        {
        //            var worldVertex = transform.TransformPoint(vertex);

        //            for (var i = 0; i < 3; i++)
        //            {
        //                maximum[i] = Mathf.Max(worldVertex[i], maximum[i]);
        //                minimum[i] = Mathf.Min(worldVertex[i], minimum[i]);
        //            }
        //        }

        //        result.SetMinMax(minimum, maximum);
        //    }

        //    return result;
        //}

        public Bounds GetRendererBounds()
        {
            var bounds = new Bounds();
            var firstPartBounds = true;

            foreach (var meshRenderer in GetComponentsInChildren<Renderer>(true))
            {
                if (firstPartBounds)
                {
                    bounds = meshRenderer.bounds;
                    firstPartBounds = false;
                }
                else
                {
                    bounds.Encapsulate(meshRenderer.bounds);
                }
            }

            return bounds;
        }

        public void UpdateAnimationClipReferences()
        {
            if (GotAnimationController())
            {
                var animationClips = animatorComponent.runtimeAnimatorController.animationClips.ToList();

                foreach (var clip in animationClips.Where(clip => !clips.Exists(animationClip => animationClip.AnimationClip == clip)))
                {
                    clips.Add(new Clip { AnimationClip = clip });
                }

                clips = clips.Where(clip => animationClips.Contains(clip.AnimationClip)).ToList();
            }
            else
            {
                clips.Clear();
            }
        }

        public bool GotAnimationController()
        {
            if (!animatorComponent)
            {
                animatorComponent = GetComponent<Animator>();
            }

            return animatorComponent.runtimeAnimatorController;
        }

        public List<string> GetCreatureAnimationTriggers() => animatorComponent.runtimeAnimatorController.animationClips.Select(clip => clip.name).ToList();

        public Bounds GetObjectBounds() => objectBounds;
    }
}
