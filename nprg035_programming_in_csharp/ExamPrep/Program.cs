using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Transactions;

namespace Exam
{
    
    interface I6
    {
        public char f();
    }
    class A6 : I6
    {
        public virtual char f() => 'A';
    }
    class B6 : A6
    {
        public override char f() => 'B';
    }
    class C6 : B6
    {
        public virtual char f() =>
        (char)(base.f() + 10);
    }
    class D6 : C6
    {
        public override char f() => 'D';
    }
    class E6 : D6
    {
        public override char f() =>
        (char)(base.f() + 5);
    }
    /// <summary>
    /// 8, 9, 10 24.01.19
    /// </summary>
    abstract class Component
    {
        protected float TimeFromLastSnapShot = 0.0f;
        public virtual void Update(float DeltaTime, float DeltaEnergy) { }

    }
    class PositionComponent : Component
    {
        public float X = 0.0f;
        public float Y = 0.0f;
        public PositionComponent() { }
        public PositionComponent(float x, float y) { this.X = x; this.Y = y; }
    }

    class HealthComponent : Component 
    {
        public float Max;
        private float current;
        public float Current {

            get { return current; }
            set 
            {
                if (value < 0.0f) { current = 0; }
                else if (value > this.Max) { current = this.Max; }
                else current = value;
            } 
        }
        public HealthComponent() { }
        public HealthComponent(float Max) {  this.Max = Max; }
        public HealthComponent(float Max, float Current) { this.Max = Max; this.Current = Current; }

        public override void Update(float DeltaTime, float DeltaEnergy)
        {
            TimeFromLastSnapShot += DeltaTime;
            if (TimeFromLastSnapShot >= 1.0f)
            {
                Current -= DeltaEnergy;
                TimeFromLastSnapShot -= 1.0f;
            }
            
        }
    }

    class BleedingEffectComponent : Component 
    {
        
        public float DamagePerSecond = 0;
        public BleedingEffectComponent() { }
        public BleedingEffectComponent(float DamagePerSecond) {  this.DamagePerSecond = DamagePerSecond; }
    }

    interface IGameObject
    {
        public void SetComponent(Component component);
        public Component? GetComponent(System.Type type);
    }


    class GameObject : IGameObject
    {
        float deltaEnergy = 0.0f;
        List<Component> components = new List<Component> { };
        public GameObject() { }
        public void SetComponent(Component component) 
        { 
            var t = component.GetType();
            if (t == typeof(BleedingEffectComponent)) { deltaEnergy = ((BleedingEffectComponent)component).DamagePerSecond; }
            components.Add(component);
        }
        public Component? GetComponent(Type type) 
        { 
            foreach (Component component1 in components) 
            {
                var t = component1.GetType();
                if (t == type) return component1;
            }
            return null;
        }


        public void Update(float DeltaTime)
        {
            
            foreach (Component component in components)
            {
                component.Update(DeltaTime, this.deltaEnergy);
            }
        }

    }
    /// 8, 9, 10 24.01.19


    class X1
    {
        public int Value
        {
            get
            {
                Console.WriteLine("X");
                return 10;
            }
        }
    }
    
    /*public class Program
    {
        static void Main(string[] args)
        {
            C6? c6 = new E6();
            Console.WriteLine(c6.f()); 
            I6 i6 = new D6();
            Console.WriteLine(i6.f());



            ///
            
            PositionComponent pc = new PositionComponent();
            Console.WriteLine("{0} and {1}", pc.X, pc.Y);
            PositionComponent pc2 = new PositionComponent(6.9f, 7.0f);
            Console.WriteLine("{0} and {1}", pc2.X, pc2.Y);
            HealthComponent hc = new HealthComponent { Max = 100, Current = 80 };
            Console.WriteLine("{0} and {1}", hc.Max, hc.Current);
            hc.Current = float.PositiveInfinity;
            Console.WriteLine("{0} and {1}", hc.Max, hc.Current);
            hc.Current = float.NegativeInfinity;
            Console.WriteLine("{0} and {1}", hc.Max, hc.Current);
            var rock = new GameObject();
            rock.SetComponent(new PositionComponent());

            // Create dead zombie standing on the rock:
            var zombie = new GameObject();
            zombie.SetComponent(new PositionComponent());
            zombie.SetComponent(new HealthComponent
            {
                Max = 30
            });
            // Create partially damaged troll nearby:
            var troll = new GameObject();
            troll.SetComponent(new PositionComponent
            {
                X = 5.5f,
                Y = 0f
            });
            troll.SetComponent(new HealthComponent
            {
                Max = 100,
                Current = 80
            });
            // Resurrect zombie to .Max health:
            if (zombie.GetComponent(typeof(HealthComponent))
            is HealthComponent health)
            {
                health.Current = float.PositiveInfinity;
            }
            // Throw rock in troll direction:
            if (rock.GetComponent(typeof(PositionComponent))
            is PositionComponent position)
            {
                position.X += 0.25f;
            }

            PositionComponent comp = (PositionComponent)rock.GetComponent(typeof(PositionComponent));
            Console.WriteLine("rock {0} and {1}", comp.X, comp.Y);
            
            ///
            
            var x1 = new X1();
            if (x1.Value > 5 && x1.Value < 15 && x1.Value != 8)
            {
                Console.WriteLine("OK1");
            }
            if (x1.Value is > 5 and < 15 and not 8)
            {
                Console.WriteLine("OK2");
            }

        }
    }*/
}