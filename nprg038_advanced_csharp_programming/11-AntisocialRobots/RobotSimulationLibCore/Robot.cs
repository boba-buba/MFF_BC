// IMPORTANT NOTE: You should NOT change this file as part of your solution! Put you implementation into RobotSimulation.cs file only.

//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: Robot.cs
//
//--------------------------------------------------------------------------

namespace AntisocialRobots
{
    /// <summary>Represents one robot.</summary>
    [System.Diagnostics.DebuggerDisplay("Location = {Location}")]
    public class Robot
    {

        public Robot(bool isMovable = true)
        {
            IsMovable = isMovable;
        }

        public int Id;

        /// <summary>The current location of the room.</summary>
        public RoomPoint Location;

        // frame of last move
        public int lastmoved;

        /// <summary>Indicates whether this robot is movable.</summary>
        public bool IsMovable { get; private set; }
    }
}
