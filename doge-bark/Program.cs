using NAudio.Wave;

Console.WriteLine("Give output folder:");
var outputPath = Console.ReadLine();

AudioParser audioParser = new();
audioParser.StartRecording(outputPath);

public class AudioParser
{
    private readonly int threshold;
    private readonly int clipLength;
    private readonly string outputPath;

    private WaveFileWriter writer = null;
    private List<short> audioBuffer = new();
    private bool stopRecording = false; // flag for stopping recording

    public AudioParser() { }
    public AudioParser(string outputPath, int threshold, int clipLength)
    {
        this.threshold = threshold;
        this.clipLength = clipLength;
        this.outputPath = outputPath;
    }

    private void WaveInDataAvailable(object sender, WaveInEventArgs e)
    {
        string outputPath = Path.Combine(this.outputPath, "Clips");
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // check if recording should stop
        if (stopRecording)
        {
            // close the file and reset the writer
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
            return;
        }

        // calculate the RMS value of the audio data
        double rms = 0;
        for (int i = 0; i < e.BytesRecorded; i += 2)
        {
            short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
            rms += Math.Pow(sample, 2);
        }
        rms = Math.Sqrt(rms / (e.BytesRecorded / 2));

        // add the audio data to the buffer
        for (int i = 0; i < e.BytesRecorded; i += 2)
        {
            short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
            audioBuffer.Add(sample);
        }

        // check if the RMS value exceeds the threshold
        if (rms > threshold)
        {
            // start a new recording if necessary
            if (writer == null)
            {
                string filename = Path.Combine(outputPath, $"clip{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.wav");
                writer = new WaveFileWriter(filename, new WaveFormat(44100, 16, 1));
            }

            // write the audio data to the file
            writer.Write(e.Buffer, 0, e.BytesRecorded);

            // check if the recording has exceeded the clip length
            if (writer.Length > clipLength * writer.WaveFormat.AverageBytesPerSecond)
            {
                // close the file and reset the writer
                writer.Dispose();
                writer = null;
            }
        }
    }

    public void StartRecording(string outputPath, int threshold = 10000, int clipLength = 25)
    {
        WaveInEvent waveIn = new()
        {
            WaveFormat = new WaveFormat(44100, 16, 1)
        };

        waveIn.DataAvailable += new EventHandler<WaveInEventArgs>((sender, e) =>
        {
            AudioParser audioParser = new AudioParser(outputPath, threshold, clipLength);
            audioParser.WaveInDataAvailable(sender, e);
        });
        waveIn.StartRecording();

        Console.WriteLine("Recording... Press any key to stop.");
        Console.ReadKey();

        // set the flag to stop recording
        stopRecording = true;

        waveIn.StopRecording();
        waveIn.Dispose();
    }
}
