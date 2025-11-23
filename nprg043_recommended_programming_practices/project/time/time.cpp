#include <iostream>
#include "cmd_args_parser.hpp"

using namespace cmd_args_parser;

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
    // try: ./build/time output.txt -p --verbose --seconds=25.3 -a -s 23.5

    /* Create configuration model */
    CmdParser m = CmdParser("time");

    /* Options and arguments definition */
    auto f = m.defineOptionWithParam("format", String());
    auto& format = f
        .addSynonym("f")
        .addDescription("Specify output format, possibly overriding the format specified in the environment variable TIME.")
        .required(false)
        .paramRequired(false)
        .buildOptionWithParam();
    auto& portability = m.defineOptionWithoutParam("portability")
        .addSynonym("p")
        .addDescription("Use the portable output format.")
        .required(false)
        .buildOptionWithoutParam();

    auto& append = m.defineOptionWithoutParam("append")
        .addSynonym("a")
        .addDescription("(Used together with -o.) Do not overwrite but append.")
        .required(false)
        .buildOptionWithoutParam();

    auto& verbose = m.defineOptionWithoutParam("verbose")
        .addSynonym("v")
        .addSynonym("ver")
        .required(false)
        .addDescription("Give very verbose output about all the program knows about.")
        .buildOptionWithoutParam();

    auto& seconds = m.defineOptionWithParam("seconds", UserProgram::Double())
        .addSynonym("s")
        .required(false)
        .buildOptionWithParam();

    auto& output = m.definePlainArg("output", String())
        .buildPlain();

    m.setVersion("v1.0.0");

    if (argc == 1) {
        m.help();
        return 1;
    }

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
        std::cout << "You chose this format: " << format.getFirstValue<std::string>() << std::endl;
        std::cout << "You chose this format: " << format.getLastValue<std::string>() << std::endl;
    }

    if (portability.isPresent())
    {
        std::cout << "Portable format is used" << std::endl;
    }

    std::cout << "Output file is " << output.getFirstValue<std::string>() << std::endl;

    if (append)
    {
        std::cout << "File will not be overwritten" << std::endl;
    }
    if (verbose)
    {
        std::cout << "Very verbose output" << std::endl;
    }
    if (seconds)
    {
        std::cout << "Seconds are " << seconds.getFirstValue<double>() + seconds.getLastValue<double>() << std::endl;
    }

    return 0;
}