// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Header("Sounds")]
    [Tooltip("Sounds used for connecting bricks")]
    [SerializeField] AudioClip[] _clickSounds = null;
    [Tooltip("Sounds used for falling and colliding bricks")]
    [SerializeField] AudioClip[] _fallingSounds = null;
    [Tooltip("Sounds used for duck quacks")]
    [SerializeField] AudioClip[] _quacks = null;
    AudioSource _audioSource;

    private void Awake()
    {
        if(Instance)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void PlaySound(AudioClip[] clips)
    {
        _audioSource.pitch = Random.Range(0.9f, 1.1f);
        _audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    public void PlayClick()
    {
        PlaySound(_clickSounds);
    }

    public void PlayFall()
    {
        PlaySound(_fallingSounds);
    }

    public void PlayQuack()
    {
        PlaySound(_quacks);
    }
}
