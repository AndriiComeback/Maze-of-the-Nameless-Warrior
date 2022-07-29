using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit {
    [field: SerializeField] public Sprite Sprite { set; get; }
    [field: SerializeField] public string BattleDescription { set; get; }
    [field: SerializeField] public EnemyType Type { set; get; }
}

public enum EnemyType { Goblin, Snake };