using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IHMP.MediaCollector.Workers
{
    public static class FaceWorker
    {
        private static HaarCascade face;
        private static Image<Gray, byte> result, TrainedFace = null;

        static FaceWorker()
        {
            face = new HaarCascade("haarcascade_frontalface_default.xml");
        }

        public static async Task<bool> Sync()
        {
            Console.WriteLine("Start face worker...");
            try
            {
                var medias = Directory.GetDirectories(Constants.MediasPath);
                var faceIds = GetFaceIds();

                foreach (var media in medias)
                {
                    string mediaID = media.Split(Path.DirectorySeparatorChar).Last();
                    var isExist = faceIds.Any(s => s == mediaID);
                    if (isExist)
                        continue;

                    var imagePath = Directory.GetFiles(media).First();

                    Image<Bgr, Byte> _img = new Image<Bgr, Byte>(imagePath);
                    Image<Gray, Byte> grayImg = _img.Convert<Gray, Byte>();

                    MCvAvgComp[][] facesDetected = grayImg.DetectHaarCascade(face, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
                    foreach (MCvAvgComp f in facesDetected[0])
                    {
                        result = _img.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                        TrainedFace = _img.Copy(f.rect).Convert<Gray, byte>();
                        break;
                    }

                    TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    TrainedFace.Save($"{Constants.FacesPath}\\{mediaID}.bmp");
                }

                Console.WriteLine("Finish face worker...");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error worken {ex.Message}");
                return false;
            }
        }

        public static void CheckFolder()
        {
            if (!Directory.Exists(Constants.FacesPath))
            {
                Directory.CreateDirectory(Constants.FacesPath);
            }
        }

        private static List<string> GetFaceIds()
        {
            var ids = Directory.GetDirectories(Constants.FacesPath);
            List<string> idList = new List<string>();
            for (int i = 0; i < ids.Length; i++)
            {
                idList.Add(ids[i].Split(Path.DirectorySeparatorChar).Last());
            }
            return idList;
        }
    }
}
