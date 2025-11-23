#include "cmd_args_parser.hpp"
#include <iostream>
#include <string>
using namespace cmd_args_parser;

int main(int argc, char **argv) {
    CmdParser parser("numactl");

    auto& hardware = parser.defineOptionWithoutParam("hardware")
        .required(false)
        .buildOptionWithoutParam();

    auto& show = parser.defineOptionWithoutParam("show")
        .required(false)
        .buildOptionWithoutParam();

    auto& membind = parser.defineOptionWithParam("m", String())
        .required(false)
        .buildOptionWithParam();

    auto& preferred = parser.defineOptionWithParam("p", String())
        .required(false)
        .buildOptionWithParam();

    auto& interleave = parser.defineOptionWithParam("i", String())
        .required(false)
        .buildOptionWithParam();

    auto& physcpubind = parser.defineOptionWithParam("C", String())
        .required(false)
        .buildOptionWithParam();

    if(argc == 1){
        parser.help();
        return 0;
    }
    parser.parseCommand(argc, argv);

    if (show.isPresent()) {
        std::cout << "Policy: default" << std::endl;
    } else if (hardware) {
        std::cout << "Available: 2 nodes (0-1)" << std::endl;
    } else {
        std::cout << "Will run command with specified NUMA policy." << std::endl;
        if (membind) {
            std::cout << "Memory bind: " << membind.getFirstValue<std::string>() << std::endl;
        }
        if (preferred) {
            std::cout << "Preferred node: " << preferred.getFirstValue<std::string>() << std::endl;
        }
        if (interleave) {
            std::cout << "Interleave nodes: " << interleave.getFirstValue<std::string>() << std::endl;
        }
        if (physcpubind) {
            std::cout << "CPU node bind: " << physcpubind.getFirstValue<std::string>() << std::endl;
        }
    }

    return 0;
}
