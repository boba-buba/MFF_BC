# cmd_args_parser

`cmd_args_parser` is a C++ library that provides an interface for configuring the command line options and arguments that a program can receive. It also provides an interface to parse command-line options and arguments according to configuration.

## Example

try: `time -p output.txt --seconds=25.3 -s 23.5 -f 23`

```cpp
#include "cmd_args_parser.h"
namespace UserProgram {

    double convertToDouble(const std::string& value) { return std::stod(value); }
    class Double : public ValueType {
    public:
        Double() { converter = convertToDouble; }
        bool virtual validate(const std::string& val) const
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
}

int main(int argc, char** argv)
{
    /* Create configuration model */
    CmdParser m = CmdParser("time");
    auto format_number = Int();
    fromat_number.setUpperBound(24);
    fromat_number.setLowerBound(0);
    /* Options and arguments definition */
    auto& format = m.defineOptionWithParam("format", std::move(format))
        .addSynonym("f")
        .addDescription("Specify output format, possibly overriding the format specified in the environment variable TIME.")
        .required(false)
        .paramRequired(true)
        .buildOptionWithParam();
    auto& portability = m.defineOptionWithoutParam("portability")
        .addSynonym("p")
        .addDescription("Use the portable output format.")
        .required(false)
        .buildOptionWithoutParam();
    auto& output = m.definePlainArg("output", String())
        .buildPlain();
    auto& seconds = m.defineOptionWithParam("seconds", UserProgram::Double())
        .addSynonym("s")
        .required(false)
        .buildOptionWithParam();

    m.setVersion("v1.0.0");

    if (argc == 1) {
        m.help();
        return 1;
    }
    /* Parsing */
    try
    {
        m.parseCommand(argc, argv);
    }
    catch (CmdParserException& e)
    {
        std::cout << e.what() << std::endl;
        return 1;
    }
    /* Retrieve values */
    if (format)
    {
        std::cout << "You chose this format: " << format.getFirstValue<int>() << std::endl;
    }
    if (portability.isPresent())
    {
        std::cout << "Portable format is used" << std::endl;
    }

    std::cout << "Output file is " << output.getValue<std::string>() << std::endl;

    if (seconds)
    {
        std::cout << "Sum of seconds is " << seconds.getFirstValue<double>() + seconds.getLastValue<double>() << std::endl;
    }
    return 0;
}
```
For more tutorials, see [API Documentation](./APIdoc.md).
For detailed example, see [time subproject](./time/time.cpp).

## Building
### Building project on Fedora 39
Every subproject has its own Makefile for building. There is a Makefile in root directory, that generates documentation, start tests and automatically build every subproject.

```bash
make #builds every subproject
```

#### Generating documentation

```bash
make docs
```
Generating the documentation. New folder `./lib/docs` is created. Open file `./lib/docs/html/index.html` in your browser to look through documenation.

#### Rebuilding project

```bash
make clean # This will not delete test build
make
```
To rebuild the library, first you need to delete old build.

## Running tests

This project uses xUnit testing framework [Google Test](https://github.com/google/googletest).

```bash
# Download Google Test framework and build tests inside gtest/build directory
make test-build

# Run tests
make test

# Remove gtest/build directory
make test-clean
```