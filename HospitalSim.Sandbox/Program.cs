using HospitalSim.Core.Catalog;
using HospitalSim.Core.Entities;
using HospitalSim.Core.Enum;
using System.Runtime.CompilerServices;

namespace HospitalSim.Sandbox
{
    static class Program
    {
        public static void Main(string[] args)
        {
            var person1 = new Person
            {
                FirstName = "Mike",
                LastName = "Musk",
                DateOfBirth = new DateTime(1996, 12, 25),
                Address = "Hamburg blabla",
                Email = "test1@gmail.com",
                Gender = Gender.male
            };
            var person2 = new Person
            {
                FirstName = "steve",
                LastName = "Musk",
                DateOfBirth = new DateTime(1991, 12, 25),
                Address = "Hamburg blabla",
                Email = "test1@gmail.com",
                Gender = Gender.male
            };
            var person3 = new Person
            {
                FirstName = "catalina",
                LastName = "joah",
                DateOfBirth = new DateTime(1976, 12, 25),
                Address = "Hannover blabla",
                Email = "test1@gmail.com",
                Gender = Gender.female
            };
            List<Person> people= new() { person1, person2, person3 };

            var hospital = Hospital.CreateDefault();
            hospital.SeedPeople(people);
            var bed = hospital.GetFreeBeds().First();
            var patient = new Patient
            {
                Person = person1,
                Bed = bed,
                Sickness = SicknessCatalog.Get(SicknessKind.HeartAttacks),
                AdmittedAt = new DateTime(2026, 10, 23),
                ExpDischargeAt = new DateTime(2026, 10, 30)
            };
            hospital.AddPatient(patient);
            hospital.ReleasePatient(patient);

            Console.WriteLine($"State :  Patient name: {patient.Person.FullName}   patient Dichargedate: {patient.DischargedAt}\n bed and Room number: {patient.Bed.Number} ({patient.Bed.Room.Number})\n sickness status: {patient.Sickness.Kind} Patient Status: {patient.PatientStatus}\n the Bed now is free {bed.IsFree}  ");
        }
    }
}