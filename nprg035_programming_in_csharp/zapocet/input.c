#define VAR3 45
#include additional.cpp
#define VAR 15
#define DEBUG
#ifdef VAR
	print __LINE__
#ifdef DEBUG
		print DEBUG
#endif
#ifdef FOO
		print FOO
#else
		print BAR
		print __FILE__
#endif
#endif
#undef VAR
#define DEBUG 50
#ifdef VAR
		print VAR

#else
		print NOT_VAR
		print DEBUG
#endif
#ifdef VAR3
		print VAR3
#else
		print NOT_VAR3
#endif