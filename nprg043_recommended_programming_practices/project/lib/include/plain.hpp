#include "cmd_args_parser.hpp"

namespace cmd_args_parser
{
    class CmdParser;

    /** @brief Class that represents immutable plain argument after building. */
    class Plain final {
    public:
        friend CmdParser;

        /** Ctor for option.
         * @param name name for plain argument to be identified by.
         * @param description of plain argument for help text.
         * @param type of the value which will be stored.
         */
        Plain(const std::string& name, const std::string& description, std::unique_ptr<ValueType> type)
        {
            name_ = name;
            description_ = description;
            type_ = std::move(type);
        }

        /** Return the name Plain argument.
         * @return name of the Plain argument.
         */
        std::string getName() const
        {
            return name_;
        }

        /** Return description of the Plain argument.
         * @param os output stream, std::cout by default.
         */
        void getDescription(std::ostream& os = std::cout) const
        {
            os << description_;
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
        /** Wrrapper function for validation of the type and bounds of the value. Called during parsing command line
         * @param val value to validate
         * @return True if valid, false if not
         */
        bool validate(const std::string& val)
        {
            return type_.get()->validate(val);
        }

        /** Add value after validation.
         * @param value to add.
         */
        void addValue(std::string& v)
        {
            values_.emplace_back(v);
            present_ = true;
        }

        /** Set present on command line flag. */
        void setPresent()
        {
            present_ = true;
        }

        /** Type of the value. */
        std::unique_ptr<ValueType> type_;
        /** Actual value(s) parsed from command-line stored here */
        std::vector<std::string> values_;

        /** Description for help text */
        std::string description_ = "";
        /** Stores all the names (ahort and long), that were specified for option. */
        std::string name_;
        /** Stores the flag whether the option is required. Bu default is true. */
        bool required_ = true;
        /** Flag whether plain argument was on command-line. */
        bool present_ = false;
    };
}