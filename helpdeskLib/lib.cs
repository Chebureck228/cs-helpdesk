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
    public Priority Priority { get; private set; }
    public Status Status { get; set; }

    public void SetPriority(Priority priority)
    {
        Priority = priority;
    }

    internal Ticket(string title, int id, Priority priority)
    {
        Title = title;
        Id = id; 
        Priority = priority;
        Status = Status.Waiting;
    }

    public override string ToString()
    {
        return $"{Title}:{Id}";
    }
}

public class TicketDesk
{
    private int nextId = 0;
    public delegate void TicketHandler(string message);
    public event TicketHandler? TicketCreated;
    public event TicketHandler? TicketTaken;
    public event TicketHandler? TicketClosed;

    private LinkedList<Ticket> tasksList = new();
    private Dictionary<int, Ticket> taskDict = new();
    private Stack<UndoEntry> undoHistory = new();

    public Ticket Create(string title, Priority priority)
    {
        if (title.IsWhiteSpace())
            throw new ArgumentException("Заголовок заявки не может быть пустым");
        nextId++;
        Ticket ticket = new Ticket(title, nextId, priority);
        tasksList.AddLast(ticket);
        taskDict.Add(ticket.Id, ticket);
        undoHistory.Push(new UndoEntry(ticket, DeskAction.Create));
        TicketCreated?.Invoke($"В TicketDesk была создана новая заявка: {ticket}");
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
                break;
            }
            node = node.Previous;
        }
        node = tasksList.Last;
        while (node != null && ticket != null)
        {
            if (node.Value.Status == Status.Waiting 
                && node.Value.Priority > ticket.Priority)
            {
                ticket = node.Value;
                break;
            }
            node = node.Previous;
        }
        if (ticket != null)
        {
            ticket.Status = Status.InProgress;
            undoHistory.Push(new UndoEntry(ticket, DeskAction.Create));
            TicketTaken?.Invoke($"Заявка {ticket} была взята в работу");
            return true;
        }
        return false;
    }

    public bool TryClose(int id)
    {
        Ticket? ticket;
        if (taskDict.TryGetValue(id, out ticket))
        {
            taskDict.Remove(id);
            tasksList.Remove(ticket);
            undoHistory.Push(new UndoEntry(ticket, DeskAction.Close));
            TicketClosed?.Invoke($"Заявка {ticket} была закрыта");
            return true;
        }
        return false;
    }

    public bool TryEscalate(int id)
    {
        Ticket? ticket;
        if (taskDict.TryGetValue(id, out ticket))
        {
            ticket.SetPriority(Priority.High);
            tasksList.Remove(ticket);
            tasksList.AddLast(ticket);
            return true;
        }
        return false;
    }

    public bool TryUndo()
    {
        if (undoHistory.Count != 0)
        {
            UndoEntry? undo = undoHistory.Pop();
            Ticket? ticket = undo.Ticket;
            if (undo.DeskAction == DeskAction.Create)
            {
                tasksList.Remove(ticket);
                taskDict.Remove(ticket.Id);
            }
            else if (undo.DeskAction == DeskAction.Close)
            {
                tasksList.AddLast(ticket);
                taskDict.Add(ticket.Id, ticket);
            }
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