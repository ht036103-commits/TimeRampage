using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;

public sealed class AnimationEventRelay : MonoBehaviour
{
    public event Action AttackHit;

    // Duoc goi boi Animation Event tren clip tan cong.
    public void OnAttackHit()
    {
        AttackHit?.Invoke();
    }
}

