#ifndef casem_hpp_
#define casem_hpp_

#include "cktables.hpp"
#include "ckcontext.hpp"
#include "ckgrptokens.hpp"

#include <iostream>
#include <stdlib.h>
#include <stdio.h>   

namespace casem {
//////////////////LEXICAL//////////////////////

    void parse_text(const char* text);
    int intstr_to_int(const char* num);
    int hexstr_to_int(const char* num);

    int parse_int(char* text, cecko::context* ctx);
    int parse_hex_esc(const char* text, cecko::context* ctx);
    int parse_char_error(const char* text, cecko::context* ctx);

    std::string parse_hex_esc_str(const char* text, cecko::context* ctx);
    std::string parse_str_error(const char* text, cecko::context* ctx);
///////////////////SEMANTIC/////////////////////
    enum keywords{Const, Star};
    enum declaratition{function, variable, };

    typedef struct {
        declaratition declaration_type;
        cecko::CIName name;
        bool ptr;
        bool constant_var;
        bool constant_ptr;
    } declarator_t;

    cecko::CKTypeSafeObs get_etype(std::string& etype, cecko::context* ctx);
    void parse_declaration(cecko::context* ctx, std::list<cecko::CKTypeRefSafePack>* specifiers, std::list<casem::declarator_t*>* declarators, cecko::loc_t line);
    void parse_direct_declarator(std::list<declarator_t*>* l, int count, ...);
    void add_ptr_to_declarator(std::list<keywords>* pointer_list, std::list<declarator_t*>* drct_declarator_list);

}

#endif
