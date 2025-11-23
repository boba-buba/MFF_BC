namespace DateTimeUnitTests;
using System;
using System.Globalization;

public class UnitTest1
{
    /// <summary>
    ///  method "Date parse(String)"
    /// </summary>
    /// <param name="input"></param>
    /// <param name="expected"></param>
    [Theory]
    [InlineData("01/10/2009 7:34 PM", "Saturday, January 10, 2009 7:34 PM")]
    [InlineData("10.01.2009 19:34", "Thursday, October 1, 2009 7:34 PM")]
    public void ParseMethodTest(string input, string expected)
    {
        DateTime dt;
        var culture = CultureInfo.CreateSpecificCulture("en-US");
        dt = DateTime.Parse(input, culture);

        Assert.Equal(expected, dt.ToString("f", culture));
    }

    /// <summary>
    /// the method "String format(Date)"
    /// </summary>
    /// <param name="input"></param>
    /// <param name="expected"></param>
    /// <param name="pattern"></param>
    [Theory]
    [InlineData("01/10/2009 7:34 PM", "01/10/2009 19:34", "MM/dd/yyyy HH:mm")]
    [InlineData("01/10/2009 7:34 PM", "01-10-2009 19:34", "MM-dd-yyyy HH:mm")]
    [InlineData("01/10/2009 7:34 PM", "10,01,2009 19.34", "dd,MM,yyyy HH.mm")]
    public void StringPatternInConstructorTest(string input, string expected, string pattern)
    {
        DateTime dt;
        dt = DateTime.Parse(input);

        Assert.Equal(expected, dt.ToString(pattern));
    }

    /// <summary>
    ///  Constructor that accepts "String pattern"
    /// </summary>
    /// <param name="input"></param>
    /// <param name="expected"></param>
    /// <param name="pattern"></param>
    [Theory]
    [InlineData("06/15/2008", "6/15/2008 12:00:00 AM", "MM/dd/yyyy")]
    [InlineData("10,01,2009 19.34", "1/10/2009 7:34:00 PM", "dd,MM,yyyy HH.mm")]
    [InlineData("20~02~2000, 12:09:09", "2/20/2000 12:09:09 PM", "dd~MM~yyyy, HH:mm:ss")]
    public void ConstructorAcceptingStringPatternsTests(string input, string expected, string pattern)
    {
        DateTime dt = DateTime.ParseExact(input, pattern, null);
        Assert.Equal(expected, dt.ToString());
    }
}