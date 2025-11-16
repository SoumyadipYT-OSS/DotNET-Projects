

namespace BankingApp
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("==== Bank Customer Details ====");


            var customer1 = new BankCustomer("Nidhi", "Agarwal");
            Console.WriteLine(customer1.ToString());

            var customer2 = new BankCustomer("Sharmila", "Thakur");
            Console.WriteLine(customer2);

            var customer3 = new BankCustomer("Aniket", "Rout");
            Console.WriteLine(customer3);



            var account1 = new BankAccount(customer1.CustomerId);
            var account2 = new BankAccount(customer2.CustomerId);
            var account3 = new BankAccount(customer3.CustomerId);

            Console.WriteLine($"Account 1: Account # {account1.AccountNumber}, type {account1.AccountType}, balance {account1.Balance}, customer ID {account1.CustomerId}");
            Console.WriteLine($"Account 2: Account # {account2.AccountNumber}, type {account2.AccountType}, balance {account2.Balance}, customer ID {account2.CustomerId}");
            Console.WriteLine($"Account 3: Account # {account3.AccountNumber}, type {account3.AccountType}, balance {account3.Balance}, customer ID {account3.CustomerId}");

        }
    }
}