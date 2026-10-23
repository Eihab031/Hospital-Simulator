

namespace HospitalSim.Core.Entities
{
    public class Stats
    {
        // represent Current Session Activity E.x ...(total arrivals,total Deceased,total healed,etc...)

        public int TotalArrivals { get; private set; } // track every arrivals
        public int TotalAdmissions { get; private set; } //track admitted Patients
        public int TotalTransferred { get; private set; } //track arrivals that no Beds for them 
        public int TotalDischarged { get; private set; } // Patients who healed
        public int TotalDeceased { get; private set; }    // Died Patients :(
        public int PeakOccupancy{ get; private set; }     // track peak admissions 

        public void RecordArrival() => TotalArrivals++;
        public void RecordAdmission() => TotalAdmissions++;
        public void RecordTransfer() => TotalTransferred++;
        public void RecordDischarge() => TotalDischarged++;
        public void RecordDecease() => TotalArrivals++;
        public void UpdatePeak(int currentPeak)
        {
            PeakOccupancy = Math.Max(PeakOccupancy, currentPeak);
        }
    }
}
