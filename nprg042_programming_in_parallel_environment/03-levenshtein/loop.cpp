#include <iostream>
#include <vector>

void antidiagonalTraversal(const std::vector<std::vector<int>>& matrix) {
    int rows = matrix.size();
    int cols = matrix[0].size();

    //Traverse each antidiagonal
    for (int sum = 0; sum <= rows + cols - 2; ++sum) {
        // Traverse each element of the current antidiagonal
        for (int i = 0; i <= sum; ++i) {
            int row = sum - i;
            int col =  i;
            // Ensure the current position is within matrix bounds
            if (row < rows && col >= 0 && col < cols) {
                //std::cout << matrix[row][col] << " ";
                std::cout << row << " " << col << std::endl;
            }
        }
        std::cout << std::endl; // Move to the next antidiagonal
    }

    
}

int main() {
    std::vector<std::vector<int>> matrix = {
        {1, 2, 3},
        {4, 5, 6},
        {7, 8, 9},
        {10, 11, 12},
        {13, 14, 15},
        {16, 17, 18}
    };

    antidiagonalTraversal(matrix);

    return 0;
}
