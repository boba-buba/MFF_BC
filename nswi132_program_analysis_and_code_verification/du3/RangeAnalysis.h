#ifndef LLVM_TRANSFORMS_UTILS_RANGEANALYSIS_H
#define LLVM_TRANSFORMS_UTILS_RANGEANALYSIS_H

#include "llvm/IR/PassManager.h"

namespace llvm {

    class RangeAnalysisPass : public PassInfoMixin<RangeAnalysisPass> {
    public:
      PreservedAnalyses run(Function &F, FunctionAnalysisManager &M);
    };

    } // namespace llvm

#endif // LLVM_TRANSFORMS_UTILS_RANGEANALYSIS_H