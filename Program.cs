using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;
using System.Diagnostics;
using CircleRGB_v2;

namespace CircleBP
{
    public Simpad sim = new Simpad();

    internal class Program
    {
        

        public static async Task Main()
        {
            var colors = new Dictionary<int, string>(){
                {0, "#0000ff"},
                {1, "#00ff00"},
                {2, "#ffff00"},
                {3, "#ff0000" }
            }; // Color dict. TODO: make this configurable

            var currentScore = new string[4];

            var sim = new CircleRGB_v2.Simpad();
            await sim.Init();
            Console.WriteLine("CircleRGB v2 || By Willow\n\nPlease Keep This Window Open!!!");

            await Task.Run(async () =>
            {
                Stopwatch stopwatch;
                _sreader.WithTimes = true;
                var baseAddresses = new OsuBaseAddresses();
                bool deviceIsRainbow = false;
                bool devicePlay = false;
                string curComboColor = "";

                while (true)
                {

                    stopwatch = Stopwatch.StartNew();
                    _sreader.TryRead(baseAddresses.GeneralData); // Initialize data reader

                    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.SongSelect)
                    {
                        currentScore = new string[4];
                        curComboColor = "";
                        if (devicePlay)
                        {
                            devicePlay = false;
                        }
                        if (!deviceIsRainbow)
                        {
                            await sim.setMode("rainbow");
                            deviceIsRainbow = true;
                        }
                    }
                    else
                        baseAddresses.SongSelectionScores.Scores.Clear();

                    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.ResultsScreen)
                    {
                        if (devicePlay)
                        {
                            await sim.setKeypad("#000000");
                            currentScore = new string[4];
                            curComboColor = "";
                            devicePlay = false;
                        }
                    }


                    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing)
                    {
                        if (!devicePlay)
                        {
                            await sim.setMode("on");
                            await sim.setKeypad("#000000");
                            deviceIsRainbow = false;
                            devicePlay = true;
                        }

                        _sreader.TryReadProperty(baseAddresses.Player, nameof(Player.Hit300), out var hit300);
                        _sreader.TryReadProperty(baseAddresses.Player, nameof(Player.Hit100), out var hit100);
                        _sreader.TryReadProperty(baseAddresses.Player, nameof(Player.Hit50), out var hit50);
                        _sreader.TryReadProperty(baseAddresses.Player, nameof(Player.HitMiss), out var hitmiss);

                        try
                        {
                            List<String> testList = new List<String>();
                            testList.Add(hit300.ToString());
                            testList.Add(hit100.ToString());
                            testList.Add(hit50.ToString());
                            testList.Add(hitmiss.ToString());

                            for (int i = 0; i < testList.Count; i++)
                            {
                                if (currentScore[i] != testList[i])
                                {
                                    currentScore[i] = testList[i];
                                    if (testList[i] != "0")
                                    {
                                        if (i.ToString() != curComboColor)
                                        {
                                            curComboColor = i.ToString();
                                            await sim.setKeypad(colors[i]);
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            if (devicePlay)
                            {
                                currentScore = new string[4];
                                await sim.setKeypad("#000000");
                                devicePlay = false;
                                curComboColor = "";
                            }
                        }

                    }
                    else
                    {
                        if (devicePlay)
                        {
                            currentScore = new string[4];
                            await sim.setKeypad("#000000");
                            devicePlay = false;
                        }
                    }

                    var hitErrors = baseAddresses.Player?.HitErrors;
                    if (hitErrors != null)
                    {
                        var hitErrorsCount = hitErrors.Count;
                        hitErrors.Clear();
                        hitErrors.Add(hitErrorsCount);
                    }

                    stopwatch.Stop();

                    _sreader.ReadTimes.Clear();
                    await Task.Delay(33);
                }
            });
        }
    }
}
}