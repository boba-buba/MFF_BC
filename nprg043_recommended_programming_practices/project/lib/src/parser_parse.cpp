#include <string>
#include "cmd_args_parser.hpp"
#include "cmd_parser_exceptions.hpp"

namespace cmd_args_parser {

    class Option;
    class Plain;
    void addRange(std::vector<std::string>& v, std::string& beg, std::string& end)
    {
        int range_beg = std::stoi(beg);
        int range_end = std::stoi(end);
        if (range_end < range_beg)
        {
            std::swap(range_beg, range_end);
        }
        for (int i = range_beg; i < range_end; i++)
        {
            v.push_back(std::to_string(i));
        }
    }

    //TODO: check if ok and add to parseOptionWithParam
    //range is supported only for int types
    std::vector<std::string> CmdParser::getAllValues(std::string& arg)
    {
        std::vector<std::string> values;
        std::string val = "";
        bool is_range = false;
        std::string range_beg;
        std::string range_end;

        for (char c : arg)
        {
            if (c == ',')
            {
                if (is_range)
                {
                    range_end = val;
                    addRange(values, range_beg, range_end);
                    is_range = false;
                }
                values.push_back(val);
                val = "";
            }
            else if (c == '-')
            {
                is_range = true;
                range_beg = val;
                val = "";
            }
            else
            {
                val += c;
            }
        }
        if (val != "")
        {
            if (is_range)
            {
                range_end = val;
                addRange(values, range_beg, range_end);
            }
            values.push_back(val);
        }
        return values;
    }

    void CmdParser::checkMandatoryOnCmd()
    {
        //check if all mandatory options/plains present
        for (const auto& val : optionsWithoutParam_) {
            if (val.required_ && !val.isPresent()) {
                throw ParsingExceptionMandatory(val.getName());
            }
        }
        for (const auto& val : optionsWithParam_) {
            if (val.required_ && !val.isPresent()) {
                throw ParsingExceptionMandatory(val.getName());
            }
            if (val && val.paramRequired_ && !val.paramPresent_) {
                throw ParsingExceptionMandatory(val.getName());
            }
        }
        for (const auto& val : plains_) {
            if (!val.present_) {
                throw ParsingExceptionMandatory(val.getName());
            }
        }

    }

    void CmdParser::parseCommand(int argc, char** argv)
    {
        int i = 0;
        bool isDelPresent = false;
        int plainIdx = 0;
        if (argc > 0) {
            const std::string firstArg = argv[0];
            if (firstArg.compare(name_) == 0 || firstArg[0] == '.') { // skip program name
                i = 1;
            }
        }
        for (i; i < argc; i++) {
            std::string arg = argv[i];
            if (arg.compare("--") == 0) {
                isDelPresent = true;
            }
            else if (!isDelPresent && argv[i][0] == '-') {
                if (argv[i][1] == '-') {
                    //parse long
                    parseLong(argv[i] += 2);
                }
                else {
                    //parse short
                    if (i + 1 == argc || argv[i + 1][0] == '-') {
                        parseShort(argv[i] += 1, "");
                    }
                    else {
                        //if option with non mandatory param, and there is no such param and it tries to read next plain arg as param then error.
                        // with api: if -f f.txt then need to -f -- f.txt
                        parseShort(argv[i] += 1, argv[i + 1]);
                        i++;
                    }
                }
            }
            else {
                // parse plain
                // must be in defined order
                parsePlain(argv[i], plainIdx);
                plainIdx++;
            }
        }

        checkMandatoryOnCmd();
    }

    bool CmdParser::validatePlainByIdx(int idx, std::string& val)
    {
        return plains_[idx].validate(val);
    }

    void CmdParser::addValuePlainByIdx(int idx, std::string& val)
    {
        plains_[idx].addValue(val);
    }

    int CmdParser::getPlainsSize()
    {
        return (int)plains_.size();
    }
    bool CmdParser::isUnique(const std::string& name)
    {
        for (const auto& op : optionsWithParam_)
        {
            if (op.hasName(name))
                return false;
        }

        for (const auto& op : optionsWithoutParam_)
        {
            if (op.hasName(name))
                return false;
        }
        return true;
    }

    /**
     * @brief Parse and validate plain arguments.
     * @param arg argument to parse and validate
     * @param idx index of the plain argument on command line
     *
     * @return true if the argument was validated and added, false otherwise
     */
    bool CmdParser::parsePlain(std::string arg, int idx)
    {
        if (idx >= getPlainsSize())
        {
            throw ParsingExceptionNoSuchEl(arg);
        }
        else if (validatePlainByIdx(idx, arg))
        {
            addValuePlainByIdx(idx, arg);
            return true;
        }
        else
        {
            throw ParsingExceptionInvalidValue(arg, std::to_string(idx));
        }
        return false;
    }

    void CmdParser::setPresentOptionWith(int idx)
    {
        optionsWithParam_[idx].setPresent();
    }

    void CmdParser::setPresentOptionWithout(int idx)
    {
        optionsWithoutParam_[idx].setPresent();
    }

    bool CmdParser::validateByIdx(int idx, std::string& value)
    {
        return optionsWithParam_[idx].validate(value);
    }

    void CmdParser::addValueByIdx(int idx, std::string& value)
    {
        optionsWithParam_[idx].addValue(value);
    }

    bool CmdParser::parseOptionWithParam(std::string& name, std::string& value)
    {
        // add ranges, commas
        auto pos_with_param = findOptionWithParam(name);
        auto v = getAllValues(value);

        if (pos_with_param == -1)
        {
            throw ParsingExceptionNoSuchEl(name);
        }

        for (auto& val : v) {
            if (validateByIdx(pos_with_param, val))
            {
                addValueByIdx(pos_with_param, val);
            }
            else {
                throw ParsingExceptionInvalidValue(value, name);
            }
        }
        return true;
    }

    bool CmdParser::parseOptionWithoutParam(std::string& name)
    {
        auto pos_without_param = findOptionWithoutParam(name);
        auto pos_with_param = findOptionWithParam(name);

        if (pos_without_param == -1)
        {
            if (pos_with_param == -1)
            {
                throw ParsingExceptionNoSuchEl(name);
            }
            else
            {
                setPresentOptionWith(pos_with_param);
                return true;
            }
        }
        else
        {
            setPresentOptionWithout(pos_without_param);
            return true;
        }
    }

    bool CmdParser::parseLong(std::string arg)
    {
        size_t idx = arg.find('=');

        if (idx != std::string::npos)
        {
            std::string name = arg.substr(0, idx);
            std::string value = arg.substr(idx + 1, arg.size() - idx - 1);

            return parseOptionWithParam(name, value);
        }
        return parseOptionWithoutParam(arg);
    }

    bool CmdParser::parseShort(std::string name, std::string arg)
    {

        if (arg.compare("") != 0) {
            return parseOptionWithParam(name, arg);
        }
        return parseOptionWithoutParam(name);
    }

}

