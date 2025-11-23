## What data races did you identify?

1. In function `DetermineNewLocation` there is reading of robot's (`s`) location to compute desired location for robot `r`. More than one thread can read robot's `s` location and compute desired location. 
2. In function  `SimulateOneStep_NoLocks` on line `if (_roomCells[newLoc.X, newLoc.Y] == null)` can be a race, as two or more threads read the `_roomCells[newLoc.X, newLoc.Y]` as free and would try to write there.
3. Not exactly race and not sure about that, but new function `FrameUpdate_Parallel_For` was introduced instead of `FrameUpdate_Parallel`.

## Explain your solution - why do you think it works?

Locks for every cell were added (cell is a struct therefore it needs another object to lock.). Theoretically we could use one lock for all cells, but it would decrease parallelization of the computing. Apart from cell locks, we lock robot instances as well.

1. Function `DetermineNewLocation_WithLock` fixes the race, as it locks the reading of robot's `s` location for only one thread at a time to read and comute desired location.
2. Function `SimulateOneStep_WithLocks` fixes the race. We have locks for newly computed location and the original location. If locations differ we lock new location, because we want only one thread to write to new location. When we want to change original location to null we need the lock for that too, as there could be more than one thread, that wants to write to the original location. When we want to change the location of the robot `r` we need to lock `r` as well, as more than one thread can try to write to `r`'s location newly computed location.
3. New function uses `Parallel.For` instead of creating threads, so that we have variable `i` isolated.