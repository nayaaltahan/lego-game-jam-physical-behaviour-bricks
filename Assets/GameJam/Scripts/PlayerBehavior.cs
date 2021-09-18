using System.Collections;
using System.Collections.Generic;
using LEGOMinifig;
using UnityEngine;

public abstract class PlayerBehavior : MonoBehaviour
{

    public MinifigController minifigController;

    public abstract IEnumerator Execute();

}
