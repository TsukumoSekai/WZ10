using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnit : BaseUnit
{
    public float mAttackInterval_Max = 5f;
    private float mAttackInterval_Cur = 0f;

    public GameObject mBulletPrefab;
    public AudioSource mAudioSource;
    public Transform mBulletSpawn;
    public Transform mTarget;
    public bool mIsAllowedAttack = false;

    protected override void Init()
    {
        base.Init();

        mAttackInterval_Max += mAttackInterval_Max * Random.value;
    }

    protected override void Refresh()
    {
        base.Refresh();

        if (mIsAllowedAttack)
            Attack();
    }

    private void Attack()
    {
        mAttackInterval_Cur += Time.deltaTime;

        if (mAttackInterval_Cur < mAttackInterval_Max)
            return;

        mAttackInterval_Cur = 0;

        GameObject go = Instantiate(mBulletPrefab, mBulletSpawn.position, mBulletSpawn.rotation);
        go.transform.LookAt(mTarget);
        go.GetComponent<Rigidbody>().velocity = go.transform.forward * 150;

        Bullet bullet = go.GetComponent<Bullet>();
        bullet.mDamage = 10;
        bullet.mBulletType = BulletType.EnemyBullet;

        SendMessageUpwards("SetBulletParent", go);
        mAudioSource.Play();

        Destroy(go, 10);
    }
}
