#include <iostream>
#include <ostream>
#include <fstream>
#include <vector>
#include <utility>
#include <functional>

#include <gtest/gtest.h>

#include "cmd_args_parser.hpp"
#include "cmd_parser_exceptions.hpp"


using namespace std;
using namespace cmd_args_parser;

char** vector_to_char_arr(const vector<string> & vector) {
    char** arr = new char*[vector.size() + 1];
    for (size_t i = 0; i < vector.size(); ++i) {
        arr[i] = new char[vector[i].length() + 1];
        strcpy(arr[i], vector[i].c_str());
    }
    arr[vector.size()] = nullptr;
    return arr;
}

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
