#include "cmd_args_parser.hpp"

namespace cmd_args_parser {

    CmdParser::CmdParser(const std::string& name, std::size_t optionsParamNumber, std::size_t optionsWithoutNumber, std::size_t plainsNumber)
    {
        name_ = name;
        optionsWithParam_ = std::vector<Option>();
        optionsWithParam_.reserve(optionsParamNumber);
        optionsWithoutParam_ = std::vector<Option>();
        optionsWithoutParam_.reserve(optionsWithoutNumber);
        plains_ = std::vector<Plain>();
        plains_.reserve(plainsNumber);
    }
    void parse_contatiner(const std::vector<Option>& v, std::ostream& os)
    {
        for (const auto& op : v)
        {
            os << "\t-" << op.getName();
            os << ":\n\t";
            op.getDescription(os);
            os << "\n";
        }
    }

    void CmdParser::help(std::ostream& os) const
    {
        os << name_ << ", version " << version_ << "\n";
        os << "Options without parameter:\n";
        parse_contatiner(optionsWithoutParam_, os);

        os << "Options with parameter:\n";
        parse_contatiner(optionsWithParam_, os);

        os << "Plain arguments:\n";
        for (const auto& op : plains_)
        {
            os << "\t-" << op.getName();
            os << ":\n\t";
            op.getDescription(os);
            os << "\n";
        }

    }

    void CmdParser::setVersion(const std::string& version)
    {
        version_ = version;
    }

    int CmdParser::findOptionWithParam(const std::string& name)
    {
        for (size_t i = 0; i < optionsWithParam_.size(); i++) {

            if (optionsWithParam_[i].hasName(name)) {
                return i;
            }
        }
        return -1;
    }

    int CmdParser::findOptionWithoutParam(const std::string& name)
    {
        for (size_t i = 0; i < optionsWithoutParam_.size(); i++) {

            if (optionsWithoutParam_[i].hasName(name)) {
                return i;
            }
        }
        return -1;
    }
}