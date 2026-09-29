using System;
using TacticalShooterBalanceMechanics;

namespace TacticalShooterBalanceMechanics.Demo
{
    internal static class Program
    {
        private static void Main()
        {
            var result = new AdvancedBalanceSimulation().Simulate(new BalanceScenario
            {
                Distance = 35f,
                TargetSpeed = 2.5f,
                Shots = 12,
                AimAssistStrength = 0.35f,
                Zoom = 2f,
                RecoilControl = 0.65f
            });

            Console.WriteLine("=== Tactical Shooter Balance Simulation ===");
            Console.WriteLine(result.Summary);
        }
    }
}
