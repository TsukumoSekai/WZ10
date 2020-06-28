using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeFrame;

public class BaseUnit : UnityObject
{
    /// <summary>最大生命值</summary>
    public float mHealth_Max;

    /// <summary>当前生命值</summary>
    public float mHealth_Cur;

    protected override void Init()
    {
        base.Init();

        mHealth_Cur = mHealth_Max;
    }

    public void ReceiveDamage(float damage)
    {
        mHealth_Cur -= damage;

        if (mHealth_Cur <= 0)
        {
            Destroy(this.gameObject);
            mHealth_Cur = 0;
        }
    }

    protected override void Release()
    {
        base.Release();

        SendMessageUpwards("OnUnitDestroy", this, SendMessageOptions.DontRequireReceiver);
    }
}
