#include "llvm/ADT/SmallVector.h"
#include "llvm/IR/Constants.h"
#include "llvm/IR/Function.h"
#include "llvm/IR/InstrTypes.h"
#include "llvm/IR/Instructions.h"
#include "llvm/IR/PassManager.h"
#include "llvm/IR/Value.h"
#include "llvm/IR/DebugInfoMetadata.h"
#include "llvm/IR/Type.h"
#include "llvm/Pass.h"
#include "llvm/Support/raw_ostream.h"
#include "llvm/Analysis/LoopInfo.h"
#include "llvm/Analysis/LoopAnalysisManager.h"
#include "llvm/Analysis/ScalarEvolution.h"
#include "llvm/Analysis/ScalarEvolutionExpressions.h"
#include <limits>
#include <unordered_map>
#include <algorithm>


using namespace llvm;

// Simple range abstraction. Each LLVM value is assumed to have a numeric range lower to upper.
struct Interval {
    int64_t lower;
    int64_t upper;

    Interval() : lower(std::numeric_limits<int64_t>::min()), upper(std::numeric_limits<int64_t>::max()) {}
    Interval(int64_t l, int64_t u) : lower(l), upper(u) {}

    static Interval join(const Interval &a, const Interval &b) {
        return Interval(std::min(a.lower, b.lower), std::max(a.upper, b.upper));
    }

    bool isOutOfBounds(int64_t L, int64_t U) const {
        return lower < L || upper > U;
    }

    void print(raw_ostream &OS) const {
        OS << "[" << lower << ", " << upper << "]";
    }
};

class RangeAnalysisPass : public PassInfoMixin<RangeAnalysisPass> {
    using RangeMap = std::unordered_map<const Value*, Interval>;

    int64_t ConfigLower = -100;
    int64_t ConfigUpper = 100;

    // Checks if a value is a constant and returns an interval like [5, 5]
    Interval getConstInterval(Value *V) {
        if (auto *constInt = dyn_cast<ConstantInt>(V)) {
            int64_t val = constInt->getSExtValue();
            return Interval(val, val);
        }
        return Interval();
    }

    // Estimates the result of a binary operation (add, sub, mul) between two intervals.
    // Example: [1, 3] + [2, 4] = [3, 7]
    Interval evalBinaryOp(unsigned opcode, const Interval &lhs, const Interval &rhs) {
        switch (opcode) {
            case Instruction::Add:
                return Interval(lhs.lower + rhs.lower, lhs.upper + rhs.upper);
            case Instruction::Sub:
                return Interval(lhs.lower - rhs.upper, lhs.upper - rhs.lower);
            case Instruction::Mul: {
                int64_t vals[] = {
                    lhs.lower * rhs.lower,
                    lhs.lower * rhs.upper,
                    lhs.upper * rhs.lower,
                    lhs.upper * rhs.upper
                };
                return Interval(*std::min_element(vals, vals + 4), *std::max_element(vals, vals + 4));
            }
            default:
                return Interval();
        }
    }

    void analyzeFunction(Function &F, RangeMap &Ranges, LoopInfo &LI, ScalarEvolution &SE) {

        for (BasicBlock &basicBlock : F) {
            for (Instruction &instruction : basicBlock) {
                // PHI node handling using SCEV (PHI nodes represent loops here)
                if (auto *PN = dyn_cast<PHINode>(&instruction)) {
                    // PHI nodes are used to represent induction variables.
                    // Scalar Evolution (SE) to get symbolic expressions of such variables.
                    const SCEV *S = SE.getSCEV(PN);
                    if (SE.isSCEVable(PN->getType())) {
                        const SCEVAddRecExpr *AR = dyn_cast<SCEVAddRecExpr>(S);
                        // If affine recurrence (i = i+1), then get the loop bounds
                        if (AR && AR->isAffine()) {
                            const Loop *L = AR->getLoop();
                            const SCEV *backedgeTakenCount = SE.getBackedgeTakenCount(L);
                            if (!isa<SCEVCouldNotCompute>(backedgeTakenCount)) {
                                auto range = SE.getSignedRange(S);
                                Interval interval(range.getSignedMin().getSExtValue(),
                                                  range.getSignedMax().getSExtValue());
                                Ranges[PN] = interval;
                                if (interval.isOutOfBounds(ConfigLower, ConfigUpper)) {
                                    errs() << "Warning: PHI node out of bounds: " << *PN << "\n";
                                    errs() << "         Range: ";
                                    interval.print(errs());
                                    errs() << "\n";
                                }
                            }
                        }
                    }
                }

                // Store instruction
                if (auto *SI = dyn_cast<StoreInst>(&instruction)) {
                    Value *val = SI->getValueOperand();
                    Value *ptr = SI->getPointerOperand();
                    Ranges[ptr] = Ranges.count(val) ? Ranges[val] : getConstInterval(val);
                }

                // Load instruction
                if (auto *LI = dyn_cast<LoadInst>(&instruction)) {
                    Value *ptr = LI->getPointerOperand();
                    Ranges[&instruction] = Ranges.count(ptr) ? Ranges[ptr] : Interval();
                }

                // Binary operator
                if (auto *BO = dyn_cast<BinaryOperator>(&instruction)) {
                    Value *op1 = BO->getOperand(0);
                    Value *op2 = BO->getOperand(1);
                    Interval lhs = Ranges.count(op1) ? Ranges[op1] : getConstInterval(op1);
                    Interval rhs = Ranges.count(op2) ? Ranges[op2] : getConstInterval(op2);
                    Ranges[&instruction] = evalBinaryOp(BO->getOpcode(), lhs, rhs);
                }

                // out-of-bounds variable
                if (Ranges.count(&instruction)) {
                    Interval current = Ranges[&instruction];
                    if (current.isOutOfBounds(ConfigLower, ConfigUpper)) {
                        errs() << "Warning: Out-of-bounds value at instruction: " << instruction << "\n";
                        errs() << "         Range inferred: ";
                        current.print(errs());
                        errs() << "\n";
                    }
                }
            }
        }
    }

    // Parse through main function and find variable UPPER and LOWER
    void extractBoundValuesFromMain(Function &F) {
        for (llvm::BasicBlock &basicBlock : F) {
            for (auto &instruction : basicBlock) {
                if (auto *storeInstruction = dyn_cast<StoreInst>(&instruction)) {
                    if (auto *constInt = dyn_cast<ConstantInt>(storeInstruction->getValueOperand())) {
                        if (auto *globalVar = dyn_cast<GlobalVariable>(storeInstruction->getPointerOperand())) {
                            StringRef name = globalVar->getName();
                            if (name.contains("LOWER"))
                                ConfigLower = constInt->getSExtValue();
                            else if (name.contains("UPPER"))
                                ConfigUpper = constInt->getSExtValue();
                        }
                    }
                }
            }
        }
    }

public:
    PreservedAnalyses run(Function &F, FunctionAnalysisManager &M) {
        if (F.isDeclaration()) 
		{
			return PreservedAnalyses::all();
		}

        if (F.getName() == "main")
		{
            extractBoundValuesFromMain(F);
            errs() << "Configured bounds: [" << ConfigLower << ", " << ConfigUpper << "]\n";
        }

        RangeMap Ranges;
        // nformation about all loops in function F
        LoopInfo &LI = M.getResult<LoopAnalysis>(F);
        // symbolic expressions over time
        ScalarEvolution &SE = M.getResult<ScalarEvolutionAnalysis>(F);

        analyzeFunction(F, Ranges, LI, SE);
        return PreservedAnalyses::all();
    }
};