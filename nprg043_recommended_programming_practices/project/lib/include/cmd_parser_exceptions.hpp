#ifndef CMD_PARSER_EXCEPTIONS_HPP_
#define CMD_PARSER_EXCEPTIONS_HPP_

#include <iostream>

/**
 * @addtogroup cmd_args_parser
 * @{
 */
 /**
  * @file
  * @brief Exceptions for handling parsing errors
  */
namespace cmd_args_parser {

    /** @brief Abstract class that represents interface for exceptions, that the parses can get. */
    class CmdParserException : public std::exception
    {
    public:
        /** Message that will be written to stream for user */
        virtual std::string what() = 0;
    };


    /** @brief Exception that is thrown during parseCommand when unknown option name is read. */
    class ParsingExceptionNoSuchEl : public CmdParserException {
        std::string name_;
    public:
        ParsingExceptionNoSuchEl(const std::string& name) :name_(name) {}

        /** Message that will be written to stream for user. */
        std::string what() override
        {
            std::string m = "No such option/argument defined: " + name_;
            return m;
        }
    };


    /** @brief Exception taht is thrown during parseCommand when validation fails. */
    class ParsingExceptionInvalidValue : public CmdParserException {
        /** Name of the option. */
        std::string name_;
        /** Value that could not be parsed. */
        std::string value_;
    public:
        ParsingExceptionInvalidValue(const std::string& value, const std::string& name) : name_(name), value_(value) {}

        /** Message that will be written to stream for user. */
        std::string what() override
        {
            std::string m = "Invalid argument: " + value_ + " for the option " + name_;
            return m;
        }
    };


    /** @brief Exception that is thrown during parseCommand when not all mandatory option values/plain arguments were provided */
    class ParsingExceptionMandatory : public CmdParserException {

        /* Name of the first mandatory argument/option that wasnt on command line but should be. */
        std::string name_;
    public:
        ParsingExceptionMandatory(const std::string& name) :name_(name) {}

        /** Message that will be written to stream for user. */
        std::string what()
        {
            std::string m = "Missing mandatory option/argument: " + name_;
            return m;
        }
    };


    /** @brief Exception that is thrown during BuildOption when at least one of the option names is not unique in context of parser. */
    class OptionNameException : public CmdParserException {
    public:
        /** Message that will be written to stream for user. */
        std::string what()
        {
            return "Option has non-unique name";
        }
    };
}
#endif

/** @}
 */
