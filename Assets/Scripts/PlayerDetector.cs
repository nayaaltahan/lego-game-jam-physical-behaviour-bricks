using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LEGOMinifig;

public class PlayerDetector : MonoBehaviour
{
    public MinifigController minifig;

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player"))
        {
            minifig.Explode();
        }
    }
}
