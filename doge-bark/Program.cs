using doge_bark;

// Starting animation - skip with any key
Console.Write("Welcome to doge-bark");
for (int i = 0; i < 10; i++)
{
    if (Console.KeyAvailable)
    {
        Console.ReadKey(true);
        break;
    }
    Console.Write(".");
    Thread.Sleep(300);

    if ((i + 1) % 3 == 0)
    {
        Console.Write("\b\b\b   \b\b\b");
        Thread.Sleep(300);
    }
}
Console.WriteLine();

// Options for the user
int threshold = 3000;
int clipLength = 25;
int selection = 0;
while (selection != 1)
{
    Console.WriteLine("Please select an option:");
    Console.WriteLine("1 - Run the program");
    Console.WriteLine("2 - Set Options");
    Console.WriteLine($"Current settings: clip length {clipLength} second | threshold {threshold} RMS");
    ConsoleKeyInfo userInput = Console.ReadKey();

    if (int.TryParse(userInput.KeyChar.ToString(), out selection))
    {
        Console.WriteLine();
        switch (selection)
        {
            case 1:
                Console.WriteLine("Give output folder:");
                var outputPath = Console.ReadLine();
                // If output path is not given use the directory of .exe
                outputPath ??= Directory.GetCurrentDirectory();

                AudioParser audioParser = new(outputPath, threshold, clipLength);
                Console.Clear();
                audioParser.StartRecording();
                break;
            case 2:
                // Read clipLength from user
                Console.WriteLine("Give clip length in seconds:");
                string userClipLengthInput = Console.ReadLine();
                if(int.TryParse(userClipLengthInput, out int userClipLength))
                {
                    clipLength = userClipLength;
                }

                // Read threshold from user
                Console.WriteLine("Give threshold in RMS <1000 is normal speaking voice>:");
                string userThresholdInput = Console.ReadLine();
                if(int.TryParse(userThresholdInput, out int userThreshold))
                {
                    threshold = userThreshold;
                }
                Console.Clear();
                break;
            default:
                Console.WriteLine("Invalid option");
                break;
        }
    }
    else Console.WriteLine("Invalid option");
}

// Closing animation
Console.Clear();
Console.Write("Program shutting down");
for (int i = 0; i < 10; i++)
{
    if (Console.KeyAvailable)
    {
        Console.ReadKey(true);
        break;
    }
    Console.Write(".");
    Thread.Sleep(300);

    if ((i + 1) % 3 == 0)
    {
        Console.Write("\b\b\b   \b\b\b");
        Thread.Sleep(300);
    }
}