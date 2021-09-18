using System.Collections;
using System.Collections.Generic;
using LEGOMinifig;
using LEGOModelImporter;
using UnityEngine;


public class CharacterBehavior : MonoBehaviour
{
    [SerializeField] private BrickData moveBrick;
    [SerializeField] private BrickData jumpBrick;
    [SerializeField] private BrickData rotateBrick;
    [SerializeField] private BrickData interactBrick;

    [SerializeField] private MinifigController minifig;

    enum CharacterState
    {
        Ready,
        Move,
        Jump,
        Rotate,
        Interact

    }

    void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Execute()
    {
        while (ColorOutput.Instance.Length() > 0)
        {
            Color color = ColorOutput.Instance.GetNextColor();
            if (color == moveBrick.color)
            {

            }
            else if (color == jumpBrick.color)
            {

            }
            else if (color == rotateBrick.color)
            {

            }
            else if (color == interactBrick.color)
            {

            }
        }
    }

    public void Move()
    {
        minifig.MoveTo();
    }
}
