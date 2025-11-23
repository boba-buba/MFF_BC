# Compiler
CXX = g++

# Compiler flags
CXXFLAGS = -std=c++20 -Wall
CXXFLAGSTEST = -lgtest -lgtest_main -pthread

NUMACTL_09 = build/numactl_09
NUMACTL_07 = build/numactl_07
TIME = build/time

export NUMACTL_07 NUMACTL_09 TIME

all:
	$(MAKE) -C numactl_07 $(NUMACTL_07)
	$(MAKE) -C numactl_09 $(NUMACTL_09)
	$(MAKE) -C time $(TIME)

clean:
	rm -rf lib/docs
	$(MAKE) -C numactl_07 clean
	$(MAKE) -C numactl_09 clean
	$(MAKE) -C time clean

winclean:
	rd /S /Q "build"
	rd /S /Q "-p"

docs:
	doxygen lib/Doxyfile

test-build:
	cmake -S gtest -B gtest/build
	make -C gtest/build

test:
	@if [ -f ./gtest/build/tests ]; then \
		./gtest/build/tests; \
	elif [ -f ./gtest/build/tests.exe ]; then \
		./gtest/build/tests.exe; \
	else \
		echo "Test executable not found"; \
		exit 1; \
	fi

test-clean:
	rm gtest/build -r

.PHONY: clean all