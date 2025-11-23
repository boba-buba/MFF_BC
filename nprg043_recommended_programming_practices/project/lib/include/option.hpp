#include "cmd_args_parser.hpp"

namespace cmd_args_parser
{
    class CmdParser;

    /** @brief Class that represents immutable option with/without parameter after building. */
    class Option final
    {
    public:
        friend CmdParser;

        /** Ctor for option.
         * @param names all possible names for option to be identified by.
         * @param description of option for help text.
         * @param required flag whether option is mandatory.
         * @param paramRequired flag whether the parameter is mandatory.
         * @param type of the value which will be stored.
         */
        Option(const std::vector<std::string>& names, const std::string& description, bool required, bool paramRequired, std::unique_ptr<ValueType> type) :
            names_(std::move(names)), description_(description), required_(required), paramRequired_(paramRequired), type_(std::move(type))
        {
            present_ = false;
            paramPresent_ = false;
            values_ = {};
        }

        /** Bool operator to indicate, whether option was on command line. */
        operator bool() const
        {
            return present_;
        }

        /** Return description of the Option.
         * @param os output stream, std::cout by default.
         */
        void getDescription(std::ostream& os = std::cout) const
        {
            os << description_;
        }

        /** Return one of the names of the option.
         * @return name of the option.
         */
        std::string getName() const
        {
            return names_[0];
        }

        /** Return information about whether option was on the command-line
         * @return true if optionwas on the command-line. False otherwise
         */
        bool isPresent() const
        {
            return present_;
        }

        /** Returns first parsed value.
         * If only one value on command-line, then Last and First values are the same.
         * @return value
         * @return default(T) if there were no values on the command line, or parsing has not yet been performed.
         */
        template <typename T>
        T getFirstValue() const
        {
            if (values_.size() == 0) return T();
            auto v = values_.front();
            auto t = std::any_cast<T>(type_.get()->converter(v));
            return t;
        }

        /** Return all values.
         * @return values, empty vector if none
         */
        template <typename T>
        std::vector<T> getValues() const
        {
            std::vector<T> result;
            for (auto v : values_)
            {
                result.emplace_back(std::any_cast<T>(type_.get()->converter(v)));
            }
            return result;
        }

        /** Return last parsed value.
         * If only one value on command-line, then Last and First values are the same.
         * @return value
         * @return default(T) if there were no values on the command line, or parsing has not yet been performed.
         */
        template <typename T>
        T getLastValue() const {
            auto v = values_.back();
            T t = std::any_cast<T>(type_.get()->converter(v));
            return t;
        }

    private:
        /** Wrapper for validation.
         * @param val value to validate
         * @return true if OK, els false.
         */
        bool validate(const std::string& val)
        {
            return type_.get()->validate(val);
        }

        /** Add value after validation.
         * @param value to add.
         */
        void addValue(std::string value)
        {
            present_ = true;
            paramPresent_ = true;
            values_.emplace_back(value);
        }

        /** Check whether option has name.
         * @param name to check.
         * @return true if option has that name, false otherwise.
         */
        bool hasName(const std::string& name) const
        {
            for (const auto& n : names_) {
                if (n.compare(name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /** Set present on command line flag. */
        void setPresent()
        {
            present_ = true;
        }

        /** Set parameter is present on command line flag. */
        void setParamPresent()
        {
            paramPresent_ = true;
        }

        /** All the names (ahort and long), that were specified for option. */
        std::vector<std::string> names_;
        /** Description for help text. */
        std::string description_;
        /** Flag whether the option is required. Bu default is true. */
        bool required_;
        /** Flag whether option parameter is required. */
        bool paramRequired_;
        /** Type of the value. */
        std::unique_ptr<ValueType> type_;

        /** Flag whether option was on command-line. */
        bool present_;
        /** Flag whether option parameter was on command-line. */
        bool paramPresent_;
        /** Actual value(s) parsed from command-line stored here */
        std::vector<std::string> values_;
    };
}