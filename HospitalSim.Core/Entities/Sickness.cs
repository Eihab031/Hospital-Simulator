using HospitalSim.Core.Enum;

namespace HospitalSim.Core.Entities
{
    public class Sickness
    {
        public required SicknessKind Kind { get; init; }
        public required string Name { get; init; }
        public required int MinDays { get; init; }
        public required int MaxDays { get; init; }
        public required double DeathChance { get; init; }
        public required int Weight { get; init; }
    }
}
