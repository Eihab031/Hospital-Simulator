using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSim.Core.Entities
{
    public  class Hospital
    {
        public  required IReadOnlyList<Room> Rooms {get; init;}

        public  IReadOnlyList<Patient> Patients {get;internal set; }
        public Queue<Person> People { get; internal set; } = new();
        public DateTime SimulatedNow {get; private set;}
        public Stats Stats {get;internal set; }
        
        public static Hospital CreateDefault() // Create Root Object (Hospital ) && 50 Rooms => 100 Bed
        {
            return new Hospital {
                Rooms = Enumerable.Range(1, 50).Select(n => new Room(n)).ToList()
            };
        }
        public void SeedPeople(IEnumerable<Person> peoplepool)  //Get a People List from Pool(API / DataBase)
        {
            foreach (Person person in peoplepool) 
            {
                People.Enqueue(person);
            }
        }
        public void AddPatient(Patient _patient)
        {
            if (_patient is null)
                throw new InvalidOperationException("patient not a valid value");

            List<Patient> patients = new List<Patient>();
            patients.Add(_patient);
            Patients = patients;

            // Assign Patient to a free Bed
            var freeBed=GetFreeBeds().FirstOrDefault();
            freeBed?.AssignPatient(_patient);

        }
        public void AdvanceTime(TimeSpan delta) => SimulatedNow += delta; // Advance Time
       
        public IEnumerable<Bed> GetFreeBeds()
        {
           return Rooms.SelectMany(room=>room.Beds).Where(b => b.IsFree);
        }
        public void ReleasePatient(Patient _patient)
        {
            _patient.CompleteStay(SimulatedNow);
            _patient.Bed.ReleasePatientt();
        }
    }
}
