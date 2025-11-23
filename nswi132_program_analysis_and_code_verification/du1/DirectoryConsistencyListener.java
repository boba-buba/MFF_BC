package gov.nasa.jpf.listener;

import gov.nasa.jpf.PropertyListenerAdapter;
import gov.nasa.jpf.search.Search;
import gov.nasa.jpf.vm.*;
import gov.nasa.jpf.ListenerAdapter;

import java.util.*;

public class DirectoryConsistencyListener extends PropertyListenerAdapter {
    private HashSet<String> createdFiles = new HashSet<>();
    private Boolean err = false;
    @Override
    public void executeInstruction(VM vm, ThreadInfo ti, Instruction insn) {
        MethodInfo mi = insn.getMethodInfo();

        if (mi != null) {
            String methodName = mi.getName();
            Object[] args = ti.getTopFrame().getArgumentValues(ti);

            if (args != null && args.length > 1) {
                if (methodName.equals("creat")) {
                    String fileName = args[1].toString();
                    if (createdFiles.contains(fileName)) {
                        err = true;
                        System.out.println("ERROR: Create failed for file: " + fileName);
                        System.err.println("[ERROR] Consistency of " + fileName);
                        vm.breakTransition("Consistency of " + fileName);
                    } else {
                        createdFiles.add(fileName);
                    }
                }
                if (methodName.equals("unlink")) {
                    String fileName = args[1].toString();
                    if (createdFiles.contains(fileName)) {
                        createdFiles.remove(fileName);
                    } else {
                        err = true;
                        System.out.println("ERROR: Delete failed for created file: " + fileName);
                        //vm.getSearch().error("Directory Consistency Violation: " + fileName);
                        System.err.println("[ERROR] Consistency of " + fileName);
                        vm.breakTransition("Consistency of " + fileName);
                    }
                    createdFiles.remove(args[1].toString());
                }
                if (methodName.equals("lookup")) {
                    String fileName = args[1].toString();
                    if (createdFiles.contains(fileName)) {
                        System.out.println("Lookup successful for created file: " + fileName);
                    } else {
                        err = true;
                        System.out.println("ERROR: Lookup failed for created file: " + fileName);
                        //vm.getSearch().error("Directory Consistency Violation: " + fileName);
                        System.err.println("[ERROR] Consistency of " + fileName);
                        vm.breakTransition("Consistency of " + fileName);
                    }
                }
            }

        }
    }

    @Override
    public boolean check(Search search, VM vm) {

        return !err;
    }

    @Override
    public String getErrorMessage() {
        return "Inconsistency detected!";
    }

    @Override
    public void reset() {
        createdFiles.clear();
        err = false;
    }
}