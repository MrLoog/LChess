using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputUtils
{
    private static KeyCode[] numberKeyCodes = new KeyCode[] { KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9 };

    public static bool GameRayCastEnable { get; set; } = true;

    public static Ray GetTouchRayMouse()
    {
        if (!GameRayCastEnable)
        {
            return new Ray();
        }
        else
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }

    internal static int GetNumberKeyPress()
    {
        for (int i = 0; i < numberKeyCodes.Length; ++i)
        {
            if (Input.GetKeyDown(numberKeyCodes[i]))
            {
                return i;
            }
        }
        return -1;
    }

    internal static bool HotkeyBoardGrid()
    {
        return Input.GetKeyDown(KeyCode.G);
    }

    internal static bool HotkeyAllBattle()
    {
        return Input.GetKeyDown(KeyCode.B);
    }

    internal static bool HotkeySpawnGroupEqual()
    {
        return Input.GetKeyDown(KeyCode.A);
    }

    internal static bool HotkeyRandomMode()
    {
        return Input.GetKeyDown(KeyCode.R);
    }

    internal static bool Mouse1Press()
    {
        return Input.GetMouseButtonDown(0);
    }

    internal static bool Mouse1Free()
    {
        return Input.GetMouseButtonUp(0);
    }

    internal static bool LeftMousePress()
    {
        return Input.GetMouseButtonUp(1);
    }

    internal static bool LeftShiftPress()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    internal static bool LeftControlPress()
    {
        return Input.GetKey(KeyCode.LeftControl);
    }

}
