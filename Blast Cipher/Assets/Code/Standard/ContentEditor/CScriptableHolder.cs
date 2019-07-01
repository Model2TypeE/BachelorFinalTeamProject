﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CScriptableHolder : ScriptableObject
{
    private static CScriptableHolder instance;
    public static CScriptableHolder Instance { get => instance ?? (instance = ScriptableObject.CreateInstance<CScriptableHolder>()); }

    public static bool Empty = Instance.Characters.Count == 0 && Instance.Maps.Count == 0;

    public List<CScriptableCharacter> Characters = new List<CScriptableCharacter>();
    public List<CScriptableMap> Maps = new List<CScriptableMap>();

    public void AddCharacter(CScriptableCharacter character)
    {
        Characters.Add(character);
    }

    public void AddMap(CScriptableMap map)
    {
        Maps.Add(map);
    }

}
