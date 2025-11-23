// NOTE: Put your solution here in RobotSimulation class - document your solution in comments in code + in RemovedDataRacesLog.md.
// IMPORTANT NOTE: Your solution must compile without error, and should not contain any data races!
// IMPORTANT NOTE: Your solution should be correct on every platform with regards to ANY possible thread schedulling done by OS scheduler!

//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Updated by Pavel Jezek, Charles University in Prague 
// 
//  File: RoomSimulation.cs
//
//--------------------------------------------------------------------------

namespace AntisocialRobots
{
    public class RobotSimulation : RobotSimulationBase
    {
		public enum FrameUpdateMode {
			Sequential = 0,
			Parallel,
            Parallel_WithLocks
		}

		public FrameUpdateMode UpdateMode { get; set; }

		public RobotSimulation(int size)
			: base(size) {
            for (int i = 0; i < ROOM_SIZE; i++)
            {
                for (int j = 0; j < ROOM_SIZE; j++)
                {
                    _locks[i, j] = new object();
                }
            }
		}
        private object[,] _locks;
		protected override void OnPerformFrameUpdate(int frameIdx) {
			switch (UpdateMode) {
				case FrameUpdateMode.Sequential:
					FrameUpdate_Sequential();
					break;
				case FrameUpdateMode.Parallel:
					FrameUpdate_Parallel(SimulateOneStep_NoLocks);
					break;
                case FrameUpdateMode.Parallel_WithLocks:
                    FrameUpdate_Parallel_For(SimulateOneStep_WithLocks);
                    break;
			}
		}

		private void FrameUpdate_Sequential() {
			Action<Robot> SimulateOneStep = SimulateOneStep_NoLocks;
			foreach (Robot robot in _movableRobots)
				SimulateOneStep(robot);
		}

		private void FrameUpdate_Parallel(Action<Robot> SimulateOneStep) {
			int threadCount = Environment.ProcessorCount;

			if (_movableRobots.Count < threadCount) {
				FrameUpdate_Sequential();
				return;
			}

			int robotsPerThread = _movableRobots.Count / threadCount;
			int robotsUnassigned = _movableRobots.Count % threadCount;


            Thread[] threads = new Thread[threadCount];
            for (int i = 0; i < threads.Length; i++)
            {
                int from = i * robotsPerThread;
                int to = from + robotsPerThread - 1;
                if (i == threads.Length - 1)
                {
                    to += robotsUnassigned;
                }
                threads[i] = new Thread(() =>
                {
                    for (int ri = from; ri <= to; ri++)
                    {
                        SimulateOneStep(_movableRobots[ri]);
                    }
                });
                threads[i].Start();
            }

            foreach (var t in threads)
            {
                t.Join();
            }


        }

        private void FrameUpdate_Parallel_For(Action<Robot> SimulateOneStep)
        {
            int threadCount = Environment.ProcessorCount;

            if (_movableRobots.Count < threadCount)
            {
                FrameUpdate_Sequential();
                return;
            }

            int robotsPerThread = _movableRobots.Count / threadCount;
            int robotsUnassigned = _movableRobots.Count % threadCount;
            //Here we would need to isolate i variable (like in example: i1, i2, ...)

            Parallel.For(0, threadCount, (i) =>
            {
                int from = i * robotsPerThread;
                int to = from + robotsPerThread - 1;
                if (i == threadCount - 1)
                {
                    to += robotsUnassigned;
                }

                for (int ri = from; ri <= to; ri++)
                {
                    SimulateOneStep(_movableRobots[ri]);
                }

            });
        }

            /// <summary>Performs one step of the simulation for one robot.</summary>
            /// <param name="r">The robot to perform the simulation step for.</param>
            private void SimulateOneStep_NoLocks(Robot r)
        {
            if (!r.IsMovable)
                return;

            RoomPoint origLoc = r.Location;
            RoomPoint newLoc = DetermineNewLocation(r);

            if (!newLoc.Equals(origLoc))
            {
                if (_roomCells[newLoc.X, newLoc.Y] == null)
                {
                    _roomCells[origLoc.X, origLoc.Y] = null;
                    _roomCells[newLoc.X, newLoc.Y] = r;
                    
                    r.Location = newLoc;
                }
            }
        }

        private void SimulateOneStep_WithLocks(Robot r)
        {
            if (!r.IsMovable)
                return;


            RoomPoint origLoc = r.Location;
            RoomPoint newLoc = DetermineNewLocation_WithLock(r);

            object origLocLock = _locks[origLoc.X, origLoc.Y];
            object newLocLock = _locks[newLoc.X, newLoc.Y];

            if (!newLoc.Equals(origLoc))
            {
                lock (newLocLock)
                {
                    if (_roomCells[newLoc.X, newLoc.Y] == null)
                    {
                        lock (origLocLock)
                        {
                            _roomCells[origLoc.X, origLoc.Y] = null;
                        }

                        _roomCells[newLoc.X, newLoc.Y] = r;
                        lock (r)
                        {
                            r.Location = newLoc;
                        }
                        

                    }
                }
            }
        }

        /// <summary>
        /// Computes the new desired location for the robot.
        /// This version doesn't do any thread synchronization
        /// operations, and is therefore NOT thread-safe.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private RoomPoint DetermineNewLocation(Robot r)
        {
            RoomPoint ptR = r.Location;
            double vectorX = 0, vectorY = 0;

            foreach (Robot s in _robots)
            {
                if (r == s) continue;
                RoomPoint ptS = s.Location;

                double inverseSquareDistance = 1.0 / RoomPoint.Square(ptR.DistanceTo(ptS));
                double angle = ptR.AngleTo(ptS);
                vectorX -= inverseSquareDistance * Math.Cos(angle);
                vectorY -= inverseSquareDistance * Math.Sin(angle);
            }

            return ComputeNewLocation(ptR, vectorX, vectorY, ROOM_SIZE);
        }

		private RoomPoint DetermineNewLocation_WithLock(Robot r)
		{
            RoomPoint ptR = r.Location;
            double vectorX = 0, vectorY = 0;

            foreach (Robot s in _robots)
            {
                if (r == s) continue;
                //We would need lock here, as more than one thread can compute new location, thinking that s is free, we can lock s as it is a class and has sync block
                RoomPoint ptS;
                lock (s)
                {
                    ptS = s.Location;
                }
                double inverseSquareDistance = 1.0 / RoomPoint.Square(ptR.DistanceTo(ptS));
                double angle = ptR.AngleTo(ptS);
                vectorX -= inverseSquareDistance * Math.Cos(angle);
                vectorY -= inverseSquareDistance * Math.Sin(angle);
            }

            return ComputeNewLocation(ptR, vectorX, vectorY, ROOM_SIZE);
        }

    }
}
