using System;
using System.Collections.Generic;
using UnityEngine;

namespace TacticalShooterBalanceMechanics
{
    /// <summary>
    /// Comprehensive system for managing aiming aids, recoil control, and auto-zoom mechanics
    /// in tactical shooters and their impact on game balance and dynamics.
    /// </summary>
    public class TacticalShooterAimingSystem : MonoBehaviour
    {
        [System.Serializable]
        public class AimAssistSettings
        {
            [Header("Aim Assist Parameters")]
            public float assistStrength = 0.5f; // 0-1: How strong the aim assist pull is
            public float assistRadius = 5f; // pixels/units
            public float assistSensitivityMultiplier = 0.8f;
            public bool enableAimAssist = true;
            public AimAssistType assistType = AimAssistType.TargetTracking;
        }

        [System.Serializable]
        public class RecoilControlSettings
        {
            [Header("Recoil & Spread Control")]
            public float baseRecoilX = 2f; // Vertical recoil
            public float baseRecoilY = 1f; // Horizontal recoil
            public float baseSpread = 0.5f; // Bullet spread in degrees
            public float spreadIncrementPerShot = 0.1f;
            public float spreadRecoveryRate = 0.3f; // How fast spread decreases
            public float recoilPatternIntensity = 1f;
            public AnimationCurve recoilPattern = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        [System.Serializable]
        public class AutoZoomSettings
        {
            [Header("Auto-Zoom Parameters")]
            public float minZoom = 1f;
            public float maxZoom = 4f;
            public float zoomSpeed = 5f;
            public bool enableAutoZoom = true;
            public float autoZoomDistance = 50f; // Distance at which auto-zoom triggers
            public float zoomSensitivityReduction = 0.6f; // Sensitivity multiplier when zoomed
        }

        [System.Serializable]
        public class BalanceMetrics
        {
            public float skillCeiling; // How high skill can affect performance
            public float accessibilityScore; // How accessible to new players (0-1)
            public float competitiveBalance; // Impact on competitive balance (0-1)
            public float engagementRange; // Effective combat range in units
        }

        public enum AimAssistType
        {
            None,
            TargetTracking,      // Follows target smoothly
            TargetSnap,          // Snaps to target
            RecoilCompensation,  // Auto-corrects recoil
            SlowMotion           // Slows down aiming near target
        }

        [SerializeField] private AimAssistSettings aimSettings = new AimAssistSettings();
        [SerializeField] private RecoilControlSettings recoilSettings = new RecoilControlSettings();
        [SerializeField] private AutoZoomSettings zoomSettings = new AutoZoomSettings();

        private Vector2 currentRecoil = Vector2.zero;
        private float currentSpread = 0f;
        private float currentZoom = 1f;
        private bool isAiming = false;
        private Transform targetEnemy = null;
        private List<float> accuracyLog = new List<float>();

        private void Start()
        {
            currentZoom = zoomSettings.minZoom;
            currentSpread = recoilSettings.baseSpread;
        }

        private void Update()
        {
            UpdateZoom();
            UpdateRecoilRecovery();
            if (isAiming)
            {
                ApplyAimAssist();
            }
        }

        /// <summary>
        /// Activates aiming mode and applies appropriate aim assist based on settings
        /// </summary>
        public void StartAiming(Transform target = null)
        {
            isAiming = true;
            targetEnemy = target;

            if (aimSettings.enableAimAssist)
            {
                switch (aimSettings.assistType)
                {
                    case AimAssistType.TargetTracking:
                        Debug.Log("Aim Assist: Target Tracking Active");
                        break;
                    case AimAssistType.TargetSnap:
                        Debug.Log("Aim Assist: Target Snap Active");
                        break;
                    case AimAssistType.RecoilCompensation:
                        Debug.Log("Aim Assist: Recoil Compensation Active");
                        break;
                    case AimAssistType.SlowMotion:
                        Debug.Log("Aim Assist: Slow Motion Active");
                        break;
                }
            }

            if (zoomSettings.enableAutoZoom && targetEnemy != null)
            {
                AutoZoomToTarget(targetEnemy);
            }
        }

        public void StopAiming()
        {
            isAiming = false;
            targetEnemy = null;
        }

        /// <summary>
        /// Applies aim assist based on current settings and target position
        /// </summary>
        private void ApplyAimAssist()
        {
            if (!aimSettings.enableAimAssist || targetEnemy == null)
                return;

            Vector3 directionToTarget = (targetEnemy.position - transform.position).normalized;
            Vector3 currentDirection = transform.forward;

            float dotProduct = Vector3.Dot(currentDirection, directionToTarget);

            if (dotProduct > 0.95f) // Target is close to reticle
            {
                switch (aimSettings.assistType)
                {
                    case AimAssistType.TargetTracking:
                        TrackTarget(directionToTarget);
                        break;

                    case AimAssistType.TargetSnap:
                        SnapToTarget(directionToTarget);
                        break;

                    case AimAssistType.RecoilCompensation:
                        CompensateRecoil();
                        break;

                    case AimAssistType.SlowMotion:
                        ReduceSensitivity();
                        break;
                }
            }
        }

        /// <summary>
        /// Smooth target tracking that follows enemy movement
        /// </summary>
        private void TrackTarget(Vector3 directionToTarget)
        {
            float trackingStrength = aimSettings.assistStrength * 0.5f;
            transform.forward = Vector3.Lerp(transform.forward, directionToTarget, trackingStrength * Time.deltaTime);
        }

        /// <summary>
        /// Snaps aiming to target quickly
        /// </summary>
        private void SnapToTarget(Vector3 directionToTarget)
        {
            float snapStrength = aimSettings.assistStrength;
            transform.forward = Vector3.Lerp(transform.forward, directionToTarget, snapStrength * 0.1f);
        }

        /// <summary>
        /// Compensates for weapon recoil automatically
        /// </summary>
        private void CompensateRecoil()
        {
            currentRecoil = Vector2.Lerp(currentRecoil, Vector2.zero, Time.deltaTime * 2f);
        }

        /// <summary>
        /// Reduces mouse/joystick sensitivity when near target
        /// </summary>
        private void ReduceSensitivity()
        {
            // Sensitivity is reduced by aim assist strength
            // Implementation depends on input system
        }

        /// <summary>
        /// Handles weapon firing with recoil and spread calculations
        /// </summary>
        public void Fire()
        {
            if (!isAiming)
                return;

            // Add recoil
            ApplyRecoil();

            // Increase spread
            currentSpread = Mathf.Min(
                currentSpread + recoilSettings.spreadIncrementPerShot,
                recoilSettings.baseSpread * 3f
            );

            // Calculate bullet trajectory with spread
            Vector3 bulletDirection = GetBulletDirection();

            Debug.Log($"FIRE! | Recoil: {currentRecoil} | Spread: {currentSpread}° | Zoom: {currentZoom}x");

            // Log accuracy for balance metrics
            LogAccuracy();

            // Visual feedback
            OnWeaponFired();
        }

        /// <summary>
        /// Applies recoil based on weapon profile and player control
        /// </summary>
        private void ApplyRecoil()
        {
            float recoilX = recoilSettings.baseRecoilX * recoilSettings.recoilPatternIntensity;
            float recoilY = recoilSettings.baseRecoilY * recoilSettings.recoilPatternIntensity;

            // Apply recoil pattern (curves upward typically)
            float patternInfluence = recoilSettings.recoilPattern.Evaluate(Time.time % 1f);
            recoilX *= (1f + patternInfluence * 0.5f);

            currentRecoil = new Vector2(recoilX, recoilY);

            // Apply to rotation
            transform.Rotate(-recoilX * 0.1f, recoilY * 0.1f, 0);
        }

        /// <summary>
        /// Gradually recovers from recoil over time
        /// </summary>
        private void UpdateRecoilRecovery()
        {
            currentRecoil = Vector2.Lerp(
                currentRecoil,
                Vector2.zero,
                Time.deltaTime * recoilSettings.spreadRecoveryRate
            );

            currentSpread = Mathf.Max(
                currentSpread - recoilSettings.spreadRecoveryRate * Time.deltaTime,
                recoilSettings.baseSpread
            );
        }

        /// <summary>
        /// Calculates bullet direction with current spread
        /// </summary>
        private Vector3 GetBulletDirection()
        {
            Vector3 baseDirection = transform.forward;

            // Add spread cone
            float randomAngle = UnityEngine.Random.Range(0, 360);
            float spreadRadius = currentSpread * Mathf.Deg2Rad;

            Vector3 spreadOffset = new Vector3(
                Mathf.Sin(randomAngle) * spreadRadius,
                Mathf.Cos(randomAngle) * spreadRadius,
                0
            );

            return (baseDirection + spreadOffset).normalized;
        }

        /// <summary>
        /// Auto-zoom towards distant targets
        /// </summary>
        private void AutoZoomToTarget(Transform target)
        {
            if (!zoomSettings.enableAutoZoom || target == null)
                return;

            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget > zoomSettings.autoZoomDistance)
            {
                float targetZoom = Mathf.Lerp(
                    zoomSettings.minZoom,
                    zoomSettings.maxZoom,
                    (distanceToTarget - zoomSettings.autoZoomDistance) / 100f
                );

                currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSettings.zoomSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Manual zoom control
        /// </summary>
        public void UpdateZoom()
        {
            if (!zoomSettings.enableAutoZoom)
                return;

            // This would be called from input handling
            float zoomInput = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                currentZoom += zoomInput * zoomSettings.zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, zoomSettings.minZoom, zoomSettings.maxZoom);
            }
        }

        /// <summary>
        /// Calculates effective sensitivity based on zoom level
        /// </summary>
        public float GetEffectiveSensitivity()
        {
            float baseSensitivity = 1f;
            float zoomPenalty = 1f - ((currentZoom - zoomSettings.minZoom) / (zoomSettings.maxZoom - zoomSettings.minZoom)) * zoomSettings.zoomSensitivityReduction;
            float aimAssistPenalty = aimSettings.enableAimAssist ? aimSettings.assistSensitivityMultiplier : 1f;

            return baseSensitivity * zoomPenalty * aimAssistPenalty;
        }

        /// <summary>
        /// Logs accuracy metrics for balance analysis
        /// </summary>
        private void LogAccuracy()
        {
            float accuracy = 1f / (1f + currentSpread + currentRecoil.magnitude);
            accuracyLog.Add(accuracy);
        }

        /// <summary>
        /// Analyzes balance metrics based on current settings
        /// </summary>
        public BalanceMetrics CalculateBalanceMetrics()
        {
            BalanceMetrics metrics = new BalanceMetrics();

            // Skill ceiling: Higher with less aim assist
            metrics.skillCeiling = aimSettings.enableAimAssist ? 0.6f : 1f;

            // Accessibility: Higher with aim assist
            metrics.accessibilityScore = aimSettings.enableAimAssist ? 0.8f : 0.4f;

            // Competitive balance: Medium with balanced settings
            metrics.competitiveBalance = CalculateCompetitiveBalance();

            // Effective engagement range: Extended by zoom
            metrics.engagementRange = 50f * currentZoom;

            return metrics;
        }

        /// <summary>
        /// Calculates how balanced the current settings are for competitive play
        /// </summary>
        private float CalculateCompetitiveBalance()
        {
            float balance = 0.5f;

            // Penalize too strong aim assist
            if (aimSettings.assistStrength > 0.7f)
                balance -= (aimSettings.assistStrength - 0.7f) * 0.3f;

            // Penalize excessive zoom
            if (zoomSettings.maxZoom > 4f)
                balance -= (zoomSettings.maxZoom - 4f) * 0.1f;

            // Reward controlled recoil
            if (recoilSettings.spreadRecoveryRate > 0.3f)
                balance += 0.1f;

            return Mathf.Clamp01(balance);
        }

        /// <summary>
        /// Generates a report on how each mechanic affects game balance
        /// </summary>
        public void PrintBalanceReport()
        {
            BalanceMetrics metrics = CalculateBalanceMetrics();

            Debug.Log("=== TACTICAL SHOOTER BALANCE REPORT ===");
            Debug.Log($"Aim Assist Type: {aimSettings.assistType}");
            Debug.Log($"Aim Assist Strength: {aimSettings.assistStrength}");
            Debug.Log($"Current Zoom: {currentZoom}x (Min: {zoomSettings.minZoom}x, Max: {zoomSettings.maxZoom}x)");
            Debug.Log($"Current Spread: {currentSpread}°");
            Debug.Log($"Current Recoil: {currentRecoil}");
            Debug.Log("");
            Debug.Log("--- BALANCE IMPACT ---");
            Debug.Log($"Skill Ceiling: {metrics.skillCeiling:P0} (Higher = More Skill Required)");
            Debug.Log($"Accessibility: {metrics.accessibilityScore:P0} (Higher = Easier for New Players)");
            Debug.Log($"Competitive Balance: {metrics.competitiveBalance:P0}");
            Debug.Log($"Effective Engagement Range: {metrics.engagementRange}");
            Debug.Log("");
            Debug.Log("--- DYNAMIC IMPACT ANALYSIS ---");
            Debug.Log($"Aim Assist reduces skill gap: {(aimSettings.enableAimAssist ? "YES" : "NO")}");
            Debug.Log($"Auto-Zoom extends engagement range: {(zoomSettings.enableAutoZoom ? "YES" : "NO")}");
            Debug.Log($"Recoil Control affects player mastery: {(recoilSettings.recoilPatternIntensity > 0.5f ? "HIGH" : "LOW")}");
            Debug.Log("=======================================");
        }

        private void OnWeaponFired()
        {
            // Visual/audio feedback would be implemented here
        }

        public float GetCurrentZoom() => currentZoom;
        public Vector2 GetCurrentRecoil() => currentRecoil;
        public float GetCurrentSpread() => currentSpread;
        public bool IsAiming() => isAiming;
    }
}
