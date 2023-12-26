using NAudio.Wave;

const int Channels = 4;
const int Rate = 44100;
const int Chunk = 1024;
const int Duration = 20;  // Duration in seconds


Console.WriteLine("Hello API world");



//IEnumerable<byte[]> AudioStreamGenerator(WaveInEvent waveIn, int duration)
//{
//    var endTime = DateTime.Now.AddSeconds(duration);
//    while (DateTime.Now <= endTime)
//    {
//        var buffer = new byte[Chunk];
//        waveIn.BufferMilliseconds = (int)((double)Chunk / (Rate * Channels) * 1000);
//        waveIn.Read(buffer, 0, buffer.Length);
//        yield return buffer;
//    }
//}

//IEnumerable<byte[]> KeepStreaming(WaveInEvent waveIn)
//{
//    while (true)
//    {
//        var buffer = new byte[Chunk];
//        waveIn.BufferMilliseconds = (int)((double)Chunk / (Rate * Channels) * 1000);
//        waveIn.Read(buffer, 0, buffer.Length);
//        yield return buffer;
//    }
//}
