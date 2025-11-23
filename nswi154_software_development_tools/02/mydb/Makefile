# Compiler
CC = gcc
export CC
#Flages
CFLAGS = -g -Wall
export CFLAGS
#Librar
LDLIBS := -L. -lmydb -lreadline -lhistory
export LDLIBS

CLIENT_TARGET = testclient
SERVER_TARGET = testserver
ADMIN_TARGET = testadmin
LIB_TARGET = libmydb.a
export CLIENT_TARGET SERVER_TARGET ADMIN_TARGET LIB_TARGET

all:
	$(MAKE) -C src $(LIB_TARGET)
	$(MAKE) -C src $(ADMIN_TARGET)
	$(MAKE) -C src $(SERVER_TARGET)
	$(MAKE) -C src $(CLIENT_TARGET)

clean:
	$(MAKE) -C src clean
