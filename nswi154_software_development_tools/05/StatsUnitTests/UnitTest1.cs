namespace StatsUnitTests;
using Moq;
using Xunit.Abstractions;
using System;
using System.Text;

public class UnitTest1
{

    private readonly ITestOutputHelper output;

    public UnitTest1(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void NoEmployeesEverythingMustBeZero()
    {
        //var employees = new Mock<Employees>();
        Employees employees = new Employees();
        Statistics st = new Statistics(employees);
        var sl = st.computeAverageSalary();
        var minSl = st.getMinSalary();
        Assert.Equal(0, sl);
        Assert.Equal(0, minSl);
    }

    [Fact]
    public void computeAverageSalaryTest()
    {
        Employees employees = new Employees();
        Statistics st = new Statistics(employees);
        var avgSalaray = st.computeAverageSalary();

        var empls = employees.getAll();
        var count = 0;
        var sumSalary = 0;
        var avgSalaryManually = 0;
        foreach(var id in empls) {
            count++;
            sumSalary += employees.getSalary(id);
        }
        if (count != 0) avgSalaryManually = sumSalary / count;

        Assert.Equal(avgSalaryManually, avgSalaray);
    }

    [Fact]
    public void getMinSalaryTest()
    {
        Employees employees = new Employees();
        Statistics st = new Statistics(employees);
        var minSalary = st.getMinSalary();

        var empls = employees.getAll();
        int minSalaryManually = 0;
        if (empls.Count != 0) minSalaryManually = employees.getSalary(empls.First());

        foreach(var id in empls) {
            var sal = employees.getSalary(id);
            if (minSalaryManually > sal) minSalaryManually = sal;
        }

        Assert.Equal(minSalaryManually, minSalary);
    }

    [Fact]
    public void printSalariesByNameTest()
    {
        Employees employees = new Employees();
        Statistics st = new Statistics(employees);

        var sw = new StringWriter();
        Console.SetOut(sw);
        Console.SetError(sw);
        st.printSalariesByName();
        string result = sw.ToString();

        StringBuilder sb = new StringBuilder();
        var ids = employees.getAll();
        Dictionary<int, string> staff = new Dictionary<int, string>();
        foreach (var id in ids) {
            staff.Add(id, employees.getName(id));
        }
        foreach (var pair in staff.OrderBy(pair => pair.Value)) {
            sb.Append($"{pair.Value} {employees.getSalary(pair.Key)}\n");
        }

        Assert.Equal(sb.ToString(), result);
    }
}