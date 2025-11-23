//using System.Diagnostics.Metrics;
using PhysicsUnitsLib;

namespace JumpingPlatformGame {

	// TODO: Provide your implementation of the following classes using your types from the PhysicsUnitsLib project.
	
	struct WorldPoint
	{
		public Meter X { get; set; }
		public Meter Y { get; set; }
	}

	struct WorldVector
	{
		public Meter LowerBound { get; set; }
		public Meter UpperBound { get; set; }
		public MeterPerSecond Speed { get; set; }
	}

    class Entity {
		public virtual Color Color => Color.Black;
		public WorldPoint Location;
        public virtual void Update(Second delta) { }
    }

    class MovableEntity : Entity {
		public WorldVector Horizontal;
        public override void Update(Second delta) 
		{
            Location.X += delta * Horizontal.Speed;

			if ((double)Location.X >= (double)Horizontal.UpperBound )
			{
				Horizontal.Speed = Horizontal.Speed * -1;
				Location.X = Horizontal.UpperBound;
			}
			else if ((double)Location.X <= (double)Horizontal.LowerBound)
			{
                Horizontal.Speed = Horizontal.Speed * -1;
                Location.X = Horizontal.LowerBound;
            }			
		}
    }

	class MovableJumpingEntity : MovableEntity {
        public WorldVector Vertical;
        public override void Update(Second delta) 
		{
			base.Update(delta);
	
			if (Vertical.Speed.Value == 0)
			{
				return;
			}

			Location.Y += delta * Vertical.Speed;

            if ((double)Location.Y >= (double)Vertical.UpperBound)
            {
				Vertical.Speed = Vertical.Speed * -1;
                Location.Y = Vertical.UpperBound;
            }
            else if ((double)Location.Y <= (double)Vertical.LowerBound)
            {
                Location.Y = Vertical.LowerBound;
            } 
        }
    }

	class Joe : MovableEntity {
		public override string ToString() => "Joe";
		public override Color Color => Color.Blue;
	}

	class Jack : MovableEntity {
		public override string ToString() => "Jack";
		public override Color Color => Color.LightBlue;
	}

	class Jane : MovableJumpingEntity {
		public override string ToString() => "Jane";
		public override Color Color => Color.Red;
	}

	class Jill : MovableJumpingEntity {
		public override string ToString() => "Jill";
		public override Color Color => Color.Pink;
	}
}