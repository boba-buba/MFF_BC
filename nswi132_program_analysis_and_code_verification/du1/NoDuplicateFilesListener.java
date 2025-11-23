package gov.nasa.jpf.listener;

import gov.nasa.jpf.ListenerAdapter;
import gov.nasa.jpf.PropertyListenerAdapter;
import gov.nasa.jpf.search.Search;
import gov.nasa.jpf.vm.*;

import java.util.*;

/**
 * JPF listener to enforce the "No Duplicate Files" property in DaisyFS.
 * Ensures that no two files with the same name exist in the same directory.
 *
 */
public class NoDuplicateFilesListener extends ListenerAdapter {

    // Map to store directories and their corresponding file names
    private Map<String, Set<String>> directoryFileMap = new HashMap<>();
    private Set<String> createdFiles = new HashSet<>();
    //private Set<String> names = new LinkedHashSet<String>();
    private boolean flag = false;

    @Override
    public void methodExited(VM vm, ThreadInfo currentThread, MethodInfo exitedMethod) {
        Object[] args = currentThread.getTopFrame().getArgumentValues(currentThread);

        if (args == null || args.length < 2) {
            return;
        }
//        MethodInfo mi = instruction.getMethodInfo();
        int returnValue = -1;

        if (exitedMethod.getName().equals("creat") && exitedMethod.getClassName().contains("DaisyDir")) {
            StackFrame sf = currentThread.getTopFrame();

            if (sf != null) {
                returnValue = sf.peek();
                System.out.println("Return value: " + returnValue);
            }

            if (returnValue == 0 && createdFiles.contains(args[1].toString())) {

                System.err.println("ERROR: Duplicate found!");
                vm.breakTransition("ERROR: Duplicate found: " + args[1].toString());
                vm.getSystemState().setIgnored(true);
            } else {
                createdFiles.add(args[1].toString());
                System.out.println("Created: " + args[1].toString());
            }
        }
    }
}
