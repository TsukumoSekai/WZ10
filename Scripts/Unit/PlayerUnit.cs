using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeFrame;
using WeFrame.Common;

public class PlayerUnit : BaseUnit
{
    /// <summary>物体字典</summary>
    public Dictionary<string, GameObject> mGoDict = new Dictionary<string, GameObject>();

    /// <summary>音源字典</summary>
    public Dictionary<string, AudioSource> mAudioDict = new Dictionary<string, AudioSource>();

    /// <summary>粒子字典</summary>
    public Dictionary<string, ParticleSystem> mParticleDict = new Dictionary<string, ParticleSystem>();

    private Rigidbody mRigidBody;
    public BaseUnit mTarget;//锁定的敌人

    /// <summary>高度信息</summary>
    private RaycastHit mHeightInfo;
    /// <summary>视距信息</summary>
    private RaycastHit mSightInfo;

    /// <summary>当前测距</summary>
    public float SightDistance { get => mSightInfo.distance; }
    /// <summary>当前速度</summary>
    public float Speed { get => Mathf.Max(Mathf.Abs(mHorizontalSpeed_Cur), Mathf.Abs(mVerticalSpeed_Cur)); }
    /// <summary>当前高度</summary>
    public float Height { get => mHeightInfo.distance; }

    /// <summary>是否锁定目标</summary>
    private bool mIsLock;

    /// <summary>武器预制体</summary>
    public GameObject[] mWeaponPrefabArr = new GameObject[3];

    /// <summary>最大攻击间隔</summary>
    [Header("武器攻击间隔")]
    public float[] mAttackIntervalArr_Max = { 0.05f, 0.3f, 1f };
    /// <summary>当前攻击间隔</summary>
    private float[] mAttackIntervalArr_Cur = new float[3];

    /// <summary>武器伤害</summary>
    [Header("武器伤害")]
    public float[] mBulletDamageArr = { 10f, 50f, 90f };

    /// <summary>最大弹量</summary>
    [Header("弹量")]
    public int[] mAmmunitionArr_Max = { 200, 38, 8 };
    /// <summary>当前弹量</summary>
    public int[] mAmmunitionArr_Cur = new int[3];

    /// <summary>Z轴当前速度</summary>
    private float mVerticalSpeed_Cur = 0f;
    /// <summary>Z轴速度限制</summary>
    [Header("Z轴速度限制")]
    public float mVerticalSpeed_Limit = 50f;

    /// <summary>X轴当前速度</summary>
    private float mHorizontalSpeed_Cur = 0f;
    /// <summary>X轴速度限制</summary>
    [Header("X轴速度限制")]
    public float mHorizontalSpeed_Limit = 50f;

    /// <summary>移动加速度</summary>
    [Header("移动加速度")]
    public float mMoveSpeedAccelerateVal = 10f;

    /// <summary>升力</summary>
    private float mLiftSpeed_Cur = 0f;
    /// <summary>升力上限</summary>
    private float mLiftSpeed_Limit = 10f;
    /// <summary>升力加速度</summary>
    [Header("升力加速度")]
    public float mLiftAccelerateVal = 15f;
    /// <summary>升力阻力</summary>
    [Header("升力速度限制")]
    public float mLiftResistanceVal = 0.1f;

    /// <summary>转向角度</summary>
    private float mRotateAngle = 0f;
    /// <summary>转向速度</summary>
    private float mRotateSpeed_Cur = 0f;
    /// <summary>转向加速度</summary>
    [Header("转向加速度")]
    public float mRotateAccelerate = 5f;
    /// <summary>转向速度限制</summary>
    [Header("转向速度限制")]
    public float mRotateSpeed_Limit = 30f;

    /// <summary>倾斜速度X</summary>
    private float mTiltAngle_X = 0f;
    /// <summary>倾斜速度Z</summary>
    private float mTiltAngle_Z = 0f;
    /// <summary>倾斜角度限制</summary>
    [Header("倾斜角度限制")]
    public float mTiltAngle_Limit = 15f;

    [Header("枪管控制")]
    public float mGunRotationYVal = 0f;
    public float mGunRotationZVal = -20f;
    public float mGunRotateSpeedVal = 0.2f;
    public float mGunRotateYMaxVal = 60f;
    public float mGunRotateYMinVal = -60f;
    public float mGunRotateZMaxVal = 60f;
    public float mGunRotateZMinVal = -60f;
    public float mGunRecoil = 0.15f;

    protected override void Init()
    {
        base.Init();

        mGoDict = CQuery.GetChildTagGoDict(this.gameObject, "ID=", QueryType.Contain);

        mAudioDict = CConvert.GoToComponentDict<AudioSource>(mGoDict);
        mParticleDict = CConvert.GoToComponentDict<ParticleSystem>(mGoDict);

        mRigidBody = GetComponent<Rigidbody>();
        mRotateAngle = mGoDict["WZ10"].transform.eulerAngles.y;

        for (int i = 0; i < 3; i++)
        {
            mAmmunitionArr_Cur[i] = mAmmunitionArr_Max[i];
            mAttackIntervalArr_Cur[i] = mAttackIntervalArr_Max[i];
            mWeaponPrefabArr[i] = Resources.Load<GameObject>("Prefabs/Bullet_Player_" + i.ToString());
        }
    }

    protected override void Refresh()
    {
        base.Refresh();
        /*
        //锁定敌人
        if (Input.GetButtonDown("Lock"))
        {
            if (mIsLock)
            {
                mIsLock = false;
                mTargetMark.SetActive(false);
                mTarget = null;
            }
            else if (!mIsLock && mUIConTroller.mEnemyList.Count > 0)
            {
                mIsLock = true;
                mTargetMark.SetActive(true);
                mTarget = mUIConTroller.mEnemyList[mTargetIndexNum];
            }
        }*/
        /*
        if (mIsLock && mTargetIndexNum >= 0 && mUIConTroller.mEnemyList.Count > 0 && mTargetMark)
        {
            if (mTargetIndexNum >= mUIConTroller.mEnemyList.Count)
                mTargetIndexNum = 0;
            if (Input.GetButtonDown("Change"))
            {
                if (mTargetIndexNum < mUIConTroller.mEnemyList.Count - 1)
                    mTargetIndexNum++;
                else
                    mTargetIndexNum = 0;
            }
            mTarget = mUIConTroller.mEnemyList[mTargetIndexNum];
            if (mUIConTroller.mEnemyList[mTargetIndexNum])
                mTargetMark.transform.position = Camera.main.WorldToScreenPoint(mUIConTroller.mEnemyList[mTargetIndexNum].position);
        }
        else if (mUIConTroller.mEnemyList.Count < 1)
        {
            Destroy(mTargetMark);
        }*/

        MoveControl();
        RotationControl();
        WeaponControl();

        Physics.Raycast(transform.position, Vector3.down, out mHeightInfo, 5000, 1 << 10);
    }

    /// <summary>
    /// 移动控制
    /// </summary>
    private void MoveControl()
    {
        //Z轴
        if (Input.GetAxisRaw("Vertical") != 0)
            mVerticalSpeed_Cur = Mathf.Clamp(mVerticalSpeed_Cur + Input.GetAxisRaw("Vertical") * mMoveSpeedAccelerateVal * Time.deltaTime, -mVerticalSpeed_Limit, mVerticalSpeed_Limit);
        else if (Mathf.Abs(mVerticalSpeed_Cur) > 1f)
            mVerticalSpeed_Cur += -mVerticalSpeed_Cur / Mathf.Abs(mVerticalSpeed_Cur) * mMoveSpeedAccelerateVal * 0.3f * Time.deltaTime;
        else
            mVerticalSpeed_Cur = 0;

        //X轴
        if (Input.GetAxisRaw("Horizontal") != 0)
            mHorizontalSpeed_Cur = Mathf.Clamp(mHorizontalSpeed_Cur + Input.GetAxisRaw("Horizontal") * mMoveSpeedAccelerateVal * Time.deltaTime, -mHorizontalSpeed_Limit, mHorizontalSpeed_Limit);
        else if (Mathf.Abs(mHorizontalSpeed_Cur) > 1f)
            mHorizontalSpeed_Cur += -mHorizontalSpeed_Cur / Mathf.Abs(mHorizontalSpeed_Cur) * mMoveSpeedAccelerateVal * 0.3f * Time.deltaTime;
        else
            mHorizontalSpeed_Cur = 0;

        //Y轴
        if (Input.GetAxisRaw("Lift") != 0)
            mLiftSpeed_Cur = Mathf.Clamp(mLiftSpeed_Cur + Input.GetAxisRaw("Lift") * mLiftAccelerateVal * Time.deltaTime, -mLiftSpeed_Limit, mLiftSpeed_Limit);
        else if (Mathf.Abs(mLiftSpeed_Cur) > 1f)
            mLiftSpeed_Cur += -mLiftSpeed_Cur / Mathf.Abs(mLiftSpeed_Cur) * mLiftAccelerateVal * 0.7f * Time.deltaTime;
        else
            mLiftSpeed_Cur = 0;

        mRigidBody.velocity = transform.forward * mVerticalSpeed_Cur + transform.up * mLiftSpeed_Cur + transform.right * mHorizontalSpeed_Cur;
    }

    /// <summary>
    /// 旋转控制
    /// </summary>
    private void RotationControl()
    {
        if (Input.GetAxisRaw("Turn") != 0)
            mRotateSpeed_Cur = Mathf.Clamp(mRotateSpeed_Cur + Input.GetAxisRaw("Turn") * mRotateAccelerate * Time.deltaTime, -mRotateSpeed_Limit, mRotateSpeed_Limit);
        else if (Mathf.Abs(mRotateSpeed_Cur) > 1f)
            mRotateSpeed_Cur += -mRotateSpeed_Cur / Mathf.Abs(mRotateSpeed_Cur) * mRotateAccelerate * 0.7f * Time.deltaTime;
        else
            mRotateSpeed_Cur = 0;

        mRotateAngle += mRotateSpeed_Cur * Time.deltaTime;

        mTiltAngle_X = Mathf.Clamp(mVerticalSpeed_Cur * 0.1f, -mTiltAngle_Limit, mTiltAngle_Limit);
        mTiltAngle_Z = Mathf.Clamp(mHorizontalSpeed_Cur * 0.1f, -mTiltAngle_Limit, mTiltAngle_Limit);
        mGoDict["WZ10"].transform.eulerAngles = Vector3.up * mRotateAngle;
        mGoDict["整机"].transform.localEulerAngles = new Vector3(mTiltAngle_X, 0, -mTiltAngle_Z);
    }

    /// <summary>
    /// 武器控制
    /// </summary>
    private void WeaponControl()
    {
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out mSightInfo);
        mGoDict["机炮-Root"].transform.LookAt(mSightInfo.point);

        for (int i = 0; i < 3; i++)
        {
            mAttackIntervalArr_Cur[i] += Time.deltaTime;
        }

        if (Input.GetMouseButton(0))
            Attack(0);

        if (Input.GetMouseButton(1))
            Attack(1);

        if (Input.GetMouseButton(2))
            Attack(2);
    }

    /// <summary>
    /// 攻击
    /// </summary>
    /// <param name="index">武器索引</param>
    private void Attack(int index)
    {
        if (mAmmunitionArr_Cur[index] <= 0 || mAttackIntervalArr_Cur[index] < mAttackIntervalArr_Max[index])
            return;

        mAmmunitionArr_Cur[index]--;
        mAttackIntervalArr_Cur[index] = 0;

        GameObject go = Instantiate(mWeaponPrefabArr[index]);
        Bullet bullet = go.GetComponent<Bullet>();
        bullet.mDamage = mBulletDamageArr[index];
        bullet.mBulletType = (BulletType)index;

        int orderIndex = mAmmunitionArr_Cur[index] % 2;

        switch (index)
        {
            case 0:
                go.transform.position = mGoDict["BulletSpawner"].transform.position - Vector3.up * 0.2f;
                go.transform.rotation = mGoDict["BulletSpawner"].transform.rotation;
                go.GetComponent<Rigidbody>().velocity = go.transform.forward * 500;
                mAudioDict["BulletAudio"].Play();
                mParticleDict["BulletParticle"].Play();

                mGoDict["机炮-Root"].transform.localEulerAngles += new Vector3(0, Random.Range(-mGunRecoil, mGunRecoil), Random.Range(-mGunRecoil, mGunRecoil));
                break;
            case 1:
                go.transform.position = mGoDict["RocketSpawner-" + orderIndex.ToString()].transform.position;
                go.transform.rotation = mGoDict["RocketSpawner-" + orderIndex.ToString()].transform.rotation;
                go.GetComponent<Rigidbody>().velocity = go.transform.forward * 300;
                mAudioDict["RocketAudio-" + orderIndex.ToString()].Play();
                mParticleDict["RocketParticle-" + orderIndex.ToString()].Play();
                break;
            case 2:
                go.transform.position = mGoDict["MissileSpawner-" + orderIndex.ToString()].transform.position;
                go.transform.rotation = mGoDict["MissileSpawner-" + orderIndex.ToString()].transform.rotation;
                mAudioDict["MissileAudio-" + orderIndex.ToString()].Play();
                mParticleDict["MissileParticle-" + orderIndex.ToString()].Play();
                ((TrackMissile)bullet).mSpeed = 300;
                bullet.mTarget = mTarget;
                break;
        }

        SendMessageUpwards("SetBulletParent", go);
        Destroy(go, 10f);
    }

    /// <summary>
    /// 碰撞地面
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.layer == 10)
        {
            ReceiveDamage(20f * Time.fixedDeltaTime);
        }
    }
}
