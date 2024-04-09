using System.Text.Json.Serialization;
using FinanceAssistant.core;

namespace FinanceAssistant;

public partial class Program
{
    private static string BASEPATH = Path.Join(Environment.GetEnvironmentVariable("HOME") ?? "~", "perut", "data");
    public static void Main(string[] args)
    {
        if (!args.Any())
        {
            Console.WriteLine(UNRECONIZED_OPTION_INSTRUCTION);
            return;
        }
        var db = new Core(BASEPATH);
        db.Init().Wait();
        try
        {
            Menu(ref args, db);
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("Argumentos insuficientes");
        }
        finally
        {
            db.Dispose();
        }
    }

    private static void Menu(ref string[] args, Core db)
    {
        switch (args[0])
        {
            case HELP_ARG:
                Console.WriteLine(HELP_INSTRUCTION);
                break;
            case ACCOUNTS_ARG:
                AccountOptionHandler(ref args, db);
                break;
            case EXPENSES_ARG:
                ExpenseOptionHandler(ref args, db);
                break;
            case REPORTS_ARG:
                ExpenseOptionHandler(ref args, db);
                break;
            default:
                Console.WriteLine(UNRECONIZED_OPTION_INSTRUCTION);
                break;
        }
    }

    private static void AccountOptionHandler(ref string[] args, Core db)
    {
        switch (args[1])
        {
            case "-l":
                db.ListAccounts();
                break;
            case "--s":
                if (string.IsNullOrEmpty(args[2])) throw new ArgumentException("id");
                if (!int.TryParse(args[2], out var pos)) throw new InvalidSelectedAccountException();
                db.SelectAccount(pos);
                Console.WriteLine("Conta selecionada com sucesso");
                break;
            case "--c":
                if (args[1..].Any(string.IsNullOrEmpty)) throw new ArgumentException();
                db.CreateAccount(args[2], args[3]);
                break;
            default:
                Console.WriteLine(UNRECONIZED_OPTION_INSTRUCTION);
                break;
        }
    }

    private static void ExpenseOptionHandler(ref string[] args, Core db)
    {
        switch (args[1])
        {
            case "-l":
                db.ListExpenses();
                break;
            case "-add":
                if (!double.TryParse(args[3], out var value))
                {
                    Console.WriteLine("Valor inválido.");
                    break;
                }
                if (!int.TryParse((args[4]), out var day))
                {
                    Console.WriteLine("Dia inválido.");
                    break;
                }
                string? description = null;
                if (args.Length > 5)
                {
                    description = args[5];
                }  
                db.AddExpense(args[2], value, day, description);
                Console.WriteLine("Despeza adicionada com sucesso.");
                break;
            case "-swt":
                if (args[2] == "present")
                {
                    var (y, m, _) = DateTime.Now;
                    db.SwitchWorkingExpenseSpan(y,m);
                    Console.WriteLine("Período de trabalho alterado para o presente");
                    break;
                }
                if (!int.TryParse(args[2], out var year))
                {
                    Console.WriteLine("Ano inválido");
                    break;
                }
                if (!int.TryParse(args[3], out var month))
                {
                    Console.WriteLine("Mes inválido");
                    break;
                }
                db.SwitchWorkingExpenseSpan(year, month);
                Console.WriteLine("Período de trabalho alterado para {0}/{1}", month, year);
                break;
            default:
                Console.WriteLine(UNRECONIZED_OPTION_INSTRUCTION);
                break;
        }
    }

    private static void ReportOptionHandler(ref string[] args, Core db)
    {
    }
}

public partial class Program
{
    private const string HELP_ARG = "help";
    private const string ACCOUNTS_ARG = "acc";
    private const string EXPENSES_ARG = "eps";
    private const string REPORTS_ARG = "rp";

    private const string UNRECONIZED_OPTION_INSTRUCTION = @"Opção inválida, caso esteja com dúvida utilize --help";

    private const string HELP_INSTRUCTION = @"
BEM VINDO AO ASSISTENTE FINANCEIRO DO ALAN
você provavelmente não pode usar isto.

Comandos disponívels:

Conta:
    acc -l: Lista contas disponíveis.
    acc --s $id: Selecina a conta.
    acc --c $nome $email: cria uma nova conta.

Gastos:
    eps -l: Lista todos os gastos.
    eps -add $nome:string $valor:double $data:date $description:string|null:
        cria um novo gasto.
    eps -swt $ano $mes: altera a base de gastos.

Relatorios:
    rp -g: Gera relatório de gastos do mês
    rp --all: Gera relatório de gasto de todos os meses
";
}

[JsonSerializable(typeof(Objects.ExpenseDto))]
[JsonSerializable(typeof(Objects.ExpenseDto[]))]
[JsonSerializable(typeof(Objects.AccountData))]
[JsonSerializable(typeof(Objects.AccountData[]))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
    //empty
}