using IHMP.MediaCollector.Workers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IHMP.MediaCollector
{
    class Program
    {
        static bool searchResult = true;
        static Timer _searchTimer;
        static bool faceResult = true;
        static Timer _faceTimer;

        static void Main(string[] args)
        {
            SearchWorker.CheckFolder();
            FaceWorker.CheckFolder();

            var task = Task.Run(async () =>
            {
                do
                {
                    if (searchResult)
                    {
                        var res = await SearchWorker.Sync();
                        searchResult = !res;

                        if (!searchResult)
                        {
                            Console.WriteLine("Sleeping collector worker for 60 seconds...");
                            SetSearchTimer();
                        }
                    }

                    if (faceResult)
                    {
                        var res = await FaceWorker.Sync();
                        faceResult = !res;

                        if (!faceResult)
                        {
                            Console.WriteLine("Sleeping face worker for 60 seconds...");
                            SetFaceTimer();
                        }
                    }

                    await Task.Delay(1000);

                } while (true);
            });

            task.Wait();
        }

        private static void SetSearchTimer()
        {
            _searchTimer = new Timer((s) =>
            {
                searchResult = true;
                _searchTimer.Dispose();
            }, null, 60000, Timeout.Infinite);
        }

        private static void SetFaceTimer()
        {
            _faceTimer = new Timer((s) =>
            {
                faceResult = true;
                _faceTimer.Dispose();
            }, null, 60000, Timeout.Infinite);
        }
    }
}
