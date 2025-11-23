package gov.nasa.jpf.listener;

import gov.nasa.jpf.PropertyListenerAdapter;
import gov.nasa.jpf.search.Search;
import gov.nasa.jpf.vm.VM;
import gov.nasa.jpf.vm.ThreadInfo;
import gov.nasa.jpf.vm.MethodInfo;
import gov.nasa.jpf.vm.Instruction;
import gov.nasa.jpf.vm.ClassInfo;

import java.util.*;

public class UnexpectedDeletionListener extends PropertyListenerAdapter {
    private HashSet<String> deletedFiles = new HashSet<>();
    Boolean err = false;
    @Override
    public void instructionExecuted(VM vm, ThreadInfo ti, Instruction nextInsn, Instruction executedInsn) {

        Object[] args = ti.getTopFrame().getArgumentValues(ti);

        if (args != null && args.length > 1) {
            if (executedInsn.toString().contains("DaisyDir.unlink")) {
                String fileName = args[1].toString();

                deletedFiles.add(fileName);

            }

            // because lookup is called in read and write tasks we expect for file to exist
            if (executedInsn.toString().contains("DaisyDir.lookup")) {

                String fileName = args[1].toString();
                if (deletedFiles.contains(fileName)) {
                    err = true;
                    System.err.println("ERROR: Reading deleted file: " + fileName);
                    vm.breakTransition("Reading deleted file: " + fileName);
                }
            }

            if (executedInsn.toString().contains("DaisyDir.creat")) {
                String fileName = args[1].toString();
                deletedFiles.remove(fileName);
            }
        }
    }

    @Override
    public boolean check(Search search, VM vm) {

        return !err;
    }

    @Override
    public String getErrorMessage() {
        return "ERROR: UnexpectedDeletion";
    }

    @Override
    public void reset() {
        err = false;
        deletedFiles.clear();
    }
}
