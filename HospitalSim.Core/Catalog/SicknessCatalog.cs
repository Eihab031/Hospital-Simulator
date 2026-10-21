
using HospitalSim.Core.Entities;
using HospitalSim.Core.Enum;

namespace HospitalSim.Core.Catalog
{
    public static class SicknessCatalog
    {
        public static IReadOnlyDictionary<SicknessKind, Sickness> All { get; } = new Dictionary<SicknessKind, Sickness> 
        {
            [SicknessKind.Flu]=new()
            { Kind=SicknessKind.Flu,
              Name="Flu",
              MinDays=2,
              MaxDays=4,
              DeathChance=0.0005,
              Weight=25
            },
            [SicknessKind.Fracture]= new()
            {
                Kind=SicknessKind.Fracture,
                Name="Fracture",
                MinDays=3,
                MaxDays=7,
                DeathChance=0.005,
                Weight=20
            },
            [SicknessKind.Infection]= new()
            {
                Kind=SicknessKind.Infection,
                Name="Infection",
                MinDays=2,
                MaxDays=5,
                DeathChance=0.01,
                Weight=20
            },
            [SicknessKind.Appendicitis]= new()
            {
                Kind=SicknessKind.Appendicitis,
                Name= "Appendicitis",
                MinDays=3,
                MaxDays=6,
                DeathChance=0.01,
                Weight=15
            },
            [SicknessKind.Pneumonia]= new()
            {
                Kind=SicknessKind.Pneumonia,
                Name= "Pneumonia",
                MinDays=5,
                MaxDays=10,
                DeathChance=0.02,
                Weight=10
            },
            [SicknessKind.Surgery]= new()
            {
                Kind=SicknessKind.Surgery,
                Name= "Surgery",
                MinDays=7,
                MaxDays=14,
                DeathChance=0.04,
                Weight=5
            },
            [SicknessKind.HeartAttacks]= new()
            {
                Kind=SicknessKind.HeartAttacks,
                Name= "HeartAttacks",
                MinDays=5,
                MaxDays=12,
                DeathChance=0.08,
                Weight=3
            },
            [SicknessKind.Stroke]= new()
            {
                Kind=SicknessKind.Stroke,
                Name= "Stroke",
                MinDays=7,
                MaxDays=15,
                DeathChance=0.10,
                Weight=2
            },

        };
    }
}
