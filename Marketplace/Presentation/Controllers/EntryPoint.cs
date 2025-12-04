using System;
using System.Threading.Tasks;
using Shared;

namespace Marketplace.Presentation.Controllers;

public static class EntryPoint
{
    public static async Task RunAsync(string queueName = "myqueue")
    {
        Console.WriteLine($"Starting consumer for {queueName}");

        await using var consumer = new Consumer(queueName, message =>
            //do something
            Console.WriteLine(message)
        );
    }


    private static bool ValidateMessage(string message)
    {
        // Implement validation logic here
        return !string.IsNullOrEmpty(message);
    }
}
