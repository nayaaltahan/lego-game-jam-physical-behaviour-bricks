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
    [SerializeField] private PlayerData playerData;

    [SerializeField] private MinifigController minifig;

    private Dictionary<Color, PlayerBehavior> behavior_dict;

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
        MoveBehavior moveBehavior = gameObject.AddComponent<MoveBehavior>();
        moveBehavior.SetMinifigController(minifig);
        moveBehavior.SetDuration(playerData.behaviorDuration);
        moveBehavior.SetDistance(playerData.moveDist);

        RotateBehavior rotateBehavior = gameObject.AddComponent<RotateBehavior>();
        rotateBehavior.SetMinifigController(minifig);
        rotateBehavior.SetDuration(playerData.behaviorDuration);

        JumpBehavior jumpBehavior = gameObject.AddComponent<JumpBehavior>();
        jumpBehavior.SetMinifigController(minifig);
        jumpBehavior.SetDuration(playerData.behaviorDuration);

        behavior_dict = new Dictionary<Color, PlayerBehavior>();
        behavior_dict.Add(moveBrick.color, moveBehavior);
        behavior_dict.Add(rotateBrick.color, rotateBehavior);
        behavior_dict.Add(jumpBrick.color, jumpBehavior);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            //var color = ColorOutput.Instance.GetNextColor();
            StartCoroutine(behavior_dict[Color.red].Execute());
        }

        if (Input.GetButtonDown("Fire2"))
            StartCoroutine(behavior_dict[Color.green].Execute());
    }

    public void Execute()
    {
        while (ColorOutput.Instance.Length() > 0)
        {
            var color = ColorOutput.Instance.GetNextColor();
            StartCoroutine(behavior_dict[color].Execute());
        }
    }

    public void Move()
    {
        //minifig.MoveTo();
    }
}
