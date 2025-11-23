# CmdParser

The `cmd_args_parser` library provides functionality to parse command-line based on defined options and plain arguments. It allows users to define options with or without parameters, plain arguments, parse them and retrieve the values.

## Object design:

`CmdParser` Class

Responsibility: Represents a starting point for configuration and provides parsing functionality.

Instantiation: Through a public constructor that gets the name of the program.
Multiple independent instances of the CmdParser can be created.

`Option` Class

Responsibility: Represents an option with and without parameters.

Instantiation: Option is returned by reference from the OptionBuilder's BuildOption...() method.
Can't be copied or modified.

Notes: Short and long options are determined by the dash prefix, and not by the number of characters. All the synonyms are treated the same. So the "long" option can consist of only one character and the "short" can consist of multiple characters.

Consider the following example:
```cpp
auto& format = parser.defineOptionWithParam("format", String())
    .addSynonym("f")
    .paramRequired(false)
    .buildOptionWithParam();
```
The following commands are valid:
`--f=full`
`--f` since prameter is not required
`-format full`
And the following are invalid:
`-f=full` one dash -> short option, the value must be specified after whitespace
`--f full` since full is treated as a plain argument, but no plain argument was defined


`Plain` Class

Responsibility: Represents a plain argument.

Instantiation: Option is returned by reference from the OptionBuilder's BuildPlain() method.
Can't be copied or modified.

Notes: Plain arguments are mandatory. Once defined, they must be present on command line. If not, the ParsingExceptionMandatory is thrown during parsing.
In addition, their relative position on command line is significant. They must appear in order they were defined. If the type does not correspond, the ParsingExceptionInvalidValue is thrown.
Consequently, library does not support variable number of arguments.

The very first plain argument can't start with '.', because it will be treated as program name and will be skipped.

`OptionBuilder` Class

Responsibility: Configure and build options within the CmdParser class. It allows to define options with or without parameters and customize their properties such as type of parameter, synonyms, descriptions, and optionality.

Instantiation: Returned by CmdParser's DefineOption...(args) method.

`PlainBuilder` Class

Responsibility: Configure and build plain arguments within the CmdParser class. It allows to define plain arguments and customize their properties such as descriptions and type.

Instantiation: Returned by CmdParser's DefinePlainArg(args) method.

`Exceptions`

Option definition:
OptionNameException can occur if the option with the same name was already defined. It is thrown from buildOption...() method.
The uniqueness of plain arguments' names is not checked since they differ in positions, the name is used only for help message.

Parsing:
ParsingExceptionNoSuchEl, ParsingExceptionMandatory, ParsingExceptionInvalidArgument exceptions occur during parsing. If an exception occurs, parsing stops and access to option and argument values results in undefined behaviour.
If parsing completes successfully, all defined options and arguments are guaranteed to be valid.


## Data types

`ValueType` Class

The `ValueType` class is an abstract base class representing a type of value that can be parsed from the command line. 

`Int` Class

Responsibility: Represents the integer type.

Methods:

validate(const std::string& val): Verifies that the passed value is of the correct integer type and within the range of allowed values.

setUpperBound(upperBound): Sets the upper bound for the integer value.

setLowerBound(lowerBound): Sets the lower bound for the integer value.

`String` Class

Responsibility: Represents the string type.

Methods: 

validate(const std::string& val): Verifies that the passed value is of the string type.

`Bool` Class

Responsibility: Represents the boolean type.

Methods: 

validate(bool val): Verifies that the passed value is of the boolean type.

`Enum` Class

Responsibility: Represents the enumeration type with a fixed domain of values.
Constructed with a vector of strings containing the allowed values for the enumeration.

Methods:

validate(const std::string& val): Verifies that the passed value is one of the allowed values in the enumeration's domain.

### Design considerations:

Introducing custom value types instead of relying on built-in types:

- The ValueType class provides an abstract base for representing different types of values, this allows the addition of new value types by the user.
- Implementing custom validation logic within each value type class, enforcing specific constraints and rules for each type of value.
- Restriction to types derived from ValueType ensures type safety by allowing the user to use only certain types with well-defined behaviour in the context of the command line.

## Tutorial

### How to introduce a custom type:

To provide CmdParser with a custom type you should implement the following:
1. Custom class inherited from ValueType
   - It should override `validate(const std::string& val)` method, which returns boolean depending on validation result.
   - It should validate ability to convert string to custom type, bounds or any other restrictions, etc. The default implementation returns true.
   - Any other instance methods
2. Converter function with signature `typeToCovertTo convert(const std::string& value)` implementing conversion from string to custom type.
   - this function is assigned to converter property inside constructor of the custom class

Example:
```cpp
    double convertToDouble(const std::string& value) { return std::stod(value); } // converter function
    class Double : public ValueType {
    public:
        Double() { converter = convertToDouble; } // provided in constructor
        bool virtual validate(const std::string& val) const // validation logic
        {
            try
            {
                auto _ = std::stod(val);
            }
            catch (const std::exception& e)
            {
                return false;
            }
            return true;
        }
    };
```
Class Double inherits ValueType, class Day doesnt. Therefore option with class Day cannot be created.

```cpp
    CmdParser m = CmdParser("time");

    /* The second parameter must be a value of one of the types: Int, Bool, Enum or String defined in the cmd_args_parser namespace. */
    auto& outputFail = CmdParser::OptionBuilder("output-file", std::make_unique<UserProgram::Day>(UserProgram::Day())); // fails correctly
```

### Fixed domain and Option
- If you want to create option with parameters from fixed domain:

```cpp
    CmdParser m = CmdParser("time");

    std::vector<std::string> time_zones = {"GMT+1", "GMT+2", "GMT+3", "GMT+4"};
    auto& zone = CmdParser::OptionBuilder("zone", std::make_unique<Enum>(Enum(time_zones)))
        .addSynonym("z")
        .addDescription("Defines time zone.")
        .BuildOptionWithParam(m);
```
- To restrict domain for numerical type you can specify lower and upper bounds:

```cpp
    CmdParser m = CmdParser("time");

    auto h = Int();
    h.setUpperBound(24);
    h.setLowerBound(0);
    auto& hours = CmdParser::OptionBuilder("hours", std::make_unique<Int>(std::move(h)))
        .addSynonym("h")
        .BuildOptionWithParam(m);

```

### Parsing

Since the exception may occur during parsing, it is recommended to wrap it in a try-catch block:
```
try
{
    parser.parseCommand(argc, argv);
}
catch (CmdParserException& e)
{
    std::cout << e.what() << std::endl;
    return 1;
}
```
parseCommand iterates through the arguments and parses them sequentially. If the first argument is equal to the program name given in the CmdParser constructor, it is skipped.

### Stream for output

Stream can be provided, default is std::cout.

```cpp
    CmdParser m = CmdParser("time");

    std::ofstream os("file.txt");
    m.help(os);
    m.help();
```

For the detailed example program, see time/ subproject.