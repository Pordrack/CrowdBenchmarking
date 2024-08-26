using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeLockMouse : MonoBehaviour
{
    public void OnFreeLockMouse()
    {
        if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
