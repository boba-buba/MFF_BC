namespace IntegrationTests
{
    public class ValidatorsIntegrationTests
    {
        public static void ValidateSuperOrders(IEnumerable<SuperOrder> orders, Validator<Order> validator)
        {
            foreach (var o in orders)
            {
                validator.Validate(o).Print();
            }
        }

        public static void ValidateAll<T>(IEnumerable<T> orders, Validator<Order> validator) where T : Order
        {
            foreach (var o in orders)
            {
                validator.Validate(o).Print();
            }
        }
        
        [Fact]
        public void WholeExampleOutputTest()
        {
            //Arrange
            var expected = """
                --- plain Validators ---
                  >>> Validation failed >>>
                    > "   " is empty or just whitespaces.

                  >>> Validation successful >>>

                  >>> Validation failed >>>
                    > 7 is greater than maximum 6.

                  >>> Validation successful >>>

                  >>> Validation failed >>>
                    > "Jack" length 4 is less than minimum 5.

                  >>> Validation failed >>>
                    > "hello-world" length 11 is greater than maximum 6.

                  >>> Validation successful >>>

                  >>> Validation failed >>>
                    > "" is null.

                  >>> Validation failed >>>
                    > "" is null.

                  >>> Validation failed >>>
                    > "" is null.

                  >>> Validation successful >>>

                --- AdvancedOrderValidator.Validate() ---
                  >>> Validation failed >>>
                    > "    " is empty or just whitespaces.
                    > 0 is less than minimum 0,01.
                    > "" is null.

                  >>> Validation failed >>>
                    > 0 is less than minimum 0,01.
                    > "" is null.

                  >>> Validation failed >>>
                    > 600 is greater than maximum 10.
                    > 0 is less than minimum 0,01.
                    > "" is null.

                  >>> Validation failed >>>
                    > 600 is greater than maximum 10.
                    > "" is empty or just whitespaces.
                    > "" length 0 is less than minimum 1.
                    > 0 is less than minimum 0,01.
                    > "" is null.

                  >>> Validation failed >>>
                    > "AC405-12345678" length 14 is greater than maximum 8.

                  >>> Validation successful >>>

                --- OrderValidator.Validate() ---
                  >>> Validation failed >>>
                    > "    " is empty or just whitespaces.
                    > 0 is less than minimum 0,01.
                    > "" is null.

                  >>> Validation failed >>>
                    > 0 is less than minimum 0,01.
                    > "" is null.

                  >>> Validation failed >>>
                    > 600 is greater than maximum 10.
                    > 0 is less than minimum 0,01.
                    > "" is null.

                  >>> Validation failed >>>
                    > 600 is greater than maximum 10.
                    > "" is empty or just whitespaces.
                    > "" length 0 is less than minimum 1.
                    > 0 is less than minimum 0,01.
                    > "" is null.

                  >>> Validation failed >>>
                    > "AC405-12345678" length 14 is greater than maximum 8.

                  >>> Validation successful >>>

                --- ValidateSuperOrders() ---
                  >>> Validation successful >>>

                  >>> Validation failed >>>
                    > 700 is greater than maximum 10.

                  >>> Validation failed >>>
                    > 800 is greater than maximum 10.
                    > "" is empty or just whitespaces.
                    > "" length 0 is less than minimum 1.
                    > 0 is less than minimum 0,01.

                --- ValidateAll() ---
                  >>> Validation successful >>>

                  >>> Validation failed >>>
                    > 700 is greater than maximum 10.

                  >>> Validation failed >>>
                    > 800 is greater than maximum 10.
                    > "" is empty or just whitespaces.
                    > "" length 0 is less than minimum 1.
                    > 0 is less than minimum 0,01.

                --- ValidateAll<SuperOrder>() ---
                  >>> Validation successful >>>

                  >>> Validation failed >>>
                    > 700 is greater than maximum 10.

                  >>> Validation failed >>>
                    > 800 is greater than maximum 10.
                    > "" is empty or just whitespaces.
                    > "" length 0 is less than minimum 1.
                    > 0 is less than minimum 0,01.
                
                """;

            var sw = new StringWriter();
            Console.SetOut(sw);
            Console.SetError(sw);
            //Act
            Console.Write("--- plain Validators ---\n");

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
            string result = sw.ToString();
            //Assert
            Assert.Equal(expected, result);

        }
    }
}