#include "casem.hpp"
#include <stdarg.h>

namespace casem {

//////////////////LEXICAL//////////////////////
    void parse_text(const char* text) {
        while(*text != '\0') {
            std::cout << *text << "\n";
            text++;
        }
    }

    int intstr_to_int(const char* num) {
        return std::stoi(num, nullptr, 10);
    }

    int hexstr_to_int(const char* num) {
        return std::stoi(num, nullptr, 16);
    }

    int parse_int(char* text, cecko::context* ctx) {
        int num = 0;
        bool malformed = false;
        bool out_of_range = false;
        std::string out = (std::string)text;

        if (out[0] == '0' && out[1] == 'x') {
            std::string hex_alph = "0123456789abcdef";
            for (size_t j = 2; j < out.size(); j++) {

                size_t i = hex_alph.find(std::tolower(out[j]));
                if (i == std::string::npos) {
                    malformed = true;
                    break;
                } else {
                    if (j > 8 || ( out[2] > '7' && j == 8)) {out_of_range = true;}
                    num = num * 16 + i;
                }

            }

        } else {
            for (size_t j = 0; j < out.size(); j++) {

                if (out[j] < 58 && out[j] > 47) {
                    if (j > 10 || ( out[0] > '2' && j == 10)) {out_of_range = true;}
                    num = num * 10 + (out[j] - 48);
                }
                else {
                    malformed = true;
                    break;
                }
            }
        }

        if (malformed) ctx->message(cecko::errors::BADINT, ctx->line(), out);
        if (out_of_range) {
            ctx->message(cecko::errors::INTOUTRANGE, ctx->line(), out);
            return 0x7fffffff;
        }
        return num;
    }

    static bool is_hex(char ch) {
        if (ch > 64 && ch < 71) return true;
        else if (ch > 96 && ch < 103) return true;
        else if (ch > 47 && ch < 58) return true;
        return false;
    }

    int parse_hex_esc(const char* text, cecko::context* ctx) {
        std::string out = (std::string)text;
        int num = 0;
        int count = 0;
        text++; text++;

        if (*text == '\0') {
            ctx->message(cecko::errors::BADESCAPE, ctx->line(), out);
            return num;
        }
        int number = hexstr_to_int(text);
        while (*text != '\0') {
            count++;
            if (is_hex(*text)) {
                num = *text - 55;
            } else {
                num = *text;
            }
            text++;
        }

        if (count > 2) {
            if (number > 0xff) {
                ctx->message(cecko::errors::BADESCAPE, ctx->line(), out);
                num = 255; 
            } else {
                num = number;
            }
        }
        return num;
    }

    int parse_char_error(const char* text, cecko::context* ctx) {
        std::string out = (std::string)text;
        int num = 0;
        while(*text != 0) {
            if (*text != '\\')
                num = *text;
            text++;
        }
        ctx->message(cecko::errors::BADESCAPE, ctx->line(), out);
        return num;
    }

    std::string parse_hex_esc_str(const char* text, cecko::context* ctx) {
        //(\\x)[0-9a-fA-F]*
        std::string out = "";
        int count = 0;
        out += *text; text++;
        out += *text; text++;

        while (*text != '\0') {
            out += *text;
            count++;
            text++;
        }
        if (count > 2) {
            ctx->message(cecko::errors::BADESCAPE, ctx->line(), out);
            std::string err = ""; err += '\xff';
            return err;
        }
        return out;
    }

    std::string parse_str_error(const char* text, cecko::context* ctx) {
        std::string out_err = "";
        std::string out = "";

        while (*text != '\0') {
            out_err += *text;
            if (*text != '\\') out += *text;
            text++;
        }

        ctx->message(cecko::errors::BADESCAPE, ctx->line(), out_err);
        return out;
    }

    ///////////////////SEMANTIC/////////////////////
/*
    cecko::CKTypeSafeObs get_etype(cecko::gt_etype etype, cecko::context* ctx) {
        cecko::CKTypeSafeObs tp1;
        switch (etype)
		{
		    case cecko::gt_etype::BOOL: tp1 = ctx->get_bool_type(); break;
		    case cecko::gt_etype::CHAR: tp1 = ctx->get_char_type();  break;
		    case cecko::gt_etype::INT: tp1 = ctx->get_int_type();  break;
		    default: break;
        }

        return tp1;
    }*/


    cecko::CKTypeSafeObs parse_struct_decl(cecko::context* ctx, int count,  ...) {
        cecko::CKTypeSafeObs tp1;
        va_list args;
        va_start(args, count);
        while (count > 0) {
            //char* var = va_arg(args, char*);

            count--;
        }
        va_end(args);

        return tp1;
    }

    void parse_declaration(cecko::context* ctx, std::list<cecko::CKTypeRefSafePack>* specifiers, std::list<casem::declarator_t*>* declarators, cecko::loc_t line) {
        if (specifiers->size() == 1) {
            auto it_spec = specifiers->begin();

            for (auto it = declarators->begin(); it != declarators->end(); ++it){

                cecko::CKTypeRefSafePack ptr_type;
                if ((*it)->ptr == true) {
                    auto ptr = ctx->get_pointer_type((*it_spec));
                    ptr_type = cecko::CKTypeRefSafePack(ptr, false);
                    ctx->define_var((*it)->name, ptr_type, line);

                }else {
                    ctx->define_var((*it)->name, (*it_spec), line);
                }

            }
        }
        //specifiers->clear();
        //declarators->clear();
    }


    void parse_direct_declarator(std::list<declarator_t*>* l, int count, ...) {

        va_list args;
        va_start(args, count);
        if (count == 1) {
            cecko::CIName* name = va_arg(args, cecko::CIName*);
            declarator_t* new_decl = (declarator_t*)malloc(sizeof(declarator_t));
            if (new_decl == NULL) return;
            new_decl->name = *(name);
            new_decl->declaration_type = variable;
            new_decl->constant_ptr = false;
            new_decl->constant_var = false;
            new_decl->ptr = false;
            l->emplace_back(new_decl);
            count--;
        }
        while (count > 0) {
            //char* var = va_arg(args, char*);

            count--;
        }
        va_end(args);

    }

    void add_ptr_to_declarator(std::list<keywords>* pointer_list, std::list<declarator_t*>* drct_declarator_list) {
        if (pointer_list->size() == 1) {

            if (drct_declarator_list->size() == 0) return;

            auto it = drct_declarator_list->begin(); 
            if (it == drct_declarator_list->end()) return;
            (*it)->ptr = true;
            pointer_list->pop_front();
        }
    }



}