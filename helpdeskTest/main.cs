using static System.Console;
using helpdeskLib;

class Program
{
    static int _total = 0;
    static int _passed = 0;

    static void Main(string[] args)
    {
        TicketDesk desk = new();

        Check("Попытка TryTakeNext в пустом списке", false, desk.TryTakeNext(out _));
        desk.Create("something1", Priority.High);
        desk.Create("something2", Priority.Normal);

        Ticket ticket1 = desk.Create("something3", Priority.High);
        Ticket ticket2 = desk.Create("something4", Priority.Normal);
        Ticket ticket3 = desk.Create("something5", Priority.High);
    
        Check("Id второй = первый + 1", ticket1.Id + 1, ticket2.Id);
        Check($"Status после создания: Waiting", (int)Status.Waiting, (int)ticket3.Status);
        Check($"TryFind по существующему Id -> true", true, desk.TryFind(ticket3.Id, out Ticket? found));
        Check("Та же самая заявка через TryFind(ticket3.Id, out Ticket? found): ", true, ticket3.Equals(found));
        TicketDesk emptyDesk = new TicketDesk();
        Check($"TryFind по несуществующему Id -> false", false, emptyDesk.TryFind(1, out Ticket? _));

        Throws<ArgumentException>("Неудачный Create(\"\")", () => desk.Create("", Priority.Normal));
        Ticket ticket6 = desk.Create("something6", Priority.Normal);
        Check($"Новая заявка имеет номер идущий сразу за предыдущим (5): ", 
            ticket3.Id + 1, ticket6.Id);
    }

    static void Check<T>(string name, T expected, T actual)
        where T : IEquatable<T>
    {
        _total++;
        if (!expected.Equals(actual))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine($"FAIL: expected {expected}, got {actual}");
            WriteLine($"{name}\n");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine($"PASS: expected {expected}, got {actual}");
            WriteLine($"{name}\n");
            Console.ResetColor();
            _passed++;
        }
    }

    static void Throws<TException>(string name, Action action)
        where TException : Exception
    {
        _total++;
        try
        {
            action();
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine($"FAIL: expected exception, but it wasn't");
            WriteLine($"{name}\n");
            Console.ResetColor();
        }
        catch (TException ex)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine($"Exception {ex.GetType().Name} is PASS");
            WriteLine($"{name}\n");
            Console.ResetColor();
            _passed++;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine($"FAIL: expected {typeof(TException).Name}, got {ex.GetType().Name}");
            WriteLine($"{name}\n");
            Console.ResetColor();
        }
    }
}

