﻿using System.Diagnostics;

namespace Azure.AI.Details.Common.CLI.Telemetry.Events
{
    [DebuggerDisplay("{Name}: {Outcome} {Detail}")]
    public struct VerifySavedConfigTelemetryEvent : ITelemetryEvent
    {
        public string Name => "verify_saved";

        public Outcome Outcome { get; init; }

        public string Detail { get; init; }

        public string Error { get; init; }

        public double DurationInMs { get; init; }
    }
}
