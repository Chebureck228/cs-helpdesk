namespace helpdeskLib;

public enum Priority
{
    Normal, 
    High
}

public enum Status
{
    Waiting,
    InProgress,
    Done
}

public enum DeskAction
{
    Create, 
    Close
}

public class Ticket
{
    public string Title { get; }
    public int Id { get; }
    public Priority Priority { get; }
    public Status Status { get; set; }

    internal Ticket(string title, int id, Priority priority)
    {
        Title = title;
        Id = id; 
        Priority = priority;
        Status = Status.Waiting;
    }
}

public class TicketDesk
{
    private int nextId = 0;
    private LinkedList<Ticket> tasksList = new();
    private Dictionary<int, Ticket> taskDict = new();
    private Stack<UndoEntry> undoHistory = new();
    private Stack<Ticket> closedHistory = new();

    public Ticket Create(string title, Priority priority)
    {
        if (title.IsWhiteSpace())
            throw new ArgumentException("Заголовок заявки не может быть пустым");
        nextId++;
        Ticket ticket = new Ticket(title, nextId, priority);
        tasksList.AddLast(ticket);
        taskDict.Add(ticket.Id, ticket);
        undoHistory.Push(new UndoEntry(ticket, DeskAction.Create));
        return ticket;
    }

    public bool TryFind(int id, out Ticket? ticket)
    {
        return taskDict.TryGetValue(id, out ticket);
    }

    public bool TryTakeNext(out Ticket? ticket)
    {
        ticket = null;
        LinkedListNode<Ticket>? node = tasksList.Last;
        while (node != null)
        {
            if (node.Value.Status == Status.Waiting)
            {
                ticket = node.Value;
                ticket.Status = Status.InProgress;
                break;
            }
            node = node.Previous;
        }
        if (ticket != null)
            return true;
        return false;
    }

    public bool TryEscalate(int id)
    {
        Ticket? ticket;
        taskDict.TryGetValue(id, out ticket);
        if (ticket != null)
        {
            tasksList.Remove(ticket);
            tasksList.AddLast(ticket);
            return true;
        }
        return false;
    }
}

public record UndoEntry
{
    public Ticket Ticket { get; }
    public DeskAction DeskAction { get; }

    public UndoEntry(Ticket ticket, DeskAction deskAction)
    {
        Ticket = ticket;
        DeskAction = deskAction;
    }
}