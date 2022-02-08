namespace Louis.Dutoit.FeatureMatching;

using OpenCvSharp;

public class ObjectDetection
{
    public IList<ObjectDetectionResult> DetectObjectInScenes(byte[]
        objectImageData, IList<byte[]> imagesSceneData)
    {
        var tasks = new List<Task>();

        foreach (var imageSceneData in imagesSceneData)
        {
            var task = Task.Run(() =>
            {
                using var imgObject = Mat.FromImageData(objectImageData, ImreadModes.Color);
                using var imgScene = Mat.FromImageData(imageSceneData, ImreadModes.Color);
                using var orb = ORB.Create(10000);
                using var descriptors1 = new Mat();
                using var descriptors2 = new Mat();
                
                orb.DetectAndCompute(imgObject, null, out var keyPoints1, descriptors1);
                orb.DetectAndCompute(imgScene, null, out var keyPoints2, descriptors2);
                
                using var bf = new BFMatcher(NormTypes.Hamming, crossCheck: true);
                var matches = bf.Match(descriptors1, descriptors2);
                var goodMatches = matches
                    .OrderBy(x => x.Distance)
                    .Take(10)
                    .ToArray();
                
                var srcPts = goodMatches.Select(m => keyPoints1[m.QueryIdx].Pt).Select(p => new Point2d(p.X, p.Y));
                var dstPts = goodMatches.Select(m => keyPoints2[m.TrainIdx].Pt).Select(p => new Point2d(p.X, p.Y));
                using var homography = Cv2.FindHomography(srcPts, dstPts, HomographyMethods.Ransac, 5, null);
                int h = imgObject.Height, w = imgObject.Width;
                
                var img2Bounds = new[]
                {
                    new Point2d(0, 0),
                    new Point2d(0, h - 1),
                    new Point2d(w - 1, h - 1),
                    new Point2d(w - 1, 0),
                };
                var img2BoundsTransformed = Cv2.PerspectiveTransform(img2Bounds, homography);
                using var view = imgScene.Clone();
                var drawingPoints = img2BoundsTransformed.Select(p => (Point) p).ToArray();
                Cv2.Polylines(view, new[] {drawingPoints}, true, Scalar.Red, 3);
                var imageResult = view.ToBytes(".png");

                return new ObjectDetectionResult()
                {
                    ImageData = imageResult,
                    Points = drawingPoints.Select(point => new
                        ObjectDetectionPoint {X = point.X, Y = point.Y}).ToList()
                };
            });

            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());
        
        return tasks.Select(task => ((Task<ObjectDetectionResult>) task).Result).ToList();
    }
}