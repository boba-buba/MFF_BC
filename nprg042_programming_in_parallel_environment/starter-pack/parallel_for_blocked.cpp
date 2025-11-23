#include <iostream>
#include <vector>
#include <tbb/tbb.h>

// Function to process elements in a range in parallel
void process_elements(const std::vector<int>& data) {
    // Define a lambda function to process each element in the range
    auto process_func = [&](const tbb::blocked_range<size_t>& range) {
        for (size_t i = range.begin(); i != range.end(); ++i) {
            // Process data[i]
            std::cout << "Processing element " << data[i] << " in thread " << tbb::this_tbb_thread::get_id() << std::endl;
        }
    };

    // Create a blocked range for the entire data vector
    tbb::blocked_range<size_t> range(0, data.size());

    // Perform parallel processing using tbb::parallel_for
    tbb::parallel_for(range, process_func);
}

int main() {
    // Initialize data vector
    std::vector<int> data = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24};

    // Process elements in parallel
    process_elements(data);

    return 0;
}
