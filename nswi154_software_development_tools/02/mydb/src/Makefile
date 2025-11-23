# Include propagated variables

# List of source files
CLIENT_SRCS := testclient.c client_comm.c
SERVER_SRCS := testserver.c server_comm.c
ADMIN_SRCS := testadmin.c
CODE_FILES := $(wildcard *.c) 

# List of object files (excluding those specific to binary programs)
# 1st way: doesnt work
BINARY_SRC := testclient.c testserver.c testadmin.c client_comm.c server_comm.c
LIB_SRC := $(filter-out $(BINARY_SRC), $(CODE_FILES))
LIB_OBJ := $(LIB_SRC:.c=.o)
#2nd way: works
# LIB_OBJ = btree_int.o config.o db_main.o error.o parser_conds.o tabconf.o conds.o db_engine.o memory.o parser_commands.o result.o strutil.o data.o index.o query.o sql_parser.o tabdef.o textfileio.o


# Binary program targets
testclient: $(CLIENT_SRCS:.c=.o) $(LIB_OBJ)
	$(CC) $(CFLAGS) -o $@ $^ $(LDLIBS)

testserver: $(SERVER_SRCS:.c=.o) $(LIB_OBJ)
	$(CC) $(CFLAGS) -o $@ $^ $(LDLIBS)

testadmin: $(ADMIN_SRCS:.c=.o) $(LIB_OBJ)
	$(CC) $(CFLAGS) -o $@ $^ $(LDLIBS)

# Compilation rules
%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

# Library creation rule
libmydb.a: $(LIB_OBJ)
	ar -rcvs $@ $^

# Clean rule
clean:
	rm -f *.o $(LIB_TARGET) $(ADMIN_TARGET) $(CLIENT_TARGET) $(SERVER_TARGET)