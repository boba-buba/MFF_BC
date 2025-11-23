package gov.nasa.jpf.listener;

import gov.nasa.jpf.vm.*;
import gov.nasa.jpf.vm.bytecode.INVOKEVIRTUAL;
import gov.nasa.jpf.util.Logger;
import gov.nasa.jpf.Config;
import gov.nasa.jpf.ListenerAdapter;

public class MyDaisyFSListener extends ListenerAdapter {
    private static final Logger logger = Logger.getLogger("MyDaisyFSListener");

    @Override
    public void methodEntered(JVM vm) {
        MethodInfo mi = vm.getLastMethodInfo();
        ThreadInfo ti = vm.getCurrentThread();

        if (mi.getName().equals("creat") && mi.getClassName().contains("DaisyDir")) {
            FileHandler dir = (FileHandler) ti.getArgument(0);
            FileHandle fh = (FileHandle) ti.getArgument(2);

            // Check precondition: dir != null && fh != null && dir != fh
            if (dir == null || fh == null || dir == fh) {
                logger.severe("ERROR: Precondition violation in DaisyDir.creat()");
                vm.getSystemState().setIgnored(true); // Stop execution
            }
        }
    }

    @Override
    public void methodExited(JVM vm) {
        MethodInfo mi = vm.getLastMethodInfo();
        ThreadInfo ti = vm.getCurrentThread();

        if (mi.getName().equals("creat") && mi.getClassName().contains("DaisyDir")) {
            int returnValue = ti.getReturnValue();

            // Check if file was successfully created
            if (returnValue == 0) {  // Assuming 0 means SUCCESS
                FileHandle fh = (FileHandle) ti.getArgument(2);
                FileHandler dir = (FileHandler) ti.getArgument(0);
                byte[] filename = (byte[]) ti.getArgument(1);
                
                // Ensure the file exists after creation
                FileHandle lookupFH = new FileHandle();
                int lookupResult = DaisyFS.lookup(dir, filename, lookupFH);
                if (lookupResult != 0) {
                    logger.severe("ERROR: Lookup failed after successful file creation!");
                    vm.getSystemState().setIgnored(true);
                }
            }
        }

        if (mi.getName().equals("unlink") && mi.getClassName().contains("DaisyFS")) {
            FileHandler dir = (FileHandler) ti.getArgument(0);
            byte[] filename = (byte[]) ti.getArgument(1);
            
            // Ensure lookup fails after unlink
            FileHandle checkFH = new FileHandle();
            int lookupResult = DaisyFS.lookup(dir, filename, checkFH);
            if (lookupResult == 0) {
                logger.severe("ERROR: File still exists after unlink operation!");
                vm.getSystemState().setIgnored(true);
            }
        }
    }

    @Override
    public void instructionExecuted(JVM vm) {
        Instruction insn = vm.getLastInstruction();
        if (insn instanceof INVOKEVIRTUAL) {
            MethodInfo mi = ((INVOKEVIRTUAL) insn).getInvokedMethod();

            if (mi.getName().equals("creat") && mi.getClassName().contains("DaisyDir")) {
                ThreadInfo ti = vm.getCurrentThread();
                FileHandler dir = (FileHandler) ti.getArgument(0);
                byte[] filename = (byte[]) ti.getArgument(1);
                FileHandle fh = (FileHandle) ti.getArgument(2);

                // Check for duplicate file creation
                FileHandle existingFile = new FileHandle();
                if (DaisyFS.lookup(dir, filename, existingFile) == 0) { // File already exists
                    if (DaisyFS.creat(dir, filename, fh) == 0) { // Should have failed
                        logger.severe("ERROR: Duplicate file creation detected in DaisyDir.creat()");
                        vm.getSystemState().setIgnored(true);
                    }
                }

                // Check if lock is acquired before creat
                if (!LockManager.isLocked(dir)) {
                    logger.severe("ERROR: Directory lock must be acquired before creat()");
                    vm.getSystemState().setIgnored(true);
                }
            }
        }
    }
}
