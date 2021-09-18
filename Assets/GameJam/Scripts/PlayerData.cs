using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public float moveDist;
    public float behaviorDuration;
}
