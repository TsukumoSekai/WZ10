using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackMissile : Bullet
{
    public float mRotateSpeed = 30f;
    public float mSpeed;

    protected override void Init()
    {
        base.Init();

        if (mTarget == null)
        {

        }
    }

    protected override void Refresh()
    {
        base.Refresh();

        if (mTarget != null)
        {
            Vector3 targetDirection = mTarget.transform.position - transform.position;

            float singleStep = mSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

            transform.rotation = Quaternion.LookRotation(newDirection);

        }

        transform.position += transform.forward * mSpeed * Time.deltaTime;
    }
}
