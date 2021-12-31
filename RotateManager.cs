using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RotateManagement {
    [AddComponentMenu("RotateManagement/RotateManager")]
    public class RotateManager : MonoBehaviour {
        private enum RotateType { @Rotation, @LookAt }
        public enum TransitionType { @FixedTime, @FixedTimeAndKeep, @KeepConstantSpeed }
        private Transform Self { get { return this.transform; } }
        [Header("--- ToRotation ---")]
        [SerializeField] RotateType rotateType = RotateType.Rotation;
        [SerializeField] SimpleRotationGetter simpleRotationGetter = new SimpleRotationGetter();
        [SerializeField] LookAtRotationGetter lookAtRotationGetter = new LookAtRotationGetter();
        private Quaternion ToRotation {
            get {
                IRotationGetter rg = null;
                switch (this.rotateType) {
                    case RotateType.Rotation:
                        rg = this.simpleRotationGetter;
                        break;
                    case RotateType.LookAt:
                        rg = this.lookAtRotationGetter;
                        break;
                }
                return rg?.GetRotation() ?? Quaternion.identity;
            }
        }
        [Header("--- Transition ---")]
        [SerializeField] TransitionType transitionType = TransitionType.FixedTimeAndKeep;
        [SerializeField] bool speedBase = false;
        [SerializeField] float duration = 1f;
        private float remainTime = 0f;
        private float Speed_ { get {
                if (this.speedBase) return this.duration;
                else return this.rotationDistance / this.duration;
            }
        }
        private float Time_ { get {
                if (this.speedBase) return this.rotationDistance / this.duration;
                else return this.duration;
            }
        }
        private float T { get {
                if (this.Time_ == 0f) {
                    Debug.LogError("RotateManager/time is zero.");
                    return 0f;
                } else return Mathf.Clamp01(1f - this.remainTime / this.Time_);
            }
        }
        [SerializeField] bool active = false;
        [SerializeField] bool finished = false;
        private Quaternion beginFromRotation = Quaternion.identity;
        private Quaternion beginToRotation = Quaternion.identity;
        private float rotationDistance;
        private void UpdateRotationDistance() {
            Vector3 r0 = this.beginFromRotation.eulerAngles;
            Vector3 r1 = this.beginToRotation.eulerAngles;
            for (int i = 0; i < 3; i++) {
                r0[i] = r0[i] % 360f;
                r1[i] = r1[i] % 360f;
            }
            this.rotationDistance = Vector3.Distance(r0, r1); ;
        }

        // Update is called once per frame
        void Update() {
            if (this.active) Rotate();
        }

        private void Rotate() {
            switch (this.transitionType) {
                case TransitionType.FixedTime: //一定時間
                    if (this.remainTime < 0f) {
                        this.finished = true;
                        this.active = false;
                    }
                    if (!this.finished) SlerpRotate();
                    break;
                case TransitionType.FixedTimeAndKeep: //一定時間で回転、その後回転し続ける
                    if (this.remainTime < 0f) this.finished = true;
                    SlerpRotate();
                    break;
                case TransitionType.KeepConstantSpeed: //一定速度で回転し続ける
                    this.transform.rotation = Quaternion.RotateTowards(this.Self.rotation, this.ToRotation, this.Speed_ * Time.deltaTime);
                    break;
            }
            this.remainTime -= Time.deltaTime;
            void SlerpRotate() => this.transform.rotation = Quaternion.Slerp(this.beginFromRotation, this.ToRotation, this.T);
        }

        #region SetRotation
        public RotateManager Set(Quaternion toRotation) {
            this.rotateType = RotateType.Rotation;
            this.simpleRotationGetter.Set(toRotation);
            return this;
        }
        public RotateManager Set(Func<Quaternion> toRotation) {
            this.rotateType = RotateType.Rotation;
            this.simpleRotationGetter.Set(toRotation);
            return this;
        }
        public RotateManager Set(Vector3 target, Func<Vector3> upwards = null, bool lockPitch = false) {
            this.rotateType = RotateType.LookAt;
            this.lookAtRotationGetter.Set(this.transform, target, upwards, lockPitch);
            return this;
        }
        public RotateManager Set(Func<Vector3> target, Func<Vector3> upwards = null, bool lockPitch = false) {
            this.rotateType = RotateType.LookAt;
            this.lookAtRotationGetter.Set(this.transform, target, upwards, lockPitch);
            return this;
        }
        public RotateManager Set(Transform target, Func<Vector3> upwards = null, bool lockPitch = false) {
            this.rotateType = RotateType.LookAt;
            this.lookAtRotationGetter.Set(this.transform, target, upwards, lockPitch);
            return this;
        }
        #endregion
        public void StartRotate(float duration, bool speedBase = false, TransitionType transitionType = TransitionType.FixedTimeAndKeep) {
            this.duration = duration;
            this.speedBase = speedBase;
            this.transitionType = transitionType;
            this.beginFromRotation = this.Self.rotation;
            this.beginToRotation = this.ToRotation;
            UpdateRotationDistance();
            this.finished = false;
            this.active = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="speedBase">false 推奨</param>
        public void StartRotate_FixedTime(float duration, bool speedBase = false) {
            this.StartRotate(duration, speedBase, TransitionType.FixedTime);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="speedBase">false 推奨</param>
        public void StartRotate_FixedTimeAndKeep(float duration, bool speedBase = false) {
            this.StartRotate(duration, speedBase, TransitionType.FixedTimeAndKeep);
        }
        public void StartRotate_KeepConstansSpeed(float speed) {
            this.StartRotate(speed, true, TransitionType.KeepConstantSpeed);
        }
        public bool GetActive() => this.active;
        public bool GetFinished() => this.finished;

        private interface IRotationGetter {
            Quaternion GetRotation();
        }
        [System.Serializable]
        private class SimpleRotationGetter : IRotationGetter {
            private enum SelectType { @Quaternion, @QuaternionFunc }
            [SerializeField] SelectType type;
            [SerializeField] Quaternion quaternion;
            [SerializeField] Func<Quaternion> quaternionFunc;
            public void Set(Quaternion rotation) {
                this.type = SelectType.Quaternion;
                this.quaternion = rotation;
            }
            public void Set(Func<Quaternion> rotation) {
                this.type = SelectType.QuaternionFunc;
                this.quaternionFunc = rotation;
            }
            public Quaternion GetRotation() {
                switch (this.type) {
                    case SelectType.Quaternion:
                        return this.quaternion;
                    case SelectType.QuaternionFunc:
                        return this.quaternionFunc();
                    default:
                        return Quaternion.identity;
                }
            }
        }
        [System.Serializable]
        private class LookAtRotationGetter : IRotationGetter {
            private enum LookType { @Vec3, @Vec3Func, @Transform }
            [SerializeField] Transform self;
            [SerializeField] LookType type;
            [SerializeField] Vector3 vector3 = Vector3.zero;
            [SerializeField] Func<Vector3> vector3Func = () => Vector3.zero;
            [SerializeField] Transform transform;
            [SerializeField] Func<Vector3> upwards;
            [SerializeField] bool lockPitch = false;//roll pitch yaw
            private Vector3 Target {
                get {
                    switch (this.type) {
                        case LookType.Vec3:
                            return this.vector3;
                        case LookType.Vec3Func:
                            return this.vector3Func();
                        case LookType.Transform:
                            return this.transform.position;
                        default:
                            return Vector3.zero;
                    }
                }
            }
            #region SetTarget
            public void Set(Transform self, Vector3 target, Func<Vector3> upwards = null, bool lockPitch = false) {
                this.type = LookType.Vec3;
                this.vector3 = target;
                this.Set(self, upwards, lockPitch);
            }
            public void Set(Transform self, Func<Vector3> target, Func<Vector3> upwards = null, bool lockPitch = false) {
                this.type = LookType.Vec3Func;
                this.vector3Func = target;
                this.Set(self, upwards, lockPitch);
            }
            public void Set(Transform self, Transform target, Func<Vector3> upwards = null, bool lockPitch = false) {
                this.type = LookType.Transform;
                this.transform = target;
                this.Set(self, upwards, lockPitch);
            }
            private void Set(Transform self, Func<Vector3> upwards, bool lockPitch = false) {
                this.self = self;
                this.upwards = upwards;
                this.lockPitch = lockPitch;
            }
            #endregion
            public Quaternion GetRotation() {
                Vector3 _upward = this.upwards != null ? this.upwards() : Vector3.up;
                Vector3 targetPos = this.Target;
                if (this.lockPitch) {
                    Vector3 localTargetPos = this.self.InverseTransformPoint(targetPos);
                    localTargetPos.y = 0f;
                    targetPos = this.self.TransformPoint(localTargetPos);
                }
                Vector3 direction = targetPos - self.position;
                Quaternion rotation = Quaternion.LookRotation(direction, _upward);
                return rotation; //lockPitch使う
            }
        }
    }
    public static class AddComponenter_RotateManager {
        public static RotateManager ComponentRotateManager(this GameObject self) {
            return self.ComponentCheck<RotateManager>();
        }
        private static T ComponentCheck<T>(this GameObject self) where T : MonoBehaviour {
            T component = self.GetComponent<T>();
            if (component == null) {
                component = self.AddComponent<T>();
                Debug.Log(component.GetType() + " was componented.", self);
            }
            return component;
        }
    }
}