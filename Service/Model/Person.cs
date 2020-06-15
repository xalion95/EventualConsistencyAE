namespace Service.Model
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsRemoved { get; set; }

        public Person Copy()
        {
            return new Person
            {
                Id = Id,
                Name = Name
            };
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(GetType() == obj.GetType())) return false;

            var o = (Person) obj;

            return o.Id == Id || o.IsRemoved == IsRemoved;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsRemoved.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Id} - {Name}";
        }
    }
}