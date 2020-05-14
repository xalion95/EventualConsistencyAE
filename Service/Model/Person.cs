namespace Service.Model
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Status Status { get; set; } = Status.NEW;

        public Person Copy()
        {
            return new Person
            {
                Id = Id,
                Name = Name,
                Status = Status
            };
        }
    }
}