﻿using System;
using UnityEditor;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    public class MovementModule : Module {
        [Header("--- BODY ---")]
        /// <summary> Required to set the target rotations of the joints </summary>
        private Quaternion[] _initialJointsRotation;
        private ConfigurableJoint[] _joints;
        private Transform[] _animatedBones;

        [Header("--- ANIMATION ---")]
        private AnimatorHelper _animatorHelper;

        [Header("--- MOVEMENT ---")]
        public bool _enableMovement = true;

        private Vector2 _movementInput;

        [Header("--- INVERSE KINEMATICS ---")]
        public bool _enableIK = true;

        [Tooltip("Those values define the rotation range in which the target direction influences arm movement.")]
        public float minTargetDirAngle = - 30,
                     maxTargetDirAngle = 60;

        [Space(10)]
        [Tooltip("The limits of the arms direction. How far down/up should they be able to point?")]
        public float minArmsAngle = -70;
        public float maxArmsAngle = 83;
        [Tooltip("The limits of the look direction. How far down/up should the character be able to look?")]
        public float minLookAngle = -50, maxLookAngle = 60;

        [Space(10)]
        [Tooltip("The vertical offset of the look direction in reference to the target direction.")]
        public float lookAngleOffset;
        [Tooltip("The vertical offset of the arms direction in reference to the target direction.")]
        public float armsAngleOffset;
        [Tooltip("Defines the orientation of the hands")]
        public float handsRotationOffset = 0;

        [Space(10)]
        [Tooltip("How far apart to place the arms")]
        public float armsHorizontalSeparation = 0.75f;

        [Tooltip("The distance from the body to the hands in relation to how high/low they are. " +
                 "Allows to create more realistic movement patterns.")]
        public AnimationCurve armsDistance;

        private Vector3 _armsDir, _lookDir, _targetDir, _targetDir2D;
        private Transform _animTorso, _physTorso, _chest;
        private float _targetDirVerticalPercent;



        private void Start() {
            _joints = _activeRagdoll.Joints;
            _animatedBones = _activeRagdoll.AnimatedBones;
            _animatorHelper = _activeRagdoll.AnimatorHelper;

            _initialJointsRotation = new Quaternion[_joints.Length];
            for (int i = 0; i < _joints.Length; i++) {
                _initialJointsRotation[i] = _joints[i].transform.localRotation;
            }
        }

        void FixedUpdate() {
            UpdateJointTargets();
            UpdateIK();
            UpdateMovement();
        }

        /// <summary> Makes the physical bones match the rotation of the animated ones </summary>
        private void UpdateJointTargets() {
            for (int i = 0; i < _joints.Length; i++) {
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], _animatedBones[i + 1].localRotation, _initialJointsRotation[i]);
            }
        }

        private void UpdateIK() {
            if (!_enableIK) {
                _animatorHelper.LeftArmIKWeight = 0;
                _animatorHelper.RightArmIKWeight = 0;
                _animatorHelper.LookIKWeight = 0;
                return;
            }
            _animatorHelper.LookIKWeight = 1;

            _targetDir = _activeRagdoll.TargetDirection;
            _animTorso = _activeRagdoll.AnimatedTorso;
            _physTorso = _activeRagdoll.PhysicalTorso.transform;
            _chest = _activeRagdoll.GetAnimatedBone(HumanBodyBones.Spine);
            ReflectBackwards();
            _targetDir2D = Auxiliary.GetFloorProjection(_targetDir);
            CalculateVerticalPercent();

            UpdateLookIK();
            UpdateArmsIK();
        }

        /// <summary> Reflect the direction when looking backwards, avoids neck-breaking twists </summary>
        /// <param name=""></param>
        private void ReflectBackwards() {
            bool lookingBackwards = Vector3.Angle(_targetDir, _animTorso.forward) > 90;
            if (lookingBackwards) _targetDir = Vector3.Reflect(_targetDir, _animTorso.forward);
        }

        /// <summary> Calculate the vertical inlinacion percentage of the target direction
        /// (how much it is looking up) </summary>
        private void CalculateVerticalPercent() {
            float directionAngle = Vector3.Angle(_targetDir, Vector3.up);
            directionAngle -= 90;
            _targetDirVerticalPercent = 1 - Mathf.Clamp01((directionAngle - minTargetDirAngle) / Mathf.Abs(maxTargetDirAngle - minTargetDirAngle));
        }

        private void UpdateLookIK() {
            float lookVerticalAngle = _targetDirVerticalPercent * Mathf.Abs(maxLookAngle - minLookAngle) + minLookAngle;
            lookVerticalAngle += lookAngleOffset;
            _lookDir = Quaternion.AngleAxis(-lookVerticalAngle, _animTorso.right) * _targetDir2D;

            Vector3 lookPoint = _activeRagdoll.GetAnimatedBone(HumanBodyBones.Head).position + _lookDir;
            _animatorHelper.LookAtPoint(lookPoint);
        }

        private void UpdateArmsIK() {
            float armsVerticalAngle = _targetDirVerticalPercent * Mathf.Abs(maxArmsAngle - minArmsAngle) + minArmsAngle;
            armsVerticalAngle += armsAngleOffset;
            _armsDir = Quaternion.AngleAxis(-armsVerticalAngle, _animTorso.right) * _targetDir2D;

            float currentArmsDistance = armsDistance.Evaluate(_targetDirVerticalPercent);

            Vector3 armsMiddleTarget = _chest.position + _armsDir * currentArmsDistance;
            Vector3 upRef = Vector3.Cross(_armsDir, _animTorso.right).normalized;
            Vector3 armsHorizontalVec = Vector3.Cross(_armsDir, upRef).normalized;
            Quaternion handsRot = Quaternion.LookRotation(_armsDir, upRef);

            _animatorHelper.LeftHandTarget.position = armsMiddleTarget + armsHorizontalVec * armsHorizontalSeparation / 2;
            _animatorHelper.RightHandTarget.position = armsMiddleTarget - armsHorizontalVec * armsHorizontalSeparation / 2;
            _animatorHelper.LeftHandTarget.rotation = handsRot * Quaternion.Euler(0, 0, 45 + handsRotationOffset);
            _animatorHelper.RightHandTarget.rotation = handsRot * Quaternion.Euler(0, 0, -45 - handsRotationOffset);
        }

        /// <summary> Update the movement animation and rotation of the character </summary>
        private void UpdateMovement() {
            if (_movementInput == Vector2.zero || !_enableMovement) {
                PlayAnimation("Idle");
                return;
            }

            float angleOffset = Vector2.SignedAngle(_movementInput, Vector2.up);
            Vector3 targetForward = Quaternion.AngleAxis(angleOffset, Vector3.up) * Auxiliary.GetFloorProjection(_activeRagdoll.TargetDirection);
            _activeRagdoll.AnimatedAnimator.transform.rotation = Quaternion.LookRotation(targetForward, Vector3.up);

            PlayAnimation("Moving", _movementInput.magnitude);
        }

        /// <summary> Plays an animation using the animator. The speed doesn't change the actual
        /// speed of the animator, but a parameter of the same name that can be used to multiply
        /// the speed of certain animations. </summary>
        /// <param name="animation">The name of the animation state to be played</param>
        /// <param name="speed">The speed to be set</param>
        public void PlayAnimation(string animation, float speed = 1) {
            var animator = _activeRagdoll.AnimatedAnimator;

            animator.Play(animation);
            animator.SetFloat("speed", speed);
        }

        public void InputMove(Vector2 movement) {
            _movementInput = movement;
        }
        
        public void InputLeftArm(float weight) {
            if (!_enableIK)
                return;

            _animatorHelper.LeftArmIKWeight = weight;
        }

        public void InputRightArm(float weight) {
            if (!_enableIK)
                return;

            _animatorHelper.RightArmIKWeight = weight;
        }
    }
} // namespace ActiveRagdoll