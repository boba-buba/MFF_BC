# Compiler
CXX := g++
# Compiler flags
CXXFLAGS := -std=c++17 -Wall -Wextra -I../lib/include
# Source files
SRCDIR := ../lib/src
SOURCES := $(wildcard $(SRCDIR)/*.cpp) numactl.cpp
# Object files
OBJDIR := build/obj
OBJECTS := $(patsubst $(SRCDIR)/%.cpp,$(OBJDIR)/%.o,$(SOURCES))
# Executable name
TARGET := build/numactl_07

.PHONY: all clean

all: build/numactl_07

# Build the executable
build/numactl_07: $(OBJECTS)
	@mkdir -p $(@D)
	$(CXX) $(CXXFLAGS) $^ -o $@

# Compile source files
$(OBJDIR)/%.o: $(SRCDIR)/%.cpp
	@mkdir -p $(@D)
	$(CXX) $(CXXFLAGS) -c $< -o $@

# Compile numactl.cpp
$(OBJDIR)/numactl.o: numactl.cpp
	@mkdir -p $(@D)
	$(CXX) $(CXXFLAGS) -c $< -o $@

clean:
	rm -rf $(OBJDIR) build
