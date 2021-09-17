// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System;
using System.Collections.Generic;
using UnityEngine;

namespace LEGOMinifig
{
    public class MinifigFaceAnimationController : MonoBehaviour
    {
        public enum FaceAnimation
        {
            Accept,
            Blink,
            BlinkTwice,
            Complain,
            Cool,
            Dissatisfied,
            Doubtful,
            Error,
            Frustrated,
            Impressed,
            Laugh,
            Mad,
            Sleepy,
            Smile,
            Surprised,
            Wink
        }

        [Serializable]
        class AnimationData
        {
            public Texture2D[] textures;
            public float framesPerSecond = 24f;
            public float pauseFrame = 0.0f;
            public float pauseDuration = 0.0f;
        }

        [SerializeField]
        Transform face;
        [SerializeField]
        Texture2D defaultTexture;
        [SerializeField]
        List<FaceAnimation> animations = new List<FaceAnimation>();
        [SerializeField]
        List<AnimationData> animationData = new List<AnimationData>();

        Material faceMaterial;

        enum State
        {
            Stopped,
            Playing,
            Paused
        }
        State currentState;

        AnimationData currentAnimationData;
        float currentFrame;
        int showingFrame;
        float framesPerSecond;
        float pauseTime;

        int shaderTextureId;

        public void Init(Transform face, Texture2D defaultTexture)
        {
            this.face = face;
            this.defaultTexture = defaultTexture;
        }

        /// <param name="pingPong">If the animation should be reverse sampled from last frame.</param>
        /// <param name="pauseFrame">Normalized frames 0-1, where 0 is start and 1 is the end of the animation.</param>
        /// <param name="pauseDuration">Duration the pause should be.</param>
        public void AddAnimation(FaceAnimation animation, Texture2D[] textures, float framesPerSecond = 24f, bool pingPong = false, float pauseFrame = 0.0f, float pauseDuration = 0.0f)
        {
            if (!animations.Contains(animation))
            {
                if (framesPerSecond <= 0.0f)
                {
                    Debug.LogError("Frames per second must be positive");
                    return;
                }

                animations.Add(animation);

                if (pingPong)
                {
                    var result = new Texture2D[textures.Length * 2 - 1];
                    for (var i = 0; i < result.Length; ++i)
                    {
                        result[i] = textures[(int)Mathf.PingPong(i, textures.Length - 1)];
                    }

                    textures = result;
                }

                var animationData = new AnimationData
                {
                    textures = textures,
                    framesPerSecond = framesPerSecond,
                    pauseFrame = pauseFrame,
                    pauseDuration = pauseDuration
                };
                this.animationData.Add(animationData);
            }
            else
            {
                Debug.LogErrorFormat("Face animation controller already contains animation {0}", animation);
            }
        }

        public bool HasAnimation(FaceAnimation animation)
        {
            return animations.IndexOf(animation) >= 0;
        }

        public void PlayAnimation(FaceAnimation animation)
        {
            var animationIndex = animations.IndexOf(animation);
            if (animationIndex < 0)
            {
                Debug.LogErrorFormat("Face animation controller does not contain animation {0}", animation);
                return;
            }

            currentAnimationData = animationData[animationIndex];
            currentFrame = 0.0f;
            showingFrame = -1;
            pauseTime = 0.0f;
            framesPerSecond = currentAnimationData.framesPerSecond;

            currentState = State.Playing;
        }

        void Start()
        {
            faceMaterial = face.GetComponent<Renderer>().material;
            shaderTextureId = Shader.PropertyToID("_BaseMap");
        }

        void Update()
        {
            if (currentState != State.Stopped)
            {
                if (currentState == State.Paused)
                {
                    pauseTime += Time.deltaTime;

                    if (pauseTime >= currentAnimationData.pauseDuration)
                    {
                        currentFrame += 1.0f;
                        currentState = State.Playing;
                    }
                    else
                    {
                        return;
                    }
                }

                currentFrame += Time.deltaTime * framesPerSecond;

                var wholeFrame = Mathf.FloorToInt(currentFrame);
                if (wholeFrame != showingFrame)
                {
                    if (wholeFrame >= currentAnimationData.textures.Length)
                    {
                        faceMaterial.SetTexture(shaderTextureId, defaultTexture);
                        currentState = State.Stopped;
                    }
                    else
                    {
                        faceMaterial.SetTexture(shaderTextureId, currentAnimationData.textures[wholeFrame]);
                        showingFrame = wholeFrame;
                    }

                    if (currentAnimationData.pauseDuration > 0.0f)
                    {
                        if (wholeFrame == Mathf.CeilToInt(currentAnimationData.textures.Length * currentAnimationData.pauseFrame))
                        {
                            currentState = State.Paused;
                        }
                    }
                }
            }
        }
    }
}