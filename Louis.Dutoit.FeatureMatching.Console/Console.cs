using System.Text.Json;
using Louis.Dutoit.FeatureMatching;

var data = new List<byte[]>();

foreach (var imagePath in Directory.EnumerateFiles(args[1]))
{
        var imageBytes = await File.ReadAllBytesAsync(imagePath);
        data.Add(imageBytes);
}

var imageData = await File.ReadAllBytesAsync(args[0]);
var detectObjectInScenesResults = new ObjectDetection().DetectObjectInScenes(imageData, data);

foreach (var objectDetectionResult in detectObjectInScenesResults)
{
        System.Console.WriteLine($"Points:{JsonSerializer.Serialize(objectDetectionResult.Points)}");
}       