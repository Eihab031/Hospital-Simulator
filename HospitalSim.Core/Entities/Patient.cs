using HospitalSim.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSim.Core.Entities
{
    public class Patient
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required Person Person { get; init; }
        public required Sickness Sickness { get; init; }
        public  Bed Bed { get; init; }
        public  DateTime AdmittedAt {get;init;}
        public required DateTime ExpDischargeAt{ get; init; }
        public DateTime? DischargedAt { get;private set;}
        public PatientStatus PatientStatus { get; private set; } = PatientStatus.Active;
        internal  bool WillDie { get; init; }
        public double DaysStayed => ((DischargedAt??DateTime.UtcNow)-AdmittedAt).TotalDays;

        // Release Patient from Hospital => healed :) or Died :(
        public void CompleteStay(DateTime at) // simulated Time 
        {
            PatientStatus = WillDie ?  PatientStatus.Deceased:PatientStatus.Discharged;
            DischargedAt= at; 
        }
    }
}
