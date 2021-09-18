using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LEGOMinifig;

public class PlayerDetstroy : MonoBehaviour
{
    public MinifigController minifig;

    private void OnTriggerEnter(Collider Player) {
        if(CompareTag("Player"))
        {
            minifig.Explode();
        }
    }
}
