using LibCompute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Day_23
{
    internal class Program
    {
        private static Dictionary<long, Queue<long[]>> _buffers;

        private static void Main(string[] args)
        {
            const int computerCount = 50;

            _buffers = new Dictionary<long, Queue<long[]>>();
            var computers = new IntcodeComputer[computerCount];
            var ios = new IOPipe[computerCount];

            bool firstSolution = false;

            long lastNatX = -1, lastNatY = -1;
            long natX = 0, natY = 0;

            long ticks = 0;
            long lastPacketTick = 0;
            const int idleThreshold = 1250; // a little bit high just to be sure

            for (int i = 0; i < computerCount; i++)
            {
                var io = new IOPipe();
                int cp = i;
                ios[i] = io;
                io.InputInt(i);
                var buffer = new Queue<long[]>();

                _buffers.Add(i, buffer);

                computers[i] = new IntcodeComputer($"Computer {i}", "input", io);

                io.FireEveryNbOutput = 3;

                io.ReadingInt += (s, e) =>
                  {
                      if (buffer.Count == 0)
                      {
                          io.InputInt(-1);
                      }
                      else
                      {
                          var packet = buffer.Dequeue();

                          io.InputInt(packet[0]);
                          io.InputInt(packet[1]);
                      }
                  };

                io.IntOuputted += (s, e) =>
                  {
                      var address = io.ReadOutputInt();
                      var X = io.ReadOutputInt();
                      var Y = io.ReadOutputInt();
                      lastPacketTick = ticks;

                      if (address == 255)
                      {
                          natX = X;
                          natY = Y;

                          if (!firstSolution)
                          {
                              Console.WriteLine(natY);
                              firstSolution = true;
                          }
                      }
                      else
                      {
                          _buffers[address].Enqueue(new long[] { X, Y });
                      }
                  };
            }
            bool keepGoing = true;
            while (keepGoing)
            {
                foreach (var computer in computers)
                    computer.Step();

                if (_buffers.All(x => x.Value.Count == 0) && ticks - lastPacketTick > idleThreshold)
                {
                    if (lastNatY == natY)
                    {
                        Console.WriteLine(natY);
                        keepGoing = false;
                    }

                    _buffers[0].Enqueue(new[] { natX, natY });
                    lastPacketTick = ticks;

                    lastNatX = natX;
                    lastNatY = natY;
                }

                ticks++;
            }
        }
    }
}