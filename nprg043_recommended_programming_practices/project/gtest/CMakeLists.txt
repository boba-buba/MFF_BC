cmake_minimum_required(VERSION 2.4)

project(tests)

enable_language(C)
enable_language(CXX)

set(ROOT_DIR ${PROJECT_SOURCE_DIR}/..)

if(CMAKE_CXX_COMPILER_ID MATCHES GNU)
    set(CMAKE_CXX_FLAGS         "-Wall -Wno-unknown-pragmas -Wno-sign-compare -Woverloaded-virtual -Wwrite-strings -Wno-unused")
    set(CMAKE_CXX_FLAGS_DEBUG   "-O0 -g3")
    set(CMAKE_CXX_FLAGS_RELEASE "-O3")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fprofile-arcs -ftest-coverage")
endif()

include_directories(
    ${ROOT_DIR}/lib/src
    ${ROOT_DIR}/lib/include
    )

add_library(
    cmd_args_parser
    ${ROOT_DIR}/lib/src/builder.cpp
    ${ROOT_DIR}/lib/src/parser_config.cpp
    ${ROOT_DIR}/lib/src/parser_parse.cpp
    ${ROOT_DIR}/lib/include/cmd_args_parser.hpp
    ${ROOT_DIR}/lib/include/cmd_parser_exceptions.hpp
    ${ROOT_DIR}/lib/include/cmd_parser_types.hpp
    ${ROOT_DIR}/lib/include/option.hpp
    ${ROOT_DIR}/lib/include/plain.hpp

    )

set(GOOGLETEST_ROOT ${ROOT_DIR}/gtest/googletest/googletest CACHE STRING "Google Test source root")

include_directories(
    ${GOOGLETEST_ROOT}
    ${GOOGLETEST_ROOT}/include
    )

set(GOOGLETEST_SOURCES
    ${GOOGLETEST_ROOT}/src/gtest-all.cc
    ${GOOGLETEST_ROOT}/src/gtest_main.cc
    )

foreach(_source ${GOOGLETEST_SOURCES})
    set_source_files_properties(${_source} PROPERTIES GENERATED 1)
endforeach()

add_library(googletest ${GOOGLETEST_SOURCES})

add_executable(
    tests
    ${ROOT_DIR}/gtest/tests.cpp
    )

add_dependencies(tests googletest)

target_link_libraries(
    tests
    googletest
    cmd_args_parser
    )
enable_testing()

add_test(unit ${ROOT_DIR}/tests)