
namespace HospitalSim.Core.Entities
{
    public class Bed
    {
        public Guid Id { get; init; }= Guid.NewGuid();
        private int number;
        public required int Number
        {
            get => number; init
            {
                if (value is not (1 or 2))
                {
                    throw new ArgumentException("Bed number can only be 1 or 2");
                }
                number = value;
            }
        }
        public required Room Room { get; init; }
        public Patient? CurrentPatient { get; private set; }=  null;
        public bool IsFree  => CurrentPatient is null;

        public void AssignPatient(Patient patient)
        {
            if (patient is null) throw new InvalidOperationException("Patient not found or invalid value");
            if(this.IsFree) { 
                CurrentPatient = patient;
                return;
            }
            throw new InvalidOperationException("Bed is already occupied...!");
        }
        public void ReleasePatientt()=> CurrentPatient=null;
       
    }
   
}
