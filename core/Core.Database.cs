using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace FinanceAssistant.core;

public partial class Core(string basePath) : IDisposable
{
    private Objects.AccountData? _workingAccount;
    private Objects.ExpenseDto[] _workingAccountExpenses = [];
    private Objects.AccountData[] _accounts = [];
    private string _workingSpan = DateTime.Now.ToString("yyyy_MM");
    private string _expensesPath = string.Empty;
    private readonly string _accountPath = Path.Join(basePath, "accounts.json");
    private int _pos = -1;
    private readonly string _statePath = Path.Join(basePath, "state.bat");

    public async Task Init()
    {
        CreateDirectoryIfNotExists();
        await ReadAccounts(_accountPath);
        await ReadState();
    }

    private async Task ReadState()
    {
        if (!File.Exists(_statePath))
        {
            File.Create(_statePath);
            return;
        }

        var buffer = await File.ReadAllBytesAsync(_statePath);
        if (buffer.Length == 0) return;
        _pos = BitConverter.ToInt32(buffer);
        if (buffer.Length > 4)
        {
            _workingSpan = Encoding.UTF8.GetString(buffer[4..]);
        } 
        SelectAccount(_pos);
    }

    public void CreateAccount(string nome, string email)
    {
        var accSize = _accounts.Length;
        Array.Resize(ref _accounts, accSize + 1);
        _accounts[accSize] = new Objects.AccountData(accSize, Guid.NewGuid(), nome, email);
        File.WriteAllText(_accountPath,
            JsonSerializer.Serialize(_accounts, AppJsonSerializerContext.Default.AccountDataArray));
        Console.WriteLine("Conta criada com sucesso");
    }

    public void ListAccounts()
    {
        var totalAccounts = _accounts.Length;
        if (totalAccounts == 0)
        {
            Console.WriteLine("Não existem contas registradas");
            return;
        }

        Unsafe.SkipInit<Objects.AccountData>(out var account);
        for (var i = totalAccounts - 1; i >= 0; i--)
        {
            account = _accounts[i];
            Console.WriteLine("[{0}] {1} - {2}", i, account.Name, account.Email);
        }
    }

    public void SelectAccount(int pos)
    {
        if (pos < 0 || pos >= _accounts.Length) throw new InvalidSelectedAccountException();
        _workingAccount = _accounts[pos];
        _pos = pos;
        _expensesPath = Path.Join(basePath, $"{_workingAccount.Guid:N}_expenses_{_workingSpan}.json");
        if (!File.Exists(_expensesPath))
        {
            return;
        }

        var content = File.ReadAllText(_expensesPath);
        if (content.Length == 0) return;
        _workingAccountExpenses = JsonSerializer.Deserialize(content,
            AppJsonSerializerContext.Default.ExpenseDtoArray) ?? throw new InvalidExpensesException();
    }

    public void ListExpenses()
    {
        if (_workingAccount == null) throw new NoAccountSelectedException();
        if (_workingAccountExpenses.Length == 0)
        {
            Console.WriteLine("Nenhum gasto registrado");
            return;
        }
        Unsafe.SkipInit<Objects.ExpenseDto>(out var actualExpense);
        for (var i = _workingAccountExpenses.Length - 1; i >= 0; i--)
        {
            actualExpense = _workingAccountExpenses[i];
            Console.WriteLine("[{0}] {1} - {2}: R$ {3} {4}", actualExpense.Guid, DateOnly.FromDateTime(actualExpense.Date),
                actualExpense.Name, actualExpense.Value, actualExpense.Description ?? string.Empty);
        }
    }

    public (string, string) GetAccountDetails()
    {
        if (_workingAccount == null) throw new NoAccountSelectedException();
        return (_workingAccount.Name, _workingAccount.Email);
    }

    public void AddExpense(string name, double value, int day, string? description)
    {
        var date = DateTime.ParseExact($"{_workingSpan}_{day}", "yyyy_MM_dd", CultureInfo.InvariantCulture);
        var pos = _workingAccountExpenses.Length;
        Array.Resize(ref _workingAccountExpenses, pos + 1);
        _workingAccountExpenses[pos] = new Objects.ExpenseDto(pos + 1, Guid.NewGuid(), name, description, date, value);
        File.WriteAllText(_expensesPath,
            JsonSerializer.Serialize(_workingAccountExpenses, AppJsonSerializerContext.Default.ExpenseDtoArray));
    }

    public void SwitchWorkingExpenseSpan(int year, int month)
    {
        if (_workingAccount == null) throw new NoAccountSelectedException();
        if (!DateTime.TryParse($"1/{month}/{year}", out _)) throw new InvalidSpanException();
        _workingSpan = month >= 10 ? $"{year}_{month}" : $"{year}_0{month}";
        _expensesPath = Path.Join(basePath, $"{_workingAccount.Guid:N}_expenses_{_workingSpan}.json");
        if (!File.Exists(_expensesPath))
        {
            File.Create(_expensesPath);
            _workingAccountExpenses = [];
            return;
        }

        var content = File.ReadAllText(_expensesPath);
        if (content.Length == 0) return;
        _workingAccountExpenses =
            JsonSerializer.Deserialize(content,
                AppJsonSerializerContext.Default.ExpenseDtoArray) ?? throw new InvalidExpensesException();
    }

    private async Task ReadAccounts(string path)
    {
        if (!File.Exists(path))
        {
            File.Create(path);
            return;
        }

        var content = await File.ReadAllTextAsync(path);
        if (string.IsNullOrEmpty(content)) return;
        _accounts = JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.AccountDataArray) ??
                    throw new InvalidAccountsException();
    }

    private void CreateDirectoryIfNotExists()
    {
        if (Directory.Exists(basePath)) return;
        Directory.CreateDirectory(basePath);
    }

    public void Dispose()
    {
        if (_pos < 0) return;
        var buffer = new byte[11];
        var posBuffer = BitConverter.GetBytes(_pos);
        var spanBuffer = Encoding.UTF8.GetBytes(_workingSpan);
        buffer[0] = posBuffer[0]; buffer[1] = posBuffer[1]; buffer[2] = posBuffer[2]; buffer[3] = posBuffer[3];
        buffer[4] = spanBuffer[0]; buffer[5] = spanBuffer[1]; buffer[6] = spanBuffer[2]; buffer[7] = spanBuffer[3];
        buffer[8] = spanBuffer[4]; buffer[9] = spanBuffer[5]; buffer[10] = spanBuffer[6];
        File.WriteAllBytes(_statePath, buffer);
    }
}