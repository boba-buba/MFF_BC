using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace MinimalisticUIFramework
{

	public struct Point
	{
		public int X { get; set; }
		public int Y { get; set; }
		public override string ToString() => $"at {X}, {Y}";
    }

    public class StackPanel : Control 
	{ 
		List<Control> _elements = new List<Control>();
		public Control AddChild(Control element)
		{
			_elements.Add(element);
			return this;
		}
        public override string ToString()
		{
			var sb = new StringBuilder("StackPanel {\n");
			
            foreach (var item in _elements)
            {
				sb.Append(item);
				sb.Append('\n');
            }
			sb.Append('}');
			return sb.ToString();
        }
    }

	public class Canvas : Control
	{
		Dictionary<Control, Point> _canvasElements = new Dictionary<Control, Point>();
		public List<Control> l = new List<Control>();
		public Control AddChild(Control el, Point p)
		{
            _canvasElements.Add(el, p);
			return this;
		}
		public override string ToString()
		{
            var sb = new StringBuilder("Canvas {\n");

            foreach (KeyValuePair<Control, Point> pair in _canvasElements)
            {
                sb.Append(pair.Key);
				sb.Append(' ');
				sb.Append(pair.Value);
                sb.Append('\n');
            }
            sb.Append('}');
            return sb.ToString();
        }
	}

	public static class ElementsExtensions
	{
		public static T PlacedIn<T>(this T element, StackPanel panel) where T : Control
		{
			panel.AddChild(element);
			return element;
		}

		public static InterELement<T> PlacedIn<T>(this T element, Canvas canvas) where T : Control
		{
			InterELement<T> el = new InterELement<T>(element, canvas);
			return el;
        }
    }

	public class InterELement<T> where T : Control
	{
		public InterELement(T element, Canvas canvas) 
		{ 
			this.element = element;
			this.canvas = canvas;
		}
		public T element;
		public Canvas canvas;
        public T At(int x, int y)
        {
			canvas.AddChild(element, new Point { X = x, Y = y });
            return element;
        }
    }
}