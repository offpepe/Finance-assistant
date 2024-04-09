namespace FinanceAssistant;

public class Objects
{
    public readonly record struct CreateUserDto(string Name, string Email);
    public readonly record struct CreateExpenseDto(string Name, DateTime Date, double Value,string? Description);
    public readonly record struct ExpenseDto(int Id, Guid Guid, string Name, string? Description, DateTime Date, double Value);
    

    public class AccountData(int id, Guid guid, string name, string email)
    {
        public int Id { get; set; } = id;
        public Guid Guid { get; set; } = guid;
        public string Name { get; set; } = name;
        public string Email { get; set; } = email;
    };
}