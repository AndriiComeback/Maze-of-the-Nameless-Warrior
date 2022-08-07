using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityType { Burst, Provoke, Blind, Protect, Stun, Meditate }

[CreateAssetMenu(menuName = "MazeAdventure/Ability")]
public class Ability : ScriptableObject
{
    [field: SerializeField] public string Title { set; get; }
    [field: SerializeField] public string Description { set;  get; }
    [field: SerializeField] public AbilityType Type { set; get; }
    [field: SerializeField] public int Cost { set; get; }
}
