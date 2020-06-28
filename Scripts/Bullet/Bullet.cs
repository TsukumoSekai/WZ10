using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeFrame;

public enum BulletType
{
    PlayerBullet_0,
    PlayerBullet_1,
    PlayerBullet_2,
    EnemyBullet
}

public class Bullet : UnityObject
{
    public BulletType mBulletType;
    public BaseUnit mTarget;
    public float mDamage;

    protected override void Init()
    {
        base.Init();

        SendMessageUpwards("OnUnitAttack", mBulletType);
    }

    protected void OnTriggerEnter(Collider other)
    {
        SendMessageUpwards("OnBulletHit", other.ClosestPoint(this.transform.position));

        if (other.gameObject.layer == 8 || other.gameObject.layer == 12)
        {
            mTarget = other.GetComponent<BaseUnit>();
            SendMessageUpwards("OnUnitHited", this);
        }

        Destroy(gameObject);
    }
}
