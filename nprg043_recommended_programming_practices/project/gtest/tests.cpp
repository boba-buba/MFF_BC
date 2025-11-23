#include <vector>
#include <cstring>
#include <string>
#include <ostream>

#include <gtest/gtest.h>

#include "cmd_args_parser.hpp"
#include "cmd_parser_exceptions.hpp"

using namespace cmd_args_parser;

/* TESTS_25 */

char** vector_to_char_arr(const std::vector<std::string> & vector) {
    char** arr = new char*[vector.size() + 1];
    for (std::size_t i = 0; i < vector.size(); ++i) {
        arr[i] = new char[vector[i].length() + 1];
        std::strcpy(arr[i], vector[i].c_str());
    }
    arr[vector.size()] = nullptr;
    return arr;
}

void test_flag(
        const std::vector<std::string> & flag_names,
        const std::vector<std::vector<std::string>> & aliases,
        const std::vector<std::string> & arguments,
        const std::vector<bool> & is_optional,
        const std::vector<bool> & expected_is_present,
        bool expected_exception
) {
    CmdParser parser = CmdParser("parser");

    std::vector<Option*> flags = {};
    flags.reserve(20);
    for (int i = 0; i < flag_names.size(); ++i) {
        if (i >= aliases.size() || aliases[i].size() == 0) {
            auto& flag = parser.defineOptionWithoutParam(flag_names[i]).required(!is_optional[i]).buildOptionWithoutParam();
            flags.push_back(&flag);
        }
        else if (aliases[i].size() == 1) {
            auto& flag = parser.defineOptionWithoutParam(flag_names[i]).addSynonym(aliases[i][0]).required(!is_optional[i]).buildOptionWithoutParam();
            flags.push_back(&flag);
        }
        else if (aliases[i].size() == 2) {
            auto& flag = parser.defineOptionWithoutParam(flag_names[i]).addSynonym(aliases[i][0]).addSynonym(aliases[i][1]).required(!is_optional[i]).buildOptionWithoutParam();
            flags.push_back(&flag);
        }
    }
    try {
        parser.parseCommand(arguments.size(), vector_to_char_arr(arguments));

        if (expected_exception) {
            EXPECT_TRUE(false);
            return;
        }
    } catch (...) {
        if (! expected_exception) {
            ASSERT_TRUE(false);
        } else {
            return;
        }
    }

    for (int i = 0; i < flag_names.size(); ++i) {
        ASSERT_EQ(flags[i]->isPresent(), expected_is_present[i]);
    }
}

TEST(Flag, OptionalPresent) {
    test_flag({"flag"}, {{"f"}}, {"--flag"}, {true}, {true}, false);
    test_flag({"flag"}, {{"f"}}, {"-f"}, {true}, {true}, false);
}

TEST(Flag, OptionalNotPresent) { // undefined option is an exception
    test_flag({"flag"}, {{"f"}}, {"parser"}, {true}, {false}, false);
    test_flag({"flag"}, {{"f"}}, {"--not_flag"}, {true}, {false}, true);
    test_flag({"flag"}, {{}}, {"-f"}, {true}, {false}, true);
}

TEST(Flag, RequiredPresent) {
    test_flag({"flag"}, {{"f"}}, {"--flag"}, {true}, {true}, false);
    test_flag({"flag"}, {{"f"}}, {"-f"}, {true}, {true}, false);
}

TEST(Flag, RequiredNotPresent) {
    test_flag({"flag"}, {{"f"}}, {"--not_flag"}, {true}, {false}, true);
    test_flag({"flag"}, {{}}, {"-f"}, {true}, {false}, true);
}

TEST(Flag, Multiple) {
    test_flag({"flag1", "flag2", "flag3"}, {{"f1"}, {"f2", "f22"}, {}}, {"--flag3", "--f2"}, {true, true, false}, {false, true, true}, false);
    test_flag({"flag1", "flag2", "flag3"}, {{"f"}, {"f2", "f22"}, {}}, {"-f", "--f22"}, {false, true, true}, {true, true, false}, false);
}

TEST(Flag, MultipleMatches) {
    test_flag({"flag1"}, {{}}, {"--flag1", "--flag1"}, {true}, {true}, false);
    test_flag({"flag1"}, {{}}, {"--flag1", "--flag1"}, {false}, {true}, false);
}

void test_option(
        const std::vector<std::string> & option_names,
        const std::vector<std::vector<std::string>> & aliases,
        const std::vector<std::string> & arguments,
        const std::vector<bool> & is_value_optional,
        const std::vector<bool> & expected_is_present,
        const std::vector<std::vector<std::string>> & expected_values,
        bool expected_exception
) {
    CmdParser parser = CmdParser("parser");

    std::vector<Option*> options = {};
    options.reserve(20);
    for (int i = 0; i < option_names.size(); ++i) {
        if (aliases[i].size() == 0) {
            auto& option = parser.defineOptionWithParam(option_names[i], String()).paramRequired(!is_value_optional[i]).buildOptionWithParam();
            options.push_back(&option);
        }
        else if (aliases[i].size() == 1) {
            auto& option = parser.defineOptionWithParam(option_names[i], String()).addSynonym(aliases[i][0]).paramRequired(!is_value_optional[i]).buildOptionWithParam();
            options.push_back(&option);
        }
        else if (aliases[i].size() == 2) {
            auto& option = parser.defineOptionWithParam(option_names[i], String()).addSynonym(aliases[i][0]).addSynonym(aliases[i][1]).paramRequired(!is_value_optional[i]).buildOptionWithParam();
            options.push_back(&option);
        }
    }

    try {
        parser.parseCommand(arguments.size(), vector_to_char_arr(arguments));

        if (expected_exception) {
            EXPECT_TRUE(false);
            return;
        }
    } catch (...) {
        if (! expected_exception) {
            ASSERT_TRUE(false);
        } else {
            return;
        }
    }

    for (int i = 0; i < option_names.size(); ++i) {
        ASSERT_EQ(options[i]->isPresent(), expected_is_present[i]);
        std::vector<std::string> values = options[i]->getValues<std::string>();
        ASSERT_EQ(expected_values[i].size(), values.size());
        ASSERT_EQ(expected_values[i], values);
    }
}

TEST(Value, OptionalPresent) {
    test_option({"option"}, {{"o"}}, {"--option=value"}, {true}, {true}, {{"value"}}, false);
    test_option({"option"}, {{"o"}}, {"-o", "value"}, {true}, {true}, {{"value"}}, false);
}

TEST(Value, OptionalNotPresent) {
    test_option({"option"}, {{"o"}}, {"--option"}, {true}, {true}, {{}}, false);
    test_option({"option"}, {{"o"}}, {"-o"}, {true}, {true}, {{}}, false);
    test_option({"option", "option2"}, {{"o"}, {}}, {"--option", "--option2=not_my_value"}, {true, true}, {true, true}, {{}, {"not_my_value"}}, false);
}

TEST(Value, RequiredPresent) {
    test_option({"option"}, {{"o"}}, {"--option=value"}, {false}, {true}, {{"value"}}, false);
    test_option({"option"}, {{"o"}}, {"-o", "value"}, {false}, {true}, {{"value"}}, false);
}

TEST(Value, RequiredNotPresent) {
    test_option({"option"}, {{"o"}}, {"--option"}, {false}, {false}, {{"value"}}, true);
    test_option({"option"}, {{"o"}}, {"-o"}, {false}, {false}, {{"value"}}, true);
    test_option({"option", "option2"}, {{"o"}, {}}, {"--option", "--option2=not_my_value"}, {false, false}, {}, {}, true);
}

TEST(Value, MultipleValues) {
    test_option({"option1"}, {{"o"}}, {"--option1=value1", "--option1=value2"}, {true}, {true}, {{"value1", "value2"}}, false);
    test_option({"option1"}, {{"o"}}, {"--option1=value1", "-o", "value2"}, {true}, {true}, {{"value1", "value2"}}, false);
    test_option({"option1", "option2"}, {{"o"}, {"p"}}, {"--option1=value1", "-p", "value2", "-o", "value3"}, {true, true}, {true, true}, {{"value1", "value3"}, {"value2"}}, false);
}

void test_plain(
        int plain_argument_count,
        const std::vector<std::string> & arguments,
        const std::vector<std::string> & expected_values,
        bool expected_exception
) {
    CmdParser parser = CmdParser("parser");

    std::vector<Plain*> plains = {};
    plains.reserve(20);
    for (int i = 0; i < plain_argument_count; ++i) {
        auto& plain = parser.definePlainArg(std::to_string(i), String()).buildPlain();
        plains.push_back(&plain);
    }

    try {
        parser.parseCommand(arguments.size(), vector_to_char_arr(arguments));

        if (expected_exception) {
            EXPECT_TRUE(false);
            return;
        }
    } catch (...) {
        if (! expected_exception) {
            ASSERT_TRUE(false);
        } else {
            return;
        }
    }

    for (int i = 0; i < plain_argument_count; ++i) {
        ASSERT_EQ(plains[i]->getFirstValue<std::string>(), expected_values[i]);
    }
}

TEST(Plain, Present) {
    test_plain(1, {"plain"}, {"plain"}, false);
    test_plain(2, {"plain1", "plain2"}, {"plain1", "plain2"}, false);
}

TEST(Plain, NotPresent) {
    test_plain(1, {}, {}, true);
    test_plain(2, {"plain1"}, {}, true);
}

TEST(Plain, MorePresent) {
    test_plain(1, {"plain", "extra_value"}, {"plain"}, true);
    test_plain(2, {"plain1", "plain2", "extra_value"}, {"plain1", "plain2"}, true);
}

TEST(Plain, AfterFlag) {
    CmdParser parser = CmdParser("parser");

    auto& flag = parser.defineOptionWithoutParam("flag").buildOptionWithoutParam();

    auto& plain = parser.definePlainArg("plain", String()).buildPlain();

    parser.parseCommand(2, vector_to_char_arr({"--flag", "plain_value"}));

    ASSERT_EQ(plain.getFirstValue<std::string>(), "plain_value");
}

void test_plain_after_option(bool is_required) {
    CmdParser parser = CmdParser("parser");

    auto& option = parser.defineOptionWithParam("option", String()).paramRequired(is_required).buildOptionWithParam();

    auto& plain = parser.definePlainArg("plain", String()).buildPlain();

    parser.parseCommand(2, vector_to_char_arr({"--option=option_value", "plain_value"}));

    EXPECT_EQ(plain.getFirstValue<std::string>(), "plain_value");
}

TEST(Plain, AfterRequiredOptionValue) {
    test_plain_after_option(true);
}

TEST(Plain, AfterOptionalOptionValue) {
    test_plain_after_option(false);
}

TEST(Type, Int) {
    CmdParser parser = CmdParser("parser");

    auto& plain = parser.definePlainArg("plain", Int()).buildPlain();

    parser.parseCommand(1, vector_to_char_arr({"155"}));

    EXPECT_EQ(plain.getFirstValue<int>(), 155);
}


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

TEST(Type, Double) {
    CmdParser parser = CmdParser("parser");

    auto& plain = parser.definePlainArg("plain", UserProgram::Double()).buildPlain();

    parser.parseCommand(1, vector_to_char_arr({"120.3"}));

    EXPECT_EQ(plain.getFirstValue<double>(), 120.3);
}

TEST(Type, IntThenDouble) {
    CmdParser parser = CmdParser("parser");

    auto& plainInt = parser.definePlainArg("plain_int", Int()).buildPlain();

    auto& plainDouble = parser.definePlainArg("plain_double", UserProgram::Double()).buildPlain();

    parser.parseCommand(2, vector_to_char_arr({"150", "188.4"}));

    EXPECT_EQ(plainInt.getFirstValue<int>(), 150);
    EXPECT_EQ(plainDouble.getFirstValue<double>(), 188.4);
}

/* TESTS_29 */


using namespace std;

auto& ArrangePlainArg(CmdParser& m, string argName) { // plain always required
    return m.definePlainArg(argName, String()).buildPlain();
}

auto& ArrangeOption(CmdParser& m, string longOptionName) {
    string startingChar; 
    startingChar += longOptionName[0];
    return m.defineOptionWithParam(longOptionName, String()).addSynonym(startingChar).required(false).paramRequired(false).buildOptionWithParam();
}

TEST(ExampleTask1, IsValid) {
    CmdParser m = CmdParser("test");
    auto& verbose = m.defineOptionWithoutParam("v").buildOptionWithoutParam();

    auto& version = m.defineOptionWithoutParam("version").buildOptionWithoutParam();

    auto& s = m.defineOptionWithParam("s", String()).buildOptionWithParam();

    auto& length = m.defineOptionWithParam("length", Int()).buildOptionWithParam();

    auto& my_filename = m.definePlainArg("my-file", String()).buildPlain();

    auto& your_filename = m.definePlainArg("your-file", String()).buildPlain();

    EXPECT_NO_THROW(m.parseCommand(8, vector_to_char_arr({"-v", "--version", "-s", "OLD", "--length=20", "--", "-my-file", "your-file"})));
    bool verbose_present = verbose.isPresent();
    bool version_present = version.isPresent();
    bool s_present = s.isPresent();
    bool length_present = length.isPresent();

    EXPECT_TRUE(verbose_present && version_present && s_present && length_present);
}

TEST(ShortOption, Used_BoolSaysIsPresent) {
    CmdParser m = CmdParser("test");
    auto& append = m.defineOptionWithoutParam("a").buildOptionWithoutParam();

    m.parseCommand(1, vector_to_char_arr({"-a"}));
    bool append_present = append.isPresent();

    EXPECT_TRUE(append_present);
}

TEST(ShortOption, NotUsed_BoolSaysNotPresent) {
    CmdParser m = CmdParser("test");
    auto& append = m.defineOptionWithoutParam("a").required(false).buildOptionWithoutParam();
    auto& list = m.defineOptionWithoutParam("l").buildOptionWithoutParam();

    m.parseCommand(1, vector_to_char_arr({"-l"}));
    bool append_present = append.isPresent();

    EXPECT_FALSE(append_present);
}

TEST(ShortOption, WrongPrefix_No_Throw) { // long and short options are defined by - and -- prefixes, not by length
    CmdParser m = CmdParser("test");
    auto& append = m.defineOptionWithoutParam("a").buildOptionWithoutParam();

    EXPECT_NO_THROW(m.parseCommand(1, vector_to_char_arr({"--a"})));
    EXPECT_TRUE(append.isPresent());
}

TEST(ShortOption, DuplicitCommandLineOptions_No_Throw) {
    CmdParser m = CmdParser("test");

    auto& append = m.defineOptionWithoutParam("a").buildOptionWithoutParam();
    auto& list = m.defineOptionWithoutParam("l").buildOptionWithoutParam();

    EXPECT_NO_THROW(m.parseCommand(3, vector_to_char_arr({"-l", "-l", "-a"})));
}


TEST(LongOption, Used_IsPresent) {
    CmdParser m = CmdParser("test");
    auto& append = m.defineOptionWithoutParam("append").buildOptionWithoutParam();
    m.parseCommand(1, vector_to_char_arr({"--append"}));
    bool append_present = append.isPresent();

    EXPECT_TRUE(append_present);
}

TEST(LongOption, NotUsed_IsNotPresent) {
    CmdParser m = CmdParser("test");
    auto& append = m.defineOptionWithoutParam("append").required(false).buildOptionWithoutParam();
    auto& list = m.defineOptionWithoutParam("list").buildOptionWithoutParam();
    m.parseCommand(1, vector_to_char_arr({"--list"}));
    bool append_present = append.isPresent();

    EXPECT_FALSE(append_present);
}

TEST(LongOption, WrongPrefix_No_Throw) {
    CmdParser m = CmdParser("test");

    auto& append = m.defineOptionWithoutParam("append").required(false).buildOptionWithoutParam();

    EXPECT_NO_THROW(m.parseCommand(1, vector_to_char_arr({"-append"})));
    EXPECT_TRUE(append.isPresent());
}

TEST(LongOption, DuplicitCommandLineOptions_No_Throw) {
    CmdParser m = CmdParser("test");

    auto& append = m.defineOptionWithoutParam("append").required(false).buildOptionWithoutParam();
    auto& list = m.defineOptionWithoutParam("l").required(false).buildOptionWithoutParam();
    EXPECT_NO_THROW(m.parseCommand(3, vector_to_char_arr({"-l", "--append", "--append"})));
}


TEST(LongOptionParametrized, CorrectValue_Valid) {
    CmdParser m = CmdParser("test");

    std::vector<std::string> time_zones = { "UTC", "PST", "EST" };
    auto& zone = m.defineOptionWithParam("zone", Enum(time_zones)).buildOptionWithParam();

    m.parseCommand(1, vector_to_char_arr({"--zone=UTC"}));

    EXPECT_EQ("UTC", zone.getLastValue<std::string>());
}

    // Parsing exception because not in the Enum list
TEST(LongOptionParametrized, IncorrectValue_Invalid) {
    CmdParser m = CmdParser("test");

    std::vector<std::string> time_zones = { "UTC", "PST", "EST" };
    auto& zone = m.defineOptionWithParam("zone", Enum(time_zones)).buildOptionWithParam();

    ASSERT_THROW(m.parseCommand(1, vector_to_char_arr({"--zone=GMT+2"})), ParsingExceptionInvalidValue);
}

TEST(LongOptionParametrized, WrongPrefix_Throws) {
    CmdParser m = CmdParser("test");

    std::vector<std::string> time_zones = { "UTC", "PST", "EST" };
    auto& zone = m.defineOptionWithParam("zone", Enum(time_zones)).buildOptionWithParam();

    ASSERT_THROW(m.parseCommand(1, vector_to_char_arr({"-zone=GMT+2"})), CmdParserException);
}


TEST(OptionParametrizedSynonyms, MultipleDefinitions_No_Throw) {
    CmdParser m = CmdParser("test");
    std::vector<std::string> time_zones = { "UTC", "PST", "EST" };

    auto& zone = m.defineOptionWithParam("zone", Enum(time_zones)).addSynonym("z").buildOptionWithParam();

    EXPECT_NO_THROW(m.parseCommand(3, vector_to_char_arr({"--zone=UTC", "-z", "UTC"})));
}

TEST(OptionParametrizedSynonyms, ShortOptionAndLongOption_SameValue) {
    CmdParser m = CmdParser("test");
    CmdParser m2 = CmdParser("test2");
    std::vector<std::string> time_zones = { "UTC", "PST", "EST" };


    auto& zone = m.defineOptionWithParam("zone", Enum(time_zones)).addSynonym("z").buildOptionWithParam();

    auto& zone2 = m2.defineOptionWithParam("zone", Enum(time_zones)).addSynonym("z").buildOptionWithParam();

    m.parseCommand(1, vector_to_char_arr({"--zone=UTC"}));
    m2.parseCommand(2, vector_to_char_arr({"-z", "UTC"}));
    string zone_value = zone.getLastValue<string>();
    string zone2_value = zone2.getLastValue<string>();

    EXPECT_EQ(zone_value, zone2_value);
}


// If validation is wrong, it should throw
TEST(IntOptionParametrized, ValidInt_Validates) {
    CmdParser m = CmdParser("test");


    auto& cores = m.defineOptionWithParam("cores", Int()).buildOptionWithParam();

    m.parseCommand(1, vector_to_char_arr({"--cores=9"}));
    bool cores_present = cores.isPresent();

    EXPECT_EQ(true, cores_present);
}

TEST(IntOptionParametrized, ValidInt_WrongInput) {
    CmdParser m = CmdParser("test");

    auto& cores = m.defineOptionWithParam("cores", Int()).buildOptionWithParam();

    ASSERT_THROW(m.parseCommand(1, vector_to_char_arr({"--cores=nine"})), ParsingExceptionInvalidValue);
}


TEST(SimplePlainArgument, IsPresent_Valid) {
    CmdParser m = CmdParser("test");
    auto& format = ArrangeOption(m, "format");
    auto& portability = ArrangeOption(m, "portability");
    auto& filename = ArrangePlainArg(m, "filename");

    EXPECT_NO_THROW(m.parseCommand(3, vector_to_char_arr({"--format=exe", "--", "-file.txt"})));
}

TEST(SimplePlainArgument, IsNotPresent_Invalid) {
    CmdParser m = CmdParser("test");
    auto& format = ArrangeOption(m, "format");
    auto& portability = ArrangeOption(m, "portability");
    auto& filename = ArrangePlainArg(m, "filename");
    EXPECT_THROW(m.parseCommand(1, vector_to_char_arr({"--format=exe"})), ParsingExceptionMandatory);
}

TEST(SimplePlainArgument, Required_IsPresent_Valid) {
    CmdParser m = CmdParser("test");
    auto& format = ArrangeOption(m, "format");
    auto& portability = ArrangeOption(m, "portability");
    auto& filename = ArrangePlainArg(m, "filename");

    EXPECT_NO_THROW(m.parseCommand(3, vector_to_char_arr({"--format=exe", "--", "-file.txt"})));
}

TEST(SimplePlainArgument, Required_NotPresent_Throws) {
    CmdParser m = CmdParser("test");
    auto& format = ArrangeOption(m, "format");
    auto& portability = ArrangeOption(m, "portability");
    auto& filename = ArrangePlainArg(m, "filename");

    ASSERT_THROW(m.parseCommand(1, vector_to_char_arr({"--format=exe"})), ParsingExceptionMandatory);
}


void AddOptionToParser_HelpMsg(CmdParser& m, string optionName, string helpString, bool parametrizedStr = false) {
    if (parametrizedStr) {
        auto& opt = m.defineOptionWithParam(optionName, String()).addDescription(helpString).required(false).buildOptionWithParam();
    } else {
        auto& opt = m.defineOptionWithoutParam(optionName).addDescription(helpString).required(false).buildOptionWithoutParam();
    }
}

TEST(ProduceHelpMessage, ShortOption_Valid) { // we have different format for help, it includes version, name of the program and list of all options and arguments
    CmdParser m = CmdParser("test");
    string helpString = "Help description for -a option";
    AddOptionToParser_HelpMsg(m, "a", helpString);
    ostringstream str;
    m.help(str);

    EXPECT_TRUE(str.str().find(helpString) != std::string::npos);
}

TEST(ProduceHelpMessage, LongOption_Valid) {
    CmdParser m = CmdParser("test");
    string helpString = "Help description for --append option";
    AddOptionToParser_HelpMsg(m, "append", helpString);
    ostringstream str;
    m.help(str);

    EXPECT_TRUE(str.str().find(helpString) != std::string::npos);
}

TEST(ProduceHelpMessage, ShortOptionParametriezd_Valid) {
    CmdParser m = CmdParser("test");
    string helpString = "Help description for -a option with parameter";
    AddOptionToParser_HelpMsg(m, "a", helpString, true);
    ostringstream str;
    m.help(str);
    
    EXPECT_TRUE(str.str().find(helpString) != std::string::npos);
}

TEST(ProduceHelpMessage, LongOptionParametrized_Valid) {
    CmdParser m = CmdParser("test");
    string helpString = "Help description for --append option with parameter";
    AddOptionToParser_HelpMsg(m, "append", helpString, true);
    ostringstream str;
    m.help(str);
    
    EXPECT_TRUE(str.str().find(helpString) != std::string::npos);
}

TEST(ProduceHelpMessage, MultipleLongOptionsParametrized_Valid) {
    CmdParser m = CmdParser("test");
    string helpStringAppend = "Help description for --append option with parameter";
    string helpStringList = "Help description for --list option with parameter";
    AddOptionToParser_HelpMsg(m, "append", helpStringAppend, true);
    AddOptionToParser_HelpMsg(m, "list", helpStringList, true);

    ostringstream str;
    m.help(str);

    EXPECT_TRUE(str.str().find(helpStringAppend) != std::string::npos);
    EXPECT_TRUE(str.str().find(helpStringList) != std::string::npos);
}

// Second option is required, and will be missing in tests for "RequiredOption" which have "Missing" in its name
CmdParser ArrangeParserHelptextThrow(string optionName1, string optionName2, bool isStringValidation = false) {
    CmdParser m = CmdParser("test");
    string helpString1 = "Help description for " + optionName1 + " option";
    string helpString2 = "Help description for " + optionName2 + " option";


    if (isStringValidation) {
        auto& opt1 = m.defineOptionWithParam(optionName1, String()).addDescription(helpString1).buildOptionWithParam();
    } else {
        auto& opt1 = m.defineOptionWithoutParam(optionName1).addDescription(helpString1).buildOptionWithoutParam();
    }

    if (isStringValidation) {
        auto& opt2 = m.defineOptionWithParam(optionName2, String()).addDescription(helpString2).buildOptionWithParam();
        } else {
        auto& opt2 = m.defineOptionWithoutParam(optionName2).addDescription(helpString2).buildOptionWithoutParam();
    }

    return m;
}

TEST(RequiredOption, ShortOptionMissing_Throws) {
    CmdParser m = ArrangeParserHelptextThrow("v", "l");

    ASSERT_THROW(m.parseCommand(1, vector_to_char_arr({"-v"})), ParsingExceptionMandatory);
}

TEST(RequiredOption, LongOptionMissing_Throws) {
    CmdParser m = ArrangeParserHelptextThrow("append", "list");

    ASSERT_THROW(m.parseCommand(1, vector_to_char_arr({"--append"})), ParsingExceptionMandatory);
}

TEST(RequiredOption, ShortOptionParametrizedMissing_Throws) {
    CmdParser m = ArrangeParserHelptextThrow("a", "l", true);

    ASSERT_THROW(m.parseCommand(2, vector_to_char_arr({"-a", "file.txt"})), ParsingExceptionMandatory);
}

TEST(RequiredOption, LongOptionParametrizedMissing_Throws) {
    CmdParser m = ArrangeParserHelptextThrow("append", "list", true);

    ASSERT_THROW(m.parseCommand(1, vector_to_char_arr({"--append=file.txt"})), ParsingExceptionMandatory);
}

class OptionWhitespace : public ::testing::TestWithParam<vector<string>> {
protected:
};
// parser gets char**, so it does not handle whitespaces..
/*
vector<string> inputStringsNonParamSuite = {
    "     --format=exe --portability=false",
    "--format=exe --portability=false    ",
    "    --format=exe --portability=false    ",
    "    --format=exe      --portability=false    ",
    "    -fexe -pfalse",
    "-fexe -pfalse    ",
    "    --fexe -pfalse    ",
    "    -fexe    -pfalse    "
};
*/
// I rewrote it to satisfy the API
vector<vector<string>> inputStringsNonParamSuite = {
    {"--format=exe", "--portability=false"},
    {"-f", "exe", "-p", "false"},
    {"--f=exe", "-p", "false"}
};
INSTANTIATE_TEST_SUITE_P(WhitespaceTests, OptionWhitespace, ::testing::ValuesIn(inputStringsNonParamSuite));

TEST_P(OptionWhitespace, TestDifferentSpaces_Valid) {
    vector<string> input = GetParam();
    CmdParser m = CmdParser("test");
    auto& format = ArrangeOption(m, "format");
    auto& portability = ArrangeOption(m, "portability");

    m.parseCommand(input.size(), vector_to_char_arr(input));
    bool format_present = format.isPresent();

    EXPECT_TRUE(format_present);
}


auto& ArrangeOptionParametrized(CmdParser& m, string longOptionName) {
    string startingChar; 
    startingChar += longOptionName[0];
    return m.defineOptionWithParam(longOptionName, String()).addSynonym(startingChar).required(false).buildOptionWithParam();
}

class OptionWhitespaceParametrized : public ::testing::TestWithParam<vector<string>> {
protected:
};
// parser gets char**, so it does not handle whitespaces...
/*
vector<string> inputStringsParametrizedSuite = {
    "     --format=exe --portability=false",
    "--format=exe --portability=false    ",
    "    --format=exe --portability=false    ",
    "    --format=exe      --portability=false    ",
    "    -fexe -pfalse",
    "-fexe -pfalse    ",
    "    --fexe -pfalse    ",
    "    -fexe    -pfalse    "
};
*/
// I rewrote it to satisfy the API
vector<vector<string>> inputStringsParametrizedSuite = {
    {"--format=exe", "--portability=false"},
    {"-f", "exe", "-p", "false"}
};

INSTANTIATE_TEST_SUITE_P(WhitespaceTests, OptionWhitespaceParametrized, ::testing::ValuesIn(inputStringsParametrizedSuite));

TEST_P(OptionWhitespaceParametrized, TestDifferentSpaces_Valid) {
    vector<string> input = GetParam();

    CmdParser m = CmdParser("test");
    auto& format = ArrangeOption(m, "format");
    auto& portability = ArrangeOption(m, "portability");

    m.parseCommand(input.size(), vector_to_char_arr(input));
    bool format_present = format.isPresent();
    EXPECT_TRUE(format_present);
}
class PlainArgument : public ::testing::TestWithParam<vector<string>> {
protected:
};

vector<vector<string>> inputStringPlainArgValid = {
    {"--format=exe", "--portability=false", "--", "-file.txt"},
    {"-f", "exe", "-p", "false", "--", "-file.txt"},
    {"-f", "exe", "--portability=false", "--", "-file.txt"},
    {"--format=exe", "-p", "false", "--", "-file.txt"}
};

INSTANTIATE_TEST_SUITE_P(WhitespaceTests, PlainArgument, ::testing::ValuesIn(inputStringPlainArgValid));

TEST_P(PlainArgument, ArgPresent_Valid) {
    vector<string> input = GetParam();
    CmdParser m = CmdParser("test");
    auto& format = ArrangeOption(m, "format");
    auto& portability = ArrangeOption(m, "portability");
    auto& filename = ArrangePlainArg(m, "filename");

    EXPECT_NO_THROW(m.parseCommand(input.size(), vector_to_char_arr(input)));
    //bool filename_present = filename.isPresent(); // if not present, throws
    //EXPECT_TRUE(filename_present);
}
