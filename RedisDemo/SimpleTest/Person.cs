namespace RedisDemo.SimpleTest
{
    class Person
    {
        public int Id;
        public string FirstName;
        public string LastName;
        public string Address;
        public string Phone;

        public Person()
        {

        }

        public Person(int id, string firstName, string lastName, string address, string phone)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Address = address;
            Phone = phone;
        }
    }
}
