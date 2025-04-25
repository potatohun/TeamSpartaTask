using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoxController : Entity
{
    protected override void Die()
    {
        BoxManager.instance.OnBoxBroken(this);
        Destroy(this.gameObject);
    }
}
