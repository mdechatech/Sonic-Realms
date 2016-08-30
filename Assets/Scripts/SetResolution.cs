using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class SetResolution : MonoBehaviour
{
    private static bool Done;

    [SerializeField]
    private int _width;

    [SerializeField]
    private int _height;

    protected void Awake()
    {
        if (Done) return;

        Done = true;
        Screen.SetResolution(_width, _height, false);
    }
}