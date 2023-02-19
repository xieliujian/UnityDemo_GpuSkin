using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUSkinMgr
{
    static GPUSkinMgr s_Instance;

    public static GPUSkinMgr instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = new GPUSkinMgr();
            }

            return s_Instance;
        }
    }

    public void OnUpdate()
    {

    }
}
