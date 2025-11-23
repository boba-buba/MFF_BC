# Compiler
CXX := g++
# Compiler flags
CXXFLAGS := -std=c++17 -Wall -Wextra -I../lib/include
# Source files
SRCDIR := ../lib/src
SOURCES := $(wildcard $(SRCDIR)/*.cpp) time.cpp
# Object files
OBJDIR := build/obj
OBJECTS := $(patsubst $(SRCDIR)/%.cpp,$(OBJDIR)/%.o,$(SOURCES))
# Executable name
TARGET := build/time

.PHONY: all clean

all: build/time

# Build the executable
build/time: $(OBJECTS)
	@mkdir -p $(@D)
	$(CXX) $(CXXFLAGS) $^ -o $@

# Compile source files
$(OBJDIR)/%.o: $(SRCDIR)/%.cpp
	@mkdir -p $(@D)
	$(CXX) $(CXXFLAGS) -c $< -o $@

# Compile time.cpp
$(OBJDIR)/time.o: time.cpp
	@mkdir -p $(@D)
	$(CXX) $(CXXFLAGS) -c $< -o $@

clean:
	rm -rf $(OBJDIR) build/

