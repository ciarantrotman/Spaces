﻿using System.Collections;
using System.Collections.Generic;
using Spaces.Scripts.Space;
using UnityEngine;

public static class Reference
{
    public const string PlayerTag = "Player", SpaceManagerTag = "SpaceManager";
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static GameObject Player()
    {
        return GameObject.FindWithTag(PlayerTag);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static SpaceManager SpaceManager()
    {
        return GameObject.FindWithTag(SpaceManagerTag).GetComponent<SpaceManager>();
    }
}
