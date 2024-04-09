namespace FinanceAssistant;

public sealed class EmptyExpenseNameException() : Exception("Invalid expense name");
public sealed class InvalidTypeException() : Exception("Invalid expense type");
public sealed class InvalidValueException() : Exception("Invalid expense value");
public sealed class InvalidDateException() : Exception("Invalid expense value");
public sealed class InvalidAccountsException() : Exception("Invalid account file");
public sealed class InvalidExpensesException() : Exception("Invalid expense file");
public sealed class InvalidSelectedAccountException() : Exception("Invalid account");
public sealed class NoAccountSelectedException() : Exception("Theres no account selected");
public sealed class InvalidSpanException() : Exception("Invalid span!");







