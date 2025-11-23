#ifndef LEVENSHTEIN_SERIAL_IMPLEMENTATION_HPP
#define LEVENSHTEIN_SERIAL_IMPLEMENTATION_HPP

#include <utility>
#include <vector>
#include <iostream>

template<typename C = char, typename DIST = std::size_t, bool DEBUG = false>
class EditDistance : public IEditDistance<C, DIST, DEBUG>
{
	using vec = std::vector<DIST>;
private:
	DIST mBlock_size = 64;
	DIST mCols, mRows, mRows_max, mRows_min;
    std::vector<vec> mRows_cache, mCols_cache;
	int mDiags, mWidth, mHeight;

public:
	/*
	 * \brief Perform the initialization of the functor (e.g., allocate memory buffers).
	 * \param len1, len2 Lengths of first and second string respectively.
	 */
	virtual void init(DIST len1, DIST len2)
	{
		mRows_max = std::max<DIST>(len1, len2);
		mRows_min = std::min<DIST>(len1, len2);
		mCols = mRows = mBlock_size + 1;
		mHeight = mRows_max / mBlock_size;
		mWidth = mRows_min / mBlock_size;
		mDiags = mWidth + mHeight - 1;
	}

	void compute_block_serial(vec& current_row, vec& current_col, vec& mCol, const C* s1, const C* s2, DIST row_id, DIST col_id)
	{
		current_col[0] = current_row[mRows - 1];
		for (std::size_t row = 1; row < mRows; ++row)
		{
			DIST lastUpper = mCol[row - 1];
            DIST lastLeft = mCol[row];
            current_row[0] = mCol[row];
			for (std::size_t i = 1; i < mCols; ++i)
			{
				DIST dist1 = std::min<DIST>(current_row[i], lastLeft) + 1;
				DIST s2_id = mBlock_size * row_id + i - 1;
				DIST s1_id = mBlock_size * col_id + row - 1;
				DIST dist2 = lastUpper + (s2[s2_id] == s1[s1_id] ? 0 : 1);
				lastUpper = current_row[i];
				lastLeft = current_row[i] = std::min<DIST>(dist1, dist2);
			}
			current_col[row] = current_row[mCols-1];
		}
	}

	/*
	 * \brief Compute the distance between two strings.
	 * \param str1, str2 Strings to be compared.
	 * \result The computed edit distance.
	 */
	virtual DIST compute(const std::vector<C> &str1, const std::vector<C> &str2)
	{
		// Special case (one of the strings is empty).
		if (mRows_min == 0)
			return mRows_max;

		// Make sure s1 is the shorter string and s2 is the longer one.
		const C* s1 = &str1[0];
		const C* s2 = &str2[0];
		if (str1.size() > str2.size())
			std::swap(s1, s2);


		for (int diag = 0; diag < mDiags; ++diag)
		{
			int iend;
			if (diag <= mWidth + 1) iend = 0;
			else iend = diag - mWidth + 1;

			int istart = std::min(diag, mHeight - 1);

			vec init_vector(mCols);
			if (istart == diag)
			{
				for (DIST i = 0; i < mCols; ++i) init_vector[i] = i + diag * mBlock_size;
				mCols_cache.push_back(init_vector);
			}
			if (iend == 0)
			{
				for (DIST i = 0; i < mCols; ++i) init_vector[i] = i + diag * mBlock_size;
				mRows_cache.push_back(init_vector);
			}

			#pragma omp parallel for
			for (int row = istart; row >= iend; --row)
			{
				vec current_row = mRows_cache[diag - row];
				vec current_col(mRows);
				compute_block_serial(current_row, current_col, mCols_cache[row], s1, s2, row, diag - row);
				mRows_cache[diag - row] = current_row;
				mCols_cache[row] = current_col;
			}
		}
		//std::cout << "width " << mWidth << " cols " << mCols << std::endl;
		//std::cout << "cache row " << mRows_cache.size() << " cache row[mWidth -1] " << mRows_cache[mWidth - 1].size() << std::endl;
		return mRows_cache[mWidth - 1].back();
	}
};


#endif
