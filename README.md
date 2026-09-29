# Tactical Shooter Balance Mechanics

Eine erweiterbare Unity/C#-Referenzimplementierung zur Untersuchung von Zielhilfen, Streuung, Rückstoß und automatischem Zoomen in Taktik-Shootern.

## Enthaltene Systeme

- Zielhilfe-Modi: Tracking, Snap, Slowdown und Recoil Compensation
- Rückstoß mit konfigurierbarem Muster
- Dynamische Streuung, Schussfolge und Erholung
- Automatisches und manuelles Zoomen
- Bewegungs-, Hüftfeuer- und Sprint-Multiplikatoren
- Zielgrößen-, Distanz- und Sichtlinienprüfung
- Headshot-/Trefferzonen-Unterstützung
- Trefferwahrscheinlichkeits- und Time-to-Kill-Schätzung
- Balance-Metriken für Skill Ceiling, Zugänglichkeit, Reichweite und Wettbewerbsbalance
- Schwierigkeitspresets und Fairness-Limits
- Telemetrie-Samples für A/B-Tests
- JSON-freie, Unity-kompatible C#-Datenmodelle

## Verwendung in Unity

1. Unity-Projekt mit einer unterstützten LTS-Version erstellen.
2. `TacticalShooterAimingSystem.cs` in `Assets/Scripts/` kopieren.
3. Das Script an die Kamera oder den Player hängen.
4. Im Inspector die Werte einstellen.
5. `StartAiming(target)`, `Fire()` und `StopAiming()` aus der Waffen-/Input-Logik aufrufen.
6. Für Analysezwecke `PrintBalanceReport()` aufrufen.

## Verwendung der erweiterten Simulation

`AdvancedBalanceSimulation.cs` benötigt keine Unity-API und kann in Unit-Tests, einem Balancing-Tool oder einem Server-Simulator verwendet werden:

```csharp
var simulator = new AdvancedBalanceSimulation();
var result = simulator.Simulate(new BalanceScenario
{
    Distance = 35f,
    TargetSpeed = 2.5f,
    Shots = 12,
    AimAssistStrength = 0.35f,
    Zoom = 2f,
    RecoilControl = 0.65f
});

Console.WriteLine(result.Summary);
```

## Balancing-Grundsätze

- Starke Zielhilfe sollte die Zugänglichkeit erhöhen, aber die maximale Präzision begrenzen.
- Automatisches Zoomen darf Reichweite verbessern, aber Bewegung und Sichtfeld nicht kostenlos ersetzen.
- Rückstoßkontrolle soll erlernbar sein; perfekte Kompensation darf nicht vollständig automatisiert werden.
- Streuung sollte Dauerfeuer unattraktiver machen als kontrollierte Feuerstöße.
- Jede Assistenzoption sollte serverseitig validiert und für kompetitive Modi begrenzbar sein.

## Lizenz

Dieses Repository enthält eine technische Referenzimplementierung. Eine konkrete Lizenz kann nach Bedarf ergänzt werden.
