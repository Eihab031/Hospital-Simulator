
using System;
using System.Collections.ObjectModel;

namespace HospitalSim.Core.Entities
{
    public enum Status
    {
        full,
        HalfEmpty,
        Empty
    }
    public class Room
    {
        
        private int number;
        public  int Number { get=>number; init {
                if (value <=0)
                {
                    throw new ArgumentException("Room number can't be negative or Zero");
                }
                number=value;
            } }
        public  IReadOnlyList<Bed> Beds { get; }
        public Status Status => GetStatus();

        public Room(int num)
        {
            var bed1 = new Bed() { Number=1,Room=this};
            var bed2 = new Bed()  { Number = 2, Room = this } ;
            Beds = [bed1, bed2];
            if (num <= 0)
            {
                throw new ArgumentException("Room number can't be negative or Zero");
            }
            number = num;
        }
        private Status GetStatus()
        {
            int busyBeds = Beds.Count(bed => bed.CurrentPatient is not null);
            return busyBeds switch
            {
                0 => Status.full,
                1 => Status.Empty,
                2 => Status.HalfEmpty,
                _ => throw new InvalidOperationException($" invalid Occupeny number in room {this.Number}")
            };
        }
    }
}
