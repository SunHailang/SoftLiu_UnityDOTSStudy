using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TestString : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ZString z = new ZString("Hello World!");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public unsafe class ZString
{
    private IntPtr strPtr = IntPtr.Zero;

    public void SetZString(char* str)
    {
        strPtr = new IntPtr(str);

        

        //SetZString("Hello World;");

    }

    public ZString(string str)
    {
        strPtr = Marshal.StringToCoTaskMemAuto(str);
    }

    public string GetString()
    {
        if (strPtr == IntPtr.Zero) return "";
        return Marshal.PtrToStringAnsi(strPtr);
    }
}
