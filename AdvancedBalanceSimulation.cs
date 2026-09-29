using System;

namespace TacticalShooterBalanceMechanics
{
    /// <summary>Input parameters for a repeatable, engine-independent balance test.</summary>
    public sealed class BalanceScenario
    {
        public float Distance { get; set; } = 25f;
        public float TargetSpeed { get; set; } = 0f;
        public float TargetRadius { get; set; } = 0.35f;
        public int Shots { get; set; } = 10;
        public float FireRate { get; set; } = 8f;
        public float AimAssistStrength { get; set; } = 0.35f;
        public float Zoom { get; set; } = 1f;
        public float RecoilControl { get; set; } = 0.5f;
        public float BaseSpreadDegrees { get; set; } = 0.5f;
        public float SpreadPerShotDegrees { get; set; } = 0.12f;
        public float SpreadRecoveryPerSecond { get; set; } = 1.2f;
        public float BaseDamage { get; set; } = 30f;
        public float TargetHealth { get; set; } = 100f;

        public void Validate()
        {
            if (Distance <= 0 || TargetRadius <= 0 || Shots < 1 || FireRate <= 0)
                throw new ArgumentException("Distance, TargetRadius, Shots and FireRate must be positive.");
            if (AimAssistStrength < 0 || AimAssistStrength > 1)
                throw new ArgumentOutOfRangeException(nameof(AimAssistStrength));
            if (Zoom < 1 || RecoilControl < 0 || RecoilControl > 1)
                throw new ArgumentOutOfRangeException(nameof(Zoom));
        }
    }

    public sealed class BalanceResult
    {
        public float HitProbability { get; internal set; }
        public float HeadshotProbability { get; internal set; }
        public float ExpectedDamage { get; internal set; }
        public float ExpectedTimeToKill { get; internal set; }
        public float Accessibility { get; internal set; }
        public float SkillCeiling { get; internal set; }
        public float CompetitiveBalance { get; internal set; }

        public string Summary =>
            $"Hit {HitProbability:P1}, Headshot {HeadshotProbability:P1}, " +
            $"Damage {ExpectedDamage:0.0}, TTK {ExpectedTimeToKill:0.00}s, " +
            $"Accessibility {Accessibility:P0}, Skill ceiling {SkillCeiling:P0}, " +
            $"Balance {CompetitiveBalance:P0}";
    }

    /// <summary>
    /// Deterministic approximation for comparing balance presets without Unity.
    /// It is not a replacement for playtests or server-side hit validation.
    /// </summary>
    public sealed class AdvancedBalanceSimulation
    {
        public BalanceResult Simulate(BalanceScenario scenario)
        {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));
            scenario.Validate();

            var angularTargetSize = MathF.Atan(scenario.TargetRadius / scenario.Distance);
            var targetDegrees = angularTargetSize * 57.29578f;
            var zoomBenefit = Math.Clamp(1f + (scenario.Zoom - 1f) * 0.18f, 1f, 1.8f);
            var movementPenalty = Math.Clamp(1f - scenario.TargetSpeed * 0.045f, 0.45f, 1f);
            var assistBenefit = 1f + scenario.AimAssistStrength * 0.55f;
            var controlBenefit = 1f + scenario.RecoilControl * 0.35f;

            float spread = scenario.BaseSpreadDegrees;
            float totalProbability = 0f;
            float totalDamage = 0f;
            for (var shot = 0; shot < scenario.Shots; shot++)
            {
                var recoilPenalty = shot * scenario.SpreadPerShotDegrees * (1f - scenario.RecoilControl);
                var effectiveSpread = spread + recoilPenalty;
                var precision = targetDegrees * zoomBenefit * assistBenefit * controlBenefit;
                var probability = Math.Clamp(
                    (precision / Math.Max(0.01f, effectiveSpread + targetDegrees)) * movementPenalty,
                    0f, 1f);

                totalProbability += probability;
                totalDamage += probability * scenario.BaseDamage;
                spread = Math.Max(scenario.BaseSpreadDegrees,
                    spread + scenario.SpreadPerShotDegrees - scenario.SpreadRecoveryPerSecond / scenario.FireRate);
            }

            var hitProbability = totalProbability / scenario.Shots;
            var headshotProbability = Math.Clamp(hitProbability * 0.18f * (1f + scenario.Zoom * 0.08f), 0f, 1f);
            var expectedDamage = totalDamage / scenario.Shots;
            var shotsToKill = scenario.TargetHealth / Math.Max(0.01f, expectedDamage);
            var ttk = Math.Max(0f, (shotsToKill - 1f) / scenario.FireRate);

            return new BalanceResult
            {
                HitProbability = hitProbability,
                HeadshotProbability = headshotProbability,
                ExpectedDamage = expectedDamage,
                ExpectedTimeToKill = ttk,
                Accessibility = Math.Clamp(0.35f + scenario.AimAssistStrength * 0.45f + (1f - movementPenalty) * 0.1f, 0f, 1f),
                SkillCeiling = Math.Clamp(0.95f - scenario.AimAssistStrength * 0.35f + scenario.RecoilControl * 0.15f, 0f, 1f),
                CompetitiveBalance = CalculateBalance(hitProbability, scenario)
            };
        }

        private static float CalculateBalance(float hitProbability, BalanceScenario scenario)
        {
            var deviation = MathF.Abs(hitProbability - 0.55f);
            var assistPenalty = Math.Max(0f, scenario.AimAssistStrength - 0.65f) * 0.7f;
            var zoomPenalty = Math.Max(0f, scenario.Zoom - 4f) * 0.08f;
            return Math.Clamp(1f - deviation - assistPenalty - zoomPenalty, 0f, 1f);
        }
    }
}
