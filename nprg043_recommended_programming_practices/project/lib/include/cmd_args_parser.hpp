#ifndef CMD_ARGS_PARSER_HPP_
#define CMD_ARGS_PARSER_HPP_

#include <iostream>
#include <string>
#include <vector>
#include <memory>
#include <type_traits>
#include <limits>
#include <any>

#include "cmd_parser_types.hpp"
#include "cmd_parser_exceptions.hpp"
#include "option.hpp"
#include "plain.hpp"

/**
 * @addtogroup cmd_args_parser
 * @{
 */
 /**
  * @brief Command Line Object Representations and CmdParser for cmd line parsing
  * @details CmdParser, Options and Arguments classes
  */

namespace cmd_args_parser {

    /** @brief Main class that provides API for user to create model according to which cmd line is parsed */
    class CmdParser {
    public:
        class PlainBuilder;
        class OptionBuilder;
    private:
        /** Store options, that might have parameter, predefined size by programmer */
        std::vector<Option> optionsWithParam_;
        /** Store options without parameter, predefined size by programmer*/
        std::vector<Option> optionsWithoutParam_;
        /** Store plain arguments, predefined size */
        std::vector<Plain> plains_;

        /** Store version of the program */
        std::string version_;
        /** Store name of the program */
        std::string name_;

        /** Traverse through container, that stores options with parameters and tries to find option by any of its names
         * @param name One of the option's names
         * @return index of option in container if found, -1 otherwise
         */
        int findOptionWithParam(const std::string& name);

        /** Traverse through container, that stores options without parameters and tries to find option by any of its names
         * @param name One of the option's names
         * @return index of option in container if found, -1 otherwise
         */
        int findOptionWithoutParam(const std::string& name);

        /** Parse plain argument on command line.
         * @param arg the argument itself.
         * @param idx index of the plain argument.
         */
        bool parsePlain(std::string arg, int idx);

        /** Parse option when long name on command line.
         * @param arg optionName=value(s) or optionName, to be parsed and saved
         */
        bool parseLong(std::string arg);

        /** Parse option when short name on command line.
         * @param name short option name.
         * @param arg value(s) that follows the name on command line.
         */
        bool parseShort(std::string name, std::string arg);

        /** Parse option with parameter on command line.
         * @param name one of the options name.
         * @param value value(s) that follows option.
         */
        bool parseOptionWithParam(std::string& name, std::string& value);

        /** Parso option without parameter on command line.
         * @name one of the options names.
         */
        bool parseOptionWithoutParam(std::string& name);

        /** Get all values from command line.
         * Used when more than one value was on command line.
         * @param arg value(s)
         * @return vector of separete values.
         */
        std::vector<std::string> getAllValues(std::string& arg);

        /** Check whether all mandatory arguments/options on command line.
         * @throw ParsingExceptionMandatory if not all mandatory elements on command line.
         */
        void checkMandatoryOnCmd();

        /** Set that option with parameter is on command line.
         * @param idx index at which the option is tored in container.
         */
        void setPresentOptionWith(int idx);

        /** Set that option without parameter is on command line.
         * @param idx index at which the option is tored in container.
         */
        void setPresentOptionWithout(int idx);

        /** Add value to the option's container, taht stores values.
         * @param idx index at which the option is stored.
         * @param val value to be saved.
         */
        void addValueByIdx(int idx, std::string& value);

        /** Validate the value for the option with parameter at index.
         * @param idx index at which the option is stored.
         * @param val value to be validated.
         * @return true if OK, else false.
         */
        bool validateByIdx(int idx, std::string& value);

        /** Validate the value for the plain argument at index.
         * @param idx index at which the palin argument is stored.
         * @param val value to be validated.
         * @return true if OK, else false.
         */
        bool validatePlainByIdx(int idx, std::string& val);

        /** Add value to the plain argument's container, taht stores values.
         * @param idx index at which the plain argument is stored.
         * @param val value to be saved.
         */
        void addValuePlainByIdx(int idx, std::string& val);

        /** Get number of palin arguments defined in parser.
         * @return size if plains container.
         */
        int getPlainsSize();

        /** Check if the name is unique in context of all elements in parser.
         * @param name to check.
         * @return true if unique, false otherwise.
         */
        bool isUnique(const std::string& name);

    public:
        /** Ctor for CmdParser.
         * @param name of the program that is created with help of the parser.
         * @param optionsParamNumber number of options with parameters, that will be defined (default 10).
         * @param optionsWithoutNumber number of options without parameters, that will be defined (default 10).
         * @param plainsNumber number of plain arguments, that will be defined (default 10).
         */
        CmdParser(const std::string& name, std::size_t optionsParamNumber = 10, std::size_t optionsWithoutNumber = 10, std::size_t plainsNumber = 10);

        /** Produce help message.
         * This function writes help message, which was configured with setDescription method on elements, to provided output stream.
         * @param os The output stream, uses std::cout by default.
         */
        void help(std::ostream& os = std::cout) const;

        /** Sets the version of the program.
         */
        void setVersion(const std::string& version);

        /** Parses command-line arguments.
         *
         * In case of exception nothing is read and parsed.
         *
         * @param input command-line arguments
         * @exception ParsingExceptionInvalidValue
         * @exception ParsingExceptionNoSuchEl
         * @exception ParsingExceptionMandatory
         */
        void parseCommand(int argc, char** argv);

        /** Begin definition of Option with parameter.
         * @param name of the option, that will be created.
         * @param type of the parameter, the option stores.
         * @return OptionBuilder, on which will be defined other option attributes.
         */
        template<typename T>
        typename std::enable_if<std::is_base_of<ValueType, T>::value, OptionBuilder>::type
            defineOptionWithParam(const std::string& name, T&& type)
        {
            return OptionBuilder(name, std::make_unique<T>(type), *this);
        }

        /** Begin definition of Option without parameter.
         * @param name of the option, that will be created.
         * @return OptionBuilder, on which will be defined other option attributes.
         */
        OptionBuilder defineOptionWithoutParam(const std::string& name)
        {
            return OptionBuilder(name, std::make_unique<Bool>(Bool()), *this);
        }

        /** Begin definition of Plain argument.
         * @param name of the plain argument, that will be created.
         * @param type of the value that plain arguments stores.
         * @return PlainBuilder, on which will be defined other option attributes.
         */
        template<typename T>
        typename std::enable_if<std::is_base_of<ValueType, T>::value, PlainBuilder>::type
            definePlainArg(const std::string& name, T&& type)
        {
            return PlainBuilder(name, std::make_unique<T>(type), *this);
        }

        /**
         * @brief Builder for options.
         * @details Here we do not distinguish between options with and without parameter.
         */
        class OptionBuilder final {
        public:
            friend CmdParser;

            /** Ctor for Option with parameter.
             * @param name of the option.
             * @param type type of the parameter that option stores.
             * @param parser where the option will be saved after it will be built.
             */
            OptionBuilder(const std::string& name, std::unique_ptr<ValueType> type, CmdParser& parser) : names_({ name }), type_(std::move(type)), parser_(parser)
            {
                description_ = "";
                required_ = true;
                paramRequired_ = true;
            }

            /** Ctor for Option without parameter
             * @param name of the option.
             * @param parser where the option will be saved after it will be built.
             */
            OptionBuilder(const std::string& name, CmdParser& parser) : names_({ name }), type_(std::make_unique<Bool>(Bool())), parser_(parser)
            {
                description_ = "";
                required_ = true;
                paramRequired_ = true;
            }

            /** Add a synonym of the defined Option. They will be treated the same. For example, a short option.
            * @param synonym the synonym to add.
            * @return OptionBuilder. Can be used to defime smth else on OptionBuilder.
            */
            OptionBuilder& addSynonym(const std::string& synonym);

            /** Set a required flag. True by default. Pass false to make the Option optional.
             * @param isRequired value to set.
             * @return OptionBuilder. Can be used to defime smth else on OptionBuilder.
             */
            OptionBuilder& required(bool isRequired);

            /** Add description to the Option.
             * @param desc Description to add.
             * @return OptionBuilder. Can be used to defime smth else on OptionBuilder.
             */
            OptionBuilder& addDescription(const std::string& desc);

            /** Set a paramRequired flag. True by default. Pass false to make the Parameter optional.
             * @param isOptional Value to set.
             * @return OptionBuilder. Can be used to defime smth else on OptionBuilder.
             */
            OptionBuilder& paramRequired(bool isRequired);

            /** Create an instance of the Option with parameter, that will be stored in CmdParser.
             * @return reference to the created Option.
             */
            Option& buildOptionWithParam();

            /** Ceate an instance of the Option without parameter, that will be stored in CmdParser.
             * @return reference to the created Option.
             */
            Option& buildOptionWithoutParam();

        private:
            /** Stores all the names (short and long), that were specified for option */
            std::vector<std::string> names_;
            /** Type of the option paramterer */
            std::unique_ptr<ValueType> type_;

            CmdParser& parser_;

            /** Description for help text */
            std::string description_;
            /** Stores the flag whether the option is required. By default is true */
            bool required_;
            /** Stores flag whether option parameter is required */
            bool paramRequired_;
        };


        /** @brief Builder for plain arguments. */
        class PlainBuilder final {
        public:
            friend CmdParser;

            /** Ctor for plain argument.
             * @param name of the plain argument.
             * @param type of the value that the plain argument stores.
             * @param parser that will store the plain argument after it will be built.
             */
            PlainBuilder(const std::string& name, std::unique_ptr<ValueType> type, CmdParser& parser) : name_(name), type_(std::move(type)), parser_(parser) {}

            /** Add description to the Plain argument.
             * @param desc Description to add.
             */
            PlainBuilder& addDescription(const std::string& desc);

            /** Create an instance of the Plain argument that will be stored in CmdParser.
             * @return reference to the created Plain argument.
             */
            Plain& buildPlain();

        private:
            /** Store all the names (ahort and long), that were specified for option. */
            std::string name_;
            /** Store instance of the type of the value, that the plain argument stores. */
            std::unique_ptr<ValueType> type_;
            /** Reference to parser, that will store the plain argument after building. */
            CmdParser& parser_;
            /** Description for help text. */
            std::string description_ = "";
            /** Stores the flag whether the option is required. Bu default is true. */
            bool required_ = true;
        };
    };
}


#endif

/** @}
 */
