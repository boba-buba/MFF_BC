#include "cmd_args_parser.hpp"

namespace cmd_args_parser {
    CmdParser::OptionBuilder&  CmdParser::OptionBuilder::addSynonym(const std::string& synonym)
    {
        names_.emplace_back(synonym);
        return *this;
    }
    CmdParser::OptionBuilder&  CmdParser::OptionBuilder::required(bool isRequired)
    {
        required_ = isRequired;
        return *this;
    }

    CmdParser::OptionBuilder&  CmdParser::OptionBuilder::addDescription(const std::string& desc)
    {
        description_ += desc;
        return *this;
    }

    CmdParser::OptionBuilder&  CmdParser::OptionBuilder::paramRequired(bool isRequired)
    {
        paramRequired_ = isRequired;
        return *this;
    }

    Option& CmdParser::OptionBuilder::buildOptionWithParam()
    {
        for (const auto& name : names_)
        {
            if (!parser_.isUnique(name))
            {
                throw OptionNameException();
            }
        }
        parser_.optionsWithParam_.emplace_back(std::move(names_), std::move(description_), std::move(required_), std::move(paramRequired_), std::move(type_));
        return parser_.optionsWithParam_[parser_.optionsWithParam_.size() - 1];
    }

    Option& CmdParser::OptionBuilder::buildOptionWithoutParam()
    {
        for (const auto& name : names_)
        {
            if (!parser_.isUnique(name))
            {
                throw OptionNameException();
            }
        }
        parser_.optionsWithoutParam_.emplace_back(std::move(names_), std::move(description_), std::move(required_), false, std::move(type_));
        return parser_.optionsWithoutParam_[parser_.optionsWithoutParam_.size() - 1];
    }

    CmdParser::PlainBuilder& CmdParser::PlainBuilder::addDescription(const std::string& desc)
    {
        description_ += desc;
        return *this;
    }

    Plain& CmdParser::PlainBuilder::buildPlain()
    {
        return parser_.plains_.emplace_back(name_, description_, std::move(type_));
    }
}