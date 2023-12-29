using ML.Samples.Utils;

string simplePath = @"D:\Cloud\OneDrive\codes\ML learning\simple";


//await SampleTable.GenerateFileAsync(simplePath, 30000);




string audioFilesPath = @"D:\Cloud\OneDrive\codes\ML learning\smarthome audios\v2";

string audioPath = @"D:\Cloud\OneDrive\codes\ML learning\smarthome audios";

string destcsvFile = Path.Combine(audioPath, $"{Guid.NewGuid():N}.csv");

await AudioParseHelper.WriteFilesToCsvAsync(audioFilesPath, destcsvFile);

