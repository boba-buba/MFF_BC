#include <iostream>
#include <vector>
#include <utility>
#include <sstream>
#include "cmd_args_parser.hpp"

using namespace cmd_args_parser;

void print_hardware_config(std::ostream&os = std::cout);
void print_current_config(std::ostream&os = std::cout);

struct Policy {
    std::string name = "default";
    std::string preferred_node = "current";
    std::vector<int> physcpubind = {0,1,2,3,4,5,6,7,8};
    std::vector<int> cpubind = {0,1};
    std::vector<int> nodebind = {0,1};
    std::vector<int> membind = {0,1};

    void print(std::ostream& os = std::cout){
        os << "policy: " << name << std::endl;
        os << "preferred node: " << preferred_node << std::endl;
        os << "physcpubind: ";
        for(auto number : physcpubind) {
            os << number << " ";
        }
        os << std::endl;

        os << "cpubind: ";
        for(auto number : cpubind) {
            os << number << " ";
        }
        os << std::endl;

        os << "nodebind: ";
        for(auto number : nodebind) {
            os << number << " ";
        }
        os << std::endl;

        os << "membind: ";
        for(auto number : membind) {
            os << number << " ";
        }
        os << std::endl;


    }
};

// No longer needed
/*
class IntList : public ValueType {
public:

    IntList(int min, int max) : min_(min), max_(max), list_() {}

    bool validate(const std::string & value) const  override {
        if(value == "all"){
            return true;
        }

        std::stringstream ss(value);
        std::string numberString;

        while (std::getline(ss, numberString, ',')) {

            if(numberString.find('-') != std::string::npos){
               if(!validateRange(numberString)){
                   return false;
               }
           } else {
               if(!validateInt(numberString)){
                   return false;
               }
           }
        }


        return true;
    }

    static std::vector<int> convertToIntList(const std::string& value, int min, int max){
        std::vector<int> list;

        if(value == "all"){
            for(int i = min; i <= max; ++i){
                list.push_back(i);
            }

            return list;
        }

        std::stringstream ss(value);
        std::string numberString;
        while (std::getline(ss, numberString, ',')) {
            if(numberString.find('-') != std::string::npos){
                auto range = getRange(numberString);
                for(int i = range.first; i < range.second; ++i){
                    list.push_back(i);
                }
            }
            else {
                list.push_back(getInt(numberString));
            }
        }

        return list;
    }

private:

    bool isValidRange(int number) const{
        return min_ <= number && number <= max_;
    }

    bool validateInt(const std::string& value) const {
        try {
            int intValue = std::stoi(value);
            if (!isValidRange(intValue)) {
                return false;
            }
        } catch (const std::invalid_argument&) {
            return false;

        } catch (const std::out_of_range&) {
            return false;
        }

        return true;
    }

    bool validateRange(const std::string& value) const {
        size_t pos = value.find('-');
        std::string start = value.substr(0, pos);
        std::string end = value.substr(pos + 1);
        return validateInt(start) && validateInt(end) && (getInt(start) <= getInt(end));
    }

    static std::pair<int, int> getRange(const std::string& value) {
        size_t pos = value.find('-');
        std::string start = value.substr(0, pos);
        std::string end = value.substr(pos + 1);
        return {getInt(start), getInt(end)};
    }

    static int getInt(const std::string& value) {
        return std::stoi(value);
    }

    int min_;
    int max_;
    std::vector<int> list_;
};

static std::vector<int> convertToCpuIntList(const std::string& value){
        return IntList::convertToIntList(value,0 ,31);
    }
class CpuIntList : public IntList {
public:
    CpuIntList() : IntList(0,31) {
        converter = convertToCpuIntList;
    }
};

static std::vector<int> convertToNumaIntList(const std::string& value){
        return IntList::convertToIntList(value,0 ,3);
    }
class NumaIntList : public IntList {
public:
    NumaIntList() : IntList(0,3) {
        converter = convertToNumaIntList;
    }
};
*/
int main (int argc, char **argv)
{
    // ./build/numactl -p 3 -C 1-4,7,9,12-15 time args
    // ./build/numactl --show -- command args

    /* Create configuration model */
    CmdParser m = CmdParser("numactl");
    m.setVersion("v1.0.0");

    auto interleaveInt = Int();
    interleaveInt.setLowerBound(0);
    interleaveInt.setUpperBound(3);
    auto& interleave = m.defineOptionWithParam("interleave", std::move(interleaveInt))
        .addSynonym("i")
        .addSynonym("interleave")
        .addDescription("Interleave memory allocation across given nodes.")
        .required(false)
        .paramRequired(true)
        .buildOptionWithParam();


    auto prefferedInt = Int();
    prefferedInt.setLowerBound(0);
    prefferedInt.setUpperBound(3);
    auto& preferred = m.defineOptionWithParam("preferred", std::move(prefferedInt))
        .addSynonym("p")
        .addDescription("Prefer memory allocations from given node.")
        .required(false)
        .paramRequired(true)
        .buildOptionWithParam();

    auto membindInt = Int();
    membindInt.setLowerBound(0);
    membindInt.setUpperBound(3);
    auto& membind = m.defineOptionWithParam("membind", std::move(membindInt))
        .addSynonym("m")
        .addSynonym("membind")
        .addDescription("Allocate memory from given nodes only.")
        .required(false)
        .paramRequired(true)
        .buildOptionWithParam();

    auto physcpubindInt = Int();
    physcpubindInt.setLowerBound(0);
    physcpubindInt.setUpperBound(31);
    auto& physcpubind = m.defineOptionWithParam("physcpubind", std::move(physcpubindInt))
        .addSynonym("C")
        .addSynonym("physcpubind")
        .addDescription("Run on given CPUs only.")
        .required(false)
        .paramRequired(true)
        .buildOptionWithParam();

    auto& show = m.defineOptionWithoutParam("show")
        .addSynonym("S")
        .addSynonym("show")
        .addDescription("Show current NUMA policy.")
        .required(false)
        .buildOptionWithoutParam();

    auto& hardware = m.defineOptionWithoutParam("hardware")
        .addSynonym("H")
        .addSynonym("hardware")
        .addDescription("Print hardware configuration.")
        .required(false)
        .buildOptionWithoutParam();

    auto& command = m.definePlainArg("command", String())
        .addDescription("Command used by numactl.")
        .buildPlain();

    // working under assumption that all the plain args after command will be stored into arguments
    auto& arguments = m.definePlainArg("arguments", String())
        .addDescription("Arguments accepted by command.")
        .buildPlain();

    if(argc == 1){
        m.help();
        return 0;
    }
    m.parseCommand(argc, argv);


    // Hardware flag
    if(hardware){
        print_hardware_config();
        return 0;
    }

    Policy policy;

    // Show flag
    if(show){
        policy.print();
        return 0;
    }

    // Validate new policy

    if((membind && preferred) ||
    (membind && interleave) ||
    (preferred && interleave)){
        std::cout << "Mutually exclusive options occurred, aborting!" << std::endl;
        return 1;
    }

    // Load and print the new policy

    policy.name = "new policy";
    if(preferred){
        // not sure if the preferred node should be shown here as a number or what
        policy.preferred_node = std::to_string(preferred.getFirstValue<int>());
    }

    if(membind){
        policy.membind  = membind.getValues<int>();
    }

    if(interleave){
        // im not sure what is the policy equivalent of interleave
        policy.nodebind = interleave.getValues<int>();
    }

    if(physcpubind){
        policy.physcpubind =  physcpubind.getValues<int>();
    }

    policy.print();

    return 0;
}


void print_hardware_config(std::ostream& os) {
    std::string hardware_config = R"(available: 2 nodes (0-1)
node 0 cpus: 0 2 4 6 8 10 12 14 16 18 20 22
node 0 size: 24189 MB
node 0 free: 18796 MB
node 1 cpus: 1 3 5 7 9 11 13 15 17 19 21 23
node 1 size: 24088 MB
node 1 free: 16810 MB
node distances:
node   0   1
  0:  10  20
  1:  20  10)";

    os << hardware_config << std::endl;
}