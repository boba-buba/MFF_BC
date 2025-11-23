using System.Collections.Generic;


public static class IEnumerableExtensions
{
	public static void Print(this IEnumerable<ValidationError> list)
	{
		if (!list.Any())
		{
			Console.WriteLine("  >>> Validation successful >>>\n");
			return;
		}
		Console.WriteLine("  >>> Validation failed >>>");
		foreach (var item in list)
		{

			Console.WriteLine("    > " + item);
		}
		Console.WriteLine();

	}
}



public abstract class Validator<TValue> where TValue : Order
{
	public abstract IEnumerable<ValidationError> Validate(TValue value);
	public IEnumerable<ValidationError> Validate(int amount, RangeValidator<int> rangeValidator) => rangeValidator.Validate(amount);
	public IEnumerable<ValidationError> Validate(string id, NonBlankStringValidator nonBlank, StringLengthValidator len)
	{
		var errors = nonBlank.Validate(id);
		errors.AddRange(len.Validate(id));
		return errors;
	}
	public IEnumerable<ValidationError> Validate(decimal price, RangeValidator<decimal> rangeValidator) => rangeValidator.Validate(price);
	public IEnumerable<ValidationError> Validate(string? comment, NotNullValidator notNullValidator) => notNullValidator.Validate(comment);
}

public class NonBlankStringValidator
{
	public List<ValidationError> Validate(string value)
	{
		var errors = new List<ValidationError>();

		if (value.Count() == 0)
		{
			errors.Add(new ValidationError($"\"{value}\" is empty or just whitespaces."));
		}
		else
		{
			foreach (var ch in value)
			{
				if (char.IsLetter(ch) || char.IsDigit(ch))
				{
					return errors;
				}
			}
			errors.Add(new ValidationError($"\"{value}\" is empty or just whitespaces."));
		}

		return errors;
	}
}
public class RangeValidator<TType> where TType : struct, IComparable<TType>
{

	public TType Min { get; init; }
	public TType Max { get; init; }
	
	public List<ValidationError> Validate(TType value)
	{
		var errors = new List<ValidationError>();
		if (value.CompareTo(Max) > 0) errors.Add(new ValidationError($"{value} is greater than maximum {Max}."));
		else if (value.CompareTo(Min) < 0) errors.Add(new ValidationError($"{value} is less than minimum {Min}."));
		return errors;
	}
}

public class StringLengthValidator
{
	dynamic rangeValidator;
	public StringLengthValidator(dynamic rangeValidator)
	{
		this.rangeValidator = rangeValidator;
	}

	public List<ValidationError> Validate(string value)
	{
		int length = value.Length;
		var errors = new List<ValidationError>();
		var rangeErrors = rangeValidator.Validate(length);
		foreach (var rangeError in rangeErrors)
		{
			errors.Add(new ValidationError($"\"{value}\" length " + rangeError));
		}
		return errors;
	}
}

public class NotNullValidator
{
	public List<ValidationError> Validate(object? value)
	{
		var errors = new List<ValidationError>();
		if (value == null)
			errors.Add(new ValidationError($"\"{value}\" is null."));
		return errors;
	}
}

public class ValidationError
{
	public string Reason { get; init; }

	public ValidationError(string reason)
	{
		Reason = reason;
	}

	/* TODO: any additional members */
	public override string ToString()
	{
		return Reason;
	}
}

//

public record class Order
{
	public required string Id { get; set; }
	public int Amount { get; set; }
	public decimal TotalPrice { get; set; }
	public string? Comment { get; set; }
}

public record class SuperOrder : Order { }

//

public class OrderValidator : Validator<Order>
{
	public override List<ValidationError> Validate(Order value)
	{
		var allErrors = new List<ValidationError>();
		allErrors.AddRange(Validate(value.Amount, new RangeValidator<int> { Min = 1, Max = 10 }));
		allErrors.AddRange(Validate(value.Id, new NonBlankStringValidator(), new StringLengthValidator(new RangeValidator<int> { Min = 1, Max = 8 })));
		allErrors.AddRange(Validate(value.TotalPrice, new RangeValidator<decimal> { Min = 0.01M, Max = 999.99M }));
		allErrors.AddRange(Validate(value.Comment, new NotNullValidator()));
		return allErrors;
	}
}

public class AdvancedOrderValidator : Validator<Order>
{
	// TODO:
	// ... Validate(Order value) ... {
	//	  Similar syntax as for OrderValidator, but more compact:
	//	  + without need to specify inferable types <int>, <decimal> ...
	//	  + without need for new ...
	// }

	public override List<ValidationError> Validate(Order value)
	{
		var allErrors = new List<ValidationError>();
		allErrors.AddRange(Validate(value.Amount, new RangeValidator<int> { Min = 1, Max = 10 }));
		allErrors.AddRange(Validate(value.Id, new NonBlankStringValidator(), new StringLengthValidator(new RangeValidator<int> { Min = 1, Max = 8 })));
		allErrors.AddRange(Validate(value.TotalPrice, new RangeValidator<decimal> { Min = 0.01M, Max = 999.99M }));
		allErrors.AddRange(Validate(value.Comment, new NotNullValidator()));
		return allErrors;
	}
}

class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("--- plain Validators ---");

		var nonBlankStringValidator = new NonBlankStringValidator();
		nonBlankStringValidator.Validate("   ").Print();
		nonBlankStringValidator.Validate("hello").Print();

		var rangeValidator = new RangeValidator<int> { Min = 1, Max = 6 };
		rangeValidator.Validate(7).Print();
		rangeValidator.Validate(1).Print();

		var stringLengthValidator = new StringLengthValidator(new RangeValidator<int> { Min = 5, Max = 6 });
		stringLengthValidator.Validate("Jack").Print();
		stringLengthValidator.Validate("hello-world").Print();
		stringLengthValidator.Validate("hello").Print();

		var notNullValidator = new NotNullValidator();
		object? obj = null;
		notNullValidator.Validate(obj).Print();
		string? s = null;
		notNullValidator.Validate(s).Print();
		Order? order = null;
		notNullValidator.Validate(order).Print();
		s = "hello";
		notNullValidator.Validate(s).Print();

		Console.WriteLine("--- AdvancedOrderValidator.Validate() ---");

		AdvancedOrderValidator advancedValidator = new AdvancedOrderValidator();

		var o1 = new Order { Id = "    ", Amount = 5 };
		advancedValidator.Validate(o1).Print();

		var o2 = new Order { Id = "AC405", Amount = 5 };
		advancedValidator.Validate(o2).Print();

		var o3 = new Order { Id = "AC405", Amount = 600 };
		advancedValidator.Validate(o3).Print();

		var o4 = new Order { Id = "", Amount = 600 };
		advancedValidator.Validate(o4).Print();

		var o5 = new Order { Id = "AC405-12345678", Amount = 5, TotalPrice = 42, Comment = "Best order ever" };
		advancedValidator.Validate(o5).Print();

		var o6 = new Order { Id = "AC405", Amount = 5, TotalPrice = 42, Comment = "Best order ever" };
		advancedValidator.Validate(o6).Print();

		Console.WriteLine("--- OrderValidator.Validate() ---");

		OrderValidator orderValidator = new OrderValidator();

		orderValidator.Validate(o1).Print();
		orderValidator.Validate(o2).Print();
		orderValidator.Validate(o3).Print();
		orderValidator.Validate(o4).Print();
		orderValidator.Validate(o5).Print();
		orderValidator.Validate(o6).Print();

		Console.WriteLine("--- ValidateSuperOrders() ---");

		var s1 = new SuperOrder { Id = "SO501", Amount = 5, TotalPrice = 42, Comment = "Super order 1" };
		var s2 = new SuperOrder { Id = "SO502", Amount = 700, TotalPrice = 41, Comment = "Super order 2" };
		var s3 = new SuperOrder { Id = "", Amount = 800, Comment = "Super order 2" };

		var orders = new List<SuperOrder> { s1, s2, s3 };
		ValidateSuperOrders(orders, orderValidator);

		Console.WriteLine("--- ValidateAll() ---");

		ValidateAll(orders, orderValidator);

		Console.WriteLine("--- ValidateAll<SuperOrder>() ---");

		ValidateAll<SuperOrder>(orders, orderValidator);
	}

	static void ValidateSuperOrders(IEnumerable<SuperOrder> orders, Validator<Order> validator)
	{
		foreach (var o in orders)
		{
			validator.Validate(o).Print();
		}
	}

	static void ValidateAll<T>(IEnumerable<T> orders, Validator<Order> validator) where T : Order
	{
		foreach (var o in orders)
		{
			validator.Validate(o).Print();
		}
	}
}
