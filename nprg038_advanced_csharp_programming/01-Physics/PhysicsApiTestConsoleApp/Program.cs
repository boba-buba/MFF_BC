using PhysicsUnitsLib;
namespace GamePhysics {

	// IMPORTANT NOTE: You should NOT change this file as part of your solution! Put you implementation into PhysicsUnitsLib project only.

	class Program {
		static void Main(string[] args) {
			var originalDistance = 1.5.Meters();
			var deltaDistance = 2.Meters();
			var distance = originalDistance + deltaDistance;
			Console.WriteLine($"Moving {deltaDistance} after {originalDistance} travelled equals total distance of {distance}");

			var time = 3.Seconds();
			var speed = distance / time;
			Console.WriteLine($"Distance of {distance} travelled in {time} equals speed of {speed}");

			speed *= 2;
			Console.WriteLine($"Doubled speed: {speed}");

			speed = 3.5.MeterPerSeconds();
			Console.WriteLine($"New speed: {speed}");

			var speed2 = distance / new Second(0);
			Console.WriteLine($"Impossible speed2: {speed2}");
			var speed3 = (distance* (-1))/ new Second(0);
            Console.WriteLine($"Impossible speed3: {speed3}");
			Console.WriteLine($"Destroying universe?? {speed2+speed3}");

            // !!! Uncommenting following line must produce Error: Operator '*=' cannot be applied to operands of type 'MeterPerSecond' and 'Meter'
            // speed *= distance;
            // !!! Uncommenting following line must produce Error: Operator '+=' cannot be applied to operands of type 'Meter' and 'Second'
            // distance += time;
        }
    }
}
