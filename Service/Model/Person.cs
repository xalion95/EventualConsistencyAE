namespace Service.Model
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Person Copy()
        {
            return new Person
            {
                Id = Id,
                Name = Name
            };
        }
    }
}