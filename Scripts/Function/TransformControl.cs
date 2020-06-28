using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameCraft
{
    public class EnumFlags : PropertyAttribute { }

    public enum TransformType
    {
        /// <summary>无</summary>
        None,
        /// <summary>全局锁定</summary>
        WorldFix,
        /// <summary>局部锁定</summary>
        LocalFix,
        /// <summary>全局增量</summary>
        WorldIncrement,
        /// <summary>局部增量</summary>
        LocalIncrement
    }

    public enum LimitType
    {
        XMax = 1,
        XMin = 2,
        YMax = 4,
        YMin = 8,
        ZMax = 16,
        ZMin = 32,
    }

    public class TransformControl : MonoBehaviour
    {
        /// <summary>坐标变换类型</summary>
        [Header("坐标变换类型")]
        public TransformType mPositionTransformType = TransformType.None;
        /// <summary>旋转变换类型</summary>
        [Header("旋转变换类型")]
        public TransformType mRotationTransformType = TransformType.None;
        /// <summary>缩放变换类型</summary>
        [Header("缩放变换类型")]
        public TransformType mScaleTransformType = TransformType.None;

        /// <summary>坐标增量/锁定值</summary>
        [Header("坐标增量/锁定值")]
        public Vector3 mPositionTransformValue = Vector3.zero;
        /// <summary>坐标增量/锁定值</summary>
        [Header("旋转增量/锁定值")]
        public Vector3 mRotationTransformValue = Vector3.zero;
        /// <summary>坐标增量/锁定值</summary>
        [Header("缩放增量/锁定值")]
        public Vector3 mScaleTransformValue = Vector3.zero;

        /// <summary>坐标限制类型</summary>
        [EnumFlags]
        [Header("坐标限制类型")]
        public LimitType mPositionLimitType = 0;
        /// <summary>旋转限制类型</summary>
        [EnumFlags]
        [Header("旋转限制类型")]
        public LimitType mRotationLimitType = 0;
        /// <summary>缩放限制类型</summary>
        [EnumFlags]
        [Header("缩放限制类型")]
        public LimitType mScaleLimitType = 0;

        /// <summary>坐标限制最大值</summary>
        [Header("坐标限制最大值")]
        public Vector3 mPositionLimitValueMax = Vector3.zero;
        /// <summary>坐标限制最小值</summary>
        [Header("坐标限制最小值")]
        public Vector3 mPositionLimitValueMin = Vector3.zero;
        /// <summary>缩放限制最大值</summary>
        [Header("缩放限制最大值")]
        public Vector3 mRotationLimitValueMax = Vector3.zero;
        /// <summary>旋转限制最小值</summary>
        [Header("旋转限制最小值")]
        public Vector3 mRotationLimitValueMin = Vector3.zero;
        /// <summary>缩放限制最大值</summary>
        [Header("缩放限制最大值")]
        public Vector3 mScaleLimitValueMax = Vector3.zero;
        /// <summary>缩放限制最小值</summary>
        [Header("缩放限制最小值")]
        public Vector3 mScaleLimitValueMin = Vector3.zero;

        private Vector3 mOriginGlobalPosition;
        private Vector3 mOriginGlobalRotation;
        private Vector3 mOriginGlobalScale;

        private Vector3 mOriginLocalPosition;
        private Vector3 mOriginLocalRotation;
        private Vector3 mOriginLocalScale;

        private void Start()
        {
            mOriginGlobalPosition = transform.position;
            mOriginGlobalRotation = transform.eulerAngles;
            mOriginGlobalScale = transform.lossyScale;

            mOriginLocalPosition = transform.localPosition;
            mOriginLocalRotation = transform.localEulerAngles;
            mOriginLocalScale = transform.localScale;
        }

        private void Update()
        {
            switch (mPositionTransformType)
            {
                case TransformType.None:
                    break;
                case TransformType.WorldFix:
                    transform.position = LimitValue(mPositionTransformValue, mPositionLimitType, mPositionLimitValueMin, mPositionLimitValueMax);
                    break;
                case TransformType.LocalFix:
                    transform.localPosition = LimitValue(mPositionTransformValue, mPositionLimitType, mPositionLimitValueMin, mPositionLimitValueMax);
                    break;
                case TransformType.WorldIncrement:
                    transform.position = LimitValue(transform.position + mPositionTransformValue * Time.deltaTime, mPositionLimitType, mPositionLimitValueMin, mPositionLimitValueMax);
                    break;
                case TransformType.LocalIncrement:
                    transform.localPosition = LimitValue(transform.localPosition + mPositionTransformValue * Time.deltaTime, mPositionLimitType, mPositionLimitValueMin, mPositionLimitValueMax);
                    break;
            }

            switch (mRotationTransformType)
            {
                case TransformType.None:
                    break;
                case TransformType.WorldFix:
                    transform.eulerAngles = LimitValue(mRotationTransformValue, mRotationLimitType, mRotationLimitValueMin, mRotationLimitValueMax);
                    break;
                case TransformType.LocalFix:
                    transform.localEulerAngles = LimitValue(mRotationTransformValue, mRotationLimitType, mRotationLimitValueMin, mRotationLimitValueMax);
                    break;
                case TransformType.WorldIncrement:
                    transform.eulerAngles = LimitValue(transform.eulerAngles + mRotationTransformValue * Time.deltaTime, mRotationLimitType, mRotationLimitValueMin, mRotationLimitValueMax);
                    break;
                case TransformType.LocalIncrement:
                    transform.localEulerAngles = LimitValue(transform.localEulerAngles + mRotationTransformValue * Time.deltaTime, mRotationLimitType, mRotationLimitValueMin, mRotationLimitValueMax);
                    break;
            }

            switch (mScaleTransformType)
            {
                case TransformType.None:
                    break;
                case TransformType.WorldFix:
                    Vector3 targetValue = LimitValue(mScaleTransformValue, mScaleLimitType, mScaleLimitValueMin, mScaleLimitValueMax);
                    transform.lossyScale.Set(targetValue.x, targetValue.y, targetValue.z);
                    break;
                case TransformType.LocalFix:
                    transform.localScale = LimitValue(mScaleTransformValue, mScaleLimitType, mScaleLimitValueMin, mScaleLimitValueMax);
                    break;
                case TransformType.WorldIncrement:
                    targetValue = LimitValue(transform.lossyScale + mScaleTransformValue * Time.deltaTime, mScaleLimitType, mScaleLimitValueMin, mScaleLimitValueMax);
                    transform.lossyScale.Set(targetValue.x, targetValue.y, targetValue.z);
                    break;
                case TransformType.LocalIncrement:
                    transform.localScale = LimitValue(transform.localScale + mScaleTransformValue * Time.deltaTime, mScaleLimitType, mScaleLimitValueMin, mScaleLimitValueMax);
                    break;
            }
        }

        /// <summary>
        /// 重置全局坐标
        /// </summary>
        public void ResetGlobalPosition()
        {
            transform.position = mOriginGlobalPosition;
        }

        /// <summary>
        /// 重置本地坐标
        /// </summary>
        public void ResetLocalPosition()
        {
            transform.localPosition = mOriginLocalPosition;
        }

        /// <summary>
        /// 重置全局旋转
        /// </summary>
        public void ResetGlobalRotation()
        {
            transform.eulerAngles = mOriginGlobalRotation;
        }

        /// <summary>
        /// 重置本地旋转
        /// </summary>
        public void ResetLocalRotation()
        {
            transform.localEulerAngles = mOriginLocalRotation;
        }

        /// <summary>
        /// 重置全局缩放
        /// </summary>
        public void ResetGlobalScale()
        {
            transform.lossyScale.Set(mOriginGlobalScale.x, mOriginGlobalScale.y, mOriginGlobalScale.z);
        }

        /// <summary>
        /// 重置本地缩放
        /// </summary>
        public void ResetLocalScale()
        {
            transform.localScale = mOriginLocalScale;
        }

        /// <summary>
        /// 重置全局变换
        /// </summary>
        public void ResetGlobalTransform()
        {
            transform.position = mOriginGlobalPosition;
            transform.eulerAngles = mOriginGlobalRotation;
            transform.lossyScale.Set(mOriginGlobalScale.x, mOriginGlobalScale.y, mOriginGlobalScale.z);
        }

        /// <summary>
        /// 重置本地变换
        /// </summary>
        public void ResetLocalTransform()
        {
            transform.localPosition = mOriginLocalPosition;
            transform.localEulerAngles = mOriginLocalRotation;
            transform.localScale = mOriginLocalScale;
        }

        private Vector3 LimitValue(Vector3 sourceValue, LimitType limitType, Vector3 limitMin, Vector3 limitMax)
        {
            float x = sourceValue.x, y = sourceValue.y, z = sourceValue.z;

            if ((limitType & LimitType.XMax) != 0)
            {
                x = Mathf.Clamp(x, float.MinValue, limitMax.x);
            }
            if ((limitType & LimitType.XMin) != 0)
            {
                x = Mathf.Clamp(x, limitMin.x, float.MaxValue);
            }
            if ((limitType & LimitType.YMax) != 0)
            {
                y = Mathf.Clamp(y, float.MinValue, limitMax.y);
            }
            if ((limitType & LimitType.YMin) != 0)
            {
                y = Mathf.Clamp(y, limitMin.y, float.MaxValue);
            }
            if ((limitType & LimitType.ZMax) != 0)
            {
                z = Mathf.Clamp(z, float.MinValue, limitMax.z);
            }
            if ((limitType & LimitType.ZMin) != 0)
            {
                z = Mathf.Clamp(z, limitMin.z, float.MaxValue);
            }

            return new Vector3(x, y, z);
        }
    }
}