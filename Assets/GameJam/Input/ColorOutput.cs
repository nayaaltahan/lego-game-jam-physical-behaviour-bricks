using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorOutput : MonoBehaviour
{
    private static ColorOutput _instance;

    public static ColorOutput Instance
    {
        get => _instance;
    }

    private Queue<Color> _outputColors;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        _outputColors = new Queue<Color>();
    }

    public Color GetNextColor()
    {
        return _outputColors.Dequeue();
    }

    public void Reset()
    {
        _outputColors.Clear();
    }
}
