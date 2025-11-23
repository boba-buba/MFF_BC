#ifndef SYCL_POTENTIAL_SERIAL_IMPLEMENTATION_HPP
#define SYCL_POTENTIAL_SERIAL_IMPLEMENTATION_HPP

#include <sycl/sycl.hpp>
#include "interface.hpp"
#include "data.hpp"
#include "serial.hpp"

#include <memory>
#include <vector>
#include <map>
#include <iostream>
#include <stdio.h>
#include <algorithm>
// Create an exception handler for asynchronous SYCL exceptions
static auto exception_handler = [](sycl::exception_list e_list) {
	for (std::exception_ptr const& e : e_list) {
		try {
			std::rethrow_exception(e);
		}
		catch (std::exception const& e) {
#if _DEBUG
			std::cout << "Failure" << std::endl;
#endif
			std::terminate();
		}
	}
};

template<typename F = float, typename IDX_T = std::uint32_t, typename LEN_T = std::uint32_t>
class ParallelSim {
public:
	typedef F coord_t;		// Type of point coordinates.
	typedef coord_t real_t;	// Type of additional float parameters.
	typedef IDX_T index_t;
	typedef LEN_T length_t;
	typedef Point<coord_t> point_t;
	typedef Edge<index_t> edge_t;
private:

	std::vector<edge_t> mEdges;
	std::vector<length_t> mLengths;
	index_t mPointCount = 0;
	std::vector<point_t> mVelocities;			///< Point velocity vectors.
	std::vector<point_t> mForces;				///< Preallocated buffer for force vectors.
	std::vector<point_t> mPoints;
	ModelParameters<F> mParams;
	sycl::queue q;

	void addCompulsiveForce(const std::vector<point_t>& points, index_t p1, index_t p2, length_t length, std::vector<point_t>& forces)
	{
		real_t dx = (real_t)points[p2].x - (real_t)points[p1].x;
		real_t dy = (real_t)points[p2].y - (real_t)points[p1].y;
		real_t sqLen = dx * dx + dy * dy;
		real_t fact = (real_t)std::sqrt(sqLen) * mParams.edgeCompulsion / (real_t)(length);
		dx *= fact;
		dy *= fact;
		forces[p1].x += dx;
		forces[p1].y += dy;
		forces[p2].x -= dx;
		forces[p2].y -= dy;
	}

public:
	ParallelSim(index_t pointCount, const std::vector<edge_t>& edges,
		const std::vector<length_t>& lengths, const ModelParameters<real_t>& params):
		mEdges(edges),
		mLengths(lengths),
		mPointCount(pointCount),
		mVelocities(pointCount, point_t()),
		mForces(pointCount, point_t()),
		mPoints(pointCount, point_t()),
		mParams(params),
		q(sycl::default_selector_v, exception_handler){}

	void updatePoints(std::vector<point_t>& points)
	{
		if (points.size() != mVelocities.size())
			throw (bpp::RuntimeError() << "Cannot compute next version of point positions."
				<< "Current model uses " << mVelocities.size() << " points, but the given buffer has " << points.size() << " points.");


		mForces.resize(points.size());

		{
			sycl::buffer<point_t, 1> forceBuff{ mForces.data(), sycl::range<1>(mForces.size()) };
			q.submit([&](sycl::handler& h) {

				auto forceAcc = forceBuff.template get_access<sycl::access::mode::read_write>(h);
				h.parallel_for(mPointCount, [=](sycl::id<1> idx) {
					forceAcc[idx].x = forceAcc[idx].y = (real_t)0.0;
					});
				}).wait();
		}

		{
			sycl::buffer<point_t, 1> forceBuff{ mForces.data(), sycl::range<1>(mForces.size()) };
			sycl::buffer<point_t, 1> pointBuff{ points.data(), sycl::range<1>(points.size()) };
			q.submit([&](sycl::handler& h) {
				auto pointAcc = pointBuff.template get_access<sycl::access::mode::read>(h);
				auto forceAcc = forceBuff.template get_access<sycl::access::mode::read_write>(h);
				auto count = mPointCount;
				auto repulsion = mParams.vertexRepulsion;

				h.parallel_for(mPointCount, [=](sycl::id<1> i) {
					for (index_t j = 0; j < count; ++j) {

						real_t dx = (real_t)pointAcc[i].x - (real_t)pointAcc[j].x;
						real_t dy = (real_t)pointAcc[i].y - (real_t)pointAcc[j].y;
						real_t sqLen = std::max<real_t>(dx*dx + dy*dy, (real_t)0.0001);
						real_t fact = repulsion / (sqLen * (real_t)std::sqrt(sqLen));	// mul factor
						dx *= fact;
						dy *= fact;
						forceAcc[i].x += dx;
						forceAcc[i].y += dy;
					}
					});
				});
			q.wait();

		}

		for (std::size_t i = 0; i < mEdges.size(); ++i)
			addCompulsiveForce(points, mEdges[i].p1, mEdges[i].p2, mLengths[i], mForces);

		{
			sycl::buffer<point_t, 1> velocBuff{ mVelocities.data(), sycl::range<1>(mVelocities.size()) };
			sycl::buffer<point_t, 1> forceBuff{ mForces.data(), sycl::range<1>(mForces.size()) };

			q.submit([&](sycl::handler& h) {
				auto velocAcc = velocBuff.template get_access<sycl::access::mode::read_write>(h);
				auto forceAcc = forceBuff.template get_access<sycl::access::mode::read>(h);
				auto fact = mParams.timeQuantum / mParams.vertexMass;
				auto slowdown = mParams.slowdown;

				h.parallel_for(mPointCount, [=](sycl::id<1> i) {
					velocAcc[i].x = (velocAcc[i].x + (real_t)forceAcc[i].x * fact) * slowdown;
					velocAcc[i].y = (velocAcc[i].y + (real_t)forceAcc[i].y * fact) * slowdown;
					});
				});
			q.wait();
		}


		// Update point positions.
		{
			sycl::buffer<point_t, 1> velocBuff{ mVelocities.data(), sycl::range<1>(mVelocities.size()) };
			sycl::buffer<point_t, 1> pointBuff{ points.data(), sycl::range<1>(points.size()) };
			q.submit([&](sycl::handler& h) {
				auto pointAcc = pointBuff.template get_access<sycl::access::mode::read_write>(h);
				auto velocAcc = velocBuff.template get_access<sycl::access::mode::read>(h);
				auto timeQuantum = mParams.timeQuantum;

				h.parallel_for(mPointCount, [=](sycl::id<1> i) {
					pointAcc[i].x += velocAcc[i].x * timeQuantum;
					pointAcc[i].y += velocAcc[i].y * timeQuantum;
					});
				});
			q.wait();
		}

	}

	const std::vector<point_t>& getVelocities() const { return mVelocities; }

};


/*
 * Final implementation of the tested program.
 */
template<typename F = float, typename IDX_T = std::uint32_t, typename LEN_T = std::uint32_t>
class ProgramPotential : public IProgramPotential<F, IDX_T, LEN_T>
{
public:
	typedef F coord_t;		// Type of point coordinates.
	typedef coord_t real_t;	// Type of additional float parameters.
	typedef IDX_T index_t;
	typedef LEN_T length_t;
	typedef Point<coord_t> point_t;
	typedef Edge<index_t> edge_t;
	typedef ParallelSim<coord_t, index_t, length_t> sim_t;
	//typedef SerialSimulator<coord_t, index_t, length_t> simulator_t;

private:
	std::unique_ptr<sim_t> mSimulator;

public:
	virtual void initialize(index_t points, const std::vector<edge_t>& edges, const std::vector<length_t>& lengths, index_t iterations)
	{
		mSimulator = std::make_unique<sim_t>(points, edges, lengths, this->mParams);
	}

	virtual void iteration(std::vector<point_t>& points)
	{
		mSimulator->updatePoints(points);
	}

	virtual void getVelocities(std::vector<point_t>& velocities)
	{
		velocities = mSimulator->getVelocities();
	}
};



#endif
