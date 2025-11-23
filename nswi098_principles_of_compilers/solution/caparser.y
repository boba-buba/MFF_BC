%language "c++"
%require "3.4"
// NEVER SET THIS INTERNALLY - SHALL BE SET BY CMAKE: %defines "../private/caparser.hpp"
// NEVER SET THIS INTERNALLY - SHALL BE SET BY CMAKE: %output "../private/caparser.cpp"
%locations
%define api.location.type {cecko::loc_t}
%define api.namespace {cecko}
%define api.value.type variant
%define api.token.constructor
%define api.parser.class {parser}
%define api.token.prefix {TOK_}
//%define parse.trace
%define parse.assert
%define parse.error verbose

%code requires
{
// this code is emitted to caparser.hpp

#include "ckbisonflex.hpp"

// adjust YYLLOC_DEFAULT macro for our api.location.type
#define YYLLOC_DEFAULT(res,rhs,N)	(res = (N)?YYRHSLOC(rhs, 1):YYRHSLOC(rhs, 0))

#include "ckgrptokens.hpp"
#include "ckcontext.hpp"
#include "casem.hpp"
}

%code
{
// this code is emitted to caparser.cpp

YY_DECL;

using namespace casem;
}

%param {yyscan_t yyscanner}		// the name yyscanner is enforced by Flex
%param {cecko::context * ctx}

%start cecko_s

%token EOF		0				"end of file"

%token						LBRA		"["
%token						RBRA		"]"
%token						LPAR		"("
%token						RPAR		")"
%token						DOT			"."
%token						ARROW		"->"
%token<cecko::gt_incdec>	INCDEC		"++ or --"
%token						COMMA		","
%token						AMP			"&"
%token						STAR		"*"
%token<cecko::gt_addop>		ADDOP		"+ or -"
%token						EMPH		"!"
%token<cecko::gt_divop>		DIVOP		"/ or %"
%token<cecko::gt_cmpo>		CMPO		"<, >, <=, or >="
%token<cecko::gt_cmpe>		CMPE		"== or !="
%token						DAMP		"&&"
%token						DVERT		"||"
%token						ASGN		"="
%token<cecko::gt_cass>		CASS		"*=, /=, %=, +=, or -="
%token						SEMIC		";"
%token						LCUR		"{"
%token						RCUR		"}"
%token						COLON		":"

%token						TYPEDEF		"typedef"
%token						VOID		"void"
%token<cecko::gt_etype>		ETYPE		"_Bool, char, or int"
%token						STRUCT		"struct"
%token						ENUM		"enum"
%token						CONST		"const"
%token						IF			"if"
%token						ELSE		"else"
%token						DO			"do"
%token						WHILE		"while"
%token						FOR			"for"
%token						GOTO		"goto"
%token						CONTINUE	"continue"
%token						BREAK		"break"
%token						RETURN		"return"
%token						SIZEOF		"sizeof"

%token<CIName>				IDF			"identifier"
%token<CIName>				TYPEIDF		"type identifier"
%token<int>					INTLIT		"integer literal"
%token<CIName>				STRLIT		"string literal"

/////////////////////////////////

%type<CIName> id

%type<casem::keywords> type_qualifier
%type<std::list<CKTypeRefSafePack>> declaration_specifiers
%type<std::list<casem::declarator_t*>> declarator init_declarator direct_declarator init_declarator_list
%type<CKTypeSafeObs> type_specifier
%type<std::list<casem::keywords>> /*type_qualifier_list*/ pointer
%%

///DECLARATIONS////

declaration
	: declaration_specifiers init_declarator_list SEMIC { casem::parse_declaration(ctx, &$1, &$2, @1); }
	//| declaration_specifiers SEMIC
	;

declaration_specifiers
	//: storage_class_specifier
	: type_specifier
	{
		std::list<CKTypeRefSafePack> l = std::list<CKTypeRefSafePack>{};
		CKTypeRefSafePack new_type = CKTypeRefSafePack($1,false);
		l.emplace_back(new_type);
		$$ = l;
	}
	//| type_qualifier //predchozi bude const???
	//| storage_class_specifier declaration_specifiers
	//| type_specifier declaration_specifiers
	//| type_qualifier declaration_specifiers //{}
	;


init_declarator_list
	: init_declarator { $$ = $1; }
	//| init_declarator_list COMMA init_declarator { casem::init_declarator_list_add(&$1, );}
	;

init_declarator
	: declarator { $$ = $1; }
	;

/*
storage_class_specifier 
	: TYPEDEF //{ $$ = ctx->get_type_pack() ;} //dava CKTypeRefSafePack
	;
*/
type_specifier
	: VOID { $$ = ctx->get_void_type(); }
	| ETYPE {
		switch ($1) {
		case cecko::gt_etype::BOOL: $$ = ctx->get_bool_type(); break;
		case cecko::gt_etype::CHAR: $$ = ctx->get_char_type();  break;
		case cecko::gt_etype::INT: $$ = ctx->get_int_type();  break;
		default: break; }
	}
	//| struct_or_union_specifier //{ $$ = $1; }
	//| enum_specifier //{ $$ = get_enum_obj(); }
	//| typedef_name //{ $$ = $1; }
	;

/*
typedef_name
	: TYPEIDF //{ $$ = $1; }
	;

struct_or_union_specifier
	: struct_or_union id LCUR member_declaration_list RCUR //{}
	| struct_or_union id //{ $$ = parse_struct_decl(ctx, $1, $2); }
	;

struct_or_union
	: STRUCT //{ $$ = "struct"; }
	;

member_declaration_list
	: member_declaration
	| member_declaration_list member_declaration
	;

member_declaration
	: specifier_qualifier_list member_declarator_list SEMIC
	| specifier_qualifier_list SEMIC
	;
*/
specifier_qualifier_list
	: type_specifier specifier_qualifier_list
	| type_qualifier specifier_qualifier_list
	| type_specifier
	//| type_qualifier
	;

/*
member_declarator_list
	: member_declarator
	| member_declarator_list COMMA member_declarator
	;

member_declarator
	: declarator
	;

enum_specifier
	: ENUM id LCUR enumerator_list RCUR 
	| ENUM id LCUR enumerator_list COMMA RCUR
	| ENUM id
	;

enumerator_list
	: enumerator
	| enumerator_list COMMA enumerator
	;

enumerator
	: enumeration_constant
	| enumeration_constant ASGN constant_expression
	;

enumeration_constant
	: IDF //{ $$ = $1; }
	;
*/

type_qualifier
	: CONST { $$ = casem::keywords::Const; }
	;

declarator // std::list<casem::declarator_t*>
	: pointer direct_declarator
	{
		add_ptr_to_declarator(&$1, &$2);
		$$ = $2;
	}
	| direct_declarator { $$ = $1; }
	;

direct_declarator //std::list<casem::declarator_t*>
	: id
	{
		std::list<casem::declarator_t*> new_list{};
		parse_direct_declarator(&new_list, 1, $1);
		$$ = new_list;
	}
	//| LPAR declarator RPAR
	//| direct_declarator LBRA assignment_expression RBRA
	//| direct_declarator LPAR parameter_type_list RPAR
	;

pointer //std::list<casem::keywords>
	//: STAR type_qualifier_list
	: STAR
	{
		std::list<casem::keywords> new_list_ptr = std::list<casem::keywords>{casem::keywords::Star};
		$$ = new_list_ptr;
	}
	//| STAR type_qualifier_list pointer
	//| STAR pointer { $2.emplace_back(casem::keywords::Star), $$ = $2;}
	;
/*
type_qualifier_list //consts jeden za druhym list
	: type_qualifier 
	{
		std::list<casem::keywords> new_list = std::list<casem::keywords>{};
		new_list.emplace_back(casem::keywords::Const);
		$$ = new_list;
	}
	| type_qualifier_list type_qualifier 
	{
		$1.emplace_back($2);
		$$ = $1;
	}
	;
*/
parameter_type_list
	: parameter_list
	;

parameter_list
	: parameter_declaration
	| parameter_list COMMA parameter_declaration
	;

parameter_declaration
	: declaration_specifiers declarator
	| declaration_specifiers abstract_declarator
	| declaration_specifiers
	;

type_name
	: specifier_qualifier_list abstract_declarator
	| specifier_qualifier_list
	;

abstract_declarator
	: pointer
	| pointer direct_abstract_declarator
	| direct_abstract_declarator
	;

direct_abstract_declarator
	: LPAR abstract_declarator RPAR
	| array_abstract_declarator
	| function_abstract_declarator
	;

array_abstract_declarator
	: direct_abstract_declarator LBRA assignment_expression RBRA
	| LBRA assignment_expression RBRA
	;

function_abstract_declarator
	: direct_abstract_declarator LPAR parameter_type_list RPAR
	| LPAR parameter_type_list RPAR
	;

//////////EXPRESSIONS//////////////////

id
	: IDF //{ $$ = $1; }
	;

constant
	: INTLIT //{ $$ = $1; }
	;

primary_expression
	: id
	| constant
	| STRLIT
	| LPAR expression RPAR
	;

postfix_expression
	: primary_expression
	| postfix_expression LBRA expression RBRA
	| postfix_expression LPAR argument_exprsssion_list RPAR
	| postfix_expression LPAR RPAR
	| postfix_expression DOT id
	| postfix_expression ARROW id
	| postfix_expression INCDEC
	;

argument_exprsssion_list
	: assignment_expression
	| argument_exprsssion_list COMMA assignment_expression
	;

unary_expression
	: postfix_expression
	| INCDEC unary_expression
	| unary_operator cast_expression
	| SIZEOF LPAR type_name RPAR
	;

unary_operator
	: AMP
	| STAR
	| ADDOP
	| EMPH
	;

cast_expression
	: unary_expression
	;

multiplicative_expression
	: cast_expression
	| multiplicative_expression STAR cast_expression
	| multiplicative_expression DIVOP cast_expression
	;

additive_expression
	: multiplicative_expression
	| additive_expression ADDOP multiplicative_expression
	;

relational_expression
	: additive_expression
	| relational_expression CMPO additive_expression
	;

equality_expression
	: relational_expression
	| equality_expression CMPE relational_expression
	;

logical_and_expression
	: equality_expression
	| logical_and_expression DAMP equality_expression
	;

logical_or_expression
	: logical_and_expression
	| logical_or_expression DVERT logical_and_expression
	;

assignment_expression
	: logical_or_expression
	| unary_expression assignment_operator assignment_expression
	;


assignment_operator
	: CASS
	| ASGN
	;

expression
	: assignment_expression
	;
/*
constant_expression
	: logical_or_expression
	;
*/


///STATEMENTS/////


if_part
	: IF LPAR expression RPAR
	;

other_stm
	: expression_statement
	| compound_statement
	| jump_statement
	;

statement
	: match_stm
	| unmatch_stm
	;


match_stm
	: other_stm
	| if_part match_stm ELSE match_stm
	| WHILE LPAR expression RPAR match_stm
	| DO match_stm WHILE LPAR expression RPAR SEMIC
	| FOR LPAR expression SEMIC expression SEMIC expression RPAR match_stm
	| FOR LPAR expression SEMIC expression SEMIC RPAR match_stm
	| FOR LPAR expression SEMIC SEMIC expression RPAR match_stm
	| FOR LPAR expression SEMIC SEMIC RPAR match_stm
	| FOR LPAR SEMIC expression SEMIC expression RPAR match_stm
	| FOR LPAR SEMIC expression SEMIC RPAR match_stm
	| FOR LPAR SEMIC SEMIC expression RPAR match_stm
	| FOR LPAR SEMIC SEMIC RPAR match_stm
	;

unmatch_stm
	: if_part statement
	| if_part match_stm ELSE unmatch_stm
	| WHILE LPAR expression RPAR unmatch_stm
	| DO unmatch_stm WHILE LPAR expression RPAR SEMIC
	| FOR LPAR expression SEMIC expression SEMIC expression RPAR unmatch_stm
	| FOR LPAR expression SEMIC expression SEMIC RPAR unmatch_stm
	| FOR LPAR expression SEMIC SEMIC expression RPAR unmatch_stm
	| FOR LPAR expression SEMIC SEMIC RPAR unmatch_stm
	| FOR LPAR SEMIC expression SEMIC expression RPAR unmatch_stm
	| FOR LPAR SEMIC expression SEMIC RPAR unmatch_stm
	| FOR LPAR SEMIC SEMIC expression RPAR unmatch_stm
	| FOR LPAR SEMIC SEMIC RPAR unmatch_stm
	;


compound_statement
	: LCUR block_item_list RCUR
	| LCUR RCUR
	;

block_item_list
	: block_item
	| block_item_list block_item
	;

block_item
	: declaration
	| statement
	;

expression_statement
	: expression SEMIC
	| SEMIC
	;

jump_statement
	: RETURN expression SEMIC
	| RETURN SEMIC
	;

////EXTERNAL DEFINITIONS////////

cecko_s
	: translation_unit
	;

translation_unit
	: external_declaration
	| translation_unit external_declaration
	;

external_declaration
	: function_definition
	| declaration
	;

function_definition
	: declaration_specifiers declarator compound_statement
	;


/////////////////////////////////

%%

namespace cecko {

	void parser::error(const location_type& l, const std::string& m)
	{
		ctx->message(cecko::errors::SYNTAX, l, m);
	}

}
