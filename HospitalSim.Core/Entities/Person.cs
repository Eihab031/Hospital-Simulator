using HospitalSim.Core.Enum;

namespace HospitalSim.Core.Entities
{
    public class Person
    {
        public Guid Id { get; init; }= Guid.NewGuid();
        public required string FirstName { get;init;}
        public required string LastName { get; init; }
        public required Gender Gender { get; init; }
        public required string Email {  get; init; }
        public required DateTime DateOfBirth { get; init; }
        public required string Address { get; init; }
        public string? PhotoUrl { get; init; }

        public  string FullName => $"{FirstName} {LastName}";
    }
}
