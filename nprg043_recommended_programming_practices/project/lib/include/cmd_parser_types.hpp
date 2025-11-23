#ifndef CMD_PARSER_TYPES_HPP_
#define CMD_PARSER_TYPES_HPP_

/**
 * @addtogroup cmd_args_parser
 * @{
 */
 /**
  * @file
  * @brief Value types supported by library
  */

#include <limits>
#include <string>
#include <vector>
#include <algorithm>
#include <functional>
#include <any>


namespace cmd_args_parser {

    /** @brief Abstract class for types of the values, that parser should be able to parse. Represents Interface. */
    class ValueType {
    public:
        /**
         * @brief Verifies that the passed value is of the correct type and within the range of allowed values. Called during parsing.
         *
         * @param value to validate.
         * @return True if the type is correct.
         */
        virtual bool validate(const std::string& value) const = 0;
        ~ValueType() = default;

        /** Delegate to converter function, that accepts string as parameter and returns actual value of desired type. */
        std::function<std::any(const std::string&)> converter = [](const std::string& v) { return v; };
    };

    /** @brief Represenation of the int type */
    class Int final : public ValueType {
    public:
        Int()
        {
            converter = [](const std::string& v) {return std::stoi(v); };
        }
        /**
        * @brief Verifies that the passed value is of the correct type and within the range of allowed values. Called during parsing.
        *
        * @param value to validate.
        * @return True if the type is correct.
        */
        bool virtual validate(const std::string& val) const {
            int value;
            try {
                value = std::stoi(val);
            }
            catch (std::exception& e) {
                return false;
            }
            if (value > upperBound_ || value < lowerBound_) {
                return false;
            }
            return true;
        }

        /** Set upper bound for value
         * @param upperBound
         */
        void setUpperBound(int upperBound) { upperBound_ = upperBound; }

        /** Set lower bound for value
         * @param lowerBound
         */
        void setLowerBound(int lowerBound) { lowerBound_ = lowerBound; }

    private:
        /* Store upper bound of the possible value. */
        int upperBound_ = std::numeric_limits<int>::max();
        /* Store lower bound of the possible value. */
        int lowerBound_ = std::numeric_limits<int>::min();
    };

    /** @brief Representation of String type*/
    class String final : public ValueType {
    public:
        String() {};
        /**
        * @brief Verifies that the passed value is of the correct type and within the range of allowed values. Called during parsing.
        *
        * @param value to validate.
        * @return True if the type is correct.
        */
        bool virtual validate(const std::string&) const { return true; };
    };

    /** @brief Represenation of Bool type */
    class Bool final : public ValueType {
    public:
        Bool() {
            converter = [](const std::string& v) {
                if (v == "True" || v == "true" || v == "1") {
                    return true;
                }
                else return false;
                };
        };
        /**
        * @brief Verifies that the passed value is of the correct type and within the range of allowed values. Called during parsing.
        *
        * @param value to validate.
        * @return True if the type is correct.
        */
        bool virtual validate(const std::string& val) const {
            if (val == "True" || val == "true" || val == "1" || val == "False" || val == "false" || val == "0") {
                return true;
            }
            else return false;
        };
    };

    /** @brief Representation of Enum type (fixed domain) */
    class Enum final : public ValueType {
    public:
        Enum(const std::vector<std::string>& elements) : elements_(elements) {}
        /**
        * @brief Verifies that the passed value is of the correct type and within the range of allowed values. Called during parsing.
        *
        * @param value to validate.
        * @return True if the type is correct.
        */
        bool virtual validate(const std::string& val) const {
            return (std::find(elements_.begin(), elements_.end(), val) != elements_.end());
        }
    private:
        /** fixed domain possible values */
        std::vector<std::string> elements_;
    };
}

#endif

/** @}
 */
