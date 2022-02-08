using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Louis.Dutoit.FeatureMatching.Tests;

public class FeatureMatchingUnitTest
{
    [Fact]
    public async Task ObjectShouldBeDetectedCorrectly()
    {
        var executingPath = GetExecutingPath();
        var imageScenesData = new List<byte[]>();
        foreach (var imagePath in Directory.EnumerateFiles(Path.Combine(executingPath, "Scenes")))
        {
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            imageScenesData.Add(imageBytes);
        }

        var objectImageData = await File.ReadAllBytesAsync(Path.Combine(executingPath, "Dutoit-Louis-object.jpg"));
        
        var detectObjectInScenesResults = new ObjectDetection().DetectObjectInScenes(objectImageData, imageScenesData);
        // var values = JsonSerializer.Serialize(detectObjectInScenesResults[0].Points);

        Assert.Equal("[{\"X\":741,\"Y\":671},{\"X\":118,\"Y\":1601},{\"X\":1472,\"Y\":2159},{\"X\":1866,\"Y\":1350}]",JsonSerializer.Serialize(detectObjectInScenesResults[0].Points));

        Assert.Equal("[{\"X\":2236,\"Y\":2656},{\"X\":2042,\"Y\":1706},{\"X\":692,\"Y\":1893},{\"X\":651,\"Y\":2866}]",JsonSerializer.Serialize(detectObjectInScenesResults[1].Points));
    }

    private static string GetExecutingPath()
    {
        var executingAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var executingPath = Path.GetDirectoryName(executingAssemblyPath);
        return executingPath;
    }
}