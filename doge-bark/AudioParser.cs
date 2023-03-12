using NAudio.Wave;

namespace doge_bark
{
    public class AudioParser
    {
        private readonly int threshold;
        private readonly int clipLength;
        private readonly string outputPath;

        private WaveFileWriter? writer = null;
        private bool stopRecording = false; // flag for stopping recording
        private bool recordingInProgress = false; // flag to indicate recording state

        public AudioParser(string outputPath, int threshold, int clipLength)
        {
            this.outputPath = Path.Combine(outputPath, "Clips");
            this.threshold = threshold;
            this.clipLength = clipLength;
        }

        /// <summary>
        /// Main loop for the program
        /// </summary>
        public void StartRecording()
        {
            // create the output path to the Clips
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            WaveInEvent waveIn = new()
            {
                WaveFormat = new WaveFormat(44100, 16, 1)
            };

            waveIn.DataAvailable += WaveInDataAvailable;
            waveIn.StartRecording();

            Console.WriteLine("Recording... Press any key to stop.");
            Console.ReadKey();

            // set the flag to stop recording
            stopRecording = true;

            waveIn.StopRecording();
            waveIn.Dispose();
        }

        /// <summary>
        /// EventHandler to check if we exceed the threshold and then write clipLength durations to a clip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveInDataAvailable(object sender, WaveInEventArgs e)
        {
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

            // check if the RMS value exceeds the threshold
            if (recordingInProgress)
            {
                WriteAudio(e);
            }
            else
            {
                // calculate the RMS value of the audio data
                double rms = 0;
                for (int i = 0; i < e.BytesRecorded; i += 2)
                {
                    short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
                    rms += Math.Pow(sample, 2);
                }
                rms = Math.Sqrt(rms / (e.BytesRecorded / 2));

                // check if the RMS value exceeds the threshold
                if (rms > threshold)
                {
                    recordingInProgress = true;
                    Console.WriteLine($"Threshold exceeded. Saving a clip..");
                    if (writer == null)
                    {
                        string filename = Path.Combine(outputPath, $"clip{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.wav");
                        writer = new WaveFileWriter(filename, new WaveFormat(44100, 16, 1));
                    }
                    WriteAudio(e);
                }
            }
        }

        /// <summary>
        /// Write event buffer to audio and check if we have exceeded the clipLength
        /// </summary>
        /// <param name="e">Audio event args</param>
        public void WriteAudio(WaveInEventArgs e)
        {
            // write the audio data to the file
            writer.Write(e.Buffer, 0, e.BytesRecorded);

            // check if the recording has exceeded the clip length
            if (writer.Length > clipLength * writer.WaveFormat.AverageBytesPerSecond)
            {
                // close the file and reset the writer
                writer.Dispose();
                writer = null;
                recordingInProgress = false;
            }
        }
    }
}
