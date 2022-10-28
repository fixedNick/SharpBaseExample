
StartMain();

static void StartMain()
{
    Console.Write("Enter your name: ");
    var name = Console.ReadLine();
    var response = Message.send((int)MessageRecipients.MR_BROKER, MessageTypes.MT_INIT, name);
    if (response.header.type != MessageTypes.MT_INIT)
    {
        Console.WriteLine("Name exists error!");
        return;
    }

    Console.WriteLine(response.data);

    Thread t = new Thread(ProcessMessages);
    t.Start();

    var m = Message.send((int)MessageRecipients.MR_BROKER, MessageTypes.MT_INIT);
    while (true)
    {
        var s = Console.ReadLine();
        if (s is not null)
        {
            var splitterIndex = s.IndexOf(':');
            var to = s.Substring(0, splitterIndex);
            var msg = s.Substring(splitterIndex + 1, s.Length - splitterIndex - 1);

            int msgTo = -1;

            if (to == "all")
                msgTo = (int)MessageRecipients.MR_ALL;
            else msgTo = Convert.ToInt32(to);

            Message.send(msgTo, MessageTypes.MT_DATA, msg);
        }
    }
}

static void ProcessMessages()
{
    while (true)
    {
        Thread.Sleep(1000);
        var m = Message.send((int)MessageRecipients.MR_BROKER, MessageTypes.MT_GETDATA);
        switch (m.header.type)
        {
            case MessageTypes.MT_DATA:
                Console.WriteLine(m.data);
                break;
            case MessageTypes.MT_NOTUSER:
                Console.WriteLine("User not found ");
                break;
            case MessageTypes.MT_DISCONNECT_USER:
                {
                    Console.WriteLine("Timeout, you was disconnected");
                    Message r = Message.send((int)MessageRecipients.MR_BROKER, MessageTypes.MT_EXIT, "");
                    Message.SUPER_FLAG_IS_DISCONNECTED = true;
                    Thread.Sleep(1000);
                    return;
                }
            default:
                Thread.Sleep(100);
                break;
        }
    }
}