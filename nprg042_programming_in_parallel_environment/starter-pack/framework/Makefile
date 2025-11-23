CPP=g++
CFLAGS=-Wall -O2 -std=c++11
#TBB_DIR=/usr/local
INCLUDE=. ../framework/internal /usr/include
LDFLAGS=
LIBS=tbb
LIBDIRS=/usr/include/tbb
SOURCE=k-means.cpp
HEADERS=$(shell find . -name '*.hpp')
EXECUTABLE=./k-means


.PHONY: all clear clean purge

all: $(EXECUTABLE)



# Building Targets

$(EXECUTABLE): $(SOURCE) $(HEADERS)
	@$(CPP) $(CFLAGS) $(addprefix -I,$(INCLUDE)) $(LDFLAGS) $(addprefix -L,$(LIBDIRS)) $(addprefix -l,$(LIBS)) $< -o $@



# Cleaning Stuff

clear:
	@echo $(LIBDIRS)
	@echo Removing all generated files...
	-@rm -f $(EXECUTABLE)

clean: clear

purge: clear
