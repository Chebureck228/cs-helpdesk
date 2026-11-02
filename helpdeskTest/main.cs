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

        TicketDesk deskWithoutWaiting = new TicketDesk();
        deskWithoutWaiting.Create("notWaiting1", Priority.High);
        deskWithoutWaiting.Create("notWaiting2", Priority.Normal);
        deskWithoutWaiting.Create("notWaiting3", Priority.Normal);
        
        Check("Пустой TicketDesk.TryTakeNext() == false", false, emptyDesk.TryTakeNext(out Ticket? _));

        Check("TicketDesk с объектом Status.Waiting", true, desk.TryTakeNext(out Ticket? _));
        deskWithoutWaiting.TryTakeNext(out Ticket? tick);
        deskWithoutWaiting.TryTakeNext(out Ticket? _);
        deskWithoutWaiting.TryTakeNext(out Ticket? _);
        Check("TicketDesk без объекта Status.Waiting", false, deskWithoutWaiting.TryTakeNext(out Ticket? _));

        TicketDesk deskFourEl = new TicketDesk();
        deskFourEl.Create("NormTicket1", Priority.Normal);
        deskFourEl.Create("NormTicket2", Priority.Normal);
        deskFourEl.Create("HighTicket2", Priority.High);
        deskFourEl.Create("NormTicket2", Priority.Normal);

        deskFourEl.TryTakeNext(out Ticket? IsTickHigh);
        deskFourEl.TryFind(3, out Ticket? tickHigh);
        int IdIsTickHigh = IsTickHigh.Id;
        int IdTickHigh = tickHigh.Id;
        Check("Попытка взять элемент с более высоким приоритетом из deskFourEl", IdIsTickHigh, IdTickHigh);

        TicketDesk deskTwoEl = new TicketDesk();
        deskTwoEl.Create("NormTick", Priority.Normal);
        deskTwoEl.Create("HighTick", Priority.High);

        deskTwoEl.TryTakeNext(out Ticket? highTick);
        deskTwoEl.TryFind(1, out Ticket? tickId1);
        Check("TryTakeNext в desk с двумя элементами. Ticket с Id(1)"
            +" имеет статус Waiting", true, Status.Waiting.Equals(tickId1?.Status));
        deskTwoEl.TryTakeNext(out Ticket? normTick);
        Check("TryTakeNext в desk с заявкой high в"
            +" работе\nи заявкой normal в ожидании."
            +" Берется normal в ожидании", normTick.Id, tickId1.Id);

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

