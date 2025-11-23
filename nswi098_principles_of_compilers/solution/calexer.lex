%{

// allow access to YY_DECL macro
#include "ckbisonflex.hpp"
#include "casem.hpp"

#include INCLUDE_WRAP(BISON_HEADER)
int num_mult_comm = 0;
int num_str_quote = 0;
std::string string_lit = "";
int char_lit = -1;
%}

/* NEVER SET %option outfile INTERNALLY - SHALL BE SET BY CMAKE */

%option noyywrap nounput noinput
%option batch never-interactive reentrant
%option nounistd 

%x IN_COMMENT
%x IN_CHAR
%x IN_STR

/* AVOID backup perf-report - DO NOT CREATE UNMANAGEABLE BYPRODUCT FILES */


ID				[a-zA-Z_][a-zA-Z0-9_]*

DIGIT			[0-9]

CHARCONST		[^\'\\\n]

ESC_SEQ			[\'\"\?\\\a\b\f\n\r\t\v]|((\\x)[0-9a-fA-F]{1,2})



%%

"//".*          { /* DO NOTHING */ }

"/*" 			{num_mult_comm++; BEGIN(IN_COMMENT);}
"*/"			ctx->message(cecko::errors::UNEXPENDCMT, ctx->line());
<IN_COMMENT>{

"/*"			num_mult_comm++;
"*/" 			{ num_mult_comm--; if (num_mult_comm == 0) BEGIN(INITIAL);}

[^*\n]+ 		// eat comment in chunks
"*" 			// eat the lone star
\n 				ctx->incline();
<<EOF>>			{
				ctx->message(cecko::errors::EOFINCMT, ctx->line()); 
				num_mult_comm = 0; 
				BEGIN(INITIAL);
				}
}

\"				string_lit = ""; num_str_quote++; BEGIN(IN_STR);

<IN_STR>{

\\n				string_lit += '\x0a';

\\[\"]				string_lit += '\x22';

(\\x)[0-9a-fA-F]*   string_lit += casem::parse_hex_esc_str(yytext, ctx);

\n				{BEGIN(INITIAL); ctx->incline();
				ctx->message(cecko::errors::EOLINSTRCHR, ctx->line() - 1);
				return cecko::parser::make_STRLIT(string_lit, ctx->line() - 1);
				string_lit = "";
				}

<<EOF>>			{BEGIN(INITIAL); ctx->incline();
				ctx->message(cecko::errors::EOFINSTRCHR, ctx->line() - 1);
				return cecko::parser::make_STRLIT(string_lit, ctx->line() - 1);
				string_lit = "";
				}

\"				{ BEGIN(INITIAL); return cecko::parser::make_STRLIT(string_lit, ctx->line()); string_lit = "";}


(\\)[A-Za-wy-z0-9]*	string_lit += casem::parse_str_error(yytext, ctx);

[^\\"\n]|{ESC_SEQ}	string_lit += yytext;

}



"\'"			char_lit = -1; BEGIN(IN_CHAR);

<IN_CHAR>{

(\\x)[0-9a-fA-F]*   char_lit = casem::parse_hex_esc(yytext, ctx);

\n				{BEGIN(INITIAL);ctx->incline();
				ctx->message(cecko::errors::EOLINSTRCHR, ctx->line() - 1);
				return cecko::parser::make_INTLIT(char_lit, ctx->line() -1);
				char_lit = -1;
				}

<<EOF>>			{BEGIN(INITIAL);
				ctx->message(cecko::errors::EOFINSTRCHR, ctx->line());
				return cecko::parser::make_INTLIT(char_lit, ctx->line());
				char_lit = -1;
				}

";"				{char_lit = *yytext;
				ctx->message(cecko::errors::UNTERMCHAR, ctx->line());
				BEGIN(INITIAL);
				return cecko::parser::make_INTLIT(char_lit, ctx->line()); 
				char_lit = -1;
				}

\'				{ if (char_lit == -1) { ctx->message(cecko::errors::EMPTYCHAR, ctx->line()); char_lit = 0;}

				BEGIN(INITIAL);
				return cecko::parser::make_INTLIT(char_lit, ctx->line()); 
				char_lit = -1;
				}

(\\)[A-Za-z0-9]* char_lit = casem::parse_char_error(yytext, ctx);

{CHARCONST}		char_lit = *yytext;

}



\n				ctx->incline();


"("				return cecko::parser::make_LPAR(ctx->line());

")"				return cecko::parser::make_RPAR(ctx->line());

"["				return cecko::parser::make_LBRA(ctx->line());

"]"				return cecko::parser::make_RBRA(ctx->line());

"{"				return cecko::parser::make_LCUR(ctx->line());

"}"				return cecko::parser::make_RCUR(ctx->line());

"."				return cecko::parser::make_DOT(ctx->line());

"->"			return cecko::parser::make_ARROW(ctx->line());

"++"			return cecko::parser::make_INCDEC(cecko::gt_incdec::INC, ctx->line());
"--"			return cecko::parser::make_INCDEC(cecko::gt_incdec::DEC, ctx->line());

"*="			return cecko::parser::make_CASS(cecko::gt_cass::MULA, ctx->line());
"/="			return cecko::parser::make_CASS(cecko::gt_cass::DIVA, ctx->line());
"%="			return cecko::parser::make_CASS(cecko::gt_cass::MODA, ctx->line());
"+="			return cecko::parser::make_CASS(cecko::gt_cass::ADDA, ctx->line());
"-="			return cecko::parser::make_CASS(cecko::gt_cass::SUBA, ctx->line());

"*"				return cecko::parser::make_STAR(ctx->line());

"+"				return cecko::parser::make_ADDOP(cecko::gt_addop::ADD, ctx->line());
"-"				return cecko::parser::make_ADDOP(cecko::gt_addop::SUB, ctx->line());

"/"				return cecko::parser::make_DIVOP(cecko::gt_divop::DIV, ctx->line());
"%"				return cecko::parser::make_DIVOP(cecko::gt_divop::MOD, ctx->line());

"<="			return cecko::parser::make_CMPO(cecko::gt_cmpo::LE, ctx->line());
"<"				return cecko::parser::make_CMPO(cecko::gt_cmpo::LT, ctx->line());
">="			return cecko::parser::make_CMPO(cecko::gt_cmpo::GE, ctx->line());
">"				return cecko::parser::make_CMPO(cecko::gt_cmpo::GT, ctx->line());

"=="			return cecko::parser::make_CMPE(cecko::gt_cmpe::EQ, ctx->line());
"!="			return cecko::parser::make_CMPE(cecko::gt_cmpe::NE, ctx->line());

"!"				return cecko::parser::make_EMPH(ctx->line());

"&&"			return cecko::parser::make_DAMP(ctx->line());

"||"			return cecko::parser::make_DVERT(ctx->line());

"&"				return cecko::parser::make_AMP(ctx->line());

"="				return cecko::parser::make_ASGN(ctx->line());

";"				return cecko::parser::make_SEMIC(ctx->line());

":"				return cecko::parser::make_COLON(ctx->line());

","				return cecko::parser::make_COMMA(ctx->line());



[ \t]+			{ /* DO NOTHING */ }

<<EOF>>			return cecko::parser::make_EOF(ctx->line());

break			return cecko::parser::make_BREAK(ctx->line());

const			return cecko::parser::make_CONST(ctx->line());

continue		return cecko::parser::make_CONTINUE(ctx->line());

do				return cecko::parser::make_DO(ctx->line());

else			return cecko::parser::make_ELSE(ctx->line());

enum			return cecko::parser::make_ENUM(ctx->line());

for				return cecko::parser::make_FOR(ctx->line());

goto			return cecko::parser::make_GOTO(ctx->line());

if				return cecko::parser::make_IF(ctx->line());

return 			return cecko::parser::make_RETURN(ctx->line());

sizeof			return cecko::parser::make_SIZEOF(ctx->line());

struct			return cecko::parser::make_STRUCT(ctx->line());

typedef			return cecko::parser::make_TYPEDEF(ctx->line());

void			return cecko::parser::make_VOID(ctx->line());

while			return cecko::parser::make_WHILE(ctx->line());

int				return cecko::parser::make_ETYPE(cecko::gt_etype::INT, ctx->line());

char			return cecko::parser::make_ETYPE(cecko::gt_etype::CHAR, ctx->line());

_Bool			return cecko::parser::make_ETYPE(cecko::gt_etype::BOOL, ctx->line());


{ID}			{if (ctx->is_typedef(yytext)) 
					{return cecko::parser::make_TYPEIDF(yytext, ctx->line());}
				else
					return cecko::parser::make_IDF(yytext, ctx->line());}




(0x)([0-9a-z]+)|(0X)([0-9A-Z]+) return cecko::parser::make_INTLIT(casem::parse_int(yytext, ctx), ctx->line());

{DIGIT}+[a-zA-Z]*		return cecko::parser::make_INTLIT(casem::parse_int(yytext, ctx), ctx->line());


.				ctx->message(cecko::errors::UNCHAR, ctx->line(), yytext);

%%

namespace cecko {

	yyscan_t lexer_init(FILE * iff)
	{
		yyscan_t scanner;
		yylex_init(&scanner);
		yyset_in(iff, scanner);
		return scanner;
	}

	void lexer_shutdown(yyscan_t scanner)
	{
		yyset_in(nullptr, scanner);
		yylex_destroy(scanner);
	}



}
